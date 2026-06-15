using UnityEngine;

/// <summary>
/// Loops a set of sprite frames on the SpriteRenderer to create an idle animation.
/// Frames are loaded from Resources as "&lt;resourcePrefix&gt;0", "&lt;resourcePrefix&gt;1", ...
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [Tooltip("Resources name prefix; frames are <prefix>0, <prefix>1, ...")]
    public string resourcePrefix;
    [Tooltip("Number of frames to load")]
    public int frameCount = 6;
    [Tooltip("Frames per second")]
    public float fps = 8f;
    [Tooltip("Randomize the starting frame so multiple enemies don't animate in sync")]
    public bool randomizePhase = true;

    private Sprite[] frames;
    private SpriteRenderer sr;
    private float timer;
    private int index;

    // One-shot overlay clip (e.g. a boss attack) that plays once over the loop.
    private Sprite[] oneShot;
    private int oneShotIndex;
    private float oneShotTimer, oneShotFps;
    private bool oneShotActive;

    /// <summary>Play a clip (&lt;prefix&gt;0..count-1) once, then resume the loop.</summary>
    public void PlayOnce(string prefix, int count, float clipFps)
    {
        if (count <= 0 || string.IsNullOrEmpty(prefix)) return;
        oneShot = new Sprite[count];
        int loaded = 0;
        for (int i = 0; i < count; i++) { oneShot[i] = Resources.Load<Sprite>(prefix + i); if (oneShot[i] != null) loaded++; }
        if (loaded == 0) return;
        oneShotIndex = 0; oneShotTimer = 0f; oneShotFps = Mathf.Max(1f, clipFps); oneShotActive = true;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (string.IsNullOrEmpty(resourcePrefix) || frameCount <= 0) { enabled = false; return; }

        frames = new Sprite[frameCount];
        int loaded = 0;
        for (int i = 0; i < frameCount; i++)
        {
            frames[i] = Resources.Load<Sprite>(resourcePrefix + i);
            if (frames[i] != null) loaded++;
        }
        if (loaded == 0) { enabled = false; return; }

        if (randomizePhase)
        {
            index = Random.Range(0, frameCount);
            timer = Random.value / Mathf.Max(0.01f, fps);
        }
        if (frames[index] != null) sr.sprite = frames[index];
    }

    void Update()
    {
        // One-shot clip takes over until its frames are exhausted
        if (oneShotActive)
        {
            oneShotTimer += Time.deltaTime;
            float oneFt = 1f / oneShotFps;
            while (oneShotTimer >= oneFt)
            {
                oneShotTimer -= oneFt;
                if (oneShotIndex >= oneShot.Length) { oneShotActive = false; break; }
                if (sr != null && oneShot[oneShotIndex] != null) sr.sprite = oneShot[oneShotIndex];
                oneShotIndex++;
            }
            return;
        }

        if (frames == null || fps <= 0f) return;
        timer += Time.deltaTime;
        float frameTime = 1f / fps;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            index = (index + 1) % frameCount;
            if (frames[index] != null) sr.sprite = frames[index];
        }
    }
}
