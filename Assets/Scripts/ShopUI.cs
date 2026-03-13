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

    private static readonly Color TAB_ACTIVE = new Color(0.15f, 0.15f, 0.15f, 0.95f);
    private static readonly Color TAB_INACTIVE = new Color(0.25f, 0.25f, 0.3f, 0.8f);
    private static readonly Color CARD_BG = new Color(0.18f, 0.18f, 0.22f, 0.95f);

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

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            currentTab = ShopManager.ShopCategory.Ships;
            RefreshUI();
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        PortZone port = FindFirstObjectByType<PortZone>();
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

        // Rebuild item cards
        ClearCards();
        List<ShopManager.ShopItem> items = ShopManager.Instance.GetItems(currentTab);
        foreach (var item in items)
            CreateItemCard(item);
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
        bg.color = CARD_BG;
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
            // Dark backing behind sprite to eliminate checkered transparency
            GameObject iconBg = new GameObject("IconBg");
            iconBg.transform.SetParent(card.transform, false);
            Image iconBgImg = iconBg.AddComponent<Image>();
            iconBgImg.color = new Color(0.1f, 0.1f, 0.14f, 1f);
            RectTransform iconBgRect = iconBg.GetComponent<RectTransform>();
            iconBgRect.anchorMin = new Vector2(0.08f, 0.58f);
            iconBgRect.anchorMax = new Vector2(0.92f, 0.95f);
            iconBgRect.offsetMin = Vector2.zero;
            iconBgRect.offsetMax = Vector2.zero;

            // The sprite itself
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
        nameText.color = Color.white;
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
        descText.color = new Color(0.65f, 0.65f, 0.7f);
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
            statusText.color = maxed ? Color.cyan : (canAfford ? Color.yellow : new Color(1f, 0.4f, 0.4f));
        }
        else if (owned)
        {
            statusText.text = "\u2713 Owned";
            statusText.color = new Color(0.4f, 0.9f, 0.5f);
        }
        else
        {
            statusText.text = $"{cost} Gold";
            statusText.color = canAfford ? Color.yellow : new Color(1f, 0.4f, 0.4f);
        }

        // --- Buy / Equipped button ---
        GameObject btnObj = new GameObject("BuyBtn");
        btnObj.transform.SetParent(card.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        Button btn = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.1f, 0.04f);
        btnRect.anchorMax = new Vector2(0.9f, 0.22f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        string btnLabel;
        if (maxed || owned)
        {
            btnImg.color = new Color(0.25f, 0.25f, 0.3f);
            btn.interactable = false;
            btnLabel = owned ? "\u2713 Equipped" : "Maxed";
        }
        else if (canAfford)
        {
            btnImg.color = new Color(0.18f, 0.55f, 0.25f);
            btn.interactable = true;
            btnLabel = "Buy";
        }
        else
        {
            btnImg.color = new Color(0.4f, 0.18f, 0.18f);
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
                RefreshUI();
        });
    }

    void Update()
    {
        if (shopPanel != null && shopPanel.activeSelf)
            RefreshUI();
    }
}
