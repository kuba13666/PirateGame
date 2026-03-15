using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages game state, score tracking, and serves as central communication point
/// This is a Singleton - only one instance exists in the game
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance (accessible from anywhere via GameManager.Instance)
    public static GameManager Instance { get; private set; }

    /// <summary>Full-screen black overlay shown during the death sequence.</summary>
    private GameObject deathOverlay;

    [Header("Game Stats")]
    [Tooltip("Current number of enemies killed")]
    private int killCount = 0;

    [Tooltip("Player's gold for purchasing upgrades")]
    public int gold = 500;

    [Tooltip("Player's resource stockpiles")]
    public int wood = 0;
    public int canvas = 0;
    public int metal = 0;

    [Tooltip("Damage multiplier from upgrades")]
    public float damageMultiplier = 1f;

    [Tooltip("Loot gold multiplier from upgrades")]
    public float lootMultiplier = 1f;

    // Reference to UI manager
    public UIManager uiManager;

    // Game state
    private bool isGameOver = false;

    [Header("Death Tracking")]
    public int deathCount = 0;
    public bool hasAwakened = false; // true after first death intro

    void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes (optional)
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate GameManagers
        }
    }

    void Start()
    {
        // Find UI Manager in the scene
        uiManager = FindFirstObjectByType<UIManager>();

        // Initialize kill count display
        if (uiManager != null)
        {
            uiManager.UpdateKillCount(killCount);
        }

        CreateDeathOverlay();
    }

    /// <summary>Creates a full-screen black overlay for the death sequence.</summary>
    void CreateDeathOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        deathOverlay = new GameObject("DeathOverlay");
        deathOverlay.transform.SetParent(canvas.transform, false);

        RectTransform rt = deathOverlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = deathOverlay.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.92f);
        img.raycastTarget = false;

        // Place it high in the sibling order so it covers gameplay, but dialogue renders on top
        deathOverlay.transform.SetSiblingIndex(canvas.transform.childCount - 1);

        deathOverlay.SetActive(false);
    }

    /// <summary>
    /// Increments kill count when enemy dies
    /// </summary>
    public void AddKill()
    {
        if (isGameOver) return;

        killCount++;

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateKillCount(killCount);
        }

        Debug.Log($"Enemy killed! Total kills: {killCount}");
    }

    /// <summary>
    /// Updates player health display
    /// </summary>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    public void UpdatePlayerHealth(int currentHealth, int maxHealth)
    {
        if (uiManager != null)
        {
            uiManager.UpdatePlayerHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Called when player dies — respawn instead of game over (Davy Jones curse).
    /// Shows black overlay, plays death dialogue, then opens port/shop.
    /// </summary>
    public void OnPlayerDeath()
    {
        deathCount++;
        Debug.Log($"Death #{deathCount} — The curse pulls Davy Jones back...");

        // ── 1. Close any open shop / port ──
        PortZone activePort = PortZone.GetActivePort();
        if (activePort != null)
            activePort.ForceExitPort();

        ShopUI shopUI = FindFirstObjectByType<ShopUI>();
        if (shopUI != null && shopUI.shopPanel != null)
            shopUI.shopPanel.SetActive(false);

        // ── 2. Loot loss — upgrades & crew persist, resources don't ──
        int lostGold = gold;
        int lostWood = wood;
        int lostCanvas = canvas;
        int lostMetal = metal;
        gold = 0;
        wood = 0;
        canvas = 0;
        metal = 0;
        if (lostGold + lostWood + lostCanvas + lostMetal > 0)
            Debug.Log($"Loot lost to the deep: {lostGold} gold, {lostWood} wood, {lostCanvas} canvas, {lostMetal} metal.");

        // ── 3. Black overlay + pause ──
        if (deathOverlay != null)
        {
            deathOverlay.SetActive(true);
            // Make sure overlay is below dialogue panel
            DialogueUI dui = FindFirstObjectByType<DialogueUI>();
            if (dui != null && dui.dialoguePanel != null)
                dui.dialoguePanel.transform.SetAsLastSibling();
        }
        Time.timeScale = 0f;

        // ── 4. Despawn enemies, stop waves ──
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);

        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null) spawner.enabled = false;

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.StopWaves();

        // ── 5. Respawn player at Safe Harbor ──
        PlayerController player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player != null)
            player.Respawn(new Vector3(5f, 5f, 0f));

        // ── 6. Notify quest system (may show dialogue on the overlay) ──
        bool questHandledDialogue = false;
        if (QuestManager.Instance != null)
            questHandledDialogue = QuestManager.Instance.ReportPlayerDeath(deathCount, ResumeAfterDeath);

        // ── 7. If quest didn't show a dialogue, show generic death lines ──
        if (!questHandledDialogue)
        {
            DialogueUI dialogueUI = FindFirstObjectByType<DialogueUI>();
            if (dialogueUI != null)
            {
                var lines = new List<DialogueLine>
                {
                    new DialogueLine("Voice of the Deep", "The sea refuses your soul..."),
                    new DialogueLine("Voice of the Deep", "Rise again, Davy Jones.")
                };
                dialogueUI.ShowDialogue(lines, ResumeAfterDeath);
            }
            else
            {
                ResumeAfterDeath();
            }
        }
    }

    /// <summary>
    /// Called after the death dialogue finishes. Hides overlay, opens port/shop.
    /// </summary>
    void ResumeAfterDeath()
    {
        // Hide overlay
        if (deathOverlay != null)
            deathOverlay.SetActive(false);

        // Resume time momentarily so PortZone records the correct previousTimeScale
        Time.timeScale = 1f;

        // Clear respawn protection so port can auto-trigger
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.ClearRespawnProtection();

        // Re-enable spawner (waves restart when the player exits the port)
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null) spawner.enabled = true;

        // Reset wave escalation (death resets difficulty)
        WaveManager wm = FindFirstObjectByType<WaveManager>();
        if (wm != null) wm.ResetEscalation();

        // Programmatically open the Safe Harbor port (shows shop, pauses time)
        PortZone safeHarbor = FindSafeHarbor();
        if (safeHarbor != null)
        {
            safeHarbor.TriggerPortEntry();
        }
        else
        {
            // No port found — just restart waves directly
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null) waveManager.ResetAndStart();
        }
    }

    PortZone FindSafeHarbor()
    {
        foreach (var pz in FindObjectsByType<PortZone>(FindObjectsSortMode.None))
        {
            if (pz.portName == "Safe Harbor") return pz;
            if (pz.location != null && pz.location.locationId == "safe_harbor") return pz;
        }
        return null;
    }

    /// <summary>
    /// Called when player dies (legacy — kept for compatibility)
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;

        if (uiManager != null)
        {
            uiManager.ShowGameOver();
        }

        Debug.Log($"Game Over! Final Score: {killCount} kills");

        // Stop enemy spawning
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.enabled = false;
        }
    }

    /// <summary>
    /// Restarts the game (call this from a restart button)
    /// </summary>
    public void RestartGame()
    {
        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    /// <summary>
    /// Returns current kill count
    /// </summary>
    public int GetKillCount()
    {
        return killCount;
    }
}
