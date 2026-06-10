using UnityEngine;

/// <summary>
/// Lightweight trigger for story points-of-interest (shipwreck, fog bank,
/// uncharted island). Unlike PortZone it does NOT pause time, bank loot or
/// open the shop — it only discovers the location and reports it to the
/// quest system. Boss-arena entry (Phase D) will hook in here later.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PoiZone : MonoBehaviour
{
    [Header("Location")]
    public Location location;

    [Tooltip("Ignore entries during the first seconds of the scene")]
    public float minEnterTime = 0.5f;

    private bool reported;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (Time.timeSinceLevelLoad < minEnterTime) return;

        if (location != null)
        {
            location.Discover();

            if (QuestManager.Instance != null)
                QuestManager.Instance.ReportLocationReached(location.locationId);
        }

        // Re-reporting is harmless (quest objectives ignore non-matching ids),
        // but only log the first visit.
        if (!reported)
        {
            reported = true;
            Debug.Log($"POI reached: {(location != null ? location.displayName : name)}");
        }
    }
}
