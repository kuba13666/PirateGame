using UnityEngine;

/// <summary>
/// Spawns enemies at random positions along the map edges
/// Different enemy types have different colors and HP
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab to use for spawning enemies")]
    public GameObject enemyPrefab;

    [Tooltip("Time between enemy spawns (in seconds)")]
    public float spawnInterval = 2f;

    [Tooltip("Minimum time between spawns")]
    public float minSpawnInterval = 0.5f;

    [Tooltip("How much to reduce spawn interval over time")]
    public float spawnIntervalDecreaseRate = 0.01f;

    [Header("Map Boundaries")]
    [Tooltip("Distance from player to spawn enemies")]
    public float spawnDistance = 4f;

    [Header("Enemy Types")]
    [Tooltip("Percentage chance for each enemy type (should add up to 100)")]
    public int redEnemyChance = 60;    // 1 HP
    public int blueEnemyChance = 30;   // 2 HP
    public int greenEnemyChance = 10;  // 3 HP

    // Timer for spawning
    private float spawnTimer = 0f;

    // Reference to player transform
    private Transform playerTransform;

    // Enemy type configurations
    private struct EnemyType
    {
        public Color color;
        public int health;
        public string name;
    }

    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Enemies will spawn at world origin.");
        }

        // Start spawning immediately
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        // Count down spawn timer
        spawnTimer -= Time.deltaTime;

        // Spawn enemy when timer reaches zero
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();

            // Reset timer and gradually decrease interval (difficulty increase)
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecreaseRate);
            spawnTimer = spawnInterval;
        }
    }

    /// <summary>
    /// Spawns an enemy at a random edge position with random type
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("No enemy prefab assigned to spawner!");
            return;
        }

        // Get random spawn position at map edge
        Vector3 spawnPosition = GetRandomEdgePosition();

        // Create the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Set enemy type (color and HP)
        EnemyType enemyType = GetRandomEnemyType();
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.maxHealth = enemyType.health;
            enemyController.enemyColor = enemyType.color;

            // Restart the enemy so it picks up the new values
            enemy.SetActive(false);
            enemy.SetActive(true);
        }
    }

    /// <summary>
    /// Returns a random position along the edges relative to player position
    /// </summary>
    Vector3 GetRandomEdgePosition()
    {
        // Get player position (or use origin if no player)
        Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;

        // Pick a random edge: 0=top, 1=bottom, 2=left, 3=right
        int edge = Random.Range(0, 4);

        Vector3 position = Vector3.zero;
        float randomOffset = Random.Range(-spawnDistance * 0.7f, spawnDistance * 0.7f);

        switch (edge)
        {
            case 0: // Top edge
                position = playerPos + new Vector3(randomOffset, spawnDistance, 0);
                break;
            case 1: // Bottom edge
                position = playerPos + new Vector3(randomOffset, -spawnDistance, 0);
                break;
            case 2: // Left edge
                position = playerPos + new Vector3(-spawnDistance, randomOffset, 0);
                break;
            case 3: // Right edge
                position = playerPos + new Vector3(spawnDistance, randomOffset, 0);
                break;
        }

        return position;
    }

    /// <summary>
    /// Randomly selects an enemy type based on spawn chances
    /// </summary>
    EnemyType GetRandomEnemyType()
    {
        // Generate random number from 0 to 100
        int roll = Random.Range(0, 100);

        EnemyType enemyType = new EnemyType();

        // Check which enemy type was rolled
        if (roll < redEnemyChance)
        {
            // Red enemy: 1 HP
            enemyType.color = Color.red;
            enemyType.health = 1;
            enemyType.name = "Red";
        }
        else if (roll < redEnemyChance + blueEnemyChance)
        {
            // Blue enemy: 2 HP
            enemyType.color = Color.blue;
            enemyType.health = 2;
            enemyType.name = "Blue";
        }
        else
        {
            // Green enemy: 3 HP
            enemyType.color = Color.green;
            enemyType.health = 3;
            enemyType.name = "Green";
        }

        return enemyType;
    }

    /// <summary>
    /// Visualize spawn area in Unity Editor (relative to player)
    /// </summary>
    void OnDrawGizmos()
    {
        // Get player position for visualization
        Vector3 center = Vector3.zero;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            center = player.transform.position;
        }

        // Draw spawn boundaries around player
        Gizmos.color = Color.yellow;

        // Top edge
        Gizmos.DrawLine(center + new Vector3(-spawnDistance, spawnDistance, 0), 
                       center + new Vector3(spawnDistance, spawnDistance, 0));
        // Bottom edge
        Gizmos.DrawLine(center + new Vector3(-spawnDistance, -spawnDistance, 0), 
                       center + new Vector3(spawnDistance, -spawnDistance, 0));
        // Left edge
        Gizmos.DrawLine(center + new Vector3(-spawnDistance, -spawnDistance, 0), 
                       center + new Vector3(-spawnDistance, spawnDistance, 0));
        // Right edge
        Gizmos.DrawLine(center + new Vector3(spawnDistance, -spawnDistance, 0), 
                       center + new Vector3(spawnDistance, spawnDistance, 0));
    }
}
