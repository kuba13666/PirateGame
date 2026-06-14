using UnityEngine;

/// <summary>
/// A luring ghost-light cast by the Flying Dutchman — a slow green wisp that
/// homes toward the player and burns on contact, then fades after its life.
/// </summary>
public class GhostWisp : MonoBehaviour
{
    public Transform target;
    public float speed = 1.7f;
    public int damage = 1;
    public float life = 6f;

    private float t, dmgCd;
    private SpriteRenderer sr;
    private float seed;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        seed = Random.value * 10f;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= life) { Destroy(gameObject); return; }

        if (target != null)
        {
            Vector2 d = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(d * speed * Time.deltaTime);
        }

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.55f + 0.35f * Mathf.Sin(Time.time * 8f + seed);
            sr.color = c;
        }

        if (dmgCd > 0f) dmgCd -= Time.deltaTime;
    }

    void OnTriggerStay2D(Collider2D c)
    {
        if (dmgCd > 0f || !c.CompareTag("Player")) return;
        var p = c.GetComponent<PlayerController>();
        if (p != null) { p.TakeDamage(damage); dmgCd = 1f; }
    }
}
