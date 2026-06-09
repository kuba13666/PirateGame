using UnityEngine;

/// <summary>
/// Brings the ocean to life: spawns a large, gently drifting tiled water
/// surface behind everything (anchored to the world, not the camera), and
/// keeps a small number of big wave squiggles fading in and out across the
/// view. Drop this on a single GameObject in the scene.
/// Sprites are loaded from Resources: "Water" (seamless tile) and "Wave_0..N".
/// </summary>
public class WaterManager : MonoBehaviour
{
    [Header("Water Surface")]
    public Color waterTint = Color.white;
    public Vector2 scrollSpeed = new Vector2(0.10f, 0.06f);
    public int waterSortingOrder = -100;

    [Header("Waves")]
    [Tooltip("Average seconds between wave spawns")]
    public float waveInterval = 1.2f;
    [Tooltip("Soft cap on simultaneous waves in view")]
    public int maxWaves = 7;
    public int waveSortingOrder = -50;
    [Tooltip("Target wave width in world units")]
    public float waveMinSize = 3.0f;
    public float waveMaxSize = 5.0f;

    private Sprite waterSprite;
    private Sprite[] waveSprites;
    private Camera cam;
    private float waveTimer;
    private Transform waveParent;

    void Start()
    {
        cam = Camera.main;
        waterSprite = Resources.Load<Sprite>("Water");

        var found = new System.Collections.Generic.List<Sprite>();
        for (int i = 0; ; i++)
        {
            Sprite s = Resources.Load<Sprite>("Wave_" + i);
            if (s == null) break;
            found.Add(s);
        }
        waveSprites = found.ToArray();

        CreateWaterSurface();

        if (waveSprites.Length > 0)
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
        if (waveSprites == null || waveSprites.Length == 0 || cam == null) return;

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            if (waveParent == null || waveParent.childCount < maxWaves)
                SpawnWave();
            waveTimer = waveInterval * Random.Range(0.7f, 1.3f);
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
        go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-8f, 8f));
        if (waveParent != null) go.transform.SetParent(waveParent);

        Sprite sprite = waveSprites[Random.Range(0, waveSprites.Length)];
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 1f, 1f, 0f);
        sr.sortingOrder = waveSortingOrder;

        Wave w = go.AddComponent<Wave>();
        float spriteW = Mathf.Max(0.01f, sprite.bounds.size.x);
        float scale = Random.Range(waveMinSize, waveMaxSize) / spriteW;
        // Random horizontal flip for variety.
        float flip = Random.value < 0.5f ? -1f : 1f;
        go.transform.localScale = new Vector3(scale * flip, scale, 1f);
        w.life = Random.Range(5.5f, 8.5f);
        // Waves sit in the world; just a whisper of drift with the current.
        w.drift = scrollSpeed * 0.3f;
    }
}
