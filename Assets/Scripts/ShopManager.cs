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

    // ─── Ship gameplay stats (top-down hull, cannons, HP, speed) ───
    public class ShipStats
    {
        public string hullSprite;   // Resources sprite name (top-down hull)
        public int cannonPairs;     // base cannon pairs (1 pair = 2 cannons)
        public int maxHealth;
        public float moveSpeed;
        public float worldHeight;   // target on-screen height in world units
    }

    public static readonly Dictionary<string, ShipStats> ShipStatsByName = new Dictionary<string, ShipStats>
    {
        { "Sloop",      new ShipStats { hullSprite = "Sloop_Top",      cannonPairs = 1, maxHealth = 10, moveSpeed = 2.0f, worldHeight = 1.2f } },
        { "Brigantine", new ShipStats { hullSprite = "Brigantine_Top", cannonPairs = 2, maxHealth = 14, moveSpeed = 2.4f, worldHeight = 1.55f } },
        { "Galleon",    new ShipStats { hullSprite = "Galleon_Top",    cannonPairs = 3, maxHealth = 20, moveSpeed = 1.7f, worldHeight = 1.9f } },
        { "Man O' War", new ShipStats { hullSprite = "ManOWar_Top",    cannonPairs = 4, maxHealth = 30, moveSpeed = 2.0f, worldHeight = 2.3f } },
    };

    [System.NonSerialized] public string equippedShipName = "Sloop";
    private int extraCannonPairs = 0;            // from the Extra Cannons upgrade
    private GameObject cannonProjectilePrefab;   // cached so cannons can be rebuilt

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

    void Start()
    {
        // Equip the starting ship so the player sails the proper hull from game start
        ApplyShipToPlayer(equippedShipName);
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
            spritePath = "Assets/Resources/Sloop.png",
            category = ShopCategory.Ships,
            sprite = Resources.Load<Sprite>("Sloop")
        });
        ships.Add(new ShopItem
        {
            name = "Brigantine",
            description = "Balanced speed and firepower",
            cost = 500,
            maxLevel = 1,
            spritePath = "Assets/Resources/Brigantine.png",
            category = ShopCategory.Ships,
            sprite = Resources.Load<Sprite>("Brigantine")
        });
        ships.Add(new ShopItem
        {
            name = "Galleon",
            description = "Heavy warship with many cannons",
            cost = 1500,
            maxLevel = 1,
            spritePath = "Assets/Resources/Galleon.png",
            category = ShopCategory.Ships,
            sprite = Resources.Load<Sprite>("Galleon")
        });
        ships.Add(new ShopItem
        {
            name = "Man O' War",
            description = "The mightiest ship on the seas",
            cost = 5000,
            maxLevel = 1,
            spritePath = "Assets/Resources/ManOWar.png",
            category = ShopCategory.Ships,
            sprite = Resources.Load<Sprite>("ManOWar")
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
                extraCannonPairs = item.currentLevel;
                RebuildEquippedCannons();
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

    // ─── Ship equipping ───────────────────────────────────────────────

    /// <summary>Equip an owned ship: swap the player's hull, stats and cannons.</summary>
    public void EquipShip(ShopItem ship)
    {
        if (ship == null || !ship.purchased) return;
        equippedShipName = ship.name;
        ApplyShipToPlayer(ship.name);
    }

    void RebuildEquippedCannons()
    {
        if (ShipStatsByName.TryGetValue(equippedShipName, out ShipStats s))
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null) RebuildCannons(player, s.cannonPairs + extraCannonPairs);
        }
    }

    void ApplyShipToPlayer(string shipName)
    {
        if (!ShipStatsByName.TryGetValue(shipName, out ShipStats stats)) return;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        // Hull sprite, scaled to a target world height (sprites differ in pixel size)
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        Sprite hull = Resources.Load<Sprite>(stats.hullSprite);
        if (sr != null && hull != null)
        {
            sr.sprite = hull;
            float spriteH = Mathf.Max(0.01f, hull.bounds.size.y);
            float scale = stats.worldHeight / spriteH;
            player.transform.localScale = new Vector3(scale, scale, 1f);

            BoxCollider2D col = player.GetComponent<BoxCollider2D>();
            if (col != null) col.size = hull.bounds.size;
        }

        // Stats
        player.maxHealth = stats.maxHealth;
        player.moveSpeed = stats.moveSpeed;
        player.Heal(stats.maxHealth);

        RebuildCannons(player, stats.cannonPairs + extraCannonPairs);

        // Damage-state sprites (mild at <=2/3 HP, heavy at <=1/3); fall back to healthy if missing
        Sprite mild = Resources.Load<Sprite>(stats.hullSprite + "_Mild");
        Sprite heavy = Resources.Load<Sprite>(stats.hullSprite + "_Heavy");
        player.SetHullSprites(hull, mild, heavy);
    }

    /// <summary>Destroy current cannons and lay out the given number of pairs along the hull sides.</summary>
    void RebuildCannons(PlayerController player, int pairs)
    {
        // Cache the projectile prefab from an existing cannon before destroying them
        CannonController existing = player.GetComponentInChildren<CannonController>();
        if (existing != null && existing.projectilePrefab != null)
            cannonProjectilePrefab = existing.projectilePrefab;
        if (cannonProjectilePrefab == null) return;

        foreach (CannonController cc in player.GetComponentsInChildren<CannonController>())
            DestroyImmediate(cc.gameObject);

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        float halfW = sr.sprite.bounds.extents.x;   // local (unscaled) hull half-extents
        float halfH = sr.sprite.bounds.extents.y;
        float sideX = halfW * 0.72f;

        // Size the cannon by its length (sprite barrel points up); player scale is uniform
        Sprite cannonSprite = Resources.Load<Sprite>("Cannon_Top");
        float cannonLen = cannonSprite != null ? Mathf.Max(cannonSprite.bounds.size.x, cannonSprite.bounds.size.y) : 1f;
        float targetLen = 0.26f; // cannon length in world units
        float ls = Mathf.Max(0.0001f, Mathf.Abs(player.transform.lossyScale.x));
        float cs = targetLen / (Mathf.Max(0.0001f, cannonLen) * ls);

        for (int i = 0; i < pairs; i++)
        {
            float t = pairs <= 1 ? 0.5f : (float)i / (pairs - 1);
            float y = Mathf.Lerp(halfH * 0.55f, -halfH * 0.55f, t);
            MakeCannon(player.transform, $"Cannon_L{i}", new Vector3(-sideX, y, 0f), new Vector2(-1, 0), cannonSprite, cs, true);
            MakeCannon(player.transform, $"Cannon_R{i}", new Vector3(sideX, y, 0f), new Vector2(1, 0), cannonSprite, cs, false);
        }
    }

    void MakeCannon(Transform parent, string name, Vector3 localPos, Vector2 fireDir, Sprite sprite, float cs, bool faceLeft)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(cs, cs, 1f);
        // sprite barrel points up; rotate so it points outward (left/right)
        go.transform.localRotation = Quaternion.Euler(0, 0, faceLeft ? 90f : -90f);

        if (sprite != null)
        {
            SpriteRenderer csr = go.AddComponent<SpriteRenderer>();
            csr.sprite = sprite;
            csr.sortingOrder = 4;
        }

        CannonController cc = go.AddComponent<CannonController>();
        cc.projectilePrefab = cannonProjectilePrefab;
        cc.fireDirection = fireDir;
        cc.fireRate = GameConstants.CANNON_FIRE_RATE;
        cc.spawnOffset = fireDir * GameConstants.CANNON_PROJECTILE_SPAWN_OFFSET;
    }
}
