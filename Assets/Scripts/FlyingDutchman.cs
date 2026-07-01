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
    [Tooltip("The sprite's hull direction (stern->bow) before any flip")]
    public Vector2 hullAxis = Vector2.up;

    private BossHealth hp;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private SpriteAnimator animator;
    private Color baseColor;
    private float orbitAngle, fireTimer, summonTimer, contactCd, flashTimer, repositionTimer;
    private float lockTimer, fadeCd, broadsideTimer, wispTimer;
    private bool fading, squallActive;
    private GameObject stormOverlay;
    private Vector2 arenaCenter;
    private int lastPhase = 1;

    void Start()
    {
        hp = GetComponent<BossHealth>();
        rb = GetComponent<Rigidbody2D>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<SpriteAnimator>();
        baseColor = sr != null ? sr.color : Color.white;
        orbitAngle = Random.Range(0f, 360f);
        fireTimer = 1.5f;
        summonTimer = 4f;
        broadsideTimer = 6f;
        wispTimer = 8f;
        arenaCenter = transform.position;
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

        // Ghost crew + ghost-lights + broadsides (phase 2+)
        if (phase >= 2)
        {
            summonTimer -= Time.deltaTime;
            if (summonTimer <= 0f) { SummonAdds(phase == 3 ? 3 : 2); summonTimer = 6.5f; }

            broadsideTimer -= Time.deltaTime;
            if (broadsideTimer <= 0f) { SpectralBroadside(); broadsideTimer = phase == 3 ? 3.8f : 6f; }

            wispTimer -= Time.deltaTime;
            if (wispTimer <= 0f) { SpawnWisps(phase == 3 ? 3 : 2); wispTimer = 8f; }
        }

        // Doomsday Squall begins once, in phase 3
        if (phase == 3 && !squallActive) StartSquall();

        // No rotation — like every other ship it moves on two axes with a fixed
        // heading, only mirroring horizontally to face the player (base art faces left).
        if (sr != null) sr.flipX = player.position.x > transform.position.x;

        // Normal solid colours; only Phantom Fade turns it spectral. Just a hit flash here.
        if (sr != null)
        {
            if (flashTimer > 0f) { flashTimer -= Time.deltaTime; sr.color = Color.white; }
            else sr.color = baseColor;
        }

        if (contactCd > 0f) contactCd -= Time.deltaTime;
    }

    /// <summary>Dissolve into mist (intangible), drift to a new bearing across
    /// the arena, and re-form — the ghost ship that can't be pinned down.</summary>
    // The cursed ship's only spectral moment: it dissolves into ghostly green
    // mist (intangible), drifts across the arena, and re-forms solid elsewhere.
    static readonly Color SpectralMist = new Color(0.45f, 1f, 0.78f, 0.12f);

    IEnumerator PhantomFade()
    {
        fading = true;
        if (hp != null) hp.invulnerable = true;

        // dissolve from solid into spectral mist
        float t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; if (sr != null) sr.color = Color.Lerp(baseColor, SpectralMist, t / 0.4f); yield return null; }
        if (sr != null) sr.color = SpectralMist;

        // reappear at a fresh bearing off the player
        float ang = Random.Range(0f, 360f);
        orbitAngle = ang;
        Vector2 off = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)) * (orbitRadius + 1.5f);
        transform.position = (Vector3)((Vector2)player.position + off);
        repositionTimer = 2.5f;

        yield return new WaitForSeconds(0.45f);

        // re-form back into the solid warship
        t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; if (sr != null) sr.color = Color.Lerp(SpectralMist, baseColor, t / 0.4f); yield return null; }
        if (sr != null) sr.color = baseColor;

        if (hp != null) hp.invulnerable = false;
        fading = false;
        fadeCd = 4f;
    }

    /// <summary>A full broadside — the cannons erupt (animation + a wall of muzzle
    /// fire and smoke along the flank) and loose a wide volley of cannonballs.</summary>
    void SpectralBroadside()
    {
        if (projectilePrefab == null || player == null) return;
        Vector2 toP = ((Vector2)player.position - (Vector2)transform.position).normalized;

        // The clip's baked muzzle flashes point down — mirror vertically if the player is above.
        if (sr != null) sr.flipY = toP.y > 0f;
        if (animator != null) animator.PlayOnce("Dutchman_fire_", 11, 14f); // the broadside clip

        // Cosmetic muzzle fire + gunsmoke, anchored along the flank facing the player.
        Vector2 hull = (sr != null && sr.flipX) ? new Vector2(-hullAxis.x, hullAxis.y).normalized : hullAxis.normalized;
        Vector2 flank = new Vector2(-hull.y, hull.x);            // perpendicular to the hull
        if (Vector2.Dot(flank, toP) < 0f) flank = -flank;        // the side the player is on
        var fxGo = new GameObject("BroadsideFire");
        fxGo.transform.position = transform.position + (Vector3)(flank * 1.4f);
        var fx = fxGo.AddComponent<BroadsideFire>();
        fx.dir = flank;   // smoke rolls out from the hull
        fx.axis = hull;   // the gun line runs along the hull
        fx.span = 4.5f;
        fx.puffs = 7;

        // The actual threat: a wide volley of normal cannonballs.
        const int shots = 7;
        for (int i = 0; i < shots; i++)
        {
            float spread = (i - (shots - 1) * 0.5f) * 11f * Mathf.Deg2Rad;
            Vector2 d = Rotate(toP, spread);
            var proj = Instantiate(projectilePrefab, transform.position + (Vector3)(d * 1.6f), Quaternion.identity);
            var ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null) ep.SetDirection(d);
        }
    }

    /// <summary>Cast luring ghost-lights that slowly home toward the player.</summary>
    void SpawnWisps(int n)
    {
        Sprite flame = Resources.Load<Sprite>("Flame");
        for (int i = 0; i < n; i++)
        {
            var go = new GameObject("GhostWisp");
            go.transform.position = (Vector3)((Vector2)transform.position + Random.insideUnitCircle * 2f);
            var wsr = go.AddComponent<SpriteRenderer>();
            wsr.sprite = flame;
            wsr.color = new Color(0.45f, 1f, 0.65f, 0.8f);
            wsr.sortingOrder = 6;
            if (flame != null) { float s = 0.9f / Mathf.Max(0.01f, flame.bounds.size.y); go.transform.localScale = new Vector3(s, s, 1f); }
            var wrb = go.AddComponent<Rigidbody2D>(); wrb.bodyType = RigidbodyType2D.Kinematic; wrb.gravityScale = 0f;
            var wcol = go.AddComponent<CircleCollider2D>(); wcol.isTrigger = true;
            wcol.radius = flame != null ? flame.bounds.size.y * 0.28f : 0.3f;
            var w = go.AddComponent<GhostWisp>(); w.target = player;
        }
    }

    /// <summary>Doomsday Squall — the storm that damned van der Decken: the
    /// arena darkens and the cursed ship drags the player toward it.</summary>
    void StartSquall()
    {
        squallActive = true;
        var go = new GameObject("DoomsdaySquall");
        go.transform.position = new Vector3(arenaCenter.x, arenaCenter.y, 0f);
        var ssr = go.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, Color.white); tex.Apply();
        ssr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        ssr.color = new Color(0.05f, 0.07f, 0.13f, 0f);
        ssr.sortingOrder = 8;
        go.transform.localScale = new Vector3(80f, 80f, 1f);
        stormOverlay = go;
        StartCoroutine(FadeStorm());
    }

    IEnumerator FadeStorm()
    {
        var ssr = stormOverlay != null ? stormOverlay.GetComponent<SpriteRenderer>() : null;
        float t = 0f;
        while (t < 1f && ssr != null) { t += Time.deltaTime; Color c = ssr.color; c.a = Mathf.Lerp(0f, 0.34f, t); ssr.color = c; yield return null; }
    }

    // Storm pull runs in LateUpdate so it lands after the player's own movement.
    void LateUpdate()
    {
        if (!squallActive || player == null) return;
        Vector2 d = (Vector2)transform.position - (Vector2)player.position;
        if (d.magnitude > 2.5f)
            player.transform.position += (Vector3)(d.normalized * 0.6f * Time.deltaTime);
    }

    void OnDestroy()
    {
        if (stormOverlay != null) Destroy(stormOverlay);
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
