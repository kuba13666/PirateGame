using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A sweeping wall of fire — the Flying Dutchman's broadside salvo. A row of
/// flame sprites perpendicular to the firing line advances toward the player,
/// flickering and shedding embers, dealing damage to anything it rolls over.
/// Self-destructs after its life expires. Uses the "Flame" sprite.
/// </summary>
public class FireWall : MonoBehaviour
{
    public Vector2 dir = Vector2.right; // travel direction (toward the player)
    public float speed = 5.5f;
    public float life = 1.6f;
    public int damage = 2;
    public int segments = 9;
    public float halfWidth = 3.2f;      // half the perpendicular span of the wall

    private Transform player;
    private Sprite flameSprite;
    private float age, hitCd, emberCd;
    private readonly List<Transform> flames = new List<Transform>();
    private readonly List<Vector3> baseScale = new List<Vector3>();
    private readonly List<float> phase = new List<float>();

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        flameSprite = Resources.Load<Sprite>("Flame");
        float fh = flameSprite != null ? flameSprite.bounds.size.y : 1f;

        Vector2 d = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        Vector2 perp = new Vector2(-d.y, d.x);
        for (int i = 0; i < segments; i++)
        {
            float f = segments > 1 ? (i / (float)(segments - 1) - 0.5f) : 0f; // -0.5..0.5
            var go = new GameObject("FlameSeg");
            go.transform.parent = transform;
            go.transform.position = transform.position + (Vector3)(perp * f * halfWidth * 2f);
            var fsr = go.AddComponent<SpriteRenderer>();
            fsr.sprite = flameSprite;
            fsr.sortingOrder = 7;
            // warm fire with the occasional cursed green lick
            fsr.color = (i % 3 == 1) ? new Color(0.5f, 1f, 0.55f, 0.95f) : new Color(1f, 0.55f, 0.15f, 0.95f);
            float s = (1.6f + Random.value * 0.7f) / Mathf.Max(0.01f, fh);
            var sc = new Vector3(s, s, 1f);
            go.transform.localScale = sc;
            flames.Add(go.transform);
            baseScale.Add(sc);
            phase.Add(Random.value * 6.283f);
        }
    }

    void Update()
    {
        age += Time.deltaTime;
        if (hitCd > 0f) hitCd -= Time.deltaTime;

        Vector2 d = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        transform.position += (Vector3)(d * speed * Time.deltaTime);

        // flicker each segment + fade out over the last 0.4s
        float fade = age > life - 0.4f ? Mathf.InverseLerp(life, life - 0.4f, age) : 1f;
        for (int i = 0; i < flames.Count; i++)
        {
            float fl = 0.82f + 0.3f * Mathf.Sin(Time.time * 18f + phase[i]);
            flames[i].localScale = new Vector3(baseScale[i].x, baseScale[i].y * fl, 1f);
            var fsr = flames[i].GetComponent<SpriteRenderer>();
            if (fsr != null) { var c = fsr.color; c.a = 0.95f * fade; fsr.color = c; }
        }

        // shed embers off the leading edge
        emberCd -= Time.deltaTime;
        if (emberCd <= 0f) { SpawnEmbers(2); emberCd = 0.08f; }

        // damage the player if they're caught in the advancing band
        if (player != null && hitCd <= 0f)
        {
            Vector2 toP = (Vector2)player.position - (Vector2)transform.position;
            Vector2 perp = new Vector2(-d.y, d.x);
            float along = Mathf.Abs(Vector2.Dot(toP, d));
            float side = Mathf.Abs(Vector2.Dot(toP, perp));
            if (along < 0.8f && side < halfWidth + 0.35f)
            {
                var pc = player.GetComponent<PlayerController>();
                if (pc != null) { pc.TakeDamage(damage); hitCd = 0.6f; }
            }
        }

        if (age >= life) Destroy(gameObject);
    }

    void SpawnEmbers(int n)
    {
        if (flames.Count == 0) return;
        float fh = flameSprite != null ? flameSprite.bounds.size.y : 1f;
        for (int i = 0; i < n; i++)
        {
            var src = flames[Random.Range(0, flames.Count)];
            var go = new GameObject("Ember");
            go.transform.position = src.position;
            var esr = go.AddComponent<SpriteRenderer>();
            esr.sprite = flameSprite;
            esr.sortingOrder = 8;
            esr.color = Random.value < 0.3f ? new Color(0.6f, 1f, 0.6f, 0.9f) : new Color(1f, 0.7f, 0.25f, 0.9f);
            float s = (0.35f + Random.value * 0.4f) / Mathf.Max(0.01f, fh);
            go.transform.localScale = new Vector3(s, s, 1f);
            go.AddComponent<Ember>().velocity = (Random.insideUnitCircle.normalized + dir.normalized) * Random.Range(1.5f, 3.5f);
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
