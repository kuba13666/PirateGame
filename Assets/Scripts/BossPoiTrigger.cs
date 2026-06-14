using UnityEngine;

/// <summary>
/// On a boss POI (Dutchman's Drift, White Island, the Maelstrom). When the
/// active quest's current objective is "DefeatBoss" for THIS location and the
/// player is near, it hands off to BossArenaManager to start the fight.
/// Proximity-based so it fires even though the player is already standing on
/// the POI when the preceding TravelTo objective completes.
/// </summary>
public class BossPoiTrigger : MonoBehaviour
{
    public string locationId;
    public float radius = 8f;

    private Transform player;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        // Fall back to the attached Location's id if not set explicitly.
        if (string.IsNullOrEmpty(locationId))
        {
            var loc = GetComponent<Location>();
            if (loc != null) locationId = loc.locationId;
        }
    }

    void Update()
    {
        if (player == null || BossArenaManager.InArena || BossArenaManager.Instance == null) return;

        var q = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest() : null;
        var obj = q != null ? q.GetCurrentObjective() : null;
        if (obj == null || obj.type != QuestObjective.ObjectiveType.DefeatBoss) return;
        if (obj.targetLocationId != locationId) return;

        if (Vector2.Distance(player.position, transform.position) > radius) return;

        BossArenaManager.Instance.EnterArena(obj.targetBossId, transform.position);
    }
}
