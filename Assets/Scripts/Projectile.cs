using UnityEngine;

/// <summary>
/// Controls projectile behavior (cannon balls)
/// Moves in a direction and destroys itself after a certain time
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("How fast the projectile travels")]
    public float speed = 10f;

    [Tooltip("How long before the projectile destroys itself (in seconds)")]
    public float lifetime = 3f;

    [Tooltip("Damage dealt to enemies")]
    public int damage = 1;

    // Direction the projectile is moving
    private Vector2 direction;

    void Start()
    {
        // Destroy the projectile after lifetime expires
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the projectile in its direction
        transform.Translate(direction * speed * Time.deltaTime);
    }

    /// <summary>
    /// Sets the direction the projectile should travel
    /// Called by the cannon when firing
    /// </summary>
    /// <param name="newDirection">Direction vector (should be normalized)</param>
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    /// <summary>
    /// Detects collision with enemies
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit an enemy
        if (collision.CompareTag("Enemy"))
        {
            // Get the enemy component and damage it
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Destroy the projectile after hitting
            Destroy(gameObject);
        }
    }
}
