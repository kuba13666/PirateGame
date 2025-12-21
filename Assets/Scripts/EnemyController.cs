using UnityEngine;

/// <summary>
/// Controls individual enemy behavior
/// Enemies move toward the player and have different HP based on color
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("How fast the enemy moves")]
    public float moveSpeed = 2f;

    [Tooltip("Damage dealt to player on collision")]
    public int collisionDamage = 1;

    [Header("Health & Color")]
    [Tooltip("Health points for this enemy")]
    public int maxHealth = 1;

    [Tooltip("Color of this enemy (determines HP)")]
    public Color enemyColor = Color.red;

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

        // Get sprite renderer and set color
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyColor;
        }

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

        // Destroy this enemy
        Destroy(gameObject);
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
