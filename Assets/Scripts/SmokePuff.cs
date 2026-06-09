using UnityEngine;

/// <summary>
/// A small smoke puff left after a cannon fires: drifts upward, expands and
/// fades out, then self-destroys. baseScale is a multiplier on the sprite
/// (set by the spawner to hit a target world size).
/// </summary>
public class SmokePuff : MonoBehaviour
{
    public float life = 0.5f;
    public float baseScale = 0.3f;
    public float rise = 0.25f;        // world units it drifts up over its life
    public float startAlpha = 0.55f;

    private float t;
    private SpriteRenderer sr;
    private Vector3 startPos;

    void Awake()
    {
        transform.localScale = Vector3.one * (baseScale * 0.6f);
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

        transform.localScale = Vector3.one * (baseScale * (0.6f + 0.8f * k));
        transform.position = startPos + Vector3.up * (rise * k);
        if (sr != null)
        {
            Color c = sr.color;
            c.a = startAlpha * (1f - k);
            sr.color = c;
        }
    }
}
