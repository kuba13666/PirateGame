using UnityEngine;

/// <summary>
/// A brief cannon muzzle flash: pops a bit larger and fades out, then self-destroys.
/// Spawned in world space at the muzzle, oriented along the fire direction.
/// </summary>
public class MuzzleFlash : MonoBehaviour
{
    public float life = 0.12f;
    public float baseScale = 0.22f;

    private float t;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / life;
        if (k >= 1f) { Destroy(gameObject); return; }

        transform.localScale = Vector3.one * (baseScale * (1f + 0.9f * k));
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - k;
            sr.color = c;
        }
    }
}
