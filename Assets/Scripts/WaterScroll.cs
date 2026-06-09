using UnityEngine;

/// <summary>
/// Makes a large tiled water sprite appear to flow. The sprite is bigger than
/// the camera view by a few tiles; each frame we follow the camera and add a
/// scroll offset that wraps every tileSize. Because the tiled texture repeats
/// exactly every tileSize, wrapping the offset produces a seamless, endless
/// flow with no visible jump.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WaterScroll : MonoBehaviour
{
    [Tooltip("How fast the water drifts, in world units per second")]
    public Vector2 scrollSpeed = new Vector2(0.18f, 0.12f);

    [Tooltip("World size of one repeating tile (the wrap period). Set by the spawner.")]
    public float tileSize = 1f;

    private Camera cam;
    private float ox, oy;
    private float zDepth;

    void Start()
    {
        cam = Camera.main;
        zDepth = transform.position.z;
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;

        ox = Mathf.Repeat(ox + scrollSpeed.x * Time.deltaTime, tileSize);
        oy = Mathf.Repeat(oy + scrollSpeed.y * Time.deltaTime, tileSize);

        Vector3 c = cam != null ? cam.transform.position : Vector3.zero;
        // Subtract one tile so the slack is centred and the view stays covered.
        transform.position = new Vector3(c.x + ox - tileSize, c.y + oy - tileSize, zDepth);
    }
}
