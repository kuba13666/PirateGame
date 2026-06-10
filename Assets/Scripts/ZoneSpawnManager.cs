using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zone-based ambient spawning (Biome 1, see Docs/BIOME1_DESIGN.md):
/// danger comes from WHERE you sail, not from a global wave timer.
/// Evaluates which zone the player is in and keeps a zone-specific mix and
/// density of enemies pressing in (always spawned off-screen).
/// The Awakening onslaught is owned by WaveManager; ambient spawning pauses
/// while it runs, while the player is in port, and while time is stopped.
/// </summary>
public class ZoneSpawnManager : MonoBehaviour
{
    private class Zone
    {
        public string name;
        public float interval;       // average seconds between spawns
        public int maxNearby;        // soft cap on enemies within NEARBY_RADIUS
        public float[] mix;          // weights: crab, harpy, mermaid, ship
        public System.Func<Vector2, bool> contains;
    }

    private const float NEARBY_RADIUS = 16f;   // ~on-screen: only these count toward the cap
    private const float CLEANUP_DIST = 45f;    // outrun enemies beyond this get despawned
    private const float EVALUATE_EVERY = 0.5f;

    private EnemySpawner spawner;
    private Transform player;
    private readonly List<Zone> zones = new List<Zone>();
    private Zone currentZone;
    private float spawnTimer;
    private float evaluateTimer;
    private Vector3 lastPlayerPos;
    private Vector2 heading;

    void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        BuildZones();
    }

    void BuildZones()
    {
        Vector2 home = new Vector2(GameConstants.HOME_PORT_X, GameConstants.HOME_PORT_Y);

        // Order matters: first match wins.
        zones.Add(new Zone
        {
            name = "Home Waters",
            interval = 0.6f,
            maxNearby = 24,
            mix = new[] { 1f, 0f, 0f, 0f }, // crabs only
            contains = pos => Vector2.Distance(pos, home) < 45f
        });
        zones.Add(new Zone
        {
            name = "The Deep",
            interval = 0.15f,
            maxNearby = 80,
            mix = new[] { 0.2f, 0.3f, 0.3f, 0.2f }, // everything, dense
            contains = pos => pos.y > 75f
        });
        zones.Add(new Zone
        {
            name = "Navy Waters",
            interval = 0.25f,
            maxNearby = 50,
            mix = new[] { 0.15f, 0.2f, 0.15f, 0.5f }, // ship-heavy
            contains = pos => pos.x > 25f && pos.y < -25f
        });
        zones.Add(new Zone
        {
            name = "The Hunting Grounds",
            interval = 0.2f,
            maxNearby = 55,
            mix = new[] { 0.15f, 0.4f, 0.35f, 0.1f }, // harpies & mermaids
            contains = pos => pos.x > 45f
        });
        zones.Add(new Zone
        {
            name = "The Trade Route",
            interval = 0.25f,
            maxNearby = 45,
            mix = new[] { 0.55f, 0.45f, 0f, 0f }, // crabs & harpies, light
            contains = pos => pos.x < -20f && pos.y > 10f
        });
        zones.Add(new Zone
        {
            name = "Open Waters",
            interval = 0.25f,
            maxNearby = 45,
            mix = new[] { 0.4f, 0.35f, 0.2f, 0.05f },
            contains = pos => true // fallback
        });
    }

    void Update()
    {
        if (player == null || spawner == null) return;
        if (Time.timeScale == 0f) return;                 // port / dialogue pause
        if (!spawner.enabled) return;                     // port disables spawning
        if (IsAwakeningActive()) return;                  // WaveManager owns the onslaught

        // Track the player's sailing direction (for ahead-biased spawns)
        heading = player.position - lastPlayerPos;
        lastPlayerPos = player.position;

        evaluateTimer -= Time.deltaTime;
        if (evaluateTimer <= 0f)
        {
            evaluateTimer = EVALUATE_EVERY;
            UpdateCurrentZone();
            CleanupOutrunEnemies();
        }

        if (currentZone == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = currentZone.interval * Random.Range(0.8f, 1.2f);
            TrySpawn();
        }
    }

    bool IsAwakeningActive()
    {
        var quest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest() : null;
        return quest != null && quest.id == "the_awakening";
    }

    void UpdateCurrentZone()
    {
        Vector2 pos = player.position;
        foreach (var z in zones)
        {
            if (!z.contains(pos)) continue;
            if (z != currentZone)
            {
                currentZone = z;
                spawnTimer = Mathf.Min(spawnTimer, 1f); // fresh zone bites quickly
                AnnounceZone(z.name);
            }
            return;
        }
    }

    void AnnounceZone(string zoneName)
    {
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            GameManager.Instance.uiManager.ShowZoneEnter(zoneName);
    }

    void TrySpawn()
    {
        int nearby = CountNearbyEnemies();
        int deficit = currentZone.maxNearby - nearby;
        if (deficit <= 0) return;

        // Burst when well below the cap so big zones fill fast
        int burst = deficit > currentZone.maxNearby / 2 ? 3 : 1;
        for (int i = 0; i < burst; i++)
            SpawnOne();
    }

    void SpawnOne()
    {
        GameObject prefab = PickWeighted(currentZone.mix);
        if (prefab == null) return;

        // A moving ship outruns side/behind spawns before they ever reach the
        // screen — bias spawns AHEAD of the heading so the player actually
        // meets the zone's dangers. Stationary players get surrounded evenly.
        Vector2 dir;
        if (heading.sqrMagnitude > 0.0001f && Random.value < 0.65f)
        {
            float ang = Random.Range(-70f, 70f) * Mathf.Deg2Rad;
            float c = Mathf.Cos(ang), s = Mathf.Sin(ang);
            Vector2 h = heading.normalized;
            dir = new Vector2(h.x * c - h.y * s, h.x * s + h.y * c);
        }
        else
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
        }

        float dist = EnemySpawner.OffscreenDistance(dir) + Random.Range(0f, 2f);
        spawner.SpawnEnemyPrefabAt(prefab, player.position + (Vector3)(dir * dist));
    }

    /// <summary>
    /// Counts enemies that actually threaten the player: close by, and (when
    /// sailing) not clearly behind. Outrun stragglers trailing the ship used
    /// to fill the cap and stall all new spawns for the rest of a voyage.
    /// </summary>
    int CountNearbyEnemies()
    {
        Vector3 p = player.position;
        bool moving = heading.sqrMagnitude > 0.0001f;
        Vector2 h = moving ? heading.normalized : Vector2.zero;

        int count = 0;
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Vector3 to = enemy.transform.position - p;
            if (to.magnitude >= NEARBY_RADIUS) continue;
            if (moving && Vector2.Dot(((Vector2)to).normalized, h) < -0.25f) continue; // behind us
            count++;
        }
        return count;
    }

    /// <summary>Despawn enemies the player has long outrun (no kill credit).</summary>
    void CleanupOutrunEnemies()
    {
        Vector3 p = player.position;
        int removed = 0;
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (Vector3.Distance(enemy.transform.position, p) > CLEANUP_DIST)
            {
                Destroy(enemy);
                if (++removed >= 8) break;
            }
        }
    }

    GameObject PickWeighted(float[] mix)
    {
        GameObject[] prefabs =
        {
            spawner.crabEnemyPrefab,
            spawner.harpyEnemyPrefab,
            spawner.mermaidEnemyPrefab,
            spawner.enemyShipPrefab
        };

        float total = 0f;
        for (int i = 0; i < mix.Length; i++)
            if (prefabs[i] != null) total += mix[i];
        if (total <= 0f) return null;

        float roll = Random.value * total;
        for (int i = 0; i < mix.Length; i++)
        {
            if (prefabs[i] == null) continue;
            roll -= mix[i];
            if (roll <= 0f) return prefabs[i];
        }
        return prefabs[0];
    }
}
