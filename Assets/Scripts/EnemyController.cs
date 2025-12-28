using UnityEngine;

/// <summary>
/// Controls individual enemy behavior
/// Enemies move toward the player and have different HP based on color
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("How fast the enemy moves")]
    public float moveSpeed = GameConstants.ENEMY_MOVE_SPEED;

    [Tooltip("Damage dealt to player on collision")]
    public int collisionDamage = GameConstants.ENEMY_COLLISION_DAMAGE;

    [Header("Health")]
    [Tooltip("Health points for this enemy")]
    public int maxHealth = 1;

    [Header("Loot")]
    [Tooltip("Chance to drop loot on death (0 to 1)")]
    public float lootDropChance = GameConstants.LOOT_DROP_CHANCE;

    [Tooltip("Loot prefabs to spawn")]
    public GameObject[] lootPrefabs;

    // Current health
    private int currentHealth;

    // Reference to the player (target to move toward)
    private Transform playerTransform;

    // Reference to the sprite renderer for color
    private SpriteRenderer spriteRenderer;

    // Reference to Rigidbody2D for physics-based movement
    private Rigidbody2D rb;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Find the player in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure player has 'Player' tag.");
        }

        // Get sprite renderer for flash effect
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Move toward player if player exists
        if (playerTransform != null)
        {
            MoveTowardPlayer();
        }
    }

    /// <summary>
    /// Moves the enemy toward the player's position
    /// </summary>
    void MoveTowardPlayer()
    {
        // Calculate direction to player
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // Move using Rigidbody2D for proper physics interaction
        if (rb != null)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Reduces enemy health when hit by projectile
    /// </summary>
    /// <param name="damage">Amount of damage to take</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Visual feedback: flash white briefly when hit
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashWhite());
        }

        // Die if health reaches zero
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Brief white flash effect when enemy is hit
    /// </summary>
    System.Collections.IEnumerator FlashWhite()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Called when enemy health reaches zero
    /// </summary>
    void Die()
    {
        // Notify GameManager about kill
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }

        // Try to spawn loot
        TrySpawnLoot();

        // Destroy this enemy
        Destroy(gameObject);
    }

    /// <summary>
    /// Randomly spawns loot at this enemy's position
    /// </summary>
    void TrySpawnLoot()
    {
        // Check if we should drop loot
        float roll = Random.value;
        Debug.Log($"Loot drop roll: {roll} (need <= {lootDropChance})");
        
        if (roll <= lootDropChance && lootPrefabs != null && lootPrefabs.Length > 0)
        {
            // Pick a random loot type (25% chance for each of the 4 types)
            int lootIndex = Random.Range(0, lootPrefabs.Length);
            GameObject lootPrefab = lootPrefabs[lootIndex];
            Debug.Log($"Selected loot type index: {lootIndex}/{lootPrefabs.Length}");

            // Spawn the loot at this enemy's position
            if (lootPrefab != null)
            {
                Vector3 spawnPos = transform.position;
                GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);
                Debug.Log($"Spawned {lootPrefab.name} at {spawnPos}");
            }
            else
            {
                Debug.LogWarning("Loot prefab is null!");
            }
        }
        else if (lootPrefabs == null || lootPrefabs.Length == 0)
        {
            Debug.LogWarning("No loot prefabs assigned to enemy!");
        }
    }

    /// <summary>
    /// Detects collision with player
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit the player
        if (collision.CompareTag("Player"))
        {
            // Damage the player
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(collisionDamage);
                Debug.Log("Enemy hit player!");
            }

            // Enemy dies after hitting player (like a kamikaze)
            Die();
        }
    }
}
