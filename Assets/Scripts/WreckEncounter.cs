using UnityEngine;

/// <summary>
/// Stages the "Rescue the Gunsmith" beat at the wreck POI: while that quest is
/// active and the player is at the wreck, a captive gunsmith appears (waving
/// for help) and a ring of beasts ambushes the wreck. Killing them feeds the
/// quest's DefeatEnemies objective. The ambush respects dev peace mode, and
/// the trigger is proximity-based (Update), so it fires whether you sail in or
/// turn spawning back on while parked at the wreck.
/// </summary>
public class WreckEncounter : MonoBehaviour
{
    public string questId = "rescue_gunsmith";
    public float triggerRadius = 7f;
    public int ambushCount = 10;
    public float ambushRing = 5.5f;

    private Transform player;
    private EnemySpawner spawner;
    private GameObject gunsmith;
    private bool ambushSpawned;
    private bool freed;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        spawner = FindFirstObjectByType<EnemySpawner>();
    }

    bool RescueActive()
    {
        var q = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest() : null;
        return q != null && q.id == questId;
    }

    void Update()
    {
        if (player == null) return;

        // Once the quest moves on, retire the captive (he's "joined the crew").
        if (!RescueActive())
        {
            if (gunsmith != null && !freed) { freed = true; Destroy(gunsmith, 0.1f); }
            return;
        }

        if (Vector2.Distance(player.position, transform.position) > triggerRadius) return;

        if (gunsmith == null) ShowGunsmith();
        if (!ambushSpawned && !ZoneSpawnManager.SpawningDisabled) SpawnAmbush();
    }

    void ShowGunsmith()
    {
        Sprite sprite = Resources.Load<Sprite>("Gunsmith");
        gunsmith = new GameObject("CaptiveGunsmith");
        gunsmith.transform.SetParent(transform);
        gunsmith.transform.position = transform.position + new Vector3(0f, 0.2f, 0f);

        var sr = gunsmith.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3; // above the wreck, below ships/FX
        if (sprite != null)
        {
            sr.sprite = sprite;
            float scale = 1.1f / Mathf.Max(0.01f, sprite.bounds.size.y); // ~1.1 wu tall
            gunsmith.transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            // Fallback marker if art is missing
            sr.sprite = null;
            gunsmith.transform.localScale = Vector3.one * 0.6f;
        }

        gunsmith.AddComponent<IdleSway>(); // gentle bob so he reads as alive
    }

    void SpawnAmbush()
    {
        if (spawner == null) return;
        ambushSpawned = true;

        GameObject[] pool = { spawner.crabEnemyPrefab, spawner.harpyEnemyPrefab, spawner.mermaidEnemyPrefab };
        for (int i = 0; i < ambushCount; i++)
        {
            float ang = (i / (float)ambushCount) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * (ambushRing + Random.Range(0f, 2f));
            GameObject prefab = pool[Random.Range(0, pool.Length)];
            if (prefab != null) spawner.SpawnEnemyPrefabAt(prefab, pos);
        }
    }
}
