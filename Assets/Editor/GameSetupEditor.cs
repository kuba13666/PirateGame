using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Editor script to automatically set up the game scene
/// Run this from Unity menu: Tools > Setup Pirate Game Scene
/// </summary>
public class GameSetupEditor : EditorWindow
{
    [MenuItem("Tools/Setup Pirate Game Scene")]
    static void SetupScene()
    {
        if (EditorUtility.DisplayDialog("Setup Pirate Game Scene",
            "This will create all game objects, prefabs, and UI. Continue?",
            "Yes", "Cancel"))
        {
            CreateGameObjects();
            EditorUtility.DisplayDialog("Success!", 
                "Scene setup complete! Press Play to test.", "OK");
        }
    }

    static void CreateGameObjects()
    {
        // Ensure tags exist
        EnsureTagExists("Player");
        EnsureTagExists("Enemy");

        // Create Projectile Prefab first
        GameObject projectilePrefab = CreateProjectilePrefab();

        // Create Enemy Prefab
        GameObject enemyPrefab = CreateEnemyPrefab();

        // Create Player Ship with Cannons
        GameObject player = CreatePlayerShip(projectilePrefab);

        // Create Enemy Spawner
        CreateEnemySpawner(enemyPrefab);

        // Create Game Manager
        CreateGameManager();

        // Create UI
        CreateUI();

        // Setup camera to follow player
        SetupCamera(player);

        Debug.Log("✓ Scene setup complete!");
    }

    static GameObject CreatePlayerShip(GameObject projectilePrefab)
    {
        // Create player sprite
        GameObject player = new GameObject("Player");
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.6f, 0.4f, 0.2f); // Brown color
        player.transform.localScale = new Vector3(1f, 4f, 1f);
        player.tag = "Player";

        // Add components
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 5f;
        pc.maxHealth = 10;

        // Create 6 cannons - 3 on left side, 3 on right side
        CreateCannon(player.transform, "Cannon_Left_Top", new Vector3(-0.08f, 0.08f, 0), 
            new Vector2(-1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Left_Mid", new Vector3(-0.08f, 0f, 0), 
            new Vector2(-1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Left_Bottom", new Vector3(-0.08f, -0.08f, 0), 
            new Vector2(-1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Right_Top", new Vector3(0.08f, 0.08f, 0), 
            new Vector2(1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Right_Mid", new Vector3(0.08f, 0f, 0), 
            new Vector2(1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Right_Bottom", new Vector3(0.08f, -0.08f, 0), 
            new Vector2(1, 0), projectilePrefab);

        Debug.Log("✓ Player ship created with 6 cannons (3 per side)");
        return player;
    }

    static void CreateCannon(Transform parent, string name, Vector3 position, 
        Vector2 fireDirection, GameObject projectilePrefab)
    {
        GameObject cannon = new GameObject(name);
        cannon.transform.parent = parent;
        cannon.transform.localPosition = position;

        CannonController cc = cannon.AddComponent<CannonController>();
        cc.projectilePrefab = projectilePrefab;
        cc.fireDirection = fireDirection;
        cc.fireRate = 1f;
    }

    static GameObject CreateProjectilePrefab()
    {
        // Create temporary projectile object as 2D sprite
        GameObject projectile = new GameObject("Projectile");
        projectile.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // Add sprite renderer with circle sprite
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = Color.yellow;

        // Add components
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        Projectile proj = projectile.AddComponent<Projectile>();
        proj.speed = 10f;
        proj.lifetime = 3f;
        proj.damage = 1;

        // Save as prefab
        string path = "Assets/Projectile.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectile, path);
        DestroyImmediate(projectile);

        Debug.Log("✓ Projectile prefab created");
        return prefab;
    }

    static GameObject CreateEnemyPrefab()
    {
        // Create enemy sprite
        GameObject enemy = new GameObject("Enemy");
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = Color.red;
        enemy.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        enemy.tag = "Enemy";

        // Add components
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.moveSpeed = 2f;
        ec.collisionDamage = 1;
        ec.maxHealth = 1;
        ec.enemyColor = Color.red;

        // Save as prefab
        string path = "Assets/Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        DestroyImmediate(enemy);

        Debug.Log("✓ Enemy prefab created");
        return prefab;
    }

    static void CreateEnemySpawner(GameObject enemyPrefab)
    {
        GameObject spawner = new GameObject("EnemySpawner");
        EnemySpawner es = spawner.AddComponent<EnemySpawner>();
        es.enemyPrefab = enemyPrefab;
        es.spawnInterval = 2f;
        es.minSpawnInterval = 0.5f;
        es.spawnDistance = 12f;

        Debug.Log("✓ Enemy spawner created");
    }

    static void CreateGameManager()
    {
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        Debug.Log("✓ Game manager created");
    }

    static void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem if it doesn't exist
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Create Health Text (top-left)
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(canvasObj.transform);
        TextMeshProUGUI healthText = healthTextObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "HP: 10/10";
        healthText.fontSize = 24;
        healthText.color = Color.white;
        healthText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform healthRect = healthTextObj.GetComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0, 1);
        healthRect.anchorMax = new Vector2(0, 1);
        healthRect.pivot = new Vector2(0, 1);
        healthRect.anchoredPosition = new Vector2(10, -10);
        healthRect.sizeDelta = new Vector2(200, 50);

        // Create Kill Count Text (top-right)
        GameObject killTextObj = new GameObject("KillCountText");
        killTextObj.transform.SetParent(canvasObj.transform);
        TextMeshProUGUI killText = killTextObj.AddComponent<TextMeshProUGUI>();
        killText.text = "Kills: 0";
        killText.fontSize = 24;
        killText.color = Color.white;
        killText.alignment = TextAlignmentOptions.TopRight;
        
        RectTransform killRect = killTextObj.GetComponent<RectTransform>();
        killRect.anchorMin = new Vector2(1, 1);
        killRect.anchorMax = new Vector2(1, 1);
        killRect.pivot = new Vector2(1, 1);
        killRect.anchoredPosition = new Vector2(-10, -10);
        killRect.sizeDelta = new Vector2(200, 50);

        // Create Game Over Panel
        GameObject panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(canvasObj.transform);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelObj.SetActive(false);

        // Game Over Title
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(panelObj.transform);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 48;
        titleText.color = Color.red;
        titleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 100);
        titleRect.sizeDelta = new Vector2(400, 100);

        // Final Score Text
        GameObject scoreObj = new GameObject("FinalScoreText");
        scoreObj.transform.SetParent(panelObj.transform);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "Final Score: 0 Kills";
        scoreText.fontSize = 32;
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.Center;
        
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.anchoredPosition = new Vector2(0, 0);
        scoreRect.sizeDelta = new Vector2(400, 50);

        // Restart Button
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(panelObj.transform);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f);
        Button button = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -100);
        buttonRect.sizeDelta = new Vector2(200, 50);

        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Restart";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        // Create UI Manager
        GameObject uiManagerObj = new GameObject("UIManager");
        UIManager uiManager = uiManagerObj.AddComponent<UIManager>();
        uiManager.healthText = healthText;
        uiManager.killCountText = killText;
        uiManager.gameOverPanel = panelObj;
        uiManager.finalScoreText = scoreText;

        // Hook up button
        button.onClick.AddListener(() => uiManager.OnRestartButtonClicked());

        Debug.Log("✓ UI created with health, kills, and game over screen");
    }

    /// <summary>
    /// Ensures a tag exists in the project
    /// </summary>
    static void EnsureTagExists(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag))
            {
                found = true;
                break;
            }
        }

        // Add tag if it doesn't exist
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"✓ Created tag: {tag}");
        }
    }

    /// <summary>
    /// Sets up the main camera to follow the player
    /// </summary>
    static void SetupCamera(GameObject player)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }
            cameraFollow.target = player.transform;
            cameraFollow.smoothSpeed = 0.125f;
            cameraFollow.offset = new Vector3(0, 0, -10f);
            Debug.Log("✓ Camera set to follow player");
        }
    }
}
