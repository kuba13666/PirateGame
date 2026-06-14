using UnityEngine;

/// <summary>
/// The Flying Dutchman miniboss (Q3), fought in the boss arena. Three phases
/// scaled to HP (see Docs/BIOME1_DESIGN.md §4.1):
///   1 (>66%)  Duel        — circles at broadside distance, fires single shots.
///   2 (>33%)  Ghost crew  — keeps circling, heavier volleys, summons spectral adds.
///   3 (<=33%) Doomsday    — speeds up and rams, rapid wide volleys.
/// Damageable via BossHealth (player projectiles). Spawned by BossArenaManager.
/// </summary>
[RequireComponent(typeof(BossHealth))]
public class FlyingDutchman : MonoBehaviour
{
    [HideInInspector] public GameObject projectilePrefab; // EnemyProjectile
    [HideInInspector] public GameObject[] addPrefabs;     // crab/harpy/mermaid

    public float orbitRadius = 9f;
    public float moveSpeed = 2.4f;

    private BossHealth hp;
    private Transform player;
    private SpriteRenderer sr;
    private Color baseColor;
    private float orbitAngle, fireTimer, summonTimer, contactCd;
    private int lastPhase = 1;

    void Start()
    {
        hp = GetComponent<BossHealth>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr != null ? sr.color : Color.white;
        orbitAngle = Random.Range(0f, 360f);
        fireTimer = 1.5f;
        summonTimer = 4f;
    }

    int Phase()
    {
        float f = hp != null && hp.maxHealth > 0 ? (float)hp.Health / hp.maxHealth : 1f;
        return f > 0.66f ? 1 : f > 0.33f ? 2 : 3;
    }

    void Update()
    {
        if (player == null || hp == null) return;
        int phase = Phase();
        if (phase != lastPhase) lastPhase = phase; // (hook for phase-change FX later)

        float speed = phase == 3 ? moveSpeed * 1.7f : moveSpeed;

        if (phase < 3)
        {
            // Circle the player, presenting a broadside
            orbitAngle += 34f * Time.deltaTime;
            Vector2 off = new Vector2(Mathf.Cos(orbitAngle * Mathf.Deg2Rad), Mathf.Sin(orbitAngle * Mathf.Deg2Rad)) * orbitRadius;
            Vector3 target = (Vector3)((Vector2)player.position + off);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
        else
        {
            // Doomsday: ram the player
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * speed * Time.deltaTime);
        }

        // Volleys
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireVolley(phase == 1 ? 1 : phase == 2 ? 2 : 3);
            fireTimer = phase == 3 ? 0.85f : phase == 2 ? 1.2f : 1.7f;
        }

        // Ghost crew (phase 2+)
        if (phase >= 2)
        {
            summonTimer -= Time.deltaTime;
            if (summonTimer <= 0f) { SummonAdds(phase == 3 ? 3 : 2); summonTimer = 6.5f; }
        }

        // Ghostly alpha pulse
        if (sr != null)
        {
            Color c = baseColor;
            c.a = baseColor.a * (0.78f + 0.18f * Mathf.Sin(Time.time * 3.2f));
            sr.color = c;
        }

        if (contactCd > 0f) contactCd -= Time.deltaTime;
    }

    void FireVolley(int shots)
    {
        if (projectilePrefab == null || player == null) return;
        Vector2 toP = ((Vector2)player.position - (Vector2)transform.position).normalized;
        for (int i = 0; i < shots; i++)
        {
            float spread = (i - (shots - 1) * 0.5f) * 13f * Mathf.Deg2Rad;
            Vector2 d = Rotate(toP, spread);
            var proj = Instantiate(projectilePrefab, transform.position + (Vector3)(d * 1.6f), Quaternion.identity);
            var ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null) ep.SetDirection(d);
        }
    }

    void SummonAdds(int n)
    {
        if (addPrefabs == null || addPrefabs.Length == 0) return;
        for (int i = 0; i < n; i++)
        {
            var prefab = addPrefabs[Random.Range(0, addPrefabs.Length)];
            if (prefab == null) continue;
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 4f;
            var add = Instantiate(prefab, (Vector3)pos, Quaternion.identity);
            ScaleEnemy(add, GameConstants.ENEMY_TARGET_HEIGHT);
            var asr = add.GetComponent<SpriteRenderer>();
            if (asr != null) asr.color = new Color(0.55f, 1f, 0.8f, 0.85f); // spectral tint
        }
    }

    static void ScaleEnemy(GameObject e, float targetH)
    {
        var esr = e.GetComponent<SpriteRenderer>();
        float h = (esr != null && esr.sprite != null) ? esr.sprite.bounds.size.y : 1f;
        float s = targetH / Mathf.Max(0.01f, h);
        e.transform.localScale = new Vector3(s, s, 1f);
        var col = e.GetComponent<BoxCollider2D>();
        if (col != null && esr != null && esr.sprite != null)
        {
            col.size = esr.sprite.bounds.size;
            col.offset = esr.sprite.bounds.center;
        }
    }

    void OnTriggerStay2D(Collider2D c)
    {
        if (contactCd > 0f) return;
        if (!c.CompareTag("Player")) return;
        var pc = c.GetComponent<PlayerController>();
        if (pc != null) { pc.TakeDamage(2); contactCd = 0.9f; }
    }

    static Vector2 Rotate(Vector2 v, float rad)
    {
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
