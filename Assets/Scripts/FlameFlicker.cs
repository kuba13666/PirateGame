using UnityEngine;

/// <summary>
/// Makes a flame sprite dance: pulsing upward scale, alpha flicker and a little jitter.
/// Expects the flame sprite to have a bottom pivot so it licks upward from its base.
/// </summary>
public class FlameFlicker : MonoBehaviour
{
    public float baseScale = 1f;

    private SpriteRenderer sr;
    private float seed;
    private Vector3 basePos;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        seed = Random.value * 100f;
        basePos = transform.localPosition;
    }

    void Update()
    {
        float t = Time.time * 9f + seed;
        float sy = baseScale * (1f + 0.22f * Mathf.Sin(t) + 0.10f * Mathf.Sin(t * 2.3f));
        float sx = baseScale * (1f - 0.08f * Mathf.Sin(t * 1.7f));
        transform.localScale = new Vector3(sx, sy, 1f);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.75f + 0.25f * Mathf.Abs(Mathf.Sin(t * 1.3f + 1f));
            sr.color = c;
        }

        transform.localPosition = basePos + new Vector3(0.02f * baseScale * Mathf.Sin(t * 3.1f), 0f, 0f);
    }
}
