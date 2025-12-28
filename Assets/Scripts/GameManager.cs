using UnityEngine;

/// <summary>
/// Manages game state, score tracking, and serves as central communication point
/// This is a Singleton - only one instance exists in the game
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance (accessible from anywhere via GameManager.Instance)
    public static GameManager Instance { get; private set; }

    [Header("Game Stats")]
    [Tooltip("Current number of enemies killed")]
    private int killCount = 0;

    // Reference to UI manager
    public UIManager uiManager;

    // Game state
    private bool isGameOver = false;

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
    /// Called when player dies
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
