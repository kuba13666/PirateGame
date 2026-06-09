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
        // Sync timer with all other cannons so they fire together
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
            // Reset timer for next shot, sync to exact interval
            fireTimer += fireRate;
            if (fireTimer < 0f) fireTimer = 0f;
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

        // Spawn projectile at cannon's world position
        Vector3 spawnPosition = transform.position;

        // Create the projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Cannon {gameObject.name} fired projectile at {spawnPosition}");

        // Set the projectile's direction
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDirection(fireDirection);
        }
        else
        {
            Debug.LogError("Projectile prefab missing Projectile script!");
        }

        SpawnMuzzleFlash();
    }

    private static Sprite muzzleSprite;
    private static bool muzzleLoaded;

    /// <summary>Brief fire burst at the barrel when the cannon shoots.</summary>
    void SpawnMuzzleFlash()
    {
        if (!muzzleLoaded) { muzzleSprite = Resources.Load<Sprite>("Flame"); muzzleLoaded = true; }
        if (muzzleSprite == null) return;

        Vector2 dir = fireDirection.sqrMagnitude > 0.0001f ? fireDirection.normalized : Vector2.right;
        GameObject go = new GameObject("MuzzleFlash");
        go.transform.position = transform.position + (Vector3)(dir * 0.15f);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // flame sprite points up by default
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang);

        SpriteRenderer fsr = go.AddComponent<SpriteRenderer>();
        fsr.sprite = muzzleSprite;
        fsr.color = new Color(1f, 0.95f, 0.7f); // bright muzzle tint
        fsr.sortingOrder = 7;
        go.AddComponent<MuzzleFlash>();
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
