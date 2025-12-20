using UnityEngine;

/// <summary>
/// Controls the player ship movement and health
/// Handles touch input to move the ship to tapped positions
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the ship moves toward the target position")]
    public float moveSpeed = 5f;

    [Header("Health Settings")]
    [Tooltip("Starting health of the player ship")]
    public int maxHealth = 10;

    // Current health (private, modified through TakeDamage)
    private int currentHealth;

    // Target position the ship is moving toward
    private Vector3 targetPosition;

    // Reference to the main camera (used for touch-to-world position conversion)
    private Camera mainCamera;

    // Flag to check if the ship is currently moving
    private bool isMoving = false;

    void Start()
    {
        // Initialize health to max at game start
        currentHealth = maxHealth;

        // Get reference to the main camera
        mainCamera = Camera.main;

        // Set initial target position to current position
        targetPosition = transform.position;

        // Notify GameManager about initial health
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        // Handle touch input for mobile
        HandleTouchInput();

        // Move the ship toward target position
        MoveTowardTarget();
    }

    /// <summary>
    /// Detects touch input and sets new target position
    /// </summary>
    void HandleTouchInput()
    {
        // Check for touch input (mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Only respond to the moment the finger touches the screen
            if (touch.phase == TouchPhase.Began)
            {
                SetTargetPosition(touch.position);
            }
        }
        // Also support mouse click for testing in Unity Editor
        else if (Input.GetMouseButtonDown(0))
        {
            SetTargetPosition(Input.mousePosition);
        }
    }

    /// <summary>
    /// Converts screen position to world position and sets it as target
    /// </summary>
    /// <param name="screenPosition">Position on screen (from touch or mouse)</param>
    void SetTargetPosition(Vector2 screenPosition)
    {
        // Convert screen position to world position
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        // Keep the same Z position as the player (2D game)
        worldPosition.z = transform.position.z;

        // Set the new target position
        targetPosition = worldPosition;
        isMoving = true;
    }

    /// <summary>
    /// Smoothly moves the ship toward the target position
    /// </summary>
    void MoveTowardTarget()
    {
        if (!isMoving) return;

        // Move toward target position
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Stop moving when we reach the target (within a small threshold)
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    /// <summary>
    /// Reduces player health when damaged
    /// </summary>
    /// <param name="damage">Amount of damage to take</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Make sure health doesn't go below zero
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // Update UI
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        }

        // Check if player died
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Called when player health reaches zero
    /// </summary>
    void Die()
    {
        Debug.Log("Player ship destroyed!");
        // For now, just disable the ship
        // Later you can add game over screen, restart, etc.
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    /// <summary>
    /// Returns current health (useful for UI or other systems)
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
