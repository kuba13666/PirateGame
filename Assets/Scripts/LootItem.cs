using UnityEngine;

/// <summary>
/// Represents a collectible loot item dropped by enemies
/// Different types: Gold, Wood, Canvas, Metal
/// </summary>
public class LootItem : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Type of loot this item represents")]
    public LootType lootType;

    [Tooltip("How fast the loot drifts")]
    public float driftSpeed = 0.5f;

    [Tooltip("How long before the loot disappears")]
    public float lifetime = GameConstants.LOOT_LIFETIME;

    // Timer for lifetime
    private float lifetimeTimer;

    void Start()
    {
        // Initialize lifetime timer
        lifetimeTimer = lifetime;
    }

    void Update()
    {
        // Count down lifetime
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when player collects this loot
    /// </summary>
    public void Collect()
    {
        // Add to player inventory via GameManager
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.AddLoot(lootType);
            Debug.Log($"Collected {lootType}!");
        }

        // Destroy the loot item
        Destroy(gameObject);
    }

    /// <summary>
    /// Detects collision with player for auto-collection
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }
}

/// <summary>
/// Types of loot that can drop
/// </summary>
public enum LootType
{
    Gold,   // Yellow
    Wood,   // Brown
    Canvas, // White/Beige
    Metal   // Gray
}
