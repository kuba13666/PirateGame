using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central registry of all world locations.
/// Provides lookup by id and tracks discovery state.
/// </summary>
public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    private readonly Dictionary<string, Location> locations = new Dictionary<string, Location>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-register any Location components already in the scene
        foreach (var loc in FindObjectsByType<Location>(FindObjectsSortMode.None))
            Register(loc);
    }

    public void Register(Location loc)
    {
        if (!locations.ContainsKey(loc.locationId))
            locations[loc.locationId] = loc;
    }

    public Location Get(string id)
    {
        locations.TryGetValue(id, out Location loc);
        return loc;
    }

    public List<Location> GetDiscovered()
    {
        var result = new List<Location>();
        foreach (var loc in locations.Values)
            if (loc.discovered) result.Add(loc);
        return result;
    }

    public List<Location> GetAll()
    {
        return new List<Location>(locations.Values);
    }
}
