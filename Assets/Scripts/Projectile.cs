using UnityEngine;

/// <summary>
/// Controls projectile behavior (cannon balls)
/// Moves in a direction and destroys itself after a certain time
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("How fast the projectile travels")]
    public float speed = GameConstants.PROJECTILE_SPEED;

    [Tooltip("How long before the projectile destroys itself (in seconds)")]
    public float lifetime = GameConstants.PROJECTILE_LIFETIME;

    [Tooltip("Damage dealt to enemies")]
    public int damage = GameConstants.PROJECTILE_DAMAGE;

    // Direction the projectile is moving
    private Vector2 direction;

    void Start()
    {
        // Destroy the projectile after lifetime expires
        Destroy(gameObject, lifetime);
        
        Debug.Log($"Projectile spawned with direction {direction}");
    }

    void Update()
    {
        // Move the projectile in its direction using Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    /// <summary>
    /// Sets the direction the projectile should travel
    /// Called by the cannon when firing
    /// </summary>
    /// <param name="newDirection">Direction vector (should be normalized)</param>
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        Debug.Log($"Projectile direction set to: {direction}");
    }

    /// <summary>
    /// Detects collision with enemies
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Projectile triggered by: {collision.gameObject.name} with tag: {collision.tag}");
        
        // Check if we hit an enemy
        if (collision.CompareTag("Enemy"))
        {
            // Get the enemy component and damage it
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Projectile hit enemy!");
            }

            // Destroy the projectile after hitting
            Destroy(gameObject);
        }
    }
}
