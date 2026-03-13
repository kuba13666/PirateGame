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

    private float previousTimeScale = 1f;
    private bool timePaused = false;

    private bool playerInPort = false;
    private float exitCooldownUntil = 0f;
    private const float RE_ENTRY_COOLDOWN = 1f;

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

        // Show welcome message
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowPortWelcome(portName, welcomeMessageDuration);
        }

        // Open shop UI
        ShopUI shopUI = FindFirstObjectByType<ShopUI>();
        if (shopUI != null)
        {
            shopUI.OpenShop();
        }
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
