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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        InArena = false;
    }

    void Start()
    {
        BuildFadeOverlay();
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
        if (fadeOverlay != null) SetAlpha(0f);
    }

    // ─── BOSS SPAWN (placeholder for Phase D) ───

    GameObject SpawnBoss(string bossId, Vector2 center)
    {
        var go = new GameObject("Boss_" + bossId);
        go.transform.position = new Vector3(center.x, center.y + 4f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite sprite = Resources.Load<Sprite>("Galleon_Top"); // placeholder hull
        sr.sprite = sprite;
        sr.color = new Color(0.45f, 0.55f, 0.6f); // ghostly grey-blue
        sr.sortingOrder = 2;
        float h = sprite != null ? sprite.bounds.size.y : 1f;
        float scale = 3.5f / Mathf.Max(0.01f, h); // ~3.5 wu tall, boss-sized
        go.transform.localScale = new Vector3(scale, scale, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        if (sprite != null) { col.size = sprite.bounds.size * 0.7f; col.offset = sprite.bounds.center; }

        var hp = go.AddComponent<BossHealth>();
        hp.maxHealth = 60;
        hp.onDeath = OnBossDefeated;

        go.AddComponent<IdleSway>();
        return go;
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
