using UnityEngine;

/// <summary>
/// A brief cannon muzzle flash: a bigger flash, then a smaller second flash,
/// then it fades out and self-destroys. Spawned in world space at the muzzle,
/// oriented along the fire direction.
/// </summary>
public class MuzzleFlash : MonoBehaviour
{
    public float life = 0.22f;
    public float baseScale = 0.4f;

    private float t;
    private SpriteRenderer sr;

    void Awake()
    {
        // Start small so there's no full-size pop on the first rendered frame
        transform.localScale = Vector3.one * (baseScale * 0.5f);
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / life;
        if (k >= 1f) { SpawnSmoke(); Destroy(gameObject); return; }

        // Two flashes: a bigger one, then a smaller second one near the end.
        float flash1 = Mathf.Exp(-Mathf.Pow((k - 0.13f) / 0.13f, 2f));          // peak 1.0
        float flash2 = 0.55f * Mathf.Exp(-Mathf.Pow((k - 0.58f) / 0.14f, 2f));  // smaller, later
        float e = Mathf.Max(flash1, flash2);

        transform.localScale = Vector3.one * (baseScale * (0.5f + e));
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(e * 1.2f);
            sr.color = c;
        }
    }

    private static Sprite smokeSprite;
    private static bool smokeLoaded;

    /// <summary>Leave a small smoke puff where the flash was.</summary>
    void SpawnSmoke()
    {
        if (!smokeLoaded) { smokeSprite = Resources.Load<Sprite>("Smoke"); smokeLoaded = true; }
        if (smokeSprite == null) return;
        GameObject go = new GameObject("Smoke");
        go.transform.position = transform.position;
        SpriteRenderer ssr = go.AddComponent<SpriteRenderer>();
        ssr.sprite = smokeSprite;
        ssr.color = new Color(0.78f, 0.78f, 0.78f, 0.55f);
        ssr.sortingOrder = 6;
        SmokePuff puff = go.AddComponent<SmokePuff>();
        float h = Mathf.Max(0.01f, smokeSprite.bounds.size.y);
        puff.baseScale = 0.32f / h; // ~0.32 world units
    }
}
