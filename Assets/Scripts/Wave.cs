using UnityEngine;

/// <summary>
/// A single wave squiggle on the water: slowly fades in, sits in the world
/// (barely drifting), then slowly fades out and self-destroys.
/// Scale/rotation/lifetime are set by the spawner.
/// </summary>
public class Wave : MonoBehaviour
{
    public float life = 7f;
    public float maxAlpha = 0.5f;
    public Vector2 drift = Vector2.zero;

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

        // Slow fade in (first 25%), slow fade out (last 30%).
        float a = Mathf.Min(Mathf.SmoothStep(0f, 1f, k / 0.25f),
                            Mathf.SmoothStep(0f, 1f, (1f - k) / 0.3f));

        transform.position = startPos + (Vector3)(drift * t);
        if (sr != null)
        {
            Color c = sr.color;
            c.a = a * maxAlpha;
            sr.color = c;
        }
    }
}
