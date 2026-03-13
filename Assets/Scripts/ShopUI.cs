using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for the shop that displays upgrade options
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public GameObject upgradeButtonPrefab;
    public Transform upgradeGridContainer;
    public TextMeshProUGUI goldDisplayText;
    public Button closeButton;

    private Button[] upgradeButtons;
    private TextMeshProUGUI[] upgradeNameTexts;
    private TextMeshProUGUI[] upgradeCostTexts;
    private TextMeshProUGUI[] upgradeLevelTexts;

    void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);

            // Wire close button at runtime if not assigned
            if (closeButton == null)
            {
                Transform closeTf = shopPanel.transform.Find("CloseButton");
                if (closeTf != null)
                {
                    closeButton = closeTf.GetComponent<Button>();
                }
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseShop);
            }
        }

        CreateUpgradeButtons();
    }

    void CreateUpgradeButtons()
    {
        if (ShopManager.Instance == null || upgradeGridContainer == null) return;

        int upgradeCount = ShopManager.Instance.upgrades.Length;
        upgradeButtons = new Button[upgradeCount];
        upgradeNameTexts = new TextMeshProUGUI[upgradeCount];
        upgradeCostTexts = new TextMeshProUGUI[upgradeCount];
        upgradeLevelTexts = new TextMeshProUGUI[upgradeCount];

        for (int i = 0; i < upgradeCount; i++)
        {
            int index = i; // Capture for lambda
            GameObject buttonObj = CreateUpgradeButton(index);
            
            if (buttonObj != null)
            {
                buttonObj.transform.SetParent(upgradeGridContainer, false);
            }
        }
    }

    GameObject CreateUpgradeButton(int upgradeIndex)
    {
        // Create upgrade button container
        GameObject container = new GameObject($"Upgrade_{upgradeIndex}");
        RectTransform rectTransform = container.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 120);

        // Add background
        Image bg = container.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // Create upgrade name text
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = ShopManager.Instance.upgrades[upgradeIndex].name;
        nameText.fontSize = 16;
        nameText.alignment = TextAlignmentOptions.Center;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.7f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, -5);
        upgradeNameTexts[upgradeIndex] = nameText;

        // Create level text
        GameObject levelObj = new GameObject("Level");
        levelObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI levelText = levelObj.AddComponent<TextMeshProUGUI>();
        levelText.text = "Level: 0/5";
        levelText.fontSize = 12;
        levelText.alignment = TextAlignmentOptions.Center;
        RectTransform levelRect = levelObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 0.5f);
        levelRect.anchorMax = new Vector2(1, 0.7f);
        levelRect.offsetMin = new Vector2(5, 0);
        levelRect.offsetMax = new Vector2(-5, 0);
        upgradeLevelTexts[upgradeIndex] = levelText;

        // Create cost text
        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI costText = costObj.AddComponent<TextMeshProUGUI>();
        costText.text = "Cost: 50 Gold";
        costText.fontSize = 12;
        costText.alignment = TextAlignmentOptions.Center;
        costText.color = Color.yellow;
        RectTransform costRect = costObj.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0, 0.3f);
        costRect.anchorMax = new Vector2(1, 0.5f);
        costRect.offsetMin = new Vector2(5, 0);
        costRect.offsetMax = new Vector2(-5, 0);
        upgradeCostTexts[upgradeIndex] = costText;

        // Create purchase button
        GameObject buttonObj = new GameObject("PurchaseButton");
        buttonObj.transform.SetParent(container.transform, false);
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(0.2f, 0.6f, 0.2f);
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.1f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.9f, 0.3f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        // Button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Purchase";
        buttonText.fontSize = 14;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        // Add click handler
        int index = upgradeIndex;
        button.onClick.AddListener(() => OnUpgradeButtonClicked(index));
        upgradeButtons[upgradeIndex] = button;

        return container;
    }

    void OnUpgradeButtonClicked(int upgradeIndex)
    {
        if (ShopManager.Instance.PurchaseUpgrade(upgradeIndex))
        {
            RefreshUI();
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            RefreshUI();
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Tell PortZone to exit (resume time, restart waves)
        PortZone port = FindFirstObjectByType<PortZone>();
        if (port != null)
        {
            port.ForceExitPort();
        }
    }

    void RefreshUI()
    {
        if (ShopManager.Instance == null || GameManager.Instance == null) return;

        // Update gold display
        if (goldDisplayText != null)
        {
            goldDisplayText.text = $"Gold: {GameManager.Instance.gold}";
        }

        // Update each upgrade button
        for (int i = 0; i < ShopManager.Instance.upgrades.Length; i++)
        {
            var upgrade = ShopManager.Instance.upgrades[i];
            int cost = ShopManager.Instance.GetUpgradeCost(i);
            bool canAfford = ShopManager.Instance.CanAffordUpgrade(i, GameManager.Instance.gold);
            bool maxLevel = upgrade.currentLevel >= upgrade.maxLevel;

            if (upgradeLevelTexts[i] != null)
            {
                upgradeLevelTexts[i].text = $"Level: {upgrade.currentLevel}/{upgrade.maxLevel}";
            }

            if (upgradeCostTexts[i] != null)
            {
                if (maxLevel)
                {
                    upgradeCostTexts[i].text = "MAX LEVEL";
                    upgradeCostTexts[i].color = Color.cyan;
                }
                else
                {
                    upgradeCostTexts[i].text = $"Cost: {cost} Gold";
                    upgradeCostTexts[i].color = canAfford ? Color.yellow : Color.red;
                }
            }

            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].interactable = canAfford && !maxLevel;
            }
        }
    }

    void Update()
    {
        // Refresh UI when shop is open to reflect gold changes
        if (shopPanel != null && shopPanel.activeSelf)
        {
            RefreshUI();
        }
    }
}
