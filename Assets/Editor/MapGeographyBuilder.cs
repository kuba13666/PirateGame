using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Builds the Biome 1 map geography (see Docs/BIOME1_DESIGN.md):
/// decorative islets and rock belts that shape sailing lanes, the Maelstrom
/// rock ring (with a gated gap that opens in Q5), and the three story POIs
/// (Dutchman's Drift, Gunsmith's Wreck, White Island).
/// Idempotent: re-running deletes and rebuilds the MapGeography root.
/// </summary>
public static class MapGeographyBuilder
{
    // sprite name, x, y, target world width, rotation degrees
    private struct Decor
    {
        public string sprite; public float x, y, size, rot;
        public Decor(string sprite, float x, float y, float size, float rot = 0f)
        { this.sprite = sprite; this.x = x; this.y = y; this.size = size; this.rot = rot; }
    }

    // ── Layout (world is 100x100, see design doc map sketch) ──

    static readonly Decor[] Islets =
    {
        // Trade Route channel (home → Trader's Cove)
        new Decor("Islet_0",  -6f,   9f, 2.0f),
        new Decor("Islet_1", -14f,  21f, 2.4f),
        new Decor("Islet_2", -20f,  11f, 1.8f),
        new Decor("Islet_0", -27f,  17f, 1.6f),
        // Home waters
        new Decor("Islet_1",  14f,  -4f, 1.8f),
        new Decor("Islet_2",  -3f, -12f, 2.2f),
        // Hunting Grounds (E)
        new Decor("Islet_0",  24f,   4f, 2.0f),
        new Decor("Islet_1",  33f,  20f, 2.5f),
        new Decor("Islet_2",  18f,  17f, 1.6f),
        // Navy Waters (SE)
        new Decor("Islet_0",  12f, -28f, 2.2f),
        new Decor("Islet_2",  35f, -33f, 1.7f),
        new Decor("Islet_1",  22f, -13f, 1.5f),
        // Open west / southwest
        new Decor("Islet_1", -35f,  -8f, 2.6f),
        new Decor("Islet_0", -22f, -28f, 1.9f),
        new Decor("Islet_2", -40f, -35f, 2.3f),
    };

    static readonly Decor[] Rocks =
    {
        // North rock belt (gap at x -2..6 aligns with the Maelstrom approach)
        new Decor("Rock_0", -44f, 31f, 1.2f,  15f),
        new Decor("Rock_1", -36f, 32f, 1.0f, 130f),
        new Decor("Rock_2", -28f, 30f, 1.3f, 250f),
        new Decor("Rock_0", -20f, 33f, 0.9f,  70f),
        new Decor("Rock_1", -13f, 31f, 1.1f, 310f),
        new Decor("Rock_2",  -6f, 32f, 1.0f, 180f),
        new Decor("Rock_0",   8f, 31f, 1.2f,  40f),
        new Decor("Rock_1",  16f, 33f, 0.9f, 220f),
        new Decor("Rock_2",  24f, 30f, 1.1f,  95f),
        new Decor("Rock_0",  32f, 32f, 1.3f, 160f),
        new Decor("Rock_1",  40f, 31f, 1.0f, 285f),
        new Decor("Rock_2",  46f, 33f, 0.9f,  10f),
        // Scattered hazards
        new Decor("Rock_0", -15f,   2f, 1.1f, 200f),
        new Decor("Rock_1",  28f,  -5f, 1.0f,  55f),
        new Decor("Rock_2", -30f, -20f, 1.2f, 140f),
        new Decor("Rock_0",  15f,  25f, 0.9f, 320f),
        new Decor("Rock_1", -44f,  12f, 1.1f,  80f),
        new Decor("Rock_2",  44f, -15f, 1.0f, 230f),
        new Decor("Rock_0",   8f, -38f, 1.2f, 110f),
        new Decor("Rock_1",  -8f, -30f, 0.9f, 350f),
    };

    // Maelstrom ring around (0, 42), radius 6.5, gap to the south.
    const float RING_CX = 0f, RING_CY = 42f, RING_R = 6.5f;
    static readonly float[] RingAngles = { 0f, 30f, 60f, 90f, 120f, 150f, 180f, 210f, 240f, 300f, 330f };
    static readonly float[] GateAngles = { 262f, 278f }; // close the gap until Q5

    [MenuItem("Pirate Game/Build Map Geography (Biome 1)")]
    public static void Build()
    {
        // Idempotent rebuild
        var old = GameObject.Find("MapGeography");
        if (old != null) Object.DestroyImmediate(old);

        var root = new GameObject("MapGeography");

        // ── Decorative islets & rocks ──
        var decorParent = new GameObject("Decor").transform;
        decorParent.SetParent(root.transform);
        int placed = 0;
        foreach (var d in Islets) { if (Place(d, decorParent, "Islet") != null) placed++; }
        foreach (var d in Rocks)  { if (Place(d, decorParent, "Rock") != null) placed++; }

        // ── Maelstrom ring ──
        var ringParent = new GameObject("MaelstromRing").transform;
        ringParent.SetParent(root.transform);
        for (int i = 0; i < RingAngles.Length; i++)
        {
            var d = RingDecor(RingAngles[i], i);
            if (Place(d, ringParent, "RingRock") != null) placed++;
        }
        // Gate rocks: separate parent so quest code can open the gap in Q5
        var gateParent = new GameObject("MaelstromGate").transform;
        gateParent.SetParent(ringParent);
        for (int i = 0; i < GateAngles.Length; i++)
        {
            var d = RingDecor(GateAngles[i], 100 + i);
            if (Place(d, gateParent, "GateRock") != null) placed++;
        }

        // ── Story POIs ──
        var poiParent = new GameObject("POIs").transform;
        poiParent.SetParent(root.transform);
        CreatePoi(poiParent, "dutchmans_drift", "Dutchman's Drift", new Vector2(-35f, 38f),
            "Fog", 4.0f, sortingOrder: 5, triggerScale: 1.0f); // fog renders above ships
        CreatePoi(poiParent, "gunsmith_wreck", "Gunsmith's Wreck", new Vector2(24f, 12f),
            "Wreck", 1.8f, sortingOrder: -10, triggerScale: 1.6f);
        CreatePoi(poiParent, "white_island", "The White Island", new Vector2(42f, -5f),
            "WhiteIsland", 2.6f, sortingOrder: -10, triggerScale: 1.3f);

        // ── New port island art for existing locations (if generated) ──
        ApplyLocationSprite("Loc_traders_cove", "Port_TradersCove", 4.2f);
        ApplyLocationSprite("Loc_naval_outpost", "Port_NavalOutpost", 4.2f);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"✓ Map geography built: {placed} decor objects + 3 POIs. Scene saved.");
    }

    static Decor RingDecor(float angleDeg, int i)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        string sprite = "Rock_" + (i % 3);
        return new Decor(sprite,
            RING_CX + RING_R * Mathf.Cos(rad),
            RING_CY + RING_R * Mathf.Sin(rad),
            1.4f, (i * 47f) % 360f);
    }

    static GameObject Place(Decor d, Transform parent, string prefix)
    {
        Sprite sprite = Resources.Load<Sprite>(d.sprite);
        if (sprite == null)
        {
            Debug.LogWarning($"MapGeographyBuilder: sprite '{d.sprite}' not found — skipped.");
            return null;
        }

        var go = new GameObject($"{prefix}_{d.x:F0}_{d.y:F0}");
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(d.x, d.y, 0f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, d.rot);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -10; // above water/waves, below ships

        float scale = d.size / Mathf.Max(0.01f, sprite.bounds.size.x);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        // Solid collider, slightly smaller than the art so beaches feel forgiving
        var col = go.AddComponent<BoxCollider2D>();
        col.size = sprite.bounds.size * 0.7f;
        col.offset = sprite.bounds.center;
        return go;
    }

    static void CreatePoi(Transform parent, string id, string displayName, Vector2 pos,
        string spriteName, float worldWidth, int sortingOrder, float triggerScale)
    {
        var go = new GameObject("Loc_" + id);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite sprite = Resources.Load<Sprite>(spriteName);
        float w = 1f;
        if (sprite != null)
        {
            sr.sprite = sprite;
            w = sprite.bounds.size.x;
            float scale = worldWidth / Mathf.Max(0.01f, w);
            go.transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            Debug.LogWarning($"MapGeographyBuilder: POI sprite '{spriteName}' missing for {id}.");
        }
        sr.sortingOrder = sortingOrder;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (sprite != null)
        {
            col.size = sprite.bounds.size * triggerScale;
            col.offset = sprite.bounds.center;
        }
        else
        {
            col.size = Vector2.one * 3f;
        }

        var loc = go.AddComponent<Location>();
        loc.locationId = id;
        loc.displayName = displayName;
        loc.locationType = Location.LocationType.Island;
        loc.hasShop = false;
        loc.discovered = false;
        loc.worldPosition = pos;

        var zone = go.AddComponent<PoiZone>();
        zone.location = loc;
    }

    static void ApplyLocationSprite(string objectName, string spriteName, float worldWidth)
    {
        var go = GameObject.Find(objectName);
        Sprite sprite = Resources.Load<Sprite>(spriteName);
        if (go == null || sprite == null) return;

        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        float scale = worldWidth / Mathf.Max(0.01f, sprite.bounds.size.x);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        var col = go.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = sprite.bounds.size;
            col.offset = sprite.bounds.center;
        }
        Debug.Log($"✓ {objectName} reskinned with {spriteName} ({worldWidth} wu)");
    }
}
