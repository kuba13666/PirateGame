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

    // Internal waves coroutine handle
    private Coroutine wavesRoutine;

    /// <summary>How many full wave cycles the player has survived this life.</summary>
    private int escalationLevel = 0;

    /// <summary>HP multiplier applied to spawned enemies based on escalation.</summary>
    public float EnemyHpMultiplier => 1f + escalationLevel * 0.25f;

    /// <summary>Speed multiplier applied to spawned enemies based on escalation.</summary>
    public float EnemySpeedMultiplier => 1f + escalationLevel * 0.1f;

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

        StartWaves();
    }

    void BuildDefaultWaves()
    {
        // Wave 1: 10 crabs + 2 enemy ships (for testing)
        Wave w1 = new Wave();
        w1.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 10, interval = 1f });
        w1.entries.Add(new WaveEntry { prefab = spawner.enemyShipPrefab, count = 2, interval = 2f });
        waves.Add(w1);

        // Wave 2: 8 crabs + 5 harpies
        Wave w2 = new Wave();
        w2.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 8, interval = 0.9f });
        w2.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 5, interval = 0.8f });
        waves.Add(w2);

        // Wave 3: 10 crabs + 8 harpies + 4 mermaids + 2 ships
        Wave w3 = new Wave();
        w3.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 10, interval = 0.7f });
        w3.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 8, interval = 0.6f });
        w3.entries.Add(new WaveEntry { prefab = spawner.mermaidEnemyPrefab, count = 4, interval = 0.5f });
        w3.entries.Add(new WaveEntry { prefab = spawner.enemyShipPrefab, count = 2, interval = 2f });
        waves.Add(w3);

        // Wave 4: tougher mix + 3 ships
        Wave w4 = new Wave();
        w4.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 10, interval = 0.5f });
        w4.entries.Add(new WaveEntry { prefab = spawner.mermaidEnemyPrefab, count = 8, interval = 0.4f });
        w4.entries.Add(new WaveEntry { prefab = spawner.enemyShipPrefab, count = 3, interval = 1.5f });
        waves.Add(w4);
    }

    IEnumerator RunWaves()
    {
        // If The Awakening quest is active, spawn an impossible wave first
        if (QuestManager.Instance != null)
        {
            var quest = QuestManager.Instance.GetActiveQuest();
            if (quest != null && quest.id == "the_awakening" && quest.state == Quest.QuestState.Active)
            {
                yield return StartCoroutine(RunAwakeningWave());
                yield break; // Don't run normal waves — player must die first
            }
        }

        for (int i = 0; i < waves.Count; i++)
        {
            int waveNumber = i + 1;

            // Show wave text
            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.ShowWave(waveNumber);
            }

            // Spawn all entries in this wave (scaled by escalation)
            foreach (var entry in waves[i].entries)
            {
                if (entry.prefab == null) continue;
                int scaledCount = entry.count + Mathf.FloorToInt(entry.count * escalationLevel * 0.2f);
                for (int c = 0; c < scaledCount; c++)
                {
                    if (spawner != null)
                    {
                        GameObject enemy = spawner.SpawnEnemyPrefab(entry.prefab);
                        ApplyEscalation(enemy);
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

        // All waves cleared — escalate and loop
        escalationLevel++;
        Debug.Log($"Wave cycle complete. Escalation level: {escalationLevel}");
        wavesRoutine = StartCoroutine(RunWaves());
    }

    /// <summary>
    /// The Awakening: an overwhelming flood of enemies. Player is meant to die.
    /// </summary>
    IEnumerator RunAwakeningWave()
    {
        yield return new WaitForSeconds(1f); // brief calm before the storm

        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            GameManager.Instance.uiManager.ShowWave(0); // shows "Wave 0" or whatever the UI does

        // Spawn 30 crabs almost instantly
        for (int i = 0; i < 30; i++)
        {
            if (spawner != null) spawner.SpawnEnemyPrefab(spawner.crabEnemyPrefab);
            yield return new WaitForSeconds(0.1f);
        }

        // Spawn 20 harpies fast
        for (int i = 0; i < 20; i++)
        {
            if (spawner != null) spawner.SpawnEnemyPrefab(spawner.harpyEnemyPrefab);
            yield return new WaitForSeconds(0.08f);
        }

        // Spawn 10 mermaids
        for (int i = 0; i < 10; i++)
        {
            if (spawner != null) spawner.SpawnEnemyPrefab(spawner.mermaidEnemyPrefab);
            yield return new WaitForSeconds(0.06f);
        }

        // Keep spawning until the player dies
        while (true)
        {
            if (spawner != null) spawner.SpawnEnemyPrefab(spawner.harpyEnemyPrefab);
            yield return new WaitForSeconds(0.15f);
        }
    }

    bool AnyEnemiesAlive()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length > 0;
    }

    // Public controls for pausing/resuming from PortZone
    public void StopWaves()
    {
        // StopAllCoroutines to also kill child coroutines (e.g. RunAwakeningWave)
        StopAllCoroutines();
        wavesRoutine = null;
    }

    public void StartWaves()
    {
        if (wavesRoutine == null)
        {
            wavesRoutine = StartCoroutine(RunWaves());
        }
    }

    // Reset to wave 1 and restart
    public void ResetAndStart()
    {
        StopWaves();
        StartWaves();
    }

    /// <summary>Reset escalation back to 0 (called on death).</summary>
    public void ResetEscalation()
    {
        escalationLevel = 0;
    }

    /// <summary>Apply escalation buffs to a freshly spawned enemy.</summary>
    void ApplyEscalation(GameObject enemy)
    {
        if (enemy == null || escalationLevel <= 0) return;

        var ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.maxHealth = Mathf.CeilToInt(ec.maxHealth * EnemyHpMultiplier);
            ec.moveSpeed *= EnemySpeedMultiplier;
        }

        var ship = enemy.GetComponent<EnemyShipController>();
        if (ship != null)
        {
            ship.maxHealth = Mathf.CeilToInt(ship.maxHealth * EnemyHpMultiplier);
            ship.moveSpeed *= EnemySpeedMultiplier;
        }
    }
}
