using UnityEngine;

/// <summary>
/// Data describing a world location (port, island, boss arena, etc.)
/// Attached to the location's GameObject at setup time.
/// </summary>
public class Location : MonoBehaviour
{
    public enum LocationType
    {
        Port,
        Island,
        BossArena
    }

    [Header("Identity")]
    public string locationId;
    public string displayName;
    public LocationType locationType;

    [Header("State")]
    public bool discovered = false;
    public bool hasShop = false;

    [Header("Position (set by editor)")]
    public Vector2 worldPosition;

    /// <summary>
    /// Mark this location as discovered (for compass/minimap)
    /// </summary>
    public void Discover()
    {
        if (!discovered)
        {
            discovered = true;
            Debug.Log($"Discovered: {displayName}");
        }
    }
}
