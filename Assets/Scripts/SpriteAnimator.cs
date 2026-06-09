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
