using System.Collections;
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

    public float orbitRadius = 6.5f;
    public float moveSpeed = 2.2f;

    private BossHealth hp;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private Color baseColor;
    private float orbitAngle, fireTimer, summonTimer, contactCd, flashTimer, repositionTimer;
    private float lockTimer, fadeCd;
    private bool fading;
    private int lastPhase = 1;

    void Start()
    {
        hp = GetComponent<BossHealth>();
        rb = GetComponent<Rigidbody2D>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr != null ? sr.color : Color.white;
        orbitAngle = Random.Range(0f, 360f);
        fireTimer = 1.5f;
        summonTimer = 4f;
        if (hp != null) hp.onHealthChanged += (c, m) => flashTimer = 0.12f; // hit flash
    }

    int Phase()
    {
        float f = hp != null && hp.maxHealth > 0 ? (float)hp.Health / hp.maxHealth : 1f;
        return f > 0.66f ? 1 : f > 0.33f ? 2 : 3;
    }

    void Update()
    {
        if (player == null || hp == null) return;
        if (fading) return; // the PhantomFade coroutine drives it while vanished
        int phase = Phase();
        if (phase != lastPhase) lastPhase = phase; // (hook for phase-change FX later)

        // Phantom Fade: keep it on your broadside (horizontally aligned) too long
        // and the ghost ship dissolves into mist and re-forms elsewhere.
        if (fadeCd > 0f) fadeCd -= Time.deltaTime;
        if (phase < 3 && fadeCd <= 0f && Mathf.Abs(player.position.y - transform.position.y) < 2f)
            lockTimer += Time.deltaTime;
        else
            lockTimer = Mathf.Max(0f, lockTimer - Time.deltaTime);
        if (lockTimer >= 3f) { lockTimer = 0f; StartCoroutine(PhantomFade()); return; }

        float speed = phase == 3 ? moveSpeed * 1.7f : moveSpeed;

        Vector2 cur = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 newPos;
        if (phase < 3)
        {
            // Hold a broadside bearing off the player (tracking them so it stays
            // a hittable target), and jump to a new bearing every few seconds.
            repositionTimer -= Time.deltaTime;
            if (repositionTimer <= 0f)
            {
                orbitAngle += Random.Range(80f, 150f);
                repositionTimer = phase == 2 ? 2.3f : 3.2f;
            }
            Vector2 off = new Vector2(Mathf.Cos(orbitAngle * Mathf.Deg2Rad), Mathf.Sin(orbitAngle * Mathf.Deg2Rad)) * orbitRadius;
            Vector2 target = (Vector2)player.position + off;
            newPos = Vector2.MoveTowards(cur, target, speed * Time.deltaTime);
        }
        else
        {
            // Doomsday: ram the player
            Vector2 dir = ((Vector2)player.position - cur).normalized;
            newPos = cur + dir * speed * Time.deltaTime;
        }
        // Move via the kinematic body so trigger collisions stay reliable
        if (rb != null) rb.MovePosition(newPos);
        else transform.position = newPos;

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

        // Hit flash (bright) over the ghostly alpha pulse
        if (sr != null)
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                sr.color = Color.white;
            }
            else
            {
                Color c = baseColor;
                c.a = baseColor.a * (0.78f + 0.18f * Mathf.Sin(Time.time * 3.2f));
                sr.color = c;
            }
        }

        if (contactCd > 0f) contactCd -= Time.deltaTime;
    }

    /// <summary>Dissolve into mist (intangible), drift to a new bearing across
    /// the arena, and re-form — the ghost ship that can't be pinned down.</summary>
    IEnumerator PhantomFade()
    {
        fading = true;
        if (hp != null) hp.invulnerable = true;

        // dissolve
        float t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; SetAlpha(Mathf.Lerp(baseColor.a, 0.08f, t / 0.4f)); yield return null; }
        SetAlpha(0.08f);

        // reappear at a fresh bearing off the player
        float ang = Random.Range(0f, 360f);
        orbitAngle = ang;
        Vector2 off = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)) * (orbitRadius + 1.5f);
        transform.position = (Vector3)((Vector2)player.position + off);
        repositionTimer = 2.5f;

        yield return new WaitForSeconds(0.45f);

        // re-form
        t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; SetAlpha(Mathf.Lerp(0.08f, baseColor.a, t / 0.4f)); yield return null; }
        SetAlpha(baseColor.a);

        if (hp != null) hp.invulnerable = false;
        fading = false;
        fadeCd = 4f;
    }

    void SetAlpha(float a)
    {
        if (sr == null) return;
        Color c = baseColor; c.a = a; sr.color = c;
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
