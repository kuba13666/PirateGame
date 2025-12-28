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

        // Ensure Ship sprite is properly configured
        EnsureShipSpriteImported();
        
        // Ensure enemy sprites are properly configured
        EnsureEnemySpriteImported("Assets/GameAssets/enemy_giant_crab.png");
        EnsureEnemySpriteImported("Assets/GameAssets/enemy_harpy.png");
        EnsureEnemySpriteImported("Assets/GameAssets/enemy_mermaid.png");

        // Create Projectile Prefab first
        GameObject projectilePrefab = CreateProjectilePrefab();

        // Create Loot Prefabs (4 types)
        GameObject[] lootPrefabs = CreateLootPrefabs();

        // Create Enemy Prefabs (3 types)
        GameObject crabEnemy = CreateEnemyPrefab("Assets/GameAssets/enemy_giant_crab.png", "Enemy_Crab", Color.red, 1, lootPrefabs);
        GameObject harpyEnemy = CreateEnemyPrefab("Assets/GameAssets/enemy_harpy.png", "Enemy_Harpy", Color.blue, 2, lootPrefabs);
        GameObject mermaidEnemy = CreateEnemyPrefab("Assets/GameAssets/enemy_mermaid.png", "Enemy_Mermaid", Color.green, 3, lootPrefabs);

        // Create Player Ship with Cannons
        GameObject player = CreatePlayerShip(projectilePrefab);

        // Create Enemy Spawner with all 3 enemy types
        CreateEnemySpawner(crabEnemy, harpyEnemy, mermaidEnemy);

        // Create Game Manager
        CreateGameManager();

        // Create UI
        CreateUI();

        // Create map boundaries
        CreateMapBoundaries();

        // Setup camera to follow player
        SetupCamera(player);

        Debug.Log("✓ Scene setup complete!");
    }

    static GameObject CreatePlayerShip(GameObject projectilePrefab)
    {
        // Create player sprite
        GameObject player = new GameObject("Player");
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        
        // Try to load custom ship sprite
        Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/Ship.png");
        if (customSprite != null)
        {
            sr.sprite = customSprite;
        }
        else
        {
            // Fallback to default sprite
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(0.6f, 0.4f, 0.2f); // Brown color
            Debug.LogWarning("Ship.png not found, using default sprite");
        }
        
        player.transform.localScale = new Vector3(GameConstants.PLAYER_SCALE_X, GameConstants.PLAYER_SCALE_Y, GameConstants.PLAYER_SCALE_Z);
        player.tag = "Player";

        // Add components
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = GameConstants.PLAYER_MOVE_SPEED;
        pc.maxHealth = GameConstants.PLAYER_MAX_HEALTH;
        pc.minX = GameConstants.MAP_MIN_X;
        pc.maxX = GameConstants.MAP_MAX_X;
        pc.minY = GameConstants.MAP_MIN_Y;
        pc.maxY = GameConstants.MAP_MAX_Y;

        // Create 6 cannons - 3 on left side, 3 on right side
        CreateCannon(player.transform, "Cannon_Left_Top", new Vector3(-GameConstants.CANNON_OFFSET_X, GameConstants.CANNON_OFFSET_Y, 0), 
            new Vector2(-1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Left_Mid", new Vector3(-GameConstants.CANNON_OFFSET_X, 0f, 0), 
            new Vector2(-1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Left_Bottom", new Vector3(-GameConstants.CANNON_OFFSET_X, -GameConstants.CANNON_OFFSET_Y, 0), 
            new Vector2(-1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Right_Top", new Vector3(GameConstants.CANNON_OFFSET_X, GameConstants.CANNON_OFFSET_Y, 0), 
            new Vector2(1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Right_Mid", new Vector3(GameConstants.CANNON_OFFSET_X, 0f, 0), 
            new Vector2(1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Right_Bottom", new Vector3(GameConstants.CANNON_OFFSET_X, -GameConstants.CANNON_OFFSET_Y, 0), 
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
        cc.fireRate = GameConstants.CANNON_FIRE_RATE;
        cc.spawnOffset = fireDirection * GameConstants.CANNON_PROJECTILE_SPAWN_OFFSET;
    }

    static GameObject CreateProjectilePrefab()
    {
        // Create temporary projectile object as 2D sprite
        GameObject projectile = new GameObject("Projectile");
        projectile.transform.localScale = new Vector3(GameConstants.PROJECTILE_SCALE, GameConstants.PROJECTILE_SCALE, GameConstants.PROJECTILE_SCALE);

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
        proj.speed = GameConstants.PROJECTILE_SPEED;
        proj.lifetime = GameConstants.PROJECTILE_LIFETIME;
        proj.damage = GameConstants.PROJECTILE_DAMAGE;

        // Save as prefab
        string path = "Assets/Projectile.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectile, path);
        DestroyImmediate(projectile);

        Debug.Log("✓ Projectile prefab created");
        return prefab;
    }

    static GameObject[] CreateLootPrefabs()
    {
        GameObject[] lootPrefabs = new GameObject[4];
        
        // Gold (yellow) - using custom sprite
        lootPrefabs[0] = CreateSingleLootPrefab("Loot_Gold", new Color(1f, 0.84f, 0f), LootType.Gold, "Assets/GameAssets/gold_loot.png", new Vector3(GameConstants.LOOT_GOLD_SCALE_X, GameConstants.LOOT_GOLD_SCALE_Y, GameConstants.LOOT_GOLD_SCALE_Z));
        
        // Wood (brown) - using custom sprite
        lootPrefabs[1] = CreateSingleLootPrefab("Loot_Wood", new Color(0.6f, 0.4f, 0.2f), LootType.Wood, "Assets/GameAssets/wood_loot.png", new Vector3(GameConstants.LOOT_WOOD_SCALE_X, GameConstants.LOOT_WOOD_SCALE_Y, GameConstants.LOOT_WOOD_SCALE_Z));
        
        // Canvas (beige/white) - using custom sprite
        lootPrefabs[2] = CreateSingleLootPrefab("Loot_Canvas", new Color(0.96f, 0.87f, 0.7f), LootType.Canvas, "Assets/GameAssets/canvas_loot.png", new Vector3(GameConstants.LOOT_CANVAS_SCALE_X, GameConstants.LOOT_CANVAS_SCALE_Y, GameConstants.LOOT_CANVAS_SCALE_Z));
        
        // Metal (gray) - using custom sprite
        lootPrefabs[3] = CreateSingleLootPrefab("Loot_Metal", new Color(0.7f, 0.7f, 0.7f), LootType.Metal, "Assets/GameAssets/metal_loot.png", new Vector3(GameConstants.LOOT_METAL_SCALE_X, GameConstants.LOOT_METAL_SCALE_Y, GameConstants.LOOT_METAL_SCALE_Z));
        
        Debug.Log("✓ Created 4 loot prefabs (Gold, Wood, Canvas, Metal)");
        return lootPrefabs;
    }

    static GameObject CreateSingleLootPrefab(string prefabName, Color color, LootType lootType, string spritePath = null, Vector3? scale = null)
    {
        // Create loot object
        GameObject loot = new GameObject(prefabName);
        loot.transform.localScale = scale ?? new Vector3(GameConstants.LOOT_GOLD_SCALE_X, GameConstants.LOOT_GOLD_SCALE_Y, GameConstants.LOOT_GOLD_SCALE_Z);
        
        // Add sprite renderer
        SpriteRenderer sr = loot.AddComponent<SpriteRenderer>();
        
        // Try to load custom sprite if path provided
        if (!string.IsNullOrEmpty(spritePath))
        {
            Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (customSprite != null)
            {
                sr.sprite = customSprite;
            }
            else
            {
                // Fallback to circle with color
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                sr.color = color;
                Debug.LogWarning($"{spritePath} not found, using default circle");
            }
        }
        else
        {
            // Use default circle with color
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = color;
        }
        
        // Add physics
        Rigidbody2D rb = loot.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        
        CircleCollider2D col = loot.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        
        // Add loot script
        LootItem lootItem = loot.AddComponent<LootItem>();
        lootItem.lootType = lootType;
        lootItem.lifetime = GameConstants.LOOT_LIFETIME;
        
        // Save as prefab
        string path = $"Assets/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(loot, path);
        DestroyImmediate(loot);
        
        return prefab;
    }

    static GameObject CreateEnemyPrefab(string spritePath, string prefabName, Color color, int health, GameObject[] lootPrefabs)
    {
        // Create enemy sprite
        GameObject enemy = new GameObject(prefabName);
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        
        // Try to load custom enemy sprite
        Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (customSprite != null)
        {
            sr.sprite = customSprite;
        }
        else
        {
            // Fallback to default sprite
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = color;
            Debug.LogWarning($"{spritePath} not found, using default sprite");
        }
        
        enemy.transform.localScale = new Vector3(GameConstants.ENEMY_SCALE, GameConstants.ENEMY_SCALE, GameConstants.ENEMY_SCALE);
        enemy.tag = "Enemy";

        // Add components
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.moveSpeed = GameConstants.ENEMY_MOVE_SPEED;
        ec.collisionDamage = GameConstants.ENEMY_COLLISION_DAMAGE;
        ec.maxHealth = health;
        ec.lootDropChance = GameConstants.LOOT_DROP_CHANCE;
        ec.lootPrefabs = lootPrefabs;

        // Save as prefab
        string path = $"Assets/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        DestroyImmediate(enemy);

        Debug.Log($"✓ {prefabName} prefab created");
        return prefab;
    }

    static void CreateEnemySpawner(GameObject crabPrefab, GameObject harpyPrefab, GameObject mermaidPrefab)
    {
        GameObject spawner = new GameObject("EnemySpawner");
        EnemySpawner es = spawner.AddComponent<EnemySpawner>();
        es.crabEnemyPrefab = crabPrefab;
        es.harpyEnemyPrefab = harpyPrefab;
        es.mermaidEnemyPrefab = mermaidPrefab;
        es.spawnInterval = GameConstants.SPAWN_INTERVAL;
        es.minSpawnInterval = GameConstants.MIN_SPAWN_INTERVAL;
        es.spawnDistance = GameConstants.ENEMY_SPAWN_DISTANCE;

        Debug.Log("✓ Enemy spawner created with 3 enemy types");
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

        // Create Loot Counter Panel (top-right, below kill count)
        GameObject lootPanelObj = new GameObject("LootPanel");
        lootPanelObj.transform.SetParent(canvasObj.transform);
        RectTransform lootPanelRect = lootPanelObj.AddComponent<RectTransform>();
        lootPanelRect.anchorMin = new Vector2(1, 1);
        lootPanelRect.anchorMax = new Vector2(1, 1);
        lootPanelRect.pivot = new Vector2(1, 1);
        lootPanelRect.anchoredPosition = new Vector2(-10, -60);
        lootPanelRect.sizeDelta = new Vector2(200, 40);

        // Create horizontal layout
        HorizontalLayoutGroup layoutGroup = lootPanelObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.childAlignment = TextAnchor.MiddleRight;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // Create loot counters (Gold, Wood, Canvas, Metal)
        TextMeshProUGUI goldText = CreateLootCounter(lootPanelObj.transform, "Assets/GameAssets/gold_loot.png", "GoldCounter");
        TextMeshProUGUI woodText = CreateLootCounter(lootPanelObj.transform, "Assets/GameAssets/wood_loot.png", "WoodCounter");
        TextMeshProUGUI canvasText = CreateLootCounter(lootPanelObj.transform, "Assets/GameAssets/canvas_loot.png", "CanvasCounter");
        TextMeshProUGUI metalText = CreateLootCounter(lootPanelObj.transform, "Assets/GameAssets/metal_loot.png", "MetalCounter");

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
        uiManager.goldCountText = goldText;
        uiManager.woodCountText = woodText;
        uiManager.canvasCountText = canvasText;
        uiManager.metalCountText = metalText;

        // Hook up button
        button.onClick.AddListener(() => uiManager.OnRestartButtonClicked());

        Debug.Log("✓ UI created with health, kills, loot counters, and game over screen");
    }

    static TextMeshProUGUI CreateLootCounter(Transform parent, string iconPath, string name)
    {
        // Create container
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(45, 40);

        // Create icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(container.transform);
        Image icon = iconObj.AddComponent<Image>();
        
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        if (iconSprite != null)
        {
            icon.sprite = iconSprite;
        }
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.sizeDelta = new Vector2(30, 30);

        // Create text
        GameObject textObj = new GameObject("Count");
        textObj.transform.SetParent(container.transform);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "0";
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(0, 0.5f);
        textRect.pivot = new Vector2(0, 0.5f);
        textRect.anchoredPosition = new Vector2(32, 0);
        textRect.sizeDelta = new Vector2(30, 40);

        return text;
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
    /// Ensures the Ship sprite is properly imported as a 2D sprite
    /// </summary>
    static void EnsureShipSpriteImported()
    {
        string assetPath = "Assets/GameAssets/Ship.png";
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        
        if (importer != null)
        {
            bool needsReimport = false;
            
            // Set texture type to Sprite
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }
            
            // Set sprite mode to Single
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                needsReimport = true;
            }
            
            // Set pixels per unit (adjust if needed)
            if (importer.spritePixelsPerUnit != 1024f)
            {
                importer.spritePixelsPerUnit = 1024f;
                needsReimport = true;
            }
            
            if (needsReimport)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                Debug.Log("✓ Ship sprite configured correctly");
            }
        }
        else
        {
            Debug.LogWarning("Ship.png not found in Assets folder");
        }
    }

    /// <summary>
    /// Ensures an enemy sprite is properly imported as a 2D sprite
    /// </summary>
    static void EnsureEnemySpriteImported(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        
        if (importer != null)
        {
            bool needsReimport = false;
            
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }
            
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                needsReimport = true;
            }
            
            if (needsReimport)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                Debug.Log($"✓ Enemy sprite {assetPath} configured");
            }
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
            cameraFollow.smoothSpeed = GameConstants.CAMERA_SMOOTH_SPEED;
            cameraFollow.offset = new Vector3(0, 0, GameConstants.CAMERA_OFFSET_Z);
            Debug.Log("✓ Camera set to follow player");
        }
    }

    /// <summary>
    /// Creates visible boundary walls around the map edges
    /// </summary>
    static void CreateMapBoundaries()
    {
        float minX = GameConstants.MAP_MIN_X;
        float maxX = GameConstants.MAP_MAX_X;
        float minY = GameConstants.MAP_MIN_Y;
        float maxY = GameConstants.MAP_MAX_Y;
        float wallThickness = GameConstants.WALL_THICKNESS;
        float mapWidth = GameConstants.MAP_WIDTH;
        float mapHeight = GameConstants.MAP_HEIGHT;

        // Top wall
        CreateBoundaryWall("Boundary_Top", 
            new Vector3(0, maxY + wallThickness/2, 0), 
            new Vector3(mapWidth + wallThickness * 2, wallThickness, 1));

        // Bottom wall
        CreateBoundaryWall("Boundary_Bottom", 
            new Vector3(0, minY - wallThickness/2, 0), 
            new Vector3(mapWidth + wallThickness * 2, wallThickness, 1));

        // Left wall (extend to cover full height including top/bottom walls)
        CreateBoundaryWall("Boundary_Left", 
            new Vector3(minX - wallThickness/2, 0, 0), 
            new Vector3(wallThickness, mapHeight + wallThickness * 2, 1));

        // Right wall (extend to cover full height including top/bottom walls)
        CreateBoundaryWall("Boundary_Right", 
            new Vector3(maxX + wallThickness/2, 0, 0), 
            new Vector3(wallThickness, mapHeight + wallThickness * 2, 1));

        // Set camera background to water blue
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color(0.2f, 0.4f, 0.7f); // Ocean blue
        }

        Debug.Log("✓ Map boundaries created");
    }

    static void CreateBoundaryWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = new GameObject(name);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(1f, 1f, 10f);

        // Add sprite renderer
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = new Color(0.3f, 0.2f, 0.1f, 1f); // Dark brown color
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(scale.x, scale.y); // Set actual world size
        sr.sortingOrder = -1; // Behind other objects

        // Add collider to stop movement
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = new Vector2(scale.x, scale.y);
        col.isTrigger = false; // Solid wall
    }
}
