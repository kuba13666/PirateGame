using UnityEngine;

/// <summary>
/// Gentle idle motion for a ship on water: a slow rocking rotation plus a subtle
/// scale "bob". Only touches rotation/scale, so it never fights position-based movement.
/// Captures the spawn scale in Start, so it works with the spawner's sizing.
/// </summary>
public class IdleSway : MonoBehaviour
{
    [Tooltip("Peak rocking angle in degrees")]
    public float swayAngle = 3.5f;
    [Tooltip("Rocking speed")]
    public float swaySpeed = 1.8f;
    [Tooltip("Scale bob amount (fraction of base scale)")]
    public float bob = 0.025f;

    private float seed;
    private Vector3 baseScale;

    void Start()
    {
        seed = Random.value * 100f;
        baseScale = transform.localScale;
    }

    void Update()
    {
        float t = Time.time * swaySpeed + seed;
        transform.localRotation = Quaternion.Euler(0f, 0f, swayAngle * Mathf.Sin(t));
        float p = 1f + bob * Mathf.Sin(t * 1.3f);
        transform.localScale = new Vector3(baseScale.x * p, baseScale.y * p, baseScale.z);
    }
}
