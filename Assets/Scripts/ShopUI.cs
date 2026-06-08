using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Tabbed shop UI with bookmark tabs on the left side
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Transform contentArea;
    public TextMeshProUGUI goldDisplayText;
    public TextMeshProUGUI titleText;
    public Button closeButton;

    [Header("Tab Buttons")]
    public Button tabShips;
    public Button tabEnhancements;
    public Button tabCrew;

    private ShopManager.ShopCategory currentTab = ShopManager.ShopCategory.Ships;
    private readonly List<GameObject> currentCards = new List<GameObject>();

    private static readonly Color TAB_ACTIVE = new Color(1f, 0.95f, 0.78f, 1f);
    private static readonly Color TAB_INACTIVE = new Color(0.62f, 0.55f, 0.44f, 1f);
    private static readonly Color CARD_BG = new Color(0.18f, 0.18f, 0.22f, 0.95f);

    // Pirate wood/parchment skin (PixelLab art, loaded from Resources)
    private static readonly Color INK = new Color(0.20f, 0.12f, 0.04f);       // dark ink on parchment
    private static readonly Color INK_SOFT = new Color(0.34f, 0.24f, 0.12f);  // softer brown
    private static Sprite skBg, skCard, skButton, skSign;
    private static bool skinLoaded;

    void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);

            if (closeButton == null)
            {
                Transform closeTf = shopPanel.transform.Find("CloseButton");
                if (closeTf != null)
                    closeButton = closeTf.GetComponent<Button>();
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseShop);
            }
        }

        WireTabButton(tabShips, ShopManager.ShopCategory.Ships);
        WireTabButton(tabEnhancements, ShopManager.ShopCategory.Enhancements);
        WireTabButton(tabCrew, ShopManager.ShopCategory.Crew);
    }

    void WireTabButton(Button btn, ShopManager.ShopCategory cat)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SwitchTab(cat));
    }

    void SwitchTab(ShopManager.ShopCategory cat)
    {
        currentTab = cat;
        RefreshUI();
    }

    static void LoadSkin()
    {
        if (skinLoaded) return;
        skBg = Resources.Load<Sprite>("ShopBg");
        skCard = Resources.Load<Sprite>("ShopCard");
        skButton = Resources.Load<Sprite>("ShopButton");
        skSign = Resources.Load<Sprite>("ShopSign");
        skinLoaded = true;
    }

    void ApplySkin()
    {
        LoadSkin();
        if (shopPanel == null) return;

        // Window background -> weathered wood
        var pImg = shopPanel.GetComponent<Image>();
        if (pImg != null && skBg != null) { pImg.sprite = skBg; pImg.type = Image.Type.Sliced; pImg.color = Color.white; }

        // Left tab strip -> darker wood
        var strip = shopPanel.transform.Find("TabStrip");
        var stripImg = strip != null ? strip.GetComponent<Image>() : null;
        if (stripImg != null && skBg != null) { stripImg.sprite = skBg; stripImg.type = Image.Type.Sliced; stripImg.color = new Color(0.72f, 0.72f, 0.72f); }

        // Tab buttons -> wood button sprite (tinted by SetTabHighlight)
        foreach (var tb in new[] { tabShips, tabEnhancements, tabCrew })
        {
            if (tb == null) continue;
            var ti = tb.GetComponent<Image>();
            if (ti != null && skButton != null) { ti.sprite = skButton; ti.type = Image.Type.Sliced; }
        }

        // Hanging wooden sign behind the title
        if (titleText != null && skSign != null)
        {
            var header = titleText.transform.parent;
            if (header.Find("TitleSign") == null)
            {
                var signGo = new GameObject("TitleSign");
                signGo.transform.SetParent(header, false);
                var simg = signGo.AddComponent<Image>();
                simg.sprite = skSign; simg.type = Image.Type.Simple; simg.preserveAspect = true; simg.raycastTarget = false;
                var srt = signGo.GetComponent<RectTransform>();
                var trt = titleText.GetComponent<RectTransform>();
                srt.anchorMin = trt.anchorMin; srt.anchorMax = trt.anchorMax; srt.pivot = trt.pivot;
                srt.anchoredPosition = trt.anchoredPosition; srt.sizeDelta = trt.sizeDelta;
                srt.offsetMin -= new Vector2(24, 18); srt.offsetMax += new Vector2(24, 24);
                signGo.transform.SetSiblingIndex(titleText.transform.GetSiblingIndex());
            }
            titleText.color = new Color(0.96f, 0.91f, 0.78f);
            titleText.fontStyle = FontStyles.Bold;
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            ApplySkin();
            currentTab = ShopManager.ShopCategory.Ships;
            RefreshUI();
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        // Find the port the player is actually in (not just the first one)
        PortZone port = PortZone.GetActivePort();
        if (port != null)
            port.ForceExitPort();
    }

    void RefreshUI()
    {
        if (ShopManager.Instance == null || GameManager.Instance == null) return;

        // Gold
        if (goldDisplayText != null)
            goldDisplayText.text = $"Gold: {GameManager.Instance.gold}";

        // Title
        if (titleText != null)
            titleText.text = currentTab.ToString();

        // Tab highlight
        SetTabHighlight(tabShips, currentTab == ShopManager.ShopCategory.Ships);
        SetTabHighlight(tabEnhancements, currentTab == ShopManager.ShopCategory.Enhancements);
        SetTabHighlight(tabCrew, currentTab == ShopManager.ShopCategory.Crew);

        // Rebuild item cards only when needed
        RebuildCards();
    }

    void RebuildCards()
    {
        ClearCards();
        List<ShopManager.ShopItem> items = ShopManager.Instance.GetItems(currentTab);
        foreach (var item in items)
            CreateItemCard(item);
    }

    void UpdateGoldOnly()
    {
        if (GameManager.Instance != null && goldDisplayText != null)
            goldDisplayText.text = $"Gold: {GameManager.Instance.gold}";
    }

    void SetTabHighlight(Button btn, bool active)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null)
            img.color = active ? TAB_ACTIVE : TAB_INACTIVE;
    }

    void ClearCards()
    {
        foreach (var go in currentCards)
            Destroy(go);
        currentCards.Clear();
    }

    void CreateItemCard(ShopManager.ShopItem item)
    {
        if (contentArea == null) return;

        bool hasSprite = item.sprite != null;
        bool maxed = item.currentLevel >= item.maxLevel;
        bool owned = item.purchased && item.maxLevel <= 1;
        int cost = ShopManager.Instance.GetItemCost(item);
        bool canAfford = ShopManager.Instance.CanAfford(item, GameManager.Instance.gold);

        // --- Card container ---
        GameObject card = new GameObject(item.name);
        card.transform.SetParent(contentArea, false);
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 220);
        Image bg = card.AddComponent<Image>();
        LoadSkin();
        if (skCard != null) { bg.sprite = skCard; bg.type = Image.Type.Sliced; bg.color = Color.white; }
        else bg.color = CARD_BG;
        currentCards.Add(card);

        // --- Thin accent line at top (gold for owned, dark for others) ---
        GameObject accent = new GameObject("Accent");
        accent.transform.SetParent(card.transform, false);
        Image accentImg = accent.AddComponent<Image>();
        accentImg.color = owned ? new Color(0.85f, 0.65f, 0.13f) :
                          canAfford ? new Color(0.3f, 0.7f, 0.4f) :
                          new Color(0.35f, 0.35f, 0.4f);
        RectTransform accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0, 1f);
        accentRect.anchorMax = new Vector2(1, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0, 3);

        // --- Icon area (top portion) ---
        if (hasSprite)
        {
            // Ship sits directly on the parchment card
            GameObject imgObj = new GameObject("Icon");
            imgObj.transform.SetParent(card.transform, false);
            Image icon = imgObj.AddComponent<Image>();
            icon.sprite = item.sprite;
            icon.preserveAspect = true;
            icon.type = Image.Type.Simple;
            RectTransform imgRect = imgObj.GetComponent<RectTransform>();
            imgRect.anchorMin = new Vector2(0.12f, 0.60f);
            imgRect.anchorMax = new Vector2(0.88f, 0.93f);
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
        }
        else
        {
            // Placeholder icon area with first letter
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(card.transform, false);
            Image phImg = placeholder.AddComponent<Image>();
            phImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            RectTransform phRect = placeholder.GetComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0.25f, 0.60f);
            phRect.anchorMax = new Vector2(0.75f, 0.93f);
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;

            GameObject letterObj = new GameObject("Letter");
            letterObj.transform.SetParent(placeholder.transform, false);
            TextMeshProUGUI letterText = letterObj.AddComponent<TextMeshProUGUI>();
            letterText.text = item.name.Length > 0 ? item.name[0].ToString() : "?";
            letterText.fontSize = 32;
            letterText.color = new Color(0.5f, 0.5f, 0.6f);
            letterText.alignment = TextAlignmentOptions.Center;
            RectTransform letterRect = letterObj.GetComponent<RectTransform>();
            letterRect.anchorMin = Vector2.zero;
            letterRect.anchorMax = Vector2.one;
            letterRect.offsetMin = Vector2.zero;
            letterRect.offsetMax = Vector2.zero;
        }

        // --- Item name ---
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = item.name;
        nameText.fontSize = 15;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = INK;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.46f);
        nameRect.anchorMax = new Vector2(1, 0.58f);
        nameRect.offsetMin = new Vector2(4, 0);
        nameRect.offsetMax = new Vector2(-4, 0);

        // --- Description ---
        GameObject descObj = new GameObject("Desc");
        descObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = item.description;
        descText.fontSize = 10;
        descText.color = INK_SOFT;
        descText.alignment = TextAlignmentOptions.Center;
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.36f);
        descRect.anchorMax = new Vector2(1, 0.48f);
        descRect.offsetMin = new Vector2(4, 0);
        descRect.offsetMax = new Vector2(-4, 0);

        // --- Status line (level or owned) ---
        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 11;
        statusText.alignment = TextAlignmentOptions.Center;
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.26f);
        statusRect.anchorMax = new Vector2(1, 0.36f);
        statusRect.offsetMin = new Vector2(4, 0);
        statusRect.offsetMax = new Vector2(-4, 0);

        if (item.maxLevel > 1)
        {
            statusText.text = maxed ? "MAX LEVEL" : $"Lv {item.currentLevel}/{item.maxLevel}  \u2022  {cost} Gold";
            statusText.color = maxed ? new Color(0.1f, 0.4f, 0.45f) : (canAfford ? new Color(0.45f, 0.3f, 0.05f) : new Color(0.6f, 0.12f, 0.1f));
        }
        else if (owned)
        {
            statusText.text = "\u2713 Owned";
            statusText.color = new Color(0.13f, 0.45f, 0.18f);
        }
        else
        {
            statusText.text = $"{cost} Gold";
            statusText.color = canAfford ? new Color(0.45f, 0.3f, 0.05f) : new Color(0.6f, 0.12f, 0.1f);
        }

        // --- Buy / Equipped button ---
        GameObject btnObj = new GameObject("BuyBtn");
        btnObj.transform.SetParent(card.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        if (skButton != null) { btnImg.sprite = skButton; btnImg.type = Image.Type.Sliced; }
        Button btn = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.1f, 0.04f);
        btnRect.anchorMax = new Vector2(0.9f, 0.22f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        string btnLabel;
        if (maxed || owned)
        {
            btnImg.color = new Color(0.62f, 0.60f, 0.54f);
            btn.interactable = false;
            btnLabel = owned ? "\u2713 Equipped" : "Maxed";
        }
        else if (canAfford)
        {
            btnImg.color = new Color(0.5f, 0.78f, 0.45f);
            btn.interactable = true;
            btnLabel = "Buy";
        }
        else
        {
            btnImg.color = new Color(0.78f, 0.45f, 0.4f);
            btn.interactable = false;
            btnLabel = "Buy";
        }

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = btnLabel;
        btnText.fontSize = 14;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        // Click handler
        ShopManager.ShopItem captured = item;
        btn.onClick.AddListener(() =>
        {
            if (ShopManager.Instance.Purchase(captured))
                RebuildCards();
        });
    }

    void Update()
    {
        if (shopPanel != null && shopPanel.activeSelf)
            UpdateGoldOnly();
    }
}
