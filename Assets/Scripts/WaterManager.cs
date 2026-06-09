using UnityEngine;

/// <summary>
/// Brings the ocean to life: spawns a large, gently scrolling tiled water
/// surface behind everything, and occasionally rolls a big, soft wave crest
/// across the visible area. Drop this on a single GameObject in the scene.
/// Sprites are loaded from Resources: "Water" (seamless tile) and "Wave".
/// </summary>
public class WaterManager : MonoBehaviour
{
    [Header("Water Surface")]
    public Color waterTint = Color.white;
    public Vector2 scrollSpeed = new Vector2(0.10f, 0.06f);
    public int waterSortingOrder = -100;

    [Header("Waves")]
    [Tooltip("Average seconds between wave crests")]
    public float waveInterval = 1.8f;
    public int waveSortingOrder = -50;
    [Tooltip("Target wave crest width in world units")]
    public float waveMinSize = 2.5f;
    public float waveMaxSize = 4.5f;

    private Sprite waterSprite, waveSprite;
    private Camera cam;
    private float waveTimer;
    private Transform waveParent;

    void Start()
    {
        cam = Camera.main;
        waterSprite = Resources.Load<Sprite>("Water");
        waveSprite = Resources.Load<Sprite>("Wave");

        CreateWaterSurface();

        if (waveSprite != null)
            waveParent = new GameObject("WaveContainer").transform;
    }

    void CreateWaterSurface()
    {
        if (waterSprite == null)
        {
            Debug.LogWarning("WaterManager: 'Water' sprite not found in Resources.");
            return;
        }

        GameObject go = new GameObject("WaterSurface");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = waterSprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.color = waterTint;
        sr.sortingOrder = waterSortingOrder;

        float tile = Mathf.Max(0.01f, waterSprite.bounds.size.x);
        float viewH = (cam != null ? cam.orthographicSize * 2f : 18f);
        float viewW = viewH * (cam != null ? cam.aspect : 1.78f);
        // Cover the view plus several tiles of slack for the scroll wrap + camera motion.
        sr.size = new Vector2(viewW + 6f * tile, viewH + 6f * tile);

        WaterScroll scroll = go.AddComponent<WaterScroll>();
        scroll.tileSize = tile;
        scroll.scrollSpeed = scrollSpeed;
    }

    void Update()
    {
        if (waveSprite == null || cam == null) return;

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            SpawnWave();
            waveTimer = waveInterval * Random.Range(0.6f, 1.4f);
        }
    }

    void SpawnWave()
    {
        float viewH = cam.orthographicSize * 2f;
        float viewW = viewH * cam.aspect;
        Vector3 c = cam.transform.position;
        Vector3 pos = new Vector3(
            c.x + Random.Range(-viewW * 0.5f, viewW * 0.5f),
            c.y + Random.Range(-viewH * 0.5f, viewH * 0.5f),
            0f);

        GameObject go = new GameObject("Wave");
        go.transform.position = pos;
        // Near-horizontal crest with a little tilt; it rolls (drifts) slowly.
        go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f));
        if (waveParent != null) go.transform.SetParent(waveParent);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = waveSprite;
        sr.color = new Color(1f, 1f, 1f, 0f);
        sr.sortingOrder = waveSortingOrder;

        Wave w = go.AddComponent<Wave>();
        float spriteW = Mathf.Max(0.01f, waveSprite.bounds.size.x);
        float scale = Random.Range(waveMinSize, waveMaxSize) / spriteW;
        go.transform.localScale = Vector3.one * scale;
        // Roll slowly, mostly downward, with a little sideways drift.
        w.drift = new Vector2(Random.Range(-0.04f, 0.04f), Random.Range(-0.08f, -0.03f));
    }
}
