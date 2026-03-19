using UnityEngine;

/// <summary>
/// Projectile fired by enemy ships — damages the player on contact.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    public float speed = GameConstants.ENEMY_SHIP_PROJECTILE_SPEED;
    public int damage = GameConstants.ENEMY_SHIP_PROJECTILE_DAMAGE;
    public float lifetime = GameConstants.PROJECTILE_LIFETIME;

    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
