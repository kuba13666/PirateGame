using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cosmetic broadside firing effect: a wall of muzzle fire and gunsmoke that
/// flares along the ship's flank when it looses a volley, then fades. Purely
/// visual — no damage and no movement. The actual threat is the cannonballs.
/// Uses the "Flame" and "Smoke" sprites.
/// </summary>
public class BroadsideFire : MonoBehaviour
{
    public Vector2 dir = Vector2.right; // firing direction (toward the player)
    public float span = 5f;             // length of the broadside line
    public int puffs = 7;

    private const float Life = 0.85f;
    private float age, emberCd;
    private Sprite flameSprite, smokeSprite;

    private readonly List<Transform> fires = new List<Transform>();
    private readonly List<Vector3> fireBase = new List<Vector3>();
    private readonly List<float> firePhase = new List<float>();
    private readonly List<Transform> smokes = new List<Transform>();
    private readonly List<Vector3> smokeBase = new List<Vector3>();
    private Vector2 driftDir;

    void Start()
    {
        flameSprite = Resources.Load<Sprite>("Flame");
        smokeSprite = Resources.Load<Sprite>("Smoke");
        float fh = flameSprite != null ? flameSprite.bounds.size.y : 1f;
        float sh = smokeSprite != null ? smokeSprite.bounds.size.y : 1f;

        Vector2 d = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        driftDir = d;
        Vector2 perp = new Vector2(-d.y, d.x);

        for (int i = 0; i < puffs; i++)
        {
            float f = puffs > 1 ? (i / (float)(puffs - 1) - 0.5f) : 0f; // -0.5..0.5
            Vector3 along = (Vector3)(perp * f * span);

            // muzzle flash, just off the hull toward the player
            var fp = MakeSprite(flameSprite, transform.position + along + (Vector3)(d * 0.3f),
                                new Color(1f, 0.62f, 0.2f, 1f), 8, (1.5f + Random.value * 0.7f) / Mathf.Max(0.01f, fh));
            fires.Add(fp); fireBase.Add(fp.localScale); firePhase.Add(Random.value * 6.283f);

            // gunsmoke billow behind the flash
            var sp = MakeSprite(smokeSprite, transform.position + along + (Vector3)(d * 0.55f),
                                new Color(0.82f, 0.82f, 0.82f, 0.65f), 6, (1.7f + Random.value * 0.5f) / Mathf.Max(0.01f, sh));
            smokes.Add(sp); smokeBase.Add(sp.localScale);
        }
    }

    Transform MakeSprite(Sprite s, Vector3 pos, Color col, int order, float scale)
    {
        var go = new GameObject("BroadsidePuff");
        go.transform.parent = transform;
        go.transform.position = pos;
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = s; r.color = col; r.sortingOrder = order;
        go.transform.localScale = new Vector3(scale, scale, 1f);
        return go.transform;
    }

    void Update()
    {
        age += Time.deltaTime;

        // Muzzle flash: bright instant flare, gone within ~0.3s, flickering.
        float fireFade = Mathf.Clamp01(1f - age / 0.3f);
        for (int i = 0; i < fires.Count; i++)
        {
            float fl = 0.85f + 0.3f * Mathf.Sin(Time.time * 22f + firePhase[i]);
            fires[i].localScale = new Vector3(fireBase[i].x, fireBase[i].y * fl, 1f);
            var r = fires[i].GetComponent<SpriteRenderer>();
            if (r != null) { var c = r.color; c.a = fireFade; r.color = c; }
        }

        // Gunsmoke: expands, drifts outward, fades over the full life.
        float k = age / Life;
        for (int i = 0; i < smokes.Count; i++)
        {
            smokes[i].localScale = smokeBase[i] * (1f + 0.9f * k);
            smokes[i].position += (Vector3)(driftDir * 1.2f * Time.deltaTime);
            var r = smokes[i].GetComponent<SpriteRenderer>();
            if (r != null) { var c = r.color; c.a = 0.65f * (1f - k); r.color = c; }
        }

        // A spray of embers off the gun line during the flash.
        emberCd -= Time.deltaTime;
        if (age < 0.3f && emberCd <= 0f) { SpawnEmbers(3); emberCd = 0.06f; }

        if (age >= Life) Destroy(gameObject);
    }

    void SpawnEmbers(int n)
    {
        if (fires.Count == 0) return;
        float fh = flameSprite != null ? flameSprite.bounds.size.y : 1f;
        for (int i = 0; i < n; i++)
        {
            var src = fires[Random.Range(0, fires.Count)];
            var go = new GameObject("Ember");
            go.transform.position = src.position;
            var esr = go.AddComponent<SpriteRenderer>();
            esr.sprite = flameSprite;
            esr.sortingOrder = 9;
            esr.color = new Color(1f, 0.72f, 0.28f, 0.9f);
            float s = (0.3f + Random.value * 0.35f) / Mathf.Max(0.01f, fh);
            go.transform.localScale = new Vector3(s, s, 1f);
            go.AddComponent<Ember>().velocity = (Random.insideUnitCircle.normalized + driftDir) * Random.Range(1.5f, 3.5f);
        }
    }
}

/// <summary>A tiny flickering spark that drifts, shrinks and fades, then dies.</summary>
public class Ember : MonoBehaviour
{
    public Vector2 velocity;
    private float age;
    private const float Life = 0.5f;
    private Vector3 startScale;

    void Start() { startScale = transform.localScale; }

    void Update()
    {
        age += Time.deltaTime;
        float k = age / Life;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 0.92f;
        transform.localScale = startScale * (1f - 0.7f * k);
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) { var c = sr.color; c.a = 0.9f * (1f - k); sr.color = c; }
        if (age >= Life) Destroy(gameObject);
    }
}
