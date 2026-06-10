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

        // Wave 1: 10 crabs + 2 enemy ships
        Wave w1 = new Wave();
        w1.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 10, interval = 0.4f });
        w1.entries.Add(new WaveEntry { prefab = shipPrefab, count = 2, interval = 1.2f });
        waves.Add(w1);

        // Wave 2: 8 crabs + 5 harpies
        Wave w2 = new Wave();
        w2.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 8, interval = 0.4f });
        w2.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 5, interval = 0.35f });
        waves.Add(w2);

        // Wave 3: 10 crabs + 8 harpies + 4 mermaids + 2 ships
        Wave w3 = new Wave();
        w3.entries.Add(new WaveEntry { prefab = spawner.crabEnemyPrefab, count = 10, interval = 0.35f });
        w3.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 8, interval = 0.3f });
        w3.entries.Add(new WaveEntry { prefab = spawner.mermaidEnemyPrefab, count = 4, interval = 0.3f });
        w3.entries.Add(new WaveEntry { prefab = shipPrefab, count = 2, interval = 1.2f });
        waves.Add(w3);

        // Wave 4: tougher mix + 3 ships
        Wave w4 = new Wave();
        w4.entries.Add(new WaveEntry { prefab = spawner.harpyEnemyPrefab, count = 10, interval = 0.3f });
        w4.entries.Add(new WaveEntry { prefab = spawner.mermaidEnemyPrefab, count = 8, interval = 0.25f });
        w4.entries.Add(new WaveEntry { prefab = shipPrefab, count = 3, interval = 1f });
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

        // Ambient spawning on the open sea is owned by ZoneSpawnManager
        // (Phase B): danger comes from WHERE the player sails. The timed
        // wave cycle is retired; WaveManager keeps only the Awakening.
        yield break;
    }

    // The Awakening happens in an OFF-MAP ocean pocket — same scene, but far
    // outside the world bounds, so there is genuinely nothing there: no
    // harbors, no islands, no POIs. Just empty sea and the swarm.
    static readonly Vector3 AWAKENING_POSITION = new Vector3(400f, -400f, 0f);
    const float AWAKENING_ARENA_HALF = 60f; // temporary movement bounds around the pocket
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
        // Strand the player in the off-map pocket and re-clamp their movement
        // around it (GameManager restores the real map bounds on death).
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = AWAKENING_POSITION;
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.minX = AWAKENING_POSITION.x - AWAKENING_ARENA_HALF;
                pc.maxX = AWAKENING_POSITION.x + AWAKENING_ARENA_HALF;
                pc.minY = AWAKENING_POSITION.y - AWAKENING_ARENA_HALF;
                pc.maxY = AWAKENING_POSITION.y + AWAKENING_ARENA_HALF;
            }
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
        // cannon-armed ships joining the hunt. Most spawns drop AHEAD of the
        // player's heading (running into fresh enemies, not away from old
        // ones), and every few seconds a wall of beasts cuts across their
        // course. Outrun stragglers recycle into new spawns — no escape.
        int n = 0;
        Vector3 lastPlayerPos = player != null ? player.transform.position : Vector3.zero;
        while (true)
        {
            // Estimate the player's heading from actual movement
            Vector2 heading = Vector2.zero;
            if (player != null)
            {
                Vector3 p = player.transform.position;
                heading = (p - lastPlayerPos);
                lastPlayerPos = p;
            }

            if (GameObject.FindGameObjectsWithTag("Enemy").Length >= AWAKENING_MAX_ALIVE)
                CullDistantAwakeningEnemies();

            if (GameObject.FindGameObjectsWithTag("Enemy").Length < AWAKENING_MAX_ALIVE)
            {
                n++;
                GameObject prefab =
                    (n % 10 == 0) ? spawner.enemyShipPrefab :
                    (n % 3 == 0) ? spawner.mermaidEnemyPrefab :
                    (n % 2 == 0) ? spawner.harpyEnemyPrefab :
                                   spawner.crabEnemyPrefab;
                SpawnAwakening(prefab, AwakeningSpawnPos(heading));

                // Every ~4s: a wall of harpies straight across the escape path
                if (n % 34 == 0 && heading.sqrMagnitude > 0.0001f)
                {
                    Vector2 dir = heading.normalized;
                    for (int k = -2; k <= 2; k++)
                    {
                        float ang = k * 18f * Mathf.Deg2Rad;
                        Vector2 d = Rotate(dir, ang);
                        Vector3 pos = lastPlayerPos + (Vector3)(d * 15f);
                        SpawnAwakening(spawner.harpyEnemyPrefab, pos);
                    }
                }
            }
            yield return new WaitForSeconds(0.12f);
        }
    }

    /// <summary>
    /// Spawn position for the flood: 70% in a wide arc ahead of the player's
    /// heading (off-screen), the rest anywhere on the ring. Stationary players
    /// get surrounded evenly.
    /// </summary>
    Vector3 AwakeningSpawnPos(Vector2 heading)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 center = player != null ? player.transform.position : AWAKENING_POSITION;

        Vector2 dir;
        if (heading.sqrMagnitude > 0.0001f && Random.value < 0.7f)
        {
            // Ahead: within ±75° of the escape direction
            float ang = Random.Range(-75f, 75f) * Mathf.Deg2Rad;
            dir = Rotate(heading.normalized, ang);
        }
        else
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
        }

        float dist = EnemySpawner.OffscreenDistance(dir) + Random.Range(0f, 2f);
        return center + (Vector3)(dir * dist);
    }

    static Vector2 Rotate(Vector2 v, float radians)
    {
        float c = Mathf.Cos(radians), s = Mathf.Sin(radians);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
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
    void SpawnAwakening(GameObject prefab) => SpawnAwakening(prefab, (Vector3?)null);

    void SpawnAwakening(GameObject prefab, Vector3? position)
    {
        if (spawner == null || prefab == null) return;
        GameObject enemy = position.HasValue
            ? spawner.SpawnEnemyPrefabAt(prefab, position.Value)
            : spawner.SpawnEnemyPrefab(prefab);
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
