using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages shop items across 3 categories: Ships, Enhancements, Crew
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public enum ShopCategory
    {
        Ships,
        Enhancements,
        Crew
    }

    [System.Serializable]
    public class ShopItem
    {
        public string name;
        public string description;
        public int cost;
        public bool purchased = false;
        public int currentLevel = 0;
        public int maxLevel = 1;
        public string spritePath;
        public ShopCategory category;
        [System.NonSerialized] public Sprite sprite;
    }

    [Header("Shop Items")]
    public List<ShopItem> ships = new List<ShopItem>();
    public List<ShopItem> enhancements = new List<ShopItem>();
    public List<ShopItem> crew = new List<ShopItem>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeShop();
    }

    void InitializeShop()
    {
        // --- Ships ---
        ships.Add(new ShopItem
        {
            name = "Sloop",
            description = "Fast and nimble vessel",
            cost = 0,
            purchased = true,
            maxLevel = 1,
            spritePath = "Assets/GameAssets/Sloop.png",
            category = ShopCategory.Ships,
            sprite = Resources.Load<Sprite>("Sloop")
        });
        ships.Add(new ShopItem
        {
            name = "Brigantine",
            description = "Balanced speed and firepower",
            cost = 500,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Ships
        });
        ships.Add(new ShopItem
        {
            name = "Galleon",
            description = "Heavy warship with many cannons",
            cost = 1500,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Ships
        });
        ships.Add(new ShopItem
        {
            name = "Man O' War",
            description = "The mightiest ship on the seas",
            cost = 5000,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Ships
        });

        // --- Enhancements ---
        enhancements.Add(new ShopItem
        {
            name = "Extra Cannons",
            description = "Add more cannons to your ship",
            cost = 100,
            maxLevel = 3,
            spritePath = "",
            category = ShopCategory.Enhancements
        });
        enhancements.Add(new ShopItem
        {
            name = "Reinforced Hull",
            description = "Increase maximum health",
            cost = 50,
            maxLevel = 5,
            spritePath = "",
            category = ShopCategory.Enhancements
        });
        enhancements.Add(new ShopItem
        {
            name = "Better Sails",
            description = "Increase movement speed",
            cost = 40,
            maxLevel = 5,
            spritePath = "",
            category = ShopCategory.Enhancements
        });
        enhancements.Add(new ShopItem
        {
            name = "Fire Rate",
            description = "Increase cannon fire rate",
            cost = 60,
            maxLevel = 5,
            spritePath = "",
            category = ShopCategory.Enhancements
        });
        enhancements.Add(new ShopItem
        {
            name = "Cannon Damage",
            description = "Increase projectile damage",
            cost = 70,
            maxLevel = 5,
            spritePath = "",
            category = ShopCategory.Enhancements
        });
        enhancements.Add(new ShopItem
        {
            name = "Loot Magnet",
            description = "Increase gold from loot",
            cost = 80,
            maxLevel = 5,
            spritePath = "",
            category = ShopCategory.Enhancements
        });

        // --- Crew ---
        crew.Add(new ShopItem
        {
            name = "Helmsman",
            description = "Improves ship handling",
            cost = 200,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Crew
        });
        crew.Add(new ShopItem
        {
            name = "Gunner",
            description = "Increases cannon accuracy",
            cost = 300,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Crew
        });
        crew.Add(new ShopItem
        {
            name = "Surgeon",
            description = "Slowly regenerates health",
            cost = 400,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Crew
        });
        crew.Add(new ShopItem
        {
            name = "Quartermaster",
            description = "Increases loot quality",
            cost = 350,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Crew
        });
        crew.Add(new ShopItem
        {
            name = "Lookout",
            description = "Reveals enemies at range",
            cost = 250,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Crew
        });
        crew.Add(new ShopItem
        {
            name = "Cook",
            description = "Boosts morale and max HP",
            cost = 150,
            maxLevel = 1,
            spritePath = "",
            category = ShopCategory.Crew
        });
    }

    public List<ShopItem> GetItems(ShopCategory category)
    {
        switch (category)
        {
            case ShopCategory.Ships: return ships;
            case ShopCategory.Enhancements: return enhancements;
            case ShopCategory.Crew: return crew;
            default: return ships;
        }
    }

    public int GetItemCost(ShopItem item)
    {
        if (item.maxLevel > 1)
            return item.cost * (item.currentLevel + 1);
        return item.cost;
    }

    public bool CanAfford(ShopItem item, int gold)
    {
        if (item.purchased && item.maxLevel <= 1) return false;
        if (item.currentLevel >= item.maxLevel) return false;
        return gold >= GetItemCost(item);
    }

    public bool Purchase(ShopItem item)
    {
        if (GameManager.Instance == null) return false;
        int cost = GetItemCost(item);
        if (GameManager.Instance.gold < cost) return false;
        if (item.purchased && item.maxLevel <= 1) return false;
        if (item.currentLevel >= item.maxLevel) return false;

        GameManager.Instance.gold -= cost;
        item.currentLevel++;
        if (item.maxLevel <= 1) item.purchased = true;

        ApplyItem(item);
        Debug.Log($"Purchased {item.name} (Level {item.currentLevel})");
        return true;
    }

    void ApplyItem(ShopItem item)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        switch (item.name)
        {
            case "Reinforced Hull":
                if (player != null) { player.maxHealth += 20; player.Heal(20); }
                break;
            case "Better Sails":
                if (player != null) player.moveSpeed += 0.5f;
                break;
            case "Fire Rate":
                if (player != null)
                {
                    foreach (var c in player.GetComponentsInChildren<CannonController>())
                        c.fireRate *= 0.9f;
                }
                break;
            case "Cannon Damage":
                GameManager.Instance.damageMultiplier += 0.2f;
                break;
            case "Loot Magnet":
                GameManager.Instance.lootMultiplier += 0.25f;
                break;
            default:
                Debug.Log($"{item.name} effect not yet implemented");
                break;
        }
    }
}
