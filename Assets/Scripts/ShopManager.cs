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
            description = "Nimble vessel — 2 cannons",
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
            description = "+2 cannons per level (max 6)",
            cost = 100,
            maxLevel = 2,
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
            case "Extra Cannons":
                if (player != null) AddCannonsToPlayer(player, item.currentLevel);
                break;
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

    void AddCannonsToPlayer(PlayerController player, int cannonLevel)
    {
        // Each level adds 2 cannons (1 per side)
        // Level 1: top pair, Level 2: bottom pair, Level 3: far pair
        float[] yOffsets = { GameConstants.CANNON_OFFSET_Y, -GameConstants.CANNON_OFFSET_Y, GameConstants.CANNON_OFFSET_Y * 2f };
        float yOff = yOffsets[Mathf.Clamp(cannonLevel - 1, 0, yOffsets.Length - 1)];

        // Find projectile prefab from existing cannon
        CannonController existingCannon = player.GetComponentInChildren<CannonController>();
        if (existingCannon == null || existingCannon.projectilePrefab == null)
        {
            Debug.LogWarning("No existing cannon to copy projectile prefab from");
            return;
        }
        GameObject projectilePrefab = existingCannon.projectilePrefab;

        // Left cannon
        GameObject leftCannon = new GameObject($"Cannon_Left_{cannonLevel + 1}");
        leftCannon.transform.SetParent(player.transform, false);
        leftCannon.transform.localPosition = new Vector3(-GameConstants.CANNON_OFFSET_X, yOff, 0);
        CannonController leftCC = leftCannon.AddComponent<CannonController>();
        leftCC.projectilePrefab = projectilePrefab;
        leftCC.fireDirection = new Vector2(-1, 0);
        leftCC.fireRate = existingCannon.fireRate;
        leftCC.spawnOffset = new Vector2(-1, 0) * GameConstants.CANNON_PROJECTILE_SPAWN_OFFSET;

        // Right cannon
        GameObject rightCannon = new GameObject($"Cannon_Right_{cannonLevel + 1}");
        rightCannon.transform.SetParent(player.transform, false);
        rightCannon.transform.localPosition = new Vector3(GameConstants.CANNON_OFFSET_X, yOff, 0);
        CannonController rightCC = rightCannon.AddComponent<CannonController>();
        rightCC.projectilePrefab = projectilePrefab;
        rightCC.fireDirection = new Vector2(1, 0);
        rightCC.fireRate = existingCannon.fireRate;
        rightCC.spawnOffset = new Vector2(1, 0) * GameConstants.CANNON_PROJECTILE_SPAWN_OFFSET;

        // Sync all cannons to fire at the same time
        SyncAllCannons(player);

        Debug.Log($"Added cannon pair at y={yOff} (Level {cannonLevel})");
    }

    void SyncAllCannons(PlayerController player)
    {
        CannonController[] cannons = player.GetComponentsInChildren<CannonController>();
        foreach (var c in cannons)
        {
            // Reset all timers to 0 so next frame they all fire together
            var timerField = typeof(CannonController).GetField("fireTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (timerField != null)
                timerField.SetValue(c, 0f);
        }
    }
}
