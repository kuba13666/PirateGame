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
        // Load ship prefab if reference was lost (new field, Unity serialization quirk)
        #if UNITY_EDITOR
        if (spawner.enemyShipPrefab == null)
        {
            spawner.enemyShipPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Enemy_Ship.prefab");
        }
        #endif

        GameObject shipPrefab = spawner.enemyShipPrefab;
        Debug.Log($"Building waves. Ship prefab: {(shipPrefab != null ? shipPrefab.name : "NULL")}");

        // Wave 1: 10 crabs + 2 enemy ships (for testing)
        Wave w1 = new Wave();
        w1.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 10, interval = 1f });
        w1.entries.Add(new WaveEntry { prefab = shipPrefab, count = 2, interval = 2f });
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
        w3.entries.Add(new WaveEntry { prefab = shipPrefab, count = 2, interval = 2f });
        waves.Add(w3);

        // Wave 4: tougher mix + 3 ships
        Wave w4 = new Wave();
        w4.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 10, interval = 0.5f });
        w4.entries.Add(new WaveEntry { prefab = spawner.mermaidEnemyPrefab, count = 8, interval = 0.4f });
        w4.entries.Add(new WaveEntry { prefab = shipPrefab, count = 3, interval = 1.5f });
        waves.Add(w4);
    }

    IEnumerator RunWaves()
    {
        // Wait one frame so QuestManager.Start has activated the first quest
        // (Start order is undefined — checking immediately raced and missed
        // the Awakening quest, silently running normal waves instead).
        yield return null;

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
                if (entry.prefab == null)
                {
                    Debug.LogWarning($"Wave {waveNumber}: skipping entry with null prefab");
                    continue;
                }
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

    // The Awakening happens in pure open ocean (SE quadrant), far from any
    // port, island or the map edge — overwhelmed with nowhere to run.
    static readonly Vector3 AWAKENING_POSITION = new Vector3(90f, -90f, 0f);
    const int AWAKENING_MAX_ALIVE = 120;      // FPS guard
    // Recycle range sits just beyond the off-screen spawn ring (17), so the
    // moment the player outruns part of the swarm it respawns ahead of them.
    const float AWAKENING_CULL_DIST = 24f;

    /// <summary>
    /// The Awakening: an overwhelming, unrelenting flood of enemies in open
    /// ocean. The player is meant to die — it's the curse-reveal tutorial.
    /// </summary>
    IEnumerator RunAwakeningWave()
    {
        // Strand the player in empty ocean, far from every port and island
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = AWAKENING_POSITION;
            if (Camera.main != null)
            {
                Vector3 cam = AWAKENING_POSITION;
                cam.z = Camera.main.transform.position.z;
                Camera.main.transform.position = cam;
            }
        }

        yield return new WaitForSeconds(1.5f); // a breath of calm before the storm

        // (no wave banner — the onslaught is outside the wave system)

        // Opening salvo: a tight storm of beasts from every side
        for (int i = 0; i < 25; i++)
        {
            SpawnAwakening(spawner.crabEnemyPrefab);
            yield return new WaitForSeconds(0.06f);
        }
        for (int i = 0; i < 18; i++)
        {
            SpawnAwakening(spawner.harpyEnemyPrefab);
            yield return new WaitForSeconds(0.05f);
        }

        // Unrelenting flood until the sea takes you: mixed monsters with
        // cannon-armed ships joining the hunt. If the player kites the swarm,
        // outrun stragglers are silently recycled into fresh spawns around
        // the player's CURRENT position — the pressure never stops.
        int n = 0;
        while (true)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length >= AWAKENING_MAX_ALIVE)
                CullDistantAwakeningEnemies();

            if (GameObject.FindGameObjectsWithTag("Enemy").Length < AWAKENING_MAX_ALIVE)
            {
                n++;
                if (n % 10 == 0)
                    SpawnAwakening(spawner.enemyShipPrefab);
                else if (n % 3 == 0)
                    SpawnAwakening(spawner.mermaidEnemyPrefab);
                else if (n % 2 == 0)
                    SpawnAwakening(spawner.harpyEnemyPrefab);
                else
                    SpawnAwakening(spawner.crabEnemyPrefab);
            }
            yield return new WaitForSeconds(0.12f);
        }
    }

    /// <summary>
    /// Silently despawn a few enemies the player has outrun (no kill credit,
    /// no loot — Destroy bypasses the death path) so the flood can respawn
    /// them around the player's current position.
    /// </summary>
    void CullDistantAwakeningEnemies()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        Vector3 p = player.transform.position;

        int culled = 0;
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (Vector3.Distance(enemy.transform.position, p) > AWAKENING_CULL_DIST)
            {
                Destroy(enemy);
                if (++culled >= 10) break;
            }
        }
    }

    /// <summary>
    /// Spawn an awakening enemy faster than the player's ACTUAL ship (works
    /// for any equipped hull) so running is hopeless. Varied per-enemy speed
    /// keeps the swarm loose instead of one synchronized blob.
    /// </summary>
    void SpawnAwakening(GameObject prefab)
    {
        if (spawner == null || prefab == null) return;
        GameObject enemy = spawner.SpawnEnemyPrefab(prefab);
        if (enemy == null) return;

        float playerSpeed = GameConstants.PLAYER_MOVE_SPEED;
        var pcGo = GameObject.FindGameObjectWithTag("Player");
        var pc = pcGo != null ? pcGo.GetComponent<PlayerController>() : null;
        if (pc != null) playerSpeed = pc.moveSpeed;

        var ec = enemy.GetComponent<EnemyController>();
        if (ec != null) ec.moveSpeed = playerSpeed * Random.Range(1.05f, 1.25f);
        var ship = enemy.GetComponent<EnemyShipController>();
        if (ship != null) ship.moveSpeed = playerSpeed * Random.Range(0.85f, 1.0f); // they shoot, slight chase is enough
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
