using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all UI elements: health display, kill counter, game over screen
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text element to display player health")]
    public TextMeshProUGUI healthText;

    [Tooltip("Text element to display kill count")]
    public TextMeshProUGUI killCountText;

    [Tooltip("Wave announcement text")]
    public TextMeshProUGUI waveText;

    [Tooltip("Game over panel (set inactive at start)")]
    public GameObject gameOverPanel;

    [Tooltip("Final score text on game over screen")]
    public TextMeshProUGUI finalScoreText;

    [Header("Loot Display")]
    [Tooltip("Text elements for loot counts")]
    public TextMeshProUGUI goldCountText;
    public TextMeshProUGUI woodCountText;
    public TextMeshProUGUI canvasCountText;
    public TextMeshProUGUI metalCountText;

    // Loot inventory
    // Loot counts are now tracked in GameManager (gold/wood/canvas/metal + runGold/runWood/runCanvas/runMetal)

    void Start()
    {
        if (waveText != null)
        {
            waveText.gameObject.SetActive(false);
        }

        // Make sure game over panel is hidden at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the health display
    /// </summary>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    public void UpdatePlayerHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }
    }

    /// <summary>
    /// Updates the kill counter display
    /// </summary>
    /// <param name="kills">Current kill count</param>
    public void UpdateKillCount(int kills)
    {
        if (killCountText != null)
        {
            killCountText.text = $"Kills: {kills}";
        }
    }

    /// <summary>
    /// Adds loot to the current run and updates display
    /// </summary>
    public void AddLoot(LootType lootType)
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        switch (lootType)
        {
            case LootType.Gold:
                gm.runGold++;
                break;
            case LootType.Wood:
                gm.runWood++;
                break;
            case LootType.Canvas:
                gm.runCanvas++;
                break;
            case LootType.Metal:
                gm.runMetal++;
                break;
        }

        RefreshLootDisplay();
    }

    /// <summary>
    /// Refreshes all loot counter texts from GameManager values.
    /// Shows banked amount, with run loot in brackets if > 0.
    /// Format: "5 (+3)" or just "5" if no run loot.
    /// </summary>
    public void RefreshLootDisplay()
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        UpdateLootText(goldCountText, gm.gold, gm.runGold * 10);
        UpdateLootText(woodCountText, gm.wood, gm.runWood);
        UpdateLootText(canvasCountText, gm.canvas, gm.runCanvas);
        UpdateLootText(metalCountText, gm.metal, gm.runMetal);
    }

    void UpdateLootText(TextMeshProUGUI text, int banked, int run)
    {
        if (text == null) return;
        if (run > 0)
            text.text = $"{banked} <color=#88ff88>(+{run})</color>";
        else
            text.text = banked.ToString();
    }

    /// <summary>
    /// Shows the game over screen with final score
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null && GameManager.Instance != null)
        {
            finalScoreText.text = $"Final Score: {GameManager.Instance.GetKillCount()} Kills";
        }
    }

    /// <summary>
    /// Called by Restart button (hook this up in Unity Inspector)
    /// </summary>
    public void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Shows wave text for 2 seconds then hides
    /// </summary>
    public void ShowWave(int waveNumber)
    {
        if (waveText == null) return;
        StopAllCoroutines();
        StartCoroutine(WaveRoutine(waveNumber));
    }

    System.Collections.IEnumerator WaveRoutine(int waveNumber)
    {
        waveText.gameObject.SetActive(true);
        waveText.alpha = 1f;
        waveText.text = $"Wave {waveNumber}";
        yield return new WaitForSeconds(2f);
        waveText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows port welcome message
    /// </summary>
    public void ShowPortWelcome(string portName, float duration)
    {
        if (waveText == null) return;
        StopAllCoroutines();
        StartCoroutine(PortWelcomeRoutine(portName, duration));
    }

    System.Collections.IEnumerator PortWelcomeRoutine(string portName, float duration)
    {
        waveText.gameObject.SetActive(true);
        waveText.alpha = 1f;
        waveText.text = $"Welcome to {portName}";
        yield return new WaitForSeconds(duration);
        waveText.gameObject.SetActive(false);
    }
}
