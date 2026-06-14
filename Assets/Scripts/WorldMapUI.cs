using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Full-screen world chart (your mate's hand-drawn map), opened with M. Shows a
/// "you are here" marker mapped from the player's world position. The chart
/// isn't to scale, so the marker mapping is calibratable: worldCenter maps to
/// chartAnchor (normalised 0..1 over the chart), scaled by uvPerWorldUnit.
/// </summary>
public class WorldMapUI : MonoBehaviour
{
    [Header("Marker calibration (tune so home sits under the marker)")]
    public Vector2 worldCenter = Vector2.zero;            // world point...
    public Vector2 chartAnchor = new Vector2(0.5f, 0.47f); // ...maps to this UV on the chart
    public Vector2 uvPerWorldUnit = new Vector2(0.00130f, 0.00150f);

    private GameObject panel;
    private RectTransform chartRect;
    private RectTransform marker;
    private Transform player;
    private bool open;
    private float savedTimeScale = 1f;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        BuildUI();
        SetOpen(false);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.mKey.wasPressedThisFrame) SetOpen(!open);
        else if (open && kb.escapeKey.wasPressedThisFrame) SetOpen(false);

        if (open && player != null && marker != null && chartRect != null)
        {
            Vector2 w = (Vector2)player.position - worldCenter;
            Vector2 uv = chartAnchor + new Vector2(w.x * uvPerWorldUnit.x, w.y * uvPerWorldUnit.y);
            uv.x = Mathf.Clamp01(uv.x); uv.y = Mathf.Clamp01(uv.y);
            Vector2 size = chartRect.rect.size;
            marker.anchoredPosition = new Vector2((uv.x - 0.5f) * size.x, (uv.y - 0.5f) * size.y);

            // gentle pulse so it's easy to spot
            float s = 1f + 0.25f * Mathf.Sin(Time.unscaledTime * 6f);
            marker.localScale = new Vector3(s, s, 1f);
        }
    }

    void SetOpen(bool value)
    {
        open = value;
        if (panel != null) panel.SetActive(value);
        if (value)
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            // Don't stomp a port/dialogue pause
            if (PortZone.GetActivePort() == null) Time.timeScale = savedTimeScale == 0f ? 1f : savedTimeScale;
        }
    }

    void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        panel = new GameObject("WorldMapPanel");
        panel.transform.SetParent(canvas.transform, false);
        var pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0.04f, 0.05f, 0.08f, 0.96f);
        var pRt = pImg.rectTransform;
        pRt.anchorMin = Vector2.zero; pRt.anchorMax = Vector2.one;
        pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;

        // Chart image, fit to screen preserving aspect
        var chartGo = new GameObject("Chart");
        chartGo.transform.SetParent(panel.transform, false);
        var chartImg = chartGo.AddComponent<Image>();
        chartImg.sprite = Resources.Load<Sprite>("WorldChart");
        chartImg.preserveAspect = true;
        chartImg.raycastTarget = false;
        chartRect = chartImg.rectTransform;
        chartRect.anchorMin = new Vector2(0.5f, 0.5f);
        chartRect.anchorMax = new Vector2(0.5f, 0.5f);
        chartRect.pivot = new Vector2(0.5f, 0.5f);
        Sprite cs = chartImg.sprite;
        float aspect = cs != null ? cs.rect.width / cs.rect.height : 1.83f;
        float h = 620f, w = h * aspect; // fits a 16:9 reference height comfortably
        chartRect.sizeDelta = new Vector2(w, h);

        // "You are here" marker (gold diamond)
        var mGo = new GameObject("Marker");
        mGo.transform.SetParent(chartRect, false);
        var mImg = mGo.AddComponent<Image>();
        mImg.color = new Color(1f, 0.85f, 0.2f);
        mImg.raycastTarget = false;
        marker = mImg.rectTransform;
        marker.sizeDelta = new Vector2(16, 16);
        marker.localRotation = Quaternion.Euler(0, 0, 45f); // diamond

        // Title hint
        var hintGo = new GameObject("Hint");
        hintGo.transform.SetParent(panel.transform, false);
        var hint = hintGo.AddComponent<TMPro.TextMeshProUGUI>();
        hint.text = "Compleat Chart of the Indies   —   M / Esc to close";
        hint.fontSize = 20;
        hint.alignment = TMPro.TextAlignmentOptions.Top;
        hint.color = new Color(0.9f, 0.85f, 0.7f);
        var hRt = hint.rectTransform;
        hRt.anchorMin = new Vector2(0.5f, 1f); hRt.anchorMax = new Vector2(0.5f, 1f);
        hRt.pivot = new Vector2(0.5f, 1f);
        hRt.anchoredPosition = new Vector2(0, -16);
        hRt.sizeDelta = new Vector2(800, 30);
    }
}
