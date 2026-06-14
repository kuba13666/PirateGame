using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dev/playtest helpers (editor + builds). Keys:
///   P — toggle "peace mode": stop all enemy spawning and clear current enemies.
///   K — kill all enemies currently on screen (one-shot).
/// </summary>
public class DebugControls : MonoBehaviour
{
    void Awake()
    {
        // Statics persist across editor play sessions — start each run unpaused.
        ZoneSpawnManager.SpawningDisabled = false;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.pKey.wasPressedThisFrame)
        {
            ZoneSpawnManager.SpawningDisabled = !ZoneSpawnManager.SpawningDisabled;
            if (ZoneSpawnManager.SpawningDisabled) DespawnAll();
            Debug.Log($"[DEBUG] Peace mode: {(ZoneSpawnManager.SpawningDisabled ? "ON" : "OFF")}");
        }

        if (kb.kKey.wasPressedThisFrame)
            DespawnAll();
    }

    static void DespawnAll()
    {
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(e);
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
        style.normal.textColor = ZoneSpawnManager.SpawningDisabled ? Color.yellow : new Color(1f, 1f, 1f, 0.5f);
        string txt = ZoneSpawnManager.SpawningDisabled
            ? "PEACE MODE — no spawns  (P toggle · K kill all)"
            : "P peace · K kill all";
        GUI.Label(new Rect(10, Screen.height - 26, 500, 22), txt, style);
    }
}
