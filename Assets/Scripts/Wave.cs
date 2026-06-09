using UnityEngine;

/// <summary>
/// A single rolling wave crest on the water: fades in, drifts slowly (rolling),
/// then fades out and self-destroys. Subtle and big rather than busy.
/// baseScale is set by the spawner to hit a target world width.
/// </summary>
public class Wave : MonoBehaviour
{
    public float life = 4f;
    public float maxAlpha = 0.45f;
    public Vector2 drift = new Vector2(0.0f, -0.06f);

    private float t;
    private SpriteRenderer sr;
    private Vector3 startPos;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / life;
        if (k >= 1f) { Destroy(gameObject); return; }

        // Slow fade in (first 35%), slow fade out (last 45%).
        float a = Mathf.Min(Mathf.SmoothStep(0f, 1f, k / 0.35f),
                            Mathf.SmoothStep(0f, 1f, (1f - k) / 0.45f));

        transform.position = startPos + (Vector3)(drift * t);
        if (sr != null)
        {
            Color c = sr.color;
            c.a = a * maxAlpha;
            sr.color = c;
        }
    }
}
