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
    [Tooltip("Left edge of the map")]
    public float mapMinX = -8f;

    [Tooltip("Right edge of the map")]
    public float mapMaxX = 8f;

    [Tooltip("Bottom edge of the map")]
    public float mapMinY = -4f;

    [Tooltip("Top edge of the map")]
    public float mapMaxY = 4f;

    [Header("Enemy Types")]
    [Tooltip("Percentage chance for each enemy type (should add up to 100)")]
    public int redEnemyChance = 60;    // 1 HP
    public int blueEnemyChance = 30;   // 2 HP
    public int greenEnemyChance = 10;  // 3 HP

    // Timer for spawning
    private float spawnTimer = 0f;

    // Enemy type configurations
    private struct EnemyType
    {
        public Color color;
        public int health;
        public string name;
    }

    void Start()
    {
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
    /// Returns a random position along the edges of the map
    /// </summary>
    Vector3 GetRandomEdgePosition()
    {
        // Pick a random edge: 0=top, 1=bottom, 2=left, 3=right
        int edge = Random.Range(0, 4);

        Vector3 position = Vector3.zero;

        switch (edge)
        {
            case 0: // Top edge
                position = new Vector3(Random.Range(mapMinX, mapMaxX), mapMaxY, 0);
                break;
            case 1: // Bottom edge
                position = new Vector3(Random.Range(mapMinX, mapMaxX), mapMinY, 0);
                break;
            case 2: // Left edge
                position = new Vector3(mapMinX, Random.Range(mapMinY, mapMaxY), 0);
                break;
            case 3: // Right edge
                position = new Vector3(mapMaxX, Random.Range(mapMinY, mapMaxY), 0);
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
    /// Visualize map boundaries in Unity Editor
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw map boundaries
        Gizmos.color = Color.yellow;

        // Top edge
        Gizmos.DrawLine(new Vector3(mapMinX, mapMaxY, 0), new Vector3(mapMaxX, mapMaxY, 0));
        // Bottom edge
        Gizmos.DrawLine(new Vector3(mapMinX, mapMinY, 0), new Vector3(mapMaxX, mapMinY, 0));
        // Left edge
        Gizmos.DrawLine(new Vector3(mapMinX, mapMinY, 0), new Vector3(mapMinX, mapMaxY, 0));
        // Right edge
        Gizmos.DrawLine(new Vector3(mapMaxX, mapMinY, 0), new Vector3(mapMaxX, mapMaxY, 0));
    }
}
