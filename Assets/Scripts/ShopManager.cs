using UnityEngine;

/// <summary>
/// Manages shop upgrades and their effects on the player
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [System.Serializable]
    public class Upgrade
    {
        public string name;
        public string description;
        public int baseCost;
        public int currentLevel = 0;
        public int maxLevel = 5;
        public UpgradeType type;
    }

    public enum UpgradeType
    {
        MaxHealth,
        MoveSpeed,
        FireRate,
        Damage,
        CannonCount,
        LootMultiplier
    }

    [Header("Upgrades")]
    public Upgrade[] upgrades = new Upgrade[6];

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

        InitializeUpgrades();
    }

    void InitializeUpgrades()
    {
        upgrades[0] = new Upgrade
        {
            name = "Max Health",
            description = "Increase ship's maximum health",
            baseCost = 50,
            type = UpgradeType.MaxHealth,
            maxLevel = 5
        };

        upgrades[1] = new Upgrade
        {
            name = "Move Speed",
            description = "Increase ship's movement speed",
            baseCost = 40,
            type = UpgradeType.MoveSpeed,
            maxLevel = 5
        };

        upgrades[2] = new Upgrade
        {
            name = "Fire Rate",
            description = "Increase cannon fire rate",
            baseCost = 60,
            type = UpgradeType.FireRate,
            maxLevel = 5
        };

        upgrades[3] = new Upgrade
        {
            name = "Cannon Damage",
            description = "Increase projectile damage",
            baseCost = 70,
            type = UpgradeType.Damage,
            maxLevel = 5
        };

        upgrades[4] = new Upgrade
        {
            name = "Extra Cannons",
            description = "Add more cannons to your ship",
            baseCost = 100,
            type = UpgradeType.CannonCount,
            maxLevel = 3
        };

        upgrades[5] = new Upgrade
        {
            name = "Loot Multiplier",
            description = "Increase gold from loot",
            baseCost = 80,
            type = UpgradeType.LootMultiplier,
            maxLevel = 5
        };
    }

    public int GetUpgradeCost(int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= upgrades.Length) return 0;
        
        Upgrade upgrade = upgrades[upgradeIndex];
        // Cost increases with each level: baseCost * (level + 1)
        return upgrade.baseCost * (upgrade.currentLevel + 1);
    }

    public bool CanAffordUpgrade(int upgradeIndex, int currentGold)
    {
        if (upgradeIndex < 0 || upgradeIndex >= upgrades.Length) return false;
        
        Upgrade upgrade = upgrades[upgradeIndex];
        if (upgrade.currentLevel >= upgrade.maxLevel) return false;
        
        return currentGold >= GetUpgradeCost(upgradeIndex);
    }

    public bool PurchaseUpgrade(int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= upgrades.Length) return false;
        if (GameManager.Instance == null) return false;

        Upgrade upgrade = upgrades[upgradeIndex];
        if (upgrade.currentLevel >= upgrade.maxLevel) return false;

        int cost = GetUpgradeCost(upgradeIndex);
        int currentGold = GameManager.Instance.gold;

        if (currentGold < cost) return false;

        // Deduct gold
        GameManager.Instance.gold -= cost;
        
        // Apply upgrade
        upgrade.currentLevel++;
        ApplyUpgrade(upgrade);

        Debug.Log($"Purchased {upgrade.name} (Level {upgrade.currentLevel})");
        return true;
    }

    void ApplyUpgrade(Upgrade upgrade)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        switch (upgrade.type)
        {
            case UpgradeType.MaxHealth:
                player.maxHealth += 20;
                player.Heal(20); // Also heal when upgrading
                break;

            case UpgradeType.MoveSpeed:
                player.moveSpeed += 0.5f;
                break;

            case UpgradeType.FireRate:
                CannonController[] cannons = player.GetComponentsInChildren<CannonController>();
                foreach (var cannon in cannons)
                {
                    cannon.fireRate *= 0.9f; // Reduce cooldown by 10%
                }
                break;

            case UpgradeType.Damage:
                // This would need to be stored and applied to projectiles when spawned
                GameManager.Instance.damageMultiplier += 0.2f;
                break;

            case UpgradeType.CannonCount:
                // Complex - would need to spawn new cannons
                Debug.Log("Extra cannons not yet implemented");
                break;

            case UpgradeType.LootMultiplier:
                GameManager.Instance.lootMultiplier += 0.25f;
                break;
        }
    }
}
