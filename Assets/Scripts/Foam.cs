using UnityEngine;

/// <summary>
/// A single drifting whitecap of foam on the water: fades in, drifts with the
/// current while expanding slightly, then fades out and self-destroys.
/// baseScale is a multiplier on the sprite (set by the spawner for a target size).
/// </summary>
public class Foam : MonoBehaviour
{
    public float life = 2.6f;
    public float baseScale = 0.5f;
    public float maxAlpha = 0.7f;
    public Vector2 drift = new Vector2(0.12f, 0.08f);

    private float t;
    private SpriteRenderer sr;
    private Vector3 startPos;

    void Awake()
    {
        transform.localScale = Vector3.one * baseScale;
    }

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

        // Fade in over the first 30%, fade out over the last 40%.
        float a = Mathf.Min(Mathf.SmoothStep(0f, 1f, k / 0.3f),
                            Mathf.SmoothStep(0f, 1f, (1f - k) / 0.4f));

        transform.localScale = Vector3.one * (baseScale * (1f + 0.25f * k));
        transform.position = startPos + (Vector3)(drift * t);
        if (sr != null)
        {
            Color c = sr.color;
            c.a = a * maxAlpha;
            sr.color = c;
        }
    }
}
