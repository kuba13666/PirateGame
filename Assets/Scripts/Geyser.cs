using System.Collections;
using UnityEngine;

/// <summary>
/// A telegraphed seawater geyser (Mocha Dick's phase-1 ranged attack): a dark
/// swell rises on the surface (warning), then a foamy column erupts, dealing
/// AoE damage to the player if they're still standing in it. Self-destructs.
/// Uses a generated "Geyser" sprite if present, else the soft "Smoke" puff.
/// </summary>
public class Geyser : MonoBehaviour
{
    public float warnTime = 0.85f;
    public float radius = 1.7f;
    public int damage = 2;

    IEnumerator Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        Transform player = p != null ? p.transform : null;

        Sprite smoke = Resources.Load<Sprite>("Smoke");

        // ── Telegraph: a dark swell that grows on the water ──
        var tel = MakeVis(smoke, new Color(0.08f, 0.32f, 0.5f, 0.2f), radius * 1.8f, -1);
        var tsr = tel.GetComponent<SpriteRenderer>();
        float t = 0f;
        while (t < warnTime)
        {
            t += Time.deltaTime;
            float k = t / warnTime;
            Color c = tsr.color; c.a = 0.18f + 0.32f * k; tsr.color = c;
            yield return null;
        }
        Destroy(tel);

        // ── Erupt: a foamy column + a hit if the player's still in range ──
        Sprite g = Resources.Load<Sprite>("Geyser");
        if (g == null) g = smoke;
        var col = MakeVis(g, new Color(0.85f, 0.95f, 1f, 0.95f), radius * 1.2f, 7);
        var csr = col.GetComponent<SpriteRenderer>();
        Vector3 baseScale = col.transform.localScale;

        if (player != null && Vector2.Distance(player.position, transform.position) < radius)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.TakeDamage(damage);
        }

        float e = 0f, dur = 0.5f;
        while (e < dur)
        {
            e += Time.deltaTime;
            float k = e / dur;
            col.transform.localScale = baseScale * (1f + 0.6f * k);
            Color c = csr.color; c.a = 0.95f * (1f - k); csr.color = c;
            yield return null;
        }
        Destroy(col);
        Destroy(gameObject);
    }

    GameObject MakeVis(Sprite s, Color color, float worldSize, int order)
    {
        var go = new GameObject("GeyserVis");
        go.transform.position = transform.position;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.color = color;
        sr.sortingOrder = order;
        float h = s != null ? s.bounds.size.y : 1f;
        float sc = worldSize / Mathf.Max(0.01f, h);
        go.transform.localScale = new Vector3(sc, sc, 1f);
        return go;
    }
}
