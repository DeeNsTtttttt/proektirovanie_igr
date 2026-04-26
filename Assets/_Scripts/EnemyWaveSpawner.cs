using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Waves")]
    [SerializeField, Min(1)] private int startWaveSize = 2;
    [SerializeField, Min(0)] private int waveSizeStep = 1;
    [SerializeField, Min(0.05f)] private float timeBetweenSpawns = 0.5f;
    [SerializeField, Min(0f)] private float timeBetweenWaves = 3f;
    [SerializeField, Min(1)] private int maxAliveEnemies = 8;
    [SerializeField] private bool autoStart = true;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private int currentWave;
    private bool spawning;

    public int CurrentWave => currentWave;
    public int AliveEnemies => aliveEnemies.Count;

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

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        spawning = true;

        while (true)
        {
            CleanupDeadEnemies();

            if (aliveEnemies.Count == 0)
            {
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
    }

    private IEnumerator SpawnWave(int waveSize)
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
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

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            aliveEnemies.Add(enemy);

            if (timeBetweenSpawns > 0f)
            {
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
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
