using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a health bar above each enemy that changes color based on health
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    private GameObject healthBarObj;
    private SpriteRenderer fillRenderer;
    private SpriteRenderer bgRenderer;
    private EnemyController enemyController;
    private static Material unlitMaterial;
    
    private const float BAR_WIDTH = 6f;
    private const float BAR_HEIGHT = 0.2f;
    private const float BAR_OFFSET_Y = 10f;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        EnsureUnlitMaterial();
        CreateHealthBar();
    }

    // Use an unlit material so bar colors are not darkened by 2D lights
    void EnsureUnlitMaterial()
    {
        if (unlitMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader != null)
            {
                unlitMaterial = new Material(shader);
            }
        }
    }

    void CreateHealthBar()
    {
        // Create health bar container
        healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = new Vector3(0, BAR_OFFSET_Y, 0);
        healthBarObj.transform.localScale = Vector3.one;

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarObj.transform);
        bgObj.transform.localPosition = Vector3.zero;
        
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateSprite(Color.black);
        bgRenderer.sortingOrder = 10;
        if (unlitMaterial != null)
        {
            bgRenderer.sharedMaterial = unlitMaterial;
        }
        bgObj.transform.localScale = new Vector3(BAR_WIDTH, BAR_HEIGHT, 1);

        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthBarObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        
        fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateSprite(Color.white);
        fillRenderer.color = Color.green;
        fillRenderer.sortingOrder = 11;
        if (unlitMaterial != null)
        {
            fillRenderer.sharedMaterial = unlitMaterial;
        }
        fillObj.transform.localScale = new Vector3(BAR_WIDTH, BAR_HEIGHT, 1);
    }

    void Update()
    {
        if (enemyController != null && healthBarObj != null)
        {
            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        float healthPercent = enemyController.GetCurrentHealth() / (float)enemyController.maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);
        
        // Update fill scale and position based on health
        Transform fillTransform = healthBarObj.transform.Find("Fill");
        if (fillTransform != null && fillRenderer != null)
        {
            // Scale from left, maintain alignment
            fillTransform.localScale = new Vector3(BAR_WIDTH * healthPercent, BAR_HEIGHT, 1);
            fillTransform.localPosition = new Vector3(-BAR_WIDTH + BAR_WIDTH * healthPercent, 0, 0);

            // Change color based on thresholds
            if (healthPercent <= (1f / 3f))
            {
                fillRenderer.color = Color.red;
            }
            else if (healthPercent <= 0.5f)
            {
                fillRenderer.color = Color.yellow;
            }
            else
            {
                fillRenderer.color = Color.green;
            }
        }
    }

    Sprite CreateSprite(Color color)
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        texture.name = color.ToString();
        return Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1);
    }

    void OnDestroy()
    {
        if (healthBarObj != null)
        {
            Destroy(healthBarObj);
        }
    }
}
