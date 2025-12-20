using UnityEngine;

/// <summary>
/// Controls a single cannon that auto-fires projectiles
/// Attach this to each cannon object on the player ship
/// </summary>
public class CannonController : MonoBehaviour
{
    [Header("Cannon Settings")]
    [Tooltip("Prefab of the projectile to spawn")]
    public GameObject projectilePrefab;

    [Tooltip("Direction to fire (relative to cannon): Left = (-1,0), Right = (1,0)")]
    public Vector2 fireDirection = Vector2.right;

    [Tooltip("Time between shots (in seconds)")]
    public float fireRate = 1f;

    [Tooltip("Offset from cannon position where projectile spawns")]
    public Vector2 spawnOffset = Vector2.zero;

    // Timer to track when to fire next
    private float fireTimer = 0f;

    void Start()
    {
        // Start with timer at 0 so first shot fires immediately
        fireTimer = 0f;
    }

    void Update()
    {
        // Count down the timer
        fireTimer -= Time.deltaTime;

        // Fire when timer reaches zero
        if (fireTimer <= 0f)
        {
            Fire();
            // Reset timer for next shot
            fireTimer = fireRate;
        }
    }

    /// <summary>
    /// Fires a projectile from this cannon
    /// </summary>
    void Fire()
    {
        // Make sure we have a projectile prefab assigned
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned to cannon!");
            return;
        }

        // Calculate spawn position (cannon position + offset)
        Vector3 spawnPosition = transform.position + (Vector3)spawnOffset;

        // Create the projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Set the projectile's direction
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDirection(fireDirection);
        }
    }

    /// <summary>
    /// Visualize the fire direction in the Unity Editor
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw a line showing fire direction (helpful for setup in editor)
        Gizmos.color = Color.red;
        Vector3 start = transform.position + (Vector3)spawnOffset;
        Vector3 end = start + (Vector3)fireDirection * 1f;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.1f);
    }
}
