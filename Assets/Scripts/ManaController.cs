using UnityEngine;

/// <summary>
/// Visual mana gauge controller synchronized with WorldStateManager's forced blink system
/// The mana bar visually represents the forced blink gauge timer
/// </summary>
public class ManaController : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Maximum visual scale on X axis when full")]
    public float maxScaleX = 3f;

    [Header("Color Settings (Optional)")]
    public bool useColorGradient = false;
    public Color fullColor = Color.green;
    public Color emptyColor = Color.red;

    private WorldStateManager worldStateManager;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        worldStateManager = WorldStateManager.Instance;
        if (worldStateManager == null)
        {
            Debug.LogWarning("ManaController: WorldStateManager not found! Visual gauge will not function.");
        }
    }

    void Update()
    {
        if (worldStateManager == null) return;

        // Sync with WorldStateManager's forced blink timer
        float normalizedTime = worldStateManager.GetNormalizedBlinkGaugeTime();

        // Update visual scale
        Vector3 newScale = originalScale;
        newScale.x = normalizedTime * maxScaleX;
        transform.localScale = newScale;

        // Optional: Update color based on gauge level
        if (useColorGradient && spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(emptyColor, fullColor, normalizedTime);
        }
    }

    /// <summary>
    /// Get normalized mana value (0-1)
    /// </summary>
    public float GetNormalizedMana()
    {
        if (worldStateManager == null) return 1f;
        return worldStateManager.GetNormalizedBlinkGaugeTime();
    }
}
