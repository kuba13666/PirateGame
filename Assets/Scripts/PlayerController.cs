using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Controls the player ship movement and health
/// Handles touch input to move the ship to tapped positions
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the ship moves toward the target position")]
    public float moveSpeed = GameConstants.PLAYER_MOVE_SPEED;

    [Header("Health Settings")]
    [Tooltip("Starting health of the player ship")]
    public int maxHealth = GameConstants.PLAYER_MAX_HEALTH;

    [Header("Map Boundaries")]
    [Tooltip("Minimum X boundary")]
    public float minX = GameConstants.MAP_MIN_X;
    
    [Tooltip("Maximum X boundary")]
    public float maxX = GameConstants.MAP_MAX_X;
    
    [Tooltip("Minimum Y boundary")]
    public float minY = GameConstants.MAP_MIN_Y;
    
    [Tooltip("Maximum Y boundary")]
    public float maxY = GameConstants.MAP_MAX_Y;

    // Current health (private, modified through TakeDamage)
    private int currentHealth;

    // Hull damage-state sprites (set on equip); swapped by HP thresholds
    private Sprite hullHealthy, hullMild, hullHeavy;

    // Animated fire on the damaged hull
    private readonly List<GameObject> fires = new List<GameObject>();
    private Sprite flameSprite;
    private int currentFireLevel = -1;

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

        // Cache original sprite color for damage flash
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalSpriteColor = sr.color;

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
        // Check for touch input (mobile) using new Input System
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                SetTargetPosition(touchPosition);
            }
        }
        // Also support mouse click for testing in Unity Editor
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            SetTargetPosition(mousePosition);
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

        // Clamp position within boundaries
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;

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

        UpdateDamageSprite();

        // Visual feedback: flash red (cancel previous flash first)
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashDamage());

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
    /// Heals the player
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateDamageSprite();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        }
    }

    // Cached original sprite color (set once in Start)
    private Color originalSpriteColor;
    private Coroutine flashRoutine;

    /// <summary>
    /// Flash red when taking damage
    /// </summary>
    System.Collections.IEnumerator FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalSpriteColor;
        }
        flashRoutine = null;
    }

    /// <summary>
    /// Called when player health reaches zero — Davy Jones respawns
    /// </summary>
    void Die()
    {
        Debug.Log("Davy Jones falls... but the curse won't let him rest.");

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDeath();
    }

    /// <summary>True while the player is in a post-respawn/death-sequence grace period.</summary>
    private bool respawnProtected = false;

    /// <summary>True while the player is in a post-respawn grace period (ports won't trigger).</summary>
    public bool IsRespawnProtected => respawnProtected;

    /// <summary>Clears the respawn protection so the port can open immediately.</summary>
    public void ClearRespawnProtection() => respawnProtected = false;

    /// <summary>
    /// Respawn: heal to full, teleport to safe position, clear enemies
    /// </summary>
    public void Respawn(Vector3 position)
    {
        currentHealth = maxHealth;
        UpdateDamageSprite();
        transform.position = position;
        targetPosition = position;
        isMoving = false;
        gameObject.SetActive(true);

        // Suppress port entry until explicitly cleared (after death dialogue)
        respawnProtected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
    }

    /// <summary>Set the three hull damage-state sprites for the equipped ship.</summary>
    public void SetHullSprites(Sprite healthy, Sprite mild, Sprite heavy)
    {
        hullHealthy = healthy;
        hullMild = mild;
        hullHeavy = heavy;
        UpdateDamageSprite();
    }

    /// <summary>Swap the hull sprite by remaining HP (&gt;2/3 healthy, &gt;1/3 mild, else heavy) and update fire.</summary>
    void UpdateDamageSprite()
    {
        if (hullHealthy == null) return;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        float ratio = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;
        int level = 0;
        Sprite target = hullHealthy;
        if (ratio <= 1f / 3f) { level = 2; target = hullHeavy != null ? hullHeavy : (hullMild != null ? hullMild : hullHealthy); }
        else if (ratio <= 2f / 3f) { level = 1; if (hullMild != null) target = hullMild; }
        if (sr.sprite != target) sr.sprite = target;
        UpdateFires(level);
    }

    /// <summary>Spawn/clear flicker flames based on damage level (0 none, 1 mild, 2 heavy).</summary>
    void UpdateFires(int level)
    {
        if (level == currentFireLevel) return;
        currentFireLevel = level;
        foreach (var f in fires) if (f != null) Destroy(f);
        fires.Clear();
        if (level <= 0) return;
        if (flameSprite == null) flameSprite = Resources.Load<Sprite>("Flame");
        if (flameSprite == null) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        float hw = sr.sprite.bounds.size.x;
        float hh = sr.sprite.bounds.size.y;
        float flameH = Mathf.Max(0.01f, flameSprite.bounds.size.y);
        float scale = (0.4f * hh) / flameH; // flame ~40% of hull height

        if (level >= 2)
        {
            SpawnFlame(new Vector3(-0.18f * hw, 0.20f * hh, 0f), scale);
            SpawnFlame(new Vector3(0.16f * hw, -0.04f * hh, 0f), scale * 0.9f);
            SpawnFlame(new Vector3(0.0f, -0.24f * hh, 0f), scale * 0.8f);
        }
        else
        {
            SpawnFlame(new Vector3(0.05f * hw, 0.05f * hh, 0f), scale * 0.7f);
        }
    }

    void SpawnFlame(Vector3 localPos, float scale)
    {
        GameObject go = new GameObject("Flame");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(scale, scale, 1f);
        SpriteRenderer fsr = go.AddComponent<SpriteRenderer>();
        fsr.sprite = flameSprite;
        fsr.sortingOrder = 6;
        FlameFlicker fl = go.AddComponent<FlameFlicker>();
        fl.baseScale = scale;
        fires.Add(go);
    }

    /// <summary>
    /// Returns current health (useful for UI or other systems)
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
