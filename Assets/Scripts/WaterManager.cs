using UnityEngine;

/// <summary>
/// Brings the ocean to life: spawns a large, gently scrolling tiled water
/// surface behind everything, and periodically drops drifting foam whitecaps
/// across the visible area. Drop this on a single GameObject in the scene.
/// Sprites are loaded from Resources: "Water" (seamless tile) and "Foam".
/// </summary>
public class WaterManager : MonoBehaviour
{
    [Header("Water Surface")]
    public Color waterTint = Color.white;
    public Vector2 scrollSpeed = new Vector2(0.18f, 0.12f);
    public int waterSortingOrder = -100;

    [Header("Foam")]
    [Tooltip("Average seconds between foam spawns")]
    public float foamInterval = 0.65f;
    public int foamSortingOrder = -50;
    public float foamMinSize = 0.35f;
    public float foamMaxSize = 0.8f;

    private Sprite waterSprite, foamSprite;
    private Camera cam;
    private float foamTimer;
    private Transform foamParent;

    void Start()
    {
        cam = Camera.main;
        waterSprite = Resources.Load<Sprite>("Water");
        foamSprite = Resources.Load<Sprite>("Foam");

        CreateWaterSurface();

        if (foamSprite != null)
            foamParent = new GameObject("FoamContainer").transform;
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
        if (foamSprite == null || cam == null) return;

        foamTimer -= Time.deltaTime;
        if (foamTimer <= 0f)
        {
            SpawnFoam();
            foamTimer = foamInterval * Random.Range(0.6f, 1.4f);
        }
    }

    void SpawnFoam()
    {
        float viewH = cam.orthographicSize * 2f;
        float viewW = viewH * cam.aspect;
        Vector3 c = cam.transform.position;
        Vector3 pos = new Vector3(
            c.x + Random.Range(-viewW * 0.55f, viewW * 0.55f),
            c.y + Random.Range(-viewH * 0.55f, viewH * 0.55f),
            0f);

        GameObject go = new GameObject("Foam");
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        if (foamParent != null) go.transform.SetParent(foamParent);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = foamSprite;
        sr.color = new Color(1f, 1f, 1f, 0f);
        sr.sortingOrder = foamSortingOrder;

        Foam f = go.AddComponent<Foam>();
        float h = Mathf.Max(0.01f, foamSprite.bounds.size.y);
        f.baseScale = Random.Range(foamMinSize, foamMaxSize) / h;
        go.transform.localScale = Vector3.one * f.baseScale; // set now; Awake ran before baseScale was assigned
        // Drift roughly with the current, with a little variation.
        f.drift = scrollSpeed + new Vector2(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
    }
}
