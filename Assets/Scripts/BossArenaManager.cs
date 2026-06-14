using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runs boss encounters in off-map ocean pockets (see Docs/BIOME1_DESIGN.md §4):
/// sailing into a boss POI fades the screen, teleports the ship + camera into a
/// far-off walled arena, confines movement, pauses ambient spawning, and spawns
/// the boss. On victory it reports the kill, fades, and returns the ship beside
/// the POI. Death routes through the normal respawn-at-home flow.
///
/// Phase D ships a placeholder boss so the whole loop is verifiable; Phase E
/// swaps the real Flying Dutchman / Mocha Dick fights into SpawnBoss().
/// </summary>
public class BossArenaManager : MonoBehaviour
{
    public static BossArenaManager Instance { get; private set; }
    public static bool InArena { get; private set; }

    [System.Serializable]
    public class ArenaDef
    {
        public string bossId;
        public Vector2 center;
        public float half = 28f;
    }

    // Each boss gets its own far-off-map pocket.
    private readonly List<ArenaDef> arenas = new List<ArenaDef>
    {
        new ArenaDef { bossId = "flying_dutchman", center = new Vector2(400f, 0f) },
        new ArenaDef { bossId = "mocha_dick",      center = new Vector2(500f, 0f) },
        new ArenaDef { bossId = "kraken",          center = new Vector2(600f, 0f) },
    };

    private const float FADE_TIME = 0.45f;

    private Image fadeOverlay;
    private GameObject currentBoss;
    private ArenaDef currentArena;
    private Vector3 returnPos;
    private bool transitioning;

    // Boss health bar
    private GameObject bossBar;
    private Image bossBarFill;
    private TMPro.TextMeshProUGUI bossBarName;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        InArena = false;
    }

    void Start()
    {
        BuildFadeOverlay();
        BuildBossBar();
        foreach (var a in arenas) BuildArenaWalls(a);
    }

    // ─── ENTRY ──────────────────────────────────

    /// <summary>Begin a boss fight. returnPosition is where the ship reappears on victory.</summary>
    public void EnterArena(string bossId, Vector3 returnPosition)
    {
        if (InArena || transitioning) return;
        ArenaDef arena = arenas.Find(a => a.bossId == bossId);
        if (arena == null) { Debug.LogWarning($"No arena for boss '{bossId}'"); return; }

        currentArena = arena;
        returnPos = returnPosition;
        InArena = true; // immediate: pauses ambient spawning and blocks re-entry
        StartCoroutine(EnterRoutine(arena));
    }

    IEnumerator EnterRoutine(ArenaDef arena)
    {
        transitioning = true;
        yield return Fade(0f, 1f);

        var player = GameObject.FindGameObjectWithTag("Player");
        var pc = player != null ? player.GetComponent<PlayerController>() : null;
        if (player != null)
        {
            player.transform.position = new Vector3(arena.center.x, arena.center.y - arena.half * 0.5f, 0f);
            if (pc != null)
            {
                pc.minX = arena.center.x - arena.half;
                pc.maxX = arena.center.x + arena.half;
                pc.minY = arena.center.y - arena.half;
                pc.maxY = arena.center.y + arena.half;
                pc.StopMoving();
            }
        }
        SnapCamera(arena.center);

        // Clear any stray enemies, then spawn the boss
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(e);
        currentBoss = SpawnBoss(currentArena.bossId, arena.center);

        var bhp = currentBoss != null ? currentBoss.GetComponent<BossHealth>() : null;
        if (bhp != null)
        {
            ShowBossBar(DisplayName(currentArena.bossId), bhp.maxHealth);
            bhp.onHealthChanged += UpdateBossBar;
        }

        yield return Fade(1f, 0f);
        transitioning = false;
    }

    // ─── EXIT (victory) ─────────────────────────

    void OnBossDefeated()
    {
        if (!InArena || transitioning) return;
        string bossId = currentArena != null ? currentArena.bossId : null;
        StartCoroutine(ExitRoutine(bossId));
    }

    IEnumerator ExitRoutine(string bossId)
    {
        transitioning = true;
        yield return Fade(0f, 1f);

        // Back to the open sea beside the POI
        var player = GameObject.FindGameObjectWithTag("Player");
        var pc = player != null ? player.GetComponent<PlayerController>() : null;
        if (player != null)
        {
            player.transform.position = returnPos + new Vector3(0f, -4f, 0f);
            RestoreMapBounds(pc);
            if (pc != null) pc.StopMoving();
        }
        SnapCamera(returnPos);

        InArena = false;
        currentArena = null;
        if (currentBoss != null) Destroy(currentBoss);
        HideBossBar();

        yield return Fade(1f, 0f);
        transitioning = false;

        // Now that we're back on the map, advance the quest (shows reward dialogue)
        if (!string.IsNullOrEmpty(bossId) && QuestManager.Instance != null)
            QuestManager.Instance.ReportBossDefeated(bossId);
    }

    /// <summary>
    /// Called by GameManager.OnPlayerDeath: bail out of the arena cleanly so the
    /// normal respawn-at-home flow (which restores map bounds) takes over.
    /// </summary>
    public void AbortArena()
    {
        if (!InArena && !transitioning) return;
        StopAllCoroutines();
        InArena = false;
        currentArena = null;
        transitioning = false;
        if (currentBoss != null) Destroy(currentBoss);
        HideBossBar();
        if (fadeOverlay != null) SetAlpha(0f);
    }

    // ─── BOSS HEALTH BAR ────────────────────────

    void BuildBossBar()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        bossBar = new GameObject("BossBar");
        bossBar.transform.SetParent(canvas.transform, false);
        var rt = bossBar.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -12f);
        rt.sizeDelta = new Vector2(520f, 40f);

        bossBarName = new GameObject("Name").AddComponent<TMPro.TextMeshProUGUI>();
        bossBarName.transform.SetParent(bossBar.transform, false);
        bossBarName.fontSize = 20; bossBarName.alignment = TMPro.TextAlignmentOptions.Center;
        bossBarName.color = new Color(0.95f, 0.9f, 0.8f);
        var nrt = bossBarName.rectTransform;
        nrt.anchorMin = new Vector2(0, 1); nrt.anchorMax = new Vector2(1, 1); nrt.pivot = new Vector2(0.5f, 1f);
        nrt.offsetMin = new Vector2(0, -22); nrt.offsetMax = new Vector2(0, 0);

        var back = new GameObject("BarBack").AddComponent<Image>();
        back.transform.SetParent(bossBar.transform, false);
        back.color = new Color(0.1f, 0.05f, 0.05f, 0.9f);
        var brt = back.rectTransform;
        brt.anchorMin = new Vector2(0, 0); brt.anchorMax = new Vector2(1, 0); brt.pivot = new Vector2(0.5f, 0f);
        brt.sizeDelta = new Vector2(0, 14); brt.anchoredPosition = new Vector2(0, 0);

        bossBarFill = new GameObject("BarFill").AddComponent<Image>();
        bossBarFill.transform.SetParent(back.transform, false);
        bossBarFill.color = new Color(0.8f, 0.15f, 0.2f);
        bossBarFill.type = Image.Type.Filled;
        bossBarFill.fillMethod = Image.FillMethod.Horizontal;
        bossBarFill.fillOrigin = 0;
        var frt = bossBarFill.rectTransform;
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(2, 2); frt.offsetMax = new Vector2(-2, -2);

        bossBar.SetActive(false);
    }

    void ShowBossBar(string name, int max)
    {
        if (bossBar == null) return;
        bossBar.SetActive(true);
        if (bossBarName != null) bossBarName.text = name;
        if (bossBarFill != null) bossBarFill.fillAmount = 1f;
    }

    void UpdateBossBar(int current, int max)
    {
        if (bossBarFill != null) bossBarFill.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    void HideBossBar()
    {
        if (bossBar != null) bossBar.SetActive(false);
    }

    // ─── BOSS SPAWN (placeholder for Phase D) ───

    GameObject SpawnBoss(string bossId, Vector2 center)
    {
        switch (bossId)
        {
            case "flying_dutchman": return SpawnFlyingDutchman(center);
            default: return SpawnPlaceholder(bossId, center);
        }
    }

    GameObject SpawnFlyingDutchman(Vector2 center)
    {
        var go = new GameObject("Boss_flying_dutchman");
        go.transform.position = new Vector3(center.x, center.y + 6f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite ghost = Resources.Load<Sprite>("Dutchman"); // generated spectral ship, if present
        if (ghost != null) { sr.sprite = ghost; sr.color = new Color(1f, 1f, 1f, 0.9f); }
        else { sr.sprite = Resources.Load<Sprite>("Galleon_Top"); sr.color = new Color(0.45f, 0.95f, 0.75f, 0.85f); } // spectral tint fallback
        sr.sortingOrder = 2;
        float h = sr.sprite != null ? sr.sprite.bounds.size.y : 1f;
        float scale = 3.8f / Mathf.Max(0.01f, h);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        // Kinematic body + trigger: projectiles hit it, and it can ram the player
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (sr.sprite != null) { col.size = sr.sprite.bounds.size * 0.85f; col.offset = sr.sprite.bounds.center; }

        var hp = go.AddComponent<BossHealth>();
        hp.Init(45);
        hp.onDeath = OnBossDefeated;

        var fd = go.AddComponent<FlyingDutchman>();
        var spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            if (spawner.enemyShipPrefab != null)
            {
                var esc = spawner.enemyShipPrefab.GetComponent<EnemyShipController>();
                if (esc != null) fd.projectilePrefab = esc.projectilePrefab;
            }
            fd.addPrefabs = new[] { spawner.crabEnemyPrefab, spawner.harpyEnemyPrefab, spawner.mermaidEnemyPrefab };
        }
        return go;
    }

    GameObject SpawnPlaceholder(string bossId, Vector2 center)
    {
        var go = new GameObject("Boss_" + bossId);
        go.transform.position = new Vector3(center.x, center.y + 4f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite sprite = Resources.Load<Sprite>("Galleon_Top");
        sr.sprite = sprite;
        sr.color = new Color(0.45f, 0.55f, 0.6f);
        sr.sortingOrder = 2;
        float h = sprite != null ? sprite.bounds.size.y : 1f;
        go.transform.localScale = Vector3.one * (3.5f / Mathf.Max(0.01f, h));

        var col = go.AddComponent<BoxCollider2D>();
        if (sprite != null) { col.size = sprite.bounds.size * 0.7f; col.offset = sprite.bounds.center; }

        var hp = go.AddComponent<BossHealth>();
        hp.Init(60);
        hp.onDeath = OnBossDefeated;

        go.AddComponent<IdleSway>();
        return go;
    }

    static string DisplayName(string bossId)
    {
        switch (bossId)
        {
            case "flying_dutchman": return "The Flying Dutchman";
            case "mocha_dick": return "Mocha Dick, the White Island";
            case "kraken": return "The Kraken";
            default: return bossId;
        }
    }

    // ─── HELPERS ────────────────────────────────

    void SnapCamera(Vector2 pos)
    {
        if (Camera.main == null) return;
        var t = Camera.main.transform;
        t.position = new Vector3(pos.x, pos.y, t.position.z);
    }

    static void RestoreMapBounds(PlayerController pc)
    {
        if (pc == null) return;
        pc.minX = GameConstants.MAP_MIN_X; pc.maxX = GameConstants.MAP_MAX_X;
        pc.minY = GameConstants.MAP_MIN_Y; pc.maxY = GameConstants.MAP_MAX_Y;
    }

    void BuildArenaWalls(ArenaDef a)
    {
        Sprite rock = Resources.Load<Sprite>("Rock_0");
        var ring = new GameObject($"Arena_{a.bossId}").transform;
        ring.SetParent(transform);
        int count = 28;
        for (int i = 0; i < count; i++)
        {
            float ang = (i / (float)count) * Mathf.PI * 2f;
            var go = new GameObject("Wall");
            go.transform.SetParent(ring);
            go.transform.position = new Vector3(
                a.center.x + Mathf.Cos(ang) * (a.half + 1.5f),
                a.center.y + Mathf.Sin(ang) * (a.half + 1.5f), 0f);
            go.transform.rotation = Quaternion.Euler(0, 0, (i * 53f) % 360f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = rock;
            sr.sortingOrder = -10;
            if (rock != null)
            {
                float s = 2.6f / Mathf.Max(0.01f, rock.bounds.size.x);
                go.transform.localScale = new Vector3(s, s, 1f);
            }
        }
    }

    // ─── FADE ───────────────────────────────────

    void BuildFadeOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        var go = new GameObject("ArenaFade");
        go.transform.SetParent(canvas.transform, false);
        fadeOverlay = go.AddComponent<Image>();
        fadeOverlay.color = new Color(0f, 0f, 0f, 0f);
        fadeOverlay.raycastTarget = false;
        var rt = fadeOverlay.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.transform.SetAsLastSibling();
    }

    void SetAlpha(float a)
    {
        if (fadeOverlay == null) return;
        var c = fadeOverlay.color; c.a = a; fadeOverlay.color = c;
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeOverlay == null) yield break;
        fadeOverlay.transform.SetAsLastSibling();
        float t = 0f;
        while (t < FADE_TIME)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / FADE_TIME));
            yield return null;
        }
        SetAlpha(to);
    }
}
