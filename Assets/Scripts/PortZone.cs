using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger zone that detects when player enters a port area
/// Disables enemy spawning and shows welcome message
/// </summary>
public class PortZone : MonoBehaviour
{
    [Header("Port Settings")]
    [Tooltip("Name of this port")]
    public string portName = "Safe Harbor";

    [Tooltip("How long to display the welcome message")]
    public float welcomeMessageDuration = 3f;

    [Tooltip("Ignore entries during the first seconds of the scene")]
    public float minEnterTime = 0.5f;

    [Header("Location")]
    public Location location;

    private float previousTimeScale = 1f;
    private bool timePaused = false;

    private bool playerInPort = false;
    private float exitCooldownUntil = 0f;
    private const float RE_ENTRY_COOLDOWN = 1f;

    /// <summary>
    /// Returns the PortZone the player is currently inside, or null.
    /// </summary>
    public static PortZone GetActivePort()
    {
        foreach (var pz in FindObjectsByType<PortZone>(FindObjectsSortMode.None))
            if (pz.playerInPort) return pz;
        return null;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryEnterPort();
        }
    }

    // Player stays inside trigger — needed after closing shop without leaving
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !playerInPort)
        {
            TryEnterPort();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Player physically left the zone — don't reopen
            if (playerInPort)
            {
                ForceExitPort();
            }
        }
    }

    void TryEnterPort()
    {
        if (Time.unscaledTime < exitCooldownUntil) return;
        EnterPort();
    }

    void EnterPort()
    {
        // Avoid triggering instantly at scene start
        if (Time.timeSinceLevelLoad < minEnterTime) return;

        // Don't open port/shop during post-respawn grace period
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && player.IsRespawnProtected) return;

        if (playerInPort) return;
        
        playerInPort = true;
        Debug.Log($"Player entered {portName}");

        // Pause game time
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        timePaused = true;

        // Disable enemy spawning
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.enabled = false;
            // Ensure automatic spawns are off
            spawner.manualWaveControl = true;
        }

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            // Stop any ongoing wave coroutine cleanly
            waveManager.StopWaves();
        }

        // Despawn all active enemies for a clean reset
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }

        // Bank run loot safely
        if (GameManager.Instance != null)
            GameManager.Instance.BankRunLoot();

        // Show welcome message
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowPortWelcome(portName, welcomeMessageDuration);
        }

        // Discover this location
        if (location != null)
            location.Discover();

        // Report to quest system
        if (location != null && QuestManager.Instance != null)
            QuestManager.Instance.ReportLocationReached(location.locationId);

        // Open shop UI only if this port has a shop
        bool hasShop = location == null || location.hasShop;
        if (hasShop)
        {
            ShopUI shopUI = FindFirstObjectByType<ShopUI>();
            if (shopUI != null)
            {
                shopUI.OpenShop();
            }
        }
        else
        {
            // No shop — auto-exit port after a brief moment so time resumes
            // (quest dialogue uses unscaled time, so it will still play)
            ForceExitPort();
        }
    }

    /// <summary>
    /// Programmatically enter this port (called by GameManager after death sequence).
    /// </summary>
    public void TriggerPortEntry()
    {
        playerInPort = false; // reset so EnterPort doesn't early-out
        EnterPort();
    }

    /// <summary>
    /// Called by ShopUI Close button or when player physically exits
    /// </summary>
    public void ForceExitPort()
    {
        ExitPort();
    }

    void ExitPort()
    {
        if (!playerInPort) return;
        
        playerInPort = false;
        exitCooldownUntil = Time.unscaledTime + RE_ENTRY_COOLDOWN;
        Debug.Log($"Player left {portName}");

        // Resume game time
        if (timePaused)
        {
            Time.timeScale = previousTimeScale;
            timePaused = false;
        }

        // Re-enable enemy spawning
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.enabled = true;
            // If waves are used, spawning is controlled by WaveManager
            spawner.manualWaveControl = true;
        }

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.ResetAndStart();
        }

        // Close shop UI
        ShopUI shopUI = FindFirstObjectByType<ShopUI>();
        if (shopUI != null)
        {
            shopUI.CloseShop();
        }
    }

    public bool IsPlayerInPort()
    {
        return playerInPort;
    }
}
