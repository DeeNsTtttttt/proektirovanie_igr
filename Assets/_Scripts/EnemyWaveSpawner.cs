using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("AI")]
    [SerializeField] private Transform[] patrolWaypoints;

    [Header("Waves")]
    [SerializeField, Min(1)] private int startWaveSize = 2;
    [SerializeField, Min(0)] private int waveSizeStep = 1;
    [SerializeField, Min(0)] private int maxWaves = 0;
    [SerializeField, Min(0.05f)] private float timeBetweenSpawns = 0.5f;
    [SerializeField, Min(0f)] private float timeBetweenWaves = 3f;
    [SerializeField, Min(1)] private int maxAliveEnemies = 8;
    [SerializeField] private bool autoStart = false;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private readonly List<Transform> validSpawnPoints = new List<Transform>();
    private int currentWave;
    private bool spawning;
    private bool isFinished;
    private Coroutine spawnCoroutine;

    public int CurrentWave => currentWave;
    public int AliveEnemies => aliveEnemies.Count;
    public int MaxWaves => maxWaves;
    public bool IsRunning => spawning;
    public bool IsFinished => isFinished;
    public event Action<EnemyWaveSpawner> Completed;

    private void Start()
    {
        if (autoStart)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        if (spawning)
        {
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning($"EnemyWaveSpawner '{name}': enemyPrefab is not assigned.", this);
            return;
        }

        RefreshValidSpawnPoints();
        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning(
                $"EnemyWaveSpawner '{name}': spawnPoints are not assigned (or contain only None).",
                this);
            return;
        }

        // Allow reusing the same spawner for another arena run.
        currentWave = 0;
        isFinished = false;
        aliveEnemies.Clear();

        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopWaves()
    {
        if (!spawning)
        {
            return;
        }

        spawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        spawning = true;

        while (spawning)
        {
            CleanupDeadEnemies();

            if (aliveEnemies.Count == 0)
            {
                if (maxWaves > 0 && currentWave >= maxWaves)
                {
                    isFinished = true;
                    spawning = false;
                    spawnCoroutine = null;
                    Completed?.Invoke(this);
                    yield break;
                }

                currentWave++;
                int waveSize = startWaveSize + (currentWave - 1) * waveSizeStep;
                yield return SpawnWave(waveSize);

                if (timeBetweenWaves > 0f)
                {
                    yield return new WaitForSeconds(timeBetweenWaves);
                }
            }

            yield return null;
        }

        spawnCoroutine = null;
    }

    private IEnumerator SpawnWave(int waveSize)
    {
        if (enemyPrefab == null)
        {
            yield break;
        }

        RefreshValidSpawnPoints();
        if (validSpawnPoints.Count == 0)
        {
            yield break;
        }

        for (int i = 0; i < waveSize; i++)
        {
            while (aliveEnemies.Count >= maxAliveEnemies)
            {
                CleanupDeadEnemies();
                yield return null;
            }

            Transform spawnPoint = validSpawnPoints[UnityEngine.Random.Range(0, validSpawnPoints.Count)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            AssignPatrolToEnemy(enemy);
            aliveEnemies.Add(enemy);

            if (timeBetweenSpawns > 0f)
            {
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }
    }

    private void RefreshValidSpawnPoints()
    {
        validSpawnPoints.Clear();
        if (spawnPoints == null)
        {
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                validSpawnPoints.Add(spawnPoints[i]);
            }
        }
    }

    private void AssignPatrolToEnemy(GameObject enemy)
    {
        if (enemy == null || patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            return;
        }

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai == null)
        {
            ai = enemy.GetComponentInChildren<EnemyAI>();
        }

        if (ai != null)
        {
            ai.SetWaypoints(patrolWaypoints);
        }
    }

    private void CleanupDeadEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }
}
