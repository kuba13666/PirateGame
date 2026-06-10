using UnityEngine;

/// <summary>
/// Rotates the object slowly and continuously (e.g. the Maelstrom whirlpool).
/// </summary>
public class SlowSpin : MonoBehaviour
{
    [Tooltip("Degrees per second; negative spins clockwise")]
    public float degreesPerSecond = -20f;

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
