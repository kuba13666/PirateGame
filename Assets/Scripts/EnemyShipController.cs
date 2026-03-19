using UnityEngine;

/// <summary>
/// AI controller for enemy ships — circles the player and fires cannons.
/// Uses the same Ship.png sprite as the player, tinted dark.
/// </summary>
public class EnemyShipController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = GameConstants.ENEMY_SHIP_MOVE_SPEED;
    public float circleRadius = GameConstants.ENEMY_SHIP_CIRCLE_RADIUS;

    [Header("Health")]
    public int maxHealth = GameConstants.ENEMY_SHIP_HP;
    public int collisionDamage = GameConstants.ENEMY_SHIP_COLLISION_DAMAGE;

    [Header("Combat")]
    public float fireRate = GameConstants.ENEMY_SHIP_FIRE_RATE;
    public GameObject projectilePrefab;

    [Header("Loot")]
    public float lootDropChance = GameConstants.ENEMY_SHIP_LOOT_DROP_CHANCE;
    public GameObject[] lootPrefabs;

    private int currentHealth;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float fireTimer;
    private float circleAngle;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Randomize starting orbit angle so ships don't stack
        circleAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist > circleRadius * 1.5f)
        {
            // Approach phase — move toward player
            Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
        }
        else
        {
            // Circle phase — orbit around player
            circleAngle += moveSpeed * 30f * Time.deltaTime;
            Vector2 offset = new Vector2(
                Mathf.Cos(circleAngle * Mathf.Deg2Rad),
                Mathf.Sin(circleAngle * Mathf.Deg2Rad)
            ) * circleRadius;

            Vector2 target = (Vector2)playerTransform.position + offset;
            rb.MovePosition(Vector2.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime));
        }

        // Rotate to face player
        Vector2 toPlayer = (Vector2)playerTransform.position - rb.position;
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Clamp to map bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, GameConstants.MAP_MIN_X + 1f, GameConstants.MAP_MAX_X - 1f);
        pos.y = Mathf.Clamp(pos.y, GameConstants.MAP_MIN_Y + 1f, GameConstants.MAP_MAX_Y - 1f);
        transform.position = pos;

        // Fire at player
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f && dist < circleRadius * 2f)
        {
            FireAtPlayer(toPlayer.normalized);
            fireTimer = fireRate;
        }
    }

    void FireAtPlayer(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
            ep.SetDirection(direction);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (spriteRenderer != null)
            StartCoroutine(FlashHit());

        if (currentHealth <= 0)
            Die();
    }

    System.Collections.IEnumerator FlashHit()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    public int GetCurrentHealth() => currentHealth;

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddKill();

        if (QuestManager.Instance != null)
            QuestManager.Instance.ReportEnemyKilled();

        TrySpawnLoot();
        Destroy(gameObject);
    }

    void TrySpawnLoot()
    {
        if (Random.value <= lootDropChance && lootPrefabs != null && lootPrefabs.Length > 0)
        {
            int idx = Random.Range(0, lootPrefabs.Length);
            if (lootPrefabs[idx] != null)
                Instantiate(lootPrefabs[idx], transform.position, Quaternion.identity);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage(collisionDamage);

            Die();
        }
    }
}
