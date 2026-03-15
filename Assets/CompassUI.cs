using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Minimap panel (bottom-left) showing discovered locations as colored dots
/// plus edge-of-screen arrows pointing toward off-screen discovered locations.
/// </summary>
public class CompassUI : MonoBehaviour
{
    [Header("Minimap")]
    public RectTransform minimapPanel;
    public RectTransform playerDot;

    [Header("Edge Arrows")]
    public RectTransform arrowContainer; // full-screen overlay for edge arrows

    // Runtime pools
    private readonly List<RectTransform> locationDots = new List<RectTransform>();
    private readonly List<RectTransform> edgeArrows = new List<RectTransform>();
    private readonly List<TextMeshProUGUI> dotLabels = new List<TextMeshProUGUI>();

    private Transform playerTransform;
    private float minimapHalfSize;

    // World bounds (matches GameConstants)
    private const float WORLD_MIN = -50f;
    private const float WORLD_MAX = 50f;
    private const float WORLD_SIZE = WORLD_MAX - WORLD_MIN;

    // Edge arrow settings
    private const float ARROW_MARGIN = 40f; // pixels from screen edge

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (minimapPanel != null)
            minimapHalfSize = minimapPanel.rect.width * 0.5f;
    }

    void LateUpdate()
    {
        if (playerTransform == null || LocationManager.Instance == null) return;

        Vector2 playerPos = playerTransform.position;
        List<Location> discovered = LocationManager.Instance.GetDiscovered();

        UpdatePlayerDot(playerPos);
        UpdateLocationDots(discovered, playerPos);
        UpdateEdgeArrows(discovered, playerPos);
    }

    // ─── MINIMAP ────────────────────────────────

    void UpdatePlayerDot(Vector2 worldPos)
    {
        if (playerDot == null || minimapPanel == null) return;
        playerDot.anchoredPosition = WorldToMinimap(worldPos);
    }

    void UpdateLocationDots(List<Location> locations, Vector2 playerPos)
    {
        // Ensure we have enough dots
        while (locationDots.Count < locations.Count)
            SpawnMinimapDot();

        for (int i = 0; i < locationDots.Count; i++)
        {
            if (i < locations.Count)
            {
                locationDots[i].gameObject.SetActive(true);
                locationDots[i].anchoredPosition = WorldToMinimap(locations[i].worldPosition);

                // Color by type
                Image img = locationDots[i].GetComponent<Image>();
                if (img != null) img.color = GetLocationColor(locations[i]);

                // Label
                if (i < dotLabels.Count)
                {
                    dotLabels[i].gameObject.SetActive(true);
                    dotLabels[i].text = locations[i].displayName;
                }
            }
            else
            {
                locationDots[i].gameObject.SetActive(false);
                if (i < dotLabels.Count) dotLabels[i].gameObject.SetActive(false);
            }
        }
    }

    Vector2 WorldToMinimap(Vector2 worldPos)
    {
        // Map world [-50..50] → minimap [-halfSize..halfSize]
        float nx = (worldPos.x - WORLD_MIN) / WORLD_SIZE; // 0..1
        float ny = (worldPos.y - WORLD_MIN) / WORLD_SIZE;
        float mapSize = minimapHalfSize * 2f;
        return new Vector2(nx * mapSize - minimapHalfSize, ny * mapSize - minimapHalfSize);
    }

    void SpawnMinimapDot()
    {
        // Dot
        GameObject dot = new GameObject("LocDot_" + locationDots.Count);
        dot.transform.SetParent(minimapPanel, false);
        Image img = dot.AddComponent<Image>();
        img.color = Color.green;
        RectTransform rt = dot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(8, 8);
        locationDots.Add(rt);

        // Label below dot
        GameObject lbl = new GameObject("DotLabel_" + dotLabels.Count);
        lbl.transform.SetParent(dot.transform, false);
        TextMeshProUGUI tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 8;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Top;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        RectTransform lr = lbl.GetComponent<RectTransform>();
        lr.anchoredPosition = new Vector2(0, -8);
        lr.sizeDelta = new Vector2(60, 12);
        dotLabels.Add(tmp);
    }

    // ─── EDGE ARROWS ────────────────────────────

    void UpdateEdgeArrows(List<Location> locations, Vector2 playerWorldPos)
    {
        if (arrowContainer == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Ensure pool
        while (edgeArrows.Count < locations.Count)
            SpawnEdgeArrow();

        Rect screenRect = new Rect(ARROW_MARGIN, ARROW_MARGIN,
            Screen.width - ARROW_MARGIN * 2, Screen.height - ARROW_MARGIN * 2);

        for (int i = 0; i < edgeArrows.Count; i++)
        {
            if (i >= locations.Count)
            {
                edgeArrows[i].gameObject.SetActive(false);
                continue;
            }

            Vector3 screenPos = cam.WorldToScreenPoint(locations[i].worldPosition);
            bool onScreen = screenPos.z > 0 &&
                            screenPos.x > ARROW_MARGIN && screenPos.x < Screen.width - ARROW_MARGIN &&
                            screenPos.y > ARROW_MARGIN && screenPos.y < Screen.height - ARROW_MARGIN;

            if (onScreen)
            {
                edgeArrows[i].gameObject.SetActive(false);
                continue;
            }

            // Clamp to screen edge
            edgeArrows[i].gameObject.SetActive(true);

            Vector2 dir = ((Vector2)locations[i].worldPosition - playerWorldPos).normalized;
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            // Project direction onto screen edge
            Vector2 edgePos = ClampToScreenEdge(center, dir, screenRect);

            edgeArrows[i].position = edgePos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            edgeArrows[i].rotation = Quaternion.Euler(0, 0, angle);

            // Color
            Image img = edgeArrows[i].GetComponent<Image>();
            if (img != null) img.color = GetLocationColor(locations[i]);
        }
    }

    Vector2 ClampToScreenEdge(Vector2 center, Vector2 dir, Rect bounds)
    {
        // Ray from center in direction, find intersection with screen rect
        float tMin = float.MaxValue;

        // Right edge
        if (dir.x > 0.001f)
        {
            float t = (bounds.xMax - center.x) / dir.x;
            if (t > 0 && t < tMin) tMin = t;
        }
        // Left edge
        if (dir.x < -0.001f)
        {
            float t = (bounds.xMin - center.x) / dir.x;
            if (t > 0 && t < tMin) tMin = t;
        }
        // Top edge
        if (dir.y > 0.001f)
        {
            float t = (bounds.yMax - center.y) / dir.y;
            if (t > 0 && t < tMin) tMin = t;
        }
        // Bottom edge
        if (dir.y < -0.001f)
        {
            float t = (bounds.yMin - center.y) / dir.y;
            if (t > 0 && t < tMin) tMin = t;
        }

        if (tMin == float.MaxValue) tMin = 1f;
        return center + dir * tMin;
    }

    void SpawnEdgeArrow()
    {
        GameObject arrow = new GameObject("EdgeArrow_" + edgeArrows.Count);
        arrow.transform.SetParent(arrowContainer, false);
        Image img = arrow.AddComponent<Image>();
        // Triangle-ish using default sprite, rotated to point in direction
        img.sprite = CreateArrowSprite();
        img.color = Color.white;
        img.raycastTarget = false;
        RectTransform rt = arrow.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(24, 16);
        edgeArrows.Add(rt);
    }

    // ─── HELPERS ────────────────────────────────

    static Color GetLocationColor(Location loc)
    {
        switch (loc.locationType)
        {
            case Location.LocationType.Port:
                return loc.hasShop ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.4f, 0.7f, 1f);
            case Location.LocationType.Island:
                return new Color(1f, 0.85f, 0.3f);
            case Location.LocationType.BossArena:
                return new Color(1f, 0.3f, 0.3f);
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Creates a small right-pointing triangle texture at runtime for edge arrows.
    /// </summary>
    static Sprite CreateArrowSprite()
    {
        int w = 16, h = 12;
        Texture2D tex = new Texture2D(w, h);
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        // Simple right-pointing triangle
        for (int y = 0; y < h; y++)
        {
            float fy = Mathf.Abs(y - h * 0.5f) / (h * 0.5f); // 0 at center, 1 at edge
            int xMax = Mathf.RoundToInt((1f - fy) * w);
            for (int x = 0; x < xMax; x++)
                tex.SetPixel(x, y, white);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
}
