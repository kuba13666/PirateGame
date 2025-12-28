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
    private int goldCount = 0;
    private int woodCount = 0;
    private int canvasCount = 0;
    private int metalCount = 0;

    void Start()
    {
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
    /// Adds loot to inventory and updates display
    /// </summary>
    public void AddLoot(LootType lootType)
    {
        switch (lootType)
        {
            case LootType.Gold:
                goldCount++;
                if (goldCountText != null) goldCountText.text = goldCount.ToString();
                break;
            case LootType.Wood:
                woodCount++;
                if (woodCountText != null) woodCountText.text = woodCount.ToString();
                break;
            case LootType.Canvas:
                canvasCount++;
                if (canvasCountText != null) canvasCountText.text = canvasCount.ToString();
                break;
            case LootType.Metal:
                metalCount++;
                if (metalCountText != null) metalCountText.text = metalCount.ToString();
                break;
        }
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
}
