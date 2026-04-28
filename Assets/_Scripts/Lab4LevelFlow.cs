using TMPro;
using UnityEngine;

public class Lab4LevelFlow : MonoBehaviour
{
    [System.Serializable]
    public class ArenaConfig
    {
        public string name = "Arena";
        public Collider trigger;
        public GameObject gate;
        public EnemyWaveSpawner[] spawners;
        [Min(0)] public int requiredCoins = 1;
        [TextArea] public string objectiveText = "Clear arena and collect coins";
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TMP_Text objectiveText;

    [Header("Arenas")]
    [SerializeField] private ArenaConfig[] arenas;
    [SerializeField] private bool lockGatesOnStart = true;

    private int currentArenaIndex = -1;
    private bool[] arenaStarted;
    private bool[] arenaCompleted;
    private bool[] wasInsideTrigger;
    private int[] arenaScoreBaseline;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (playerCollider == null && player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
            {
                playerCollider = player.GetComponentInChildren<Collider>();
            }
        }

        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        int count = arenas != null ? arenas.Length : 0;
        arenaStarted = new bool[count];
        arenaCompleted = new bool[count];
        wasInsideTrigger = new bool[count];
        arenaScoreBaseline = new int[count];

        if (lockGatesOnStart)
        {
            for (int i = 0; i < count; i++)
            {
                SetGateOpen(i, false);
            }
        }
    }

    private void OnEnable()
    {
        if (uiManager != null)
        {
            uiManager.ScoreChanged += OnScoreChanged;
        }

        SubscribeSpawnerEvents(true);
    }

    private void OnDisable()
    {
        if (uiManager != null)
        {
            uiManager.ScoreChanged -= OnScoreChanged;
        }

        SubscribeSpawnerEvents(false);
    }

    private void Update()
    {
        CheckArenaTriggerTransitions();
        UpdateObjectiveText();
    }

    public void EnterArena(int index)
    {
        if (!IsArenaIndexValid(index))
        {
            return;
        }

        if (index > 0 && !arenaCompleted[index - 1])
        {
            if (objectiveText != null)
            {
                objectiveText.text = $"Finish arena {index} first";
            }

            Debug.Log($"Lab4LevelFlow: Arena {index + 1} blocked. Previous arena is not completed.");
            return;
        }

        currentArenaIndex = index;

        if (arenaStarted[index])
        {
            return;
        }

        arenaStarted[index] = true;
        arenaScoreBaseline[index] = uiManager != null ? uiManager.Score : 0;

        ArenaConfig arena = arenas[index];
        if (arena.spawners != null)
        {
            foreach (EnemyWaveSpawner spawner in arena.spawners)
            {
                if (spawner != null)
                {
                    spawner.StartWaves();
                }
            }
        }

        TryCompleteArena();
    }

    public void TryCompleteArena()
    {
        TryCompleteArena(currentArenaIndex);
    }

    private void TryCompleteArena(int index)
    {
        if (!IsArenaIndexValid(index) || !arenaStarted[index] || arenaCompleted[index])
        {
            return;
        }

        bool wavesCleared = AreArenaSpawnersFinished(index);
        bool coinsCollected = GetArenaCoins(index) >= arenas[index].requiredCoins;

        if (!wavesCleared || !coinsCollected)
        {
            return;
        }

        arenaCompleted[index] = true;
        SetGateOpen(index, true);

        if (index + 1 < arenas.Length)
        {
            currentArenaIndex = index + 1;
        }
        else
        {
            currentArenaIndex = index;
            if (objectiveText != null)
            {
                objectiveText.text = "Level completed";
            }
        }
    }

    private void OnScoreChanged(int _)
    {
        TryCompleteArena();
    }

    private void OnSpawnerCompleted(EnemyWaveSpawner spawner)
    {
        if (spawner == null || arenas == null)
        {
            return;
        }

        bool handled = false;
        for (int i = 0; i < arenas.Length; i++)
        {
            ArenaConfig arena = arenas[i];
            if (arena.spawners == null)
            {
                continue;
            }

            for (int j = 0; j < arena.spawners.Length; j++)
            {
                if (arena.spawners[j] == spawner)
                {
                    TryCompleteArena(i);
                    handled = true;
                    break;
                }
            }
        }

        if (!handled)
        {
            TryCompleteArena();
        }
    }

    private void CheckArenaTriggerTransitions()
    {
        if (arenas == null || player == null)
        {
            return;
        }

        for (int i = 0; i < arenas.Length; i++)
        {
            Collider trigger = arenas[i].trigger;
            if (trigger == null)
            {
                continue;
            }

            bool inside = IsPlayerInsideTrigger(trigger);
            if (inside && !wasInsideTrigger[i])
            {
                EnterArena(i);
            }

            wasInsideTrigger[i] = inside;
        }
    }

    private bool IsPlayerInsideTrigger(Collider trigger)
    {
        if (trigger == null || player == null)
        {
            return false;
        }

        if (playerCollider != null)
        {
            return trigger.bounds.Intersects(playerCollider.bounds);
        }

        Vector3 closest = trigger.ClosestPoint(player.position);
        return (closest - player.position).sqrMagnitude <= 0.0001f;
    }

    private bool AreArenaSpawnersFinished(int index)
    {
        ArenaConfig arena = arenas[index];
        if (arena.spawners == null || arena.spawners.Length == 0)
        {
            return true;
        }

        foreach (EnemyWaveSpawner spawner in arena.spawners)
        {
            if (spawner == null)
            {
                continue;
            }

            if (!spawner.IsFinished)
            {
                return false;
            }
        }

        return true;
    }

    private int GetArenaCoins(int index)
    {
        if (uiManager == null || !arenaStarted[index])
        {
            return 0;
        }

        return Mathf.Max(0, uiManager.Score - arenaScoreBaseline[index]);
    }

    private void UpdateObjectiveText()
    {
        if (objectiveText == null || arenas == null || arenas.Length == 0)
        {
            return;
        }

        if (currentArenaIndex < 0 || currentArenaIndex >= arenas.Length)
        {
            objectiveText.text = "Enter the first arena";
            return;
        }

        ArenaConfig arena = arenas[currentArenaIndex];
        int coins = GetArenaCoins(currentArenaIndex);
        int target = arena.requiredCoins;
        int clampedCoins = Mathf.Min(coins, target);

        string wavesText = "no waves";
        if (arena.spawners != null && arena.spawners.Length > 0)
        {
            int totalCurrent = 0;
            int totalMax = 0;
            for (int i = 0; i < arena.spawners.Length; i++)
            {
                EnemyWaveSpawner spawner = arena.spawners[i];
                if (spawner == null)
                {
                    continue;
                }

                totalCurrent += spawner.CurrentWave;
                int max = spawner.MaxWaves > 0 ? spawner.MaxWaves : spawner.CurrentWave;
                totalMax += max;
            }

            wavesText = $"waves {totalCurrent}/{Mathf.Max(totalMax, totalCurrent)}";
        }

        string status = arenaCompleted[currentArenaIndex] ? "done" : "in progress";
        objectiveText.text =
            $"Arena {currentArenaIndex + 1}: {arena.objectiveText}\nCoins {clampedCoins}/{target}, {wavesText}, {status}";
    }

    private void SetGateOpen(int arenaIndex, bool open)
    {
        if (!IsArenaIndexValid(arenaIndex))
        {
            return;
        }

        GameObject gate = arenas[arenaIndex].gate;
        if (gate != null)
        {
            gate.SetActive(!open);
        }
    }

    private void SubscribeSpawnerEvents(bool subscribe)
    {
        if (arenas == null)
        {
            return;
        }

        for (int i = 0; i < arenas.Length; i++)
        {
            EnemyWaveSpawner[] spawners = arenas[i].spawners;
            if (spawners == null)
            {
                continue;
            }

            for (int j = 0; j < spawners.Length; j++)
            {
                EnemyWaveSpawner spawner = spawners[j];
                if (spawner == null)
                {
                    continue;
                }

                if (subscribe)
                {
                    spawner.Completed += OnSpawnerCompleted;
                }
                else
                {
                    spawner.Completed -= OnSpawnerCompleted;
                }
            }
        }
    }

    private bool IsArenaIndexValid(int index)
    {
        return arenas != null && index >= 0 && index < arenas.Length;
    }
}
