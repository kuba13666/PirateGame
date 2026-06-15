using System.Collections;
using UnityEngine;

/// <summary>
/// Mocha Dick, the cursed white whale (Q4 miniboss), fought in the boss arena.
/// Three HP-scaled phases (see Docs/BIOME1_DESIGN.md §4.2):
///   1 (>66%)  Surface  — slow telegraphed ram charges; always vulnerable.
///   2 (>33%)  Dives    — submerges (invulnerable), a shadow tracks the player,
///                        then it breaches there (AoE) and spits infected adds.
///   3 (<=33%) Frenzy   — faster charges, dive-breach, a tail-slam shockwave
///                        ring, more parasites.
/// Only damageable while surfaced. Spawned by BossArenaManager.
/// </summary>
[RequireComponent(typeof(BossHealth))]
public class MochaDick : MonoBehaviour
{
    [HideInInspector] public GameObject projectilePrefab; // EnemyProjectile (shockwave)
    [HideInInspector] public GameObject[] addPrefabs;      // crab/mermaid parasites

    public float moveSpeed = 2.0f;
    public float chargeSpeed = 7f;

    private BossHealth hp;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private Color baseColor;
    private float flashTimer, contactCd;
    private bool submerged;
    private int lastPhase = 1;

    void Start()
    {
        hp = GetComponent<BossHealth>();
        rb = GetComponent<Rigidbody2D>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr != null ? sr.color : Color.white;
        if (hp != null) hp.onHealthChanged += (c, m) => flashTimer = 0.12f;
        StartCoroutine(Behaviour());
    }

    int Phase()
    {
        float f = hp != null && hp.maxHealth > 0 ? (float)hp.Health / hp.maxHealth : 1f;
        return f > 0.66f ? 1 : f > 0.33f ? 2 : 3;
    }

    void Update()
    {
        // Hit flash (skip while submerged — the dive coroutine owns the alpha)
        if (sr != null && !submerged)
        {
            if (flashTimer > 0f) { flashTimer -= Time.deltaTime; sr.color = Color.white; }
            else sr.color = baseColor;
        }
        if (contactCd > 0f) contactCd -= Time.deltaTime;
    }

    IEnumerator Behaviour()
    {
        yield return new WaitForSeconds(1f);
        while (player != null && hp != null)
        {
            int phase = Phase();
            if (phase != lastPhase) lastPhase = phase;

            if (phase == 1)
            {
                yield return Charge(chargeSpeed);
                yield return Reposition(1.3f);
            }
            else if (phase == 2)
            {
                yield return Charge(chargeSpeed);
                yield return Reposition(0.8f);
                yield return Dive(1.9f);
                SpawnParasites(2);
                yield return Reposition(1.1f);
            }
            else
            {
                yield return Charge(chargeSpeed * 1.35f);
                yield return Dive(1.4f);
                TailSlam();
                SpawnParasites(3);
                yield return Reposition(0.7f);
            }
        }
    }

    // Approach, wind-up (telegraph), then lunge across the player.
    IEnumerator Charge(float speed)
    {
        if (player == null) yield break;
        float approach = 0f;
        while (approach < 1.2f && Vector2.Distance(transform.position, player.position) > 7f)
        {
            approach += Time.deltaTime;
            Vector2 d = ((Vector2)player.position - rb.position).normalized;
            FaceDir(d);
            rb.MovePosition(rb.position + d * moveSpeed * Time.deltaTime);
            yield return null;
        }
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        FaceDir(dir);
        yield return new WaitForSeconds(0.55f); // wind-up telegraph
        float t = 0f;
        while (t < 0.85f)
        {
            t += Time.deltaTime;
            rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
            yield return null;
        }
    }

    // Drift to a bearing off the player and hold (the vulnerable window).
    IEnumerator Reposition(float dur)
    {
        float t = 0f;
        float ang = Random.Range(0f, 360f);
        while (t < dur)
        {
            t += Time.deltaTime;
            if (player != null)
            {
                Vector2 target = (Vector2)player.position + new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)) * 6f;
                Vector2 d = target - rb.position;
                if (d.magnitude > 0.1f) { FaceDir(d.normalized); rb.MovePosition(rb.position + d.normalized * moveSpeed * Time.deltaTime); }
            }
            yield return null;
        }
    }

    // Submerge (invulnerable), a shadow tracks the player, then breach (AoE).
    IEnumerator Dive(float trackTime)
    {
        if (hp != null) hp.invulnerable = true;
        submerged = true;
        yield return FadeAlpha(baseColor.a, 0f, 0.4f);

        var shadow = MakeShadow();
        Vector2 shPos = transform.position;
        float t = 0f;
        while (t < trackTime)
        {
            t += Time.deltaTime;
            if (player != null) shPos = Vector2.MoveTowards(shPos, player.position, moveSpeed * 1.5f * Time.deltaTime);
            if (shadow != null) shadow.transform.position = (Vector3)shPos;
            yield return null;
        }
        if (shadow != null) Destroy(shadow);

        // breach at the shadow
        transform.position = (Vector3)shPos;
        if (rb != null) rb.position = shPos;
        submerged = false;
        yield return FadeAlpha(0f, baseColor.a, 0.22f);
        if (player != null && Vector2.Distance(player.position, transform.position) < 3.5f)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.TakeDamage(2);
        }
        if (hp != null) hp.invulnerable = false;
    }

    // Tail slam: a full ring of projectiles bursting outward.
    void TailSlam()
    {
        if (projectilePrefab == null) return;
        const int n = 14;
        for (int i = 0; i < n; i++)
        {
            float a = (i / (float)n) * Mathf.PI * 2f;
            Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            var proj = Instantiate(projectilePrefab, transform.position + (Vector3)(d * 1.5f), Quaternion.identity);
            var ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null) ep.SetDirection(d);
        }
    }

    void SpawnParasites(int count)
    {
        if (addPrefabs == null || addPrefabs.Length == 0) return;
        for (int i = 0; i < count; i++)
        {
            var prefab = addPrefabs[Random.Range(0, addPrefabs.Length)];
            if (prefab == null) continue;
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 4f;
            var add = Instantiate(prefab, (Vector3)pos, Quaternion.identity);
            ScaleEnemy(add, GameConstants.ENEMY_TARGET_HEIGHT);
            var asr = add.GetComponent<SpriteRenderer>();
            if (asr != null) asr.color = new Color(0.7f, 1f, 0.5f, 0.95f); // sickly infected tint
        }
    }

    GameObject MakeShadow()
    {
        var go = new GameObject("WhaleShadow");
        var ssr = go.AddComponent<SpriteRenderer>();
        Sprite blob = Resources.Load<Sprite>("Smoke"); // soft puff reused as an underwater shadow
        ssr.sprite = blob;
        ssr.color = new Color(0.05f, 0.1f, 0.18f, 0.5f);
        ssr.sortingOrder = -1;
        float h = blob != null ? blob.bounds.size.y : 1f;
        float s = 4f / Mathf.Max(0.01f, h);
        go.transform.localScale = new Vector3(s, s, 1f);
        go.transform.position = transform.position;
        return go;
    }

    static void ScaleEnemy(GameObject e, float targetH)
    {
        var esr = e.GetComponent<SpriteRenderer>();
        float h = (esr != null && esr.sprite != null) ? esr.sprite.bounds.size.y : 1f;
        float s = targetH / Mathf.Max(0.01f, h);
        e.transform.localScale = new Vector3(s, s, 1f);
        var col = e.GetComponent<BoxCollider2D>();
        if (col != null && esr != null && esr.sprite != null) { col.size = esr.sprite.bounds.size; col.offset = esr.sprite.bounds.center; }
    }

    void FaceDir(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // sprite's head points up
        transform.rotation = Quaternion.Euler(0f, 0f, ang);
    }

    IEnumerator FadeAlpha(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur) { t += Time.deltaTime; SetAlpha(Mathf.Lerp(from, to, t / dur)); yield return null; }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        if (sr == null) return;
        Color c = baseColor; c.a = a; sr.color = c;
    }

    void OnTriggerStay2D(Collider2D c)
    {
        if (submerged || contactCd > 0f) return;
        if (!c.CompareTag("Player")) return;
        var pc = c.GetComponent<PlayerController>();
        if (pc != null) { pc.TakeDamage(2); contactCd = 0.9f; }
    }
}
