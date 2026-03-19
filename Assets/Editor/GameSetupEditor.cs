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
        // Clean up existing objects first
        CleanupScene();

        // Ensure tags exist
        EnsureTagExists("Player");
        EnsureTagExists("Enemy");

        // Ensure Ship sprite is properly configured
        EnsureShipSpriteImported();
        
        // Ensure enemy sprites are properly configured
        EnsureEnemySpriteImported("Assets/Resources/enemy_giant_crab.png");
        EnsureEnemySpriteImported("Assets/Resources/enemy_harpy.png");
        EnsureEnemySpriteImported("Assets/Resources/enemy_mermaid.png");

        // Create Main Camera first
        CreateMainCamera();

        // Create Projectile Prefab first
        GameObject projectilePrefab = CreateProjectilePrefab();

        // Create Loot Prefabs (4 types)
        GameObject[] lootPrefabs = CreateLootPrefabs();

        // Create Enemy Prefabs (3 types)
        GameObject crabEnemy = CreateEnemyPrefab("Assets/Resources/enemy_giant_crab.png", "Enemy_Crab", Color.red, 1, lootPrefabs);
        GameObject harpyEnemy = CreateEnemyPrefab("Assets/Resources/enemy_harpy.png", "Enemy_Harpy", Color.blue, 2, lootPrefabs);
        GameObject mermaidEnemy = CreateEnemyPrefab("Assets/Resources/enemy_mermaid.png", "Enemy_Mermaid", Color.green, 3, lootPrefabs);

        // Create Enemy Ship Prefab (uses Ship.png tinted dark)
        GameObject enemyProjectilePrefab = CreateEnemyProjectilePrefab();
        GameObject enemyShip = CreateEnemyShipPrefab(lootPrefabs, enemyProjectilePrefab);

        // Create Player Ship with Cannons
        GameObject player = CreatePlayerShip(projectilePrefab);

        // Create Enemy Spawner with all enemy types
        CreateEnemySpawner(crabEnemy, harpyEnemy, mermaidEnemy, enemyShip);

        // Create Wave Manager
        CreateWaveManager(crabEnemy, harpyEnemy, mermaidEnemy, enemyShip);

        // Create Game Manager
        CreateGameManager();

        // Create UI
        CreateUI();
        CreateCompassUI();
        CreateDialogueUI();
        CreateQuestTrackerUI();

        // Create world locations (ports, islands, boss arena)
        CreateLocationManager();
        CreateAllLocations();
        CreateShopSystem();

        // Create quest system (after locations so dialogue can reference them)
        CreateQuestManager();

        // Create map boundaries
        CreateMapBoundaries();

        // Setup camera to follow player
        SetupCamera(player);

        Debug.Log("✓ Scene setup complete!");
    }

    static void CleanupScene()
    {
        // Remove all Canvas objects
        foreach (Canvas canvas in GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            GameObject.DestroyImmediate(canvas.gameObject);
        }
        
        // Remove all EventSystem objects
        foreach (UnityEngine.EventSystems.EventSystem es in GameObject.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None))
        {
            GameObject.DestroyImmediate(es.gameObject);
        }
        
        // Remove Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            GameObject.DestroyImmediate(mainCamera.gameObject);
        }
        
        // Remove old game objects
        GameObject.DestroyImmediate(GameObject.Find("Player"));
        GameObject.DestroyImmediate(GameObject.Find("EnemySpawner"));
        GameObject.DestroyImmediate(GameObject.Find("GameManager"));
        GameObject.DestroyImmediate(GameObject.Find("UIManager"));
        GameObject.DestroyImmediate(GameObject.Find("WaveManager"));
        GameObject.DestroyImmediate(GameObject.Find("ShopManager"));
        GameObject.DestroyImmediate(GameObject.Find("ShopUI"));
        GameObject.DestroyImmediate(GameObject.Find("CompassUI"));
        GameObject.DestroyImmediate(GameObject.Find("DialogueUI"));
        GameObject.DestroyImmediate(GameObject.Find("QuestTrackerUI"));
        GameObject.DestroyImmediate(GameObject.Find("QuestManager"));
        GameObject.DestroyImmediate(GameObject.Find("Port_SafeHarbor"));
        GameObject.DestroyImmediate(GameObject.Find("LocationManager"));
        
        // Remove all locations
        foreach (var loc in GameObject.FindObjectsByType<Location>(FindObjectsSortMode.None))
            GameObject.DestroyImmediate(loc.gameObject);
        
        // Remove boundary walls
        GameObject.DestroyImmediate(GameObject.Find("Boundary_Top"));
        GameObject.DestroyImmediate(GameObject.Find("Boundary_Bottom"));
        GameObject.DestroyImmediate(GameObject.Find("Boundary_Left"));
        GameObject.DestroyImmediate(GameObject.Find("Boundary_Right"));
        
        Debug.Log("✓ Scene cleaned up");
    }

    static void CreateMainCamera()
    {
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.2f, 0.4f, 0.7f); // Ocean blue
        
        cameraObj.AddComponent<AudioListener>();
        cameraObj.transform.position = new Vector3(0, 0, GameConstants.CAMERA_OFFSET_Z);
        
        Debug.Log("✓ Main Camera created with orthographic size 8");
    }

    static GameObject CreatePlayerShip(GameObject projectilePrefab)
    {
        // Create player sprite
        GameObject player = new GameObject("Player");
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        
        // Try to load custom ship sprite
        Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Ship.png");
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

        // Create 2 cannons for Sloop - 1 on left, 1 on right
        CreateCannon(player.transform, "Cannon_Left_1", new Vector3(-GameConstants.CANNON_OFFSET_X, 0f, 0), 
            new Vector2(-1, 0), projectilePrefab);
        CreateCannon(player.transform, "Cannon_Right_1", new Vector3(GameConstants.CANNON_OFFSET_X, 0f, 0), 
            new Vector2(1, 0), projectilePrefab);

        Debug.Log("✓ Player ship created with 2 cannons (Sloop)");
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
        sr.sortingOrder = 5;

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
        lootPrefabs[0] = CreateSingleLootPrefab("Loot_Gold", new Color(1f, 0.84f, 0f), LootType.Gold, "Assets/Resources/gold_loot.png", new Vector3(GameConstants.LOOT_GOLD_SCALE_X, GameConstants.LOOT_GOLD_SCALE_Y, GameConstants.LOOT_GOLD_SCALE_Z));
        
        // Wood (brown) - using custom sprite
        lootPrefabs[1] = CreateSingleLootPrefab("Loot_Wood", new Color(0.6f, 0.4f, 0.2f), LootType.Wood, "Assets/Resources/wood_loot.png", new Vector3(GameConstants.LOOT_WOOD_SCALE_X, GameConstants.LOOT_WOOD_SCALE_Y, GameConstants.LOOT_WOOD_SCALE_Z));
        
        // Canvas (beige/white) - using custom sprite
        lootPrefabs[2] = CreateSingleLootPrefab("Loot_Canvas", new Color(0.96f, 0.87f, 0.7f), LootType.Canvas, "Assets/Resources/canvas_loot.png", new Vector3(GameConstants.LOOT_CANVAS_SCALE_X, GameConstants.LOOT_CANVAS_SCALE_Y, GameConstants.LOOT_CANVAS_SCALE_Z));
        
        // Metal (gray) - using custom sprite
        lootPrefabs[3] = CreateSingleLootPrefab("Loot_Metal", new Color(0.7f, 0.7f, 0.7f), LootType.Metal, "Assets/Resources/metal_loot.png", new Vector3(GameConstants.LOOT_METAL_SCALE_X, GameConstants.LOOT_METAL_SCALE_Y, GameConstants.LOOT_METAL_SCALE_Z));
        
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
        sr.sortingOrder = 2;
        
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

        // Add health bar component
        enemy.AddComponent<EnemyHealthBar>();

        // Save as prefab
        string path = $"Assets/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        DestroyImmediate(enemy);

        Debug.Log($"✓ {prefabName} prefab created");
        return prefab;
    }

    static GameObject CreateEnemyProjectilePrefab()
    {
        GameObject proj = new GameObject("EnemyProjectile");
        proj.transform.localScale = new Vector3(GameConstants.PROJECTILE_SCALE, GameConstants.PROJECTILE_SCALE, GameConstants.PROJECTILE_SCALE);

        SpriteRenderer sr = proj.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 0.3f, 0.3f); // red cannonball
        sr.sortingOrder = 5;

        Rigidbody2D rb = proj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        proj.AddComponent<EnemyProjectile>();

        string path = "Assets/EnemyProjectile.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(proj, path);
        DestroyImmediate(proj);

        Debug.Log("✓ Enemy projectile prefab created");
        return prefab;
    }

    static GameObject CreateEnemyShipPrefab(GameObject[] lootPrefabs, GameObject enemyProjectilePrefab)
    {
        GameObject ship = new GameObject("Enemy_Ship");
        SpriteRenderer sr = ship.AddComponent<SpriteRenderer>();

        // Use player's Ship.png tinted dark
        Sprite shipSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Ship.png");
        if (shipSprite != null)
        {
            sr.sprite = shipSprite;
            sr.color = new Color(0.15f, 0.15f, 0.15f); // dark tint
        }
        else
        {
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.15f, 0.15f, 0.15f);
            Debug.LogWarning("Ship.png not found for enemy ship, using fallback");
        }
        sr.sortingOrder = 2;

        ship.transform.localScale = new Vector3(
            GameConstants.ENEMY_SHIP_SPAWNED_SCALE_X,
            GameConstants.ENEMY_SHIP_SPAWNED_SCALE_Y,
            GameConstants.ENEMY_SHIP_SPAWNED_SCALE_Z);
        ship.tag = "Enemy";

        Rigidbody2D rb = ship.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BoxCollider2D col = ship.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        EnemyShipController esc = ship.AddComponent<EnemyShipController>();
        esc.projectilePrefab = enemyProjectilePrefab;
        esc.lootPrefabs = lootPrefabs;

        ship.AddComponent<EnemyHealthBar>();

        string path = "Assets/Enemy_Ship.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(ship, path);
        DestroyImmediate(ship);

        Debug.Log("✓ Enemy ship prefab created");
        return prefab;
    }

    static void CreateEnemySpawner(GameObject crabPrefab, GameObject harpyPrefab, GameObject mermaidPrefab, GameObject enemyShipPrefab)
    {
        GameObject spawner = new GameObject("EnemySpawner");
        EnemySpawner es = spawner.AddComponent<EnemySpawner>();
        es.crabEnemyPrefab = crabPrefab;
        es.harpyEnemyPrefab = harpyPrefab;
        es.mermaidEnemyPrefab = mermaidPrefab;
        es.enemyShipPrefab = enemyShipPrefab;
        es.spawnInterval = GameConstants.SPAWN_INTERVAL;
        es.minSpawnInterval = GameConstants.MIN_SPAWN_INTERVAL;
        es.spawnDistance = GameConstants.ENEMY_SPAWN_DISTANCE;

        Debug.Log("✓ Enemy spawner created with 4 enemy types");
    }

    static void CreateWaveManager(GameObject crabPrefab, GameObject harpyPrefab, GameObject mermaidPrefab, GameObject enemyShipPrefab)
    {
        GameObject wmObj = new GameObject("WaveManager");
        WaveManager wm = wmObj.AddComponent<WaveManager>();
        EnemySpawner spawner = GameObject.FindFirstObjectByType<EnemySpawner>();
        wm.spawner = spawner;

        // Ensure spawner knows prefabs
        if (spawner != null)
        {
            spawner.crabEnemyPrefab = crabPrefab;
            spawner.harpyEnemyPrefab = harpyPrefab;
            spawner.mermaidEnemyPrefab = mermaidPrefab;
            spawner.enemyShipPrefab = enemyShipPrefab;
        }

        Debug.Log("✓ Wave manager created");
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

        // Wave Text (top center)
        GameObject waveTextObj = new GameObject("WaveText");
        waveTextObj.transform.SetParent(canvasObj.transform);
        TextMeshProUGUI waveText = waveTextObj.AddComponent<TextMeshProUGUI>();
        waveText.text = "";
        waveText.fontSize = 32;
        waveText.color = Color.white;
        waveText.alignment = TextAlignmentOptions.Top;
        
        RectTransform waveRect = waveTextObj.GetComponent<RectTransform>();
        waveRect.anchorMin = new Vector2(0.5f, 1);
        waveRect.anchorMax = new Vector2(0.5f, 1);
        waveRect.pivot = new Vector2(0.5f, 1);
        waveRect.anchoredPosition = new Vector2(0, -10);
        waveRect.sizeDelta = new Vector2(300, 60);

        // Create Loot Counter Panel (top-right, below kill count)
        GameObject lootPanelObj = new GameObject("LootPanel");
        lootPanelObj.transform.SetParent(canvasObj.transform);
        RectTransform lootPanelRect = lootPanelObj.AddComponent<RectTransform>();
        lootPanelRect.anchorMin = new Vector2(1, 1);
        lootPanelRect.anchorMax = new Vector2(1, 1);
        lootPanelRect.pivot = new Vector2(1, 1);
        lootPanelRect.anchoredPosition = new Vector2(-10, -60);
        lootPanelRect.sizeDelta = new Vector2(520, 40);

        // Create horizontal layout
        HorizontalLayoutGroup layoutGroup = lootPanelObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.childAlignment = TextAnchor.MiddleRight;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // Create loot counters (Gold, Wood, Canvas, Metal)
        TextMeshProUGUI goldText = CreateLootCounter(lootPanelObj.transform, "Assets/Resources/gold_loot.png", "GoldCounter");
        TextMeshProUGUI woodText = CreateLootCounter(lootPanelObj.transform, "Assets/Resources/wood_loot.png", "WoodCounter");
        TextMeshProUGUI canvasText = CreateLootCounter(lootPanelObj.transform, "Assets/Resources/canvas_loot.png", "CanvasCounter");
        TextMeshProUGUI metalText = CreateLootCounter(lootPanelObj.transform, "Assets/Resources/metal_loot.png", "MetalCounter");

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
        uiManager.waveText = waveText;

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
        containerRect.sizeDelta = new Vector2(125, 40);

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
        iconRect.sizeDelta = new Vector2(20, 20);
        icon.preserveAspect = true;

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
        textRect.anchoredPosition = new Vector2(22, 0);
        textRect.sizeDelta = new Vector2(100, 40);
        text.overflowMode = TextOverflowModes.Overflow;
        text.textWrappingMode = TextWrappingModes.NoWrap;

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
        string assetPath = "Assets/Resources/Ship.png";
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

    // ────────────────────────────────────────────
    //  WORLD LOCATIONS
    // ────────────────────────────────────────────

    static void CreateLocationManager()
    {
        GameObject obj = new GameObject("LocationManager");
        obj.AddComponent<LocationManager>();
        Debug.Log("✓ LocationManager created");
    }

    static void CreateAllLocations()
    {
        // Base port — home harbor with shop (near center-ish, player starts at 0,0)
        CreatePortLocation("base_port", "Safe Harbor",
            new Vector2(5f, 5f), true, true,
            "Assets/Resources/Port_1.png");

        // Trader's Cove — second port with shop (north-west)
        CreatePortLocation("traders_cove", "Trader's Cove",
            new Vector2(-30f, 25f), true, false,
            null); // placeholder

        // Naval Outpost — third port, no shop (south-east)
        CreatePortLocation("naval_outpost", "Naval Outpost",
            new Vector2(30f, -20f), false, false,
            null); // placeholder

        // Secret Island — side quest (far east, hidden)
        CreateIslandLocation("secret_island", "Forgotten Isle",
            new Vector2(40f, 10f), false,
            "Assets/Resources/island_large.png");

        // Boss Arena — final encounter (far north)
        CreateBossArenaLocation("boss_arena", "The Maelstrom",
            new Vector2(0f, 42f), false);

        Debug.Log("✓ All 5 world locations created");
    }

    static void CreatePortLocation(string id, string displayName, Vector2 pos,
        bool hasShop, bool startDiscovered, string spritePath)
    {
        GameObject obj = new GameObject("Loc_" + id);
        obj.transform.position = new Vector3(pos.x, pos.y, 0f);

        // Sprite
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        float portScale = 0.1f;
        Sprite sprite = null;
        if (!string.IsNullOrEmpty(spritePath))
        {
            EnsureEnemySpriteImported(spritePath);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }
        if (sprite != null)
        {
            sr.sprite = sprite;
            sr.color = Color.white;
        }
        else
        {
            // Placeholder: colored circle
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = hasShop ? new Color(0.3f, 0.8f, 0.4f, 0.8f)
                               : new Color(0.4f, 0.6f, 0.9f, 0.8f);
            portScale = 30f; // large placeholder (Knob.psd is natively tiny)
        }
        sr.sortingOrder = 0;
        obj.transform.localScale = Vector3.one * portScale;

        // Collider
        BoxCollider2D bc = obj.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        if (sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            bc.size = spriteSize;
        }

        // Location component
        Location loc = obj.AddComponent<Location>();
        loc.locationId = id;
        loc.displayName = displayName;
        loc.locationType = Location.LocationType.Port;
        loc.hasShop = hasShop;
        loc.discovered = startDiscovered;
        loc.worldPosition = pos;

        // Port zone (pause, shop, waves)
        PortZone pz = obj.AddComponent<PortZone>();
        pz.portName = displayName;
        pz.welcomeMessageDuration = 3f;
        pz.minEnterTime = 0.75f;
        pz.location = loc;
    }

    static void CreateIslandLocation(string id, string displayName, Vector2 pos,
        bool startDiscovered, string spritePath)
    {
        GameObject obj = new GameObject("Loc_" + id);
        obj.transform.position = new Vector3(pos.x, pos.y, 0f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        float scale = 0.05f;
        Sprite sprite = null;
        if (!string.IsNullOrEmpty(spritePath))
        {
            EnsureEnemySpriteImported(spritePath);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }
        if (sprite != null)
        {
            sr.sprite = sprite;
            sr.color = Color.white;
        }
        else
        {
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(0.6f, 0.5f, 0.2f, 0.8f); // sandy
            scale = 30f;
        }
        sr.sortingOrder = 0;
        obj.transform.localScale = Vector3.one * scale;

        // Discovery trigger (larger invisible collider)
        CircleCollider2D cc = obj.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 3f / scale; // ~3 world units

        Location loc = obj.AddComponent<Location>();
        loc.locationId = id;
        loc.displayName = displayName;
        loc.locationType = Location.LocationType.Island;
        loc.hasShop = false;
        loc.discovered = startDiscovered;
        loc.worldPosition = pos;
    }

    static void CreateBossArenaLocation(string id, string displayName, Vector2 pos,
        bool startDiscovered)
    {
        GameObject obj = new GameObject("Loc_" + id);
        obj.transform.position = new Vector3(pos.x, pos.y, 0f);

        // Placeholder — swirling vortex
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.8f, 0.2f, 0.3f, 0.7f); // ominous red
        sr.sortingOrder = 0;
        float scale = 25f;
        obj.transform.localScale = Vector3.one * scale;

        CircleCollider2D cc = obj.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 4f / scale; // ~4 world units

        Location loc = obj.AddComponent<Location>();
        loc.locationId = id;
        loc.displayName = displayName;
        loc.locationType = Location.LocationType.BossArena;
        loc.hasShop = false;
        loc.discovered = startDiscovered;
        loc.worldPosition = pos;
    }

    // ────────────────────────────────────────────
    //  COMPASS / MINIMAP UI
    // ────────────────────────────────────────────

    static void CreateQuestManager()
    {
        GameObject obj = new GameObject("QuestManager");
        obj.AddComponent<QuestManager>();
        Debug.Log("✓ QuestManager created");
    }

    static void CreateDialogueUI()
    {
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("CreateDialogueUI: no Canvas found"); return; }
        Transform canvasT = canvas.transform;

        // ── Dark panel along the bottom ──
        GameObject panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(canvasT, false);
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);
        panelBg.raycastTarget = true; // block clicks through
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0, 140);

        // Speaker name (top-left of panel, gold color)
        GameObject speakerObj = new GameObject("SpeakerText");
        speakerObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI speakerTmp = speakerObj.AddComponent<TextMeshProUGUI>();
        speakerTmp.text = "";
        speakerTmp.fontSize = 18;
        speakerTmp.fontStyle = FontStyles.Bold;
        speakerTmp.color = new Color(0.9f, 0.75f, 0.3f);
        speakerTmp.alignment = TextAlignmentOptions.TopLeft;
        speakerTmp.raycastTarget = false;
        RectTransform speakerRect = speakerObj.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0, 1);
        speakerRect.anchorMax = new Vector2(1, 1);
        speakerRect.pivot = new Vector2(0, 1);
        speakerRect.anchoredPosition = new Vector2(20, -8);
        speakerRect.sizeDelta = new Vector2(-40, 24);

        // Body text
        GameObject bodyObj = new GameObject("BodyText");
        bodyObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI bodyTmp = bodyObj.AddComponent<TextMeshProUGUI>();
        bodyTmp.text = "";
        bodyTmp.fontSize = 16;
        bodyTmp.color = Color.white;
        bodyTmp.alignment = TextAlignmentOptions.TopLeft;
        bodyTmp.raycastTarget = false;
        RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0, 0);
        bodyRect.anchorMax = new Vector2(1, 1);
        bodyRect.offsetMin = new Vector2(20, 24);
        bodyRect.offsetMax = new Vector2(-20, -36);

        // Continue hint (bottom-right)
        GameObject hintObj = new GameObject("ContinueHint");
        hintObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI hintTmp = hintObj.AddComponent<TextMeshProUGUI>();
        hintTmp.text = "[Click to continue]";
        hintTmp.fontSize = 12;
        hintTmp.color = new Color(0.6f, 0.6f, 0.6f);
        hintTmp.alignment = TextAlignmentOptions.BottomRight;
        hintTmp.raycastTarget = false;
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0, 0);
        hintRect.anchorMax = new Vector2(1, 0);
        hintRect.pivot = new Vector2(1, 0);
        hintRect.anchoredPosition = new Vector2(-16, 6);
        hintRect.sizeDelta = new Vector2(-32, 20);

        panelObj.SetActive(false); // hidden by default

        // DialogueUI component on its own object
        GameObject dialogueObj = new GameObject("DialogueUI");
        DialogueUI dialogueUI = dialogueObj.AddComponent<DialogueUI>();
        dialogueUI.dialoguePanel = panelObj;
        dialogueUI.speakerText = speakerTmp;
        dialogueUI.bodyText = bodyTmp;
        dialogueUI.continueHint = hintTmp;

        Debug.Log("✓ Dialogue UI created");
    }

    static void CreateQuestTrackerUI()
    {
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        Transform canvasT = canvas.transform;

        // Small panel below health text (top-left)
        GameObject panelObj = new GameObject("QuestTrackerPanel");
        panelObj.transform.SetParent(canvasT, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.15f, 0.7f);
        bg.raycastTarget = false;
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -65);
        panelRect.sizeDelta = new Vector2(260, 50);

        // Quest title
        GameObject titleObj = new GameObject("QuestTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "";
        titleTmp.fontSize = 13;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(0.9f, 0.75f, 0.3f);
        titleTmp.alignment = TextAlignmentOptions.TopLeft;
        titleTmp.raycastTarget = false;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.5f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(8, 0);
        titleRect.offsetMax = new Vector2(-8, -4);

        // Objective text
        GameObject objObj = new GameObject("ObjectiveText");
        objObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI objTmp = objObj.AddComponent<TextMeshProUGUI>();
        objTmp.text = "";
        objTmp.fontSize = 11;
        objTmp.color = Color.white;
        objTmp.alignment = TextAlignmentOptions.TopLeft;
        objTmp.raycastTarget = false;
        RectTransform objRect = objObj.GetComponent<RectTransform>();
        objRect.anchorMin = new Vector2(0, 0);
        objRect.anchorMax = new Vector2(1, 0.5f);
        objRect.offsetMin = new Vector2(8, 4);
        objRect.offsetMax = new Vector2(-8, 0);

        // QuestTrackerUI component
        GameObject trackerObj = new GameObject("QuestTrackerUI");
        QuestTrackerUI tracker = trackerObj.AddComponent<QuestTrackerUI>();
        tracker.trackerPanel = panelObj;
        tracker.questTitleText = titleTmp;
        tracker.objectiveText = objTmp;

        Debug.Log("✓ Quest Tracker UI created");
    }

    static void CreateCompassUI()
    {
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("CreateCompassUI: no Canvas found"); return; }
        Transform canvasT = canvas.transform;

        // ── Minimap panel (bottom-left) ──────────────
        GameObject panelObj = new GameObject("MinimapPanel");
        panelObj.transform.SetParent(canvasT, false);
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.08f, 0.15f, 0.75f);
        panelBg.raycastTarget = false;
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 0);
        panelRect.pivot = new Vector2(0, 0);
        panelRect.anchoredPosition = new Vector2(10, 10);
        panelRect.sizeDelta = new Vector2(140, 140);

        // Border
        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.6f, 0.5f, 0.3f, 0.9f); // gold-ish
        outline.effectDistance = new Vector2(2, 2);

        // ── Player dot (white) ───────────────────────
        GameObject playerDotObj = new GameObject("PlayerDot");
        playerDotObj.transform.SetParent(panelObj.transform, false);
        Image playerDotImg = playerDotObj.AddComponent<Image>();
        playerDotImg.color = Color.white;
        playerDotImg.raycastTarget = false;
        RectTransform playerDotRect = playerDotObj.GetComponent<RectTransform>();
        playerDotRect.sizeDelta = new Vector2(6, 6);

        // ── Edge arrow container (full screen overlay) ─
        GameObject arrowContainerObj = new GameObject("EdgeArrowContainer");
        arrowContainerObj.transform.SetParent(canvasT, false);
        RectTransform arrowRect = arrowContainerObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = Vector2.zero;
        arrowRect.anchorMax = Vector2.one;
        arrowRect.sizeDelta = Vector2.zero;
        arrowRect.offsetMin = Vector2.zero;
        arrowRect.offsetMax = Vector2.zero;

        // ── Title label ──────────────────────────────
        GameObject titleObj = new GameObject("MinimapTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "MAP";
        titleTmp.fontSize = 10;
        titleTmp.color = new Color(0.8f, 0.7f, 0.5f);
        titleTmp.alignment = TextAlignmentOptions.Top;
        titleTmp.raycastTarget = false;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -2);
        titleRect.sizeDelta = new Vector2(0, 14);

        // ── CompassUI component ──────────────────────
        GameObject compassObj = new GameObject("CompassUI");
        CompassUI compass = compassObj.AddComponent<CompassUI>();
        compass.minimapPanel = panelRect;
        compass.playerDot = playerDotRect;
        compass.arrowContainer = arrowRect;

        Debug.Log("✓ Compass / Minimap UI created");
    }

    /// <summary>
    /// Creates ShopManager and a tabbed Shop UI under the Canvas
    /// </summary>
    static void CreateShopSystem()
    {
        // Manager
        GameObject shopMgrObj = new GameObject("ShopManager");
        shopMgrObj.AddComponent<ShopManager>();

        // Ensure Sloop sprite is imported as Sprite type
        EnsureEnemySpriteImported("Assets/Resources/Sloop.png");
        EnsureEnemySpriteImported("Assets/Resources/Sloop.png");

        // Find Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogWarning("Canvas not found; Shop UI will not be created.");
            return;
        }

        // Create ShopUI root
        GameObject shopUIObj = new GameObject("ShopUI");
        ShopUI shopUI = shopUIObj.AddComponent<ShopUI>();

        // ------- Main Panel -------
        GameObject panel = new GameObject("ShopPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(860, 500);
        panel.SetActive(false);

        // ------- Left Tab Strip (bookmarks) -------
        GameObject tabStrip = new GameObject("TabStrip");
        tabStrip.transform.SetParent(panel.transform, false);
        RectTransform tabStripRect = tabStrip.AddComponent<RectTransform>();
        tabStripRect.anchorMin = new Vector2(0f, 0f);
        tabStripRect.anchorMax = new Vector2(0f, 1f);
        tabStripRect.pivot = new Vector2(0f, 0.5f);
        tabStripRect.anchoredPosition = Vector2.zero;
        tabStripRect.sizeDelta = new Vector2(140, 0);
        Image tabStripBg = tabStrip.AddComponent<Image>();
        tabStripBg.color = new Color(0.12f, 0.12f, 0.16f, 1f);

        // Tab buttons
        Button btnShips = CreateTabButton(tabStrip.transform, "TabShips", "Ships", 0);
        Button btnEnhance = CreateTabButton(tabStrip.transform, "TabEnhancements", "Enhance", 1);
        Button btnCrew = CreateTabButton(tabStrip.transform, "TabCrew", "Crew", 2);

        // ------- Right Content Area -------
        // Header bar (title + gold + close)
        GameObject header = new GameObject("Header");
        header.transform.SetParent(panel.transform, false);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(70, 0); // offset for tab strip
        headerRect.sizeDelta = new Vector2(-140, 50);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "Ships";
        title.fontSize = 28;
        title.color = Color.white;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(16, 0);
        titleRect.offsetMax = Vector2.zero;

        // Gold text
        GameObject goldObj = new GameObject("GoldText");
        goldObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI goldText = goldObj.AddComponent<TextMeshProUGUI>();
        goldText.text = "Gold: 0";
        goldText.fontSize = 20;
        goldText.color = Color.yellow;
        goldText.alignment = TextAlignmentOptions.MidlineRight;
        RectTransform goldRect = goldObj.GetComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0.5f, 0f);
        goldRect.anchorMax = new Vector2(0.85f, 1f);
        goldRect.offsetMin = Vector2.zero;
        goldRect.offsetMax = new Vector2(-8, 0);

        // Close button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(header.transform, false);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.6f, 0.2f, 0.2f, 0.9f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 0.1f);
        closeRect.anchorMax = new Vector2(1f, 0.9f);
        closeRect.pivot = new Vector2(1f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-8, 0);
        closeRect.sizeDelta = new Vector2(70, 0);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "Close";
        closeText.fontSize = 18;
        closeText.color = Color.white;
        closeText.alignment = TextAlignmentOptions.Center;
        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;

        // Content grid (right of tab strip, below header)
        GameObject gridObj = new GameObject("ContentArea");
        gridObj.transform.SetParent(panel.transform, false);
        RectTransform gridRect = gridObj.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 0f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.offsetMin = new Vector2(150, 10);   // left padding past tabs
        gridRect.offsetMax = new Vector2(-10, -55);   // top padding for header
        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 220);
        grid.spacing = new Vector2(12, 12);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperLeft;

        // Wire ShopUI references
        shopUI.shopPanel = panel;
        shopUI.contentArea = gridObj.transform;
        shopUI.goldDisplayText = goldText;
        shopUI.titleText = title;
        shopUI.closeButton = closeBtn;
        shopUI.tabShips = btnShips;
        shopUI.tabEnhancements = btnEnhance;
        shopUI.tabCrew = btnCrew;

        Debug.Log("✓ Tabbed shop system created (Ships / Enhancements / Crew)");
    }

    static Button CreateTabButton(Transform parent, string name, string label, int index)
    {
        float tabHeight = 50f;
        float spacing = 6f;
        float topOffset = 60f; // leave room at top

        GameObject tabObj = new GameObject(name);
        tabObj.transform.SetParent(parent, false);
        Image tabImg = tabObj.AddComponent<Image>();
        tabImg.color = new Color(0.25f, 0.25f, 0.3f, 0.8f);
        Button btn = tabObj.AddComponent<Button>();
        RectTransform tabRect = tabObj.GetComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0f, 1f);
        tabRect.anchorMax = new Vector2(1f, 1f);
        tabRect.pivot = new Vector2(0.5f, 1f);
        tabRect.anchoredPosition = new Vector2(0, -(topOffset + index * (tabHeight + spacing)));
        tabRect.sizeDelta = new Vector2(-12, tabHeight);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(tabObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btn;
    }
}
