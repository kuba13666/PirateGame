using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveEntry
    {
        public GameObject prefab;
        public int count;
        public float interval;
    }

    [System.Serializable]
    public class Wave
    {
        public List<WaveEntry> entries = new List<WaveEntry>();
    }

    [Header("References")]
    public EnemySpawner spawner;

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();

    [Header("Timing")]
    public float restBetweenWaves = 3f;

    void Start()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }
        if (spawner != null)
        {
            spawner.manualWaveControl = true;
        }

        // If no waves defined, create defaults
        if (waves.Count == 0)
        {
            BuildDefaultWaves();
        }

        StartCoroutine(RunWaves());
    }

    void BuildDefaultWaves()
    {
        // Wave 1: 10 crabs
        Wave w1 = new Wave();
        w1.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 10, interval = 1f });
        waves.Add(w1);

        // Wave 2: 8 crabs + 5 harpies
        Wave w2 = new Wave();
        w2.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 8, interval = 0.9f });
        w2.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 5, interval = 0.8f });
        waves.Add(w2);

        // Wave 3: 10 crabs + 8 harpies + 4 mermaids (faster)
        Wave w3 = new Wave();
        w3.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 10, interval = 0.7f });
        w3.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 8, interval = 0.6f });
        w3.entries.Add(new WaveEntry { prefab = spawner.mermaidEnemyPrefab, count = 4, interval = 0.5f });
        waves.Add(w3);

        // Wave 4: tougher mix
        Wave w4 = new Wave();
        w4.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 10, interval = 0.5f });
        w4.entries.Add(new WaveEntry { prefab = spawner.mermaidEnemyPrefab, count = 8, interval = 0.4f });
        waves.Add(w4);
    }

    IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            int waveNumber = i + 1;

            // Show wave text
            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.ShowWave(waveNumber);
            }

            // Spawn all entries in this wave
            foreach (var entry in waves[i].entries)
            {
                for (int c = 0; c < entry.count; c++)
                {
                    if (spawner != null)
                    {
                        spawner.SpawnEnemyPrefab(entry.prefab);
                    }
                    yield return new WaitForSeconds(entry.interval);
                }
            }

            // Wait until all enemies dead
            while (AnyEnemiesAlive())
            {
                yield return new WaitForSeconds(0.5f);
            }

            // Rest between waves (except after last wave)
            if (i < waves.Count - 1)
            {
                yield return new WaitForSeconds(restBetweenWaves);
            }
        }
    }

    bool AnyEnemiesAlive()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length > 0;
    }
}
