using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual indicator for the blink gauge using an eye sprite that shakes and changes color
/// as the time until forced blink approaches zero.
/// </summary>
public class EyeBlinkGauge : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ManaController that tracks the blink timer")]
    public ManaController manaController;

    [Tooltip("The Image component for the eye sprite")]
    private Image eyeImage;

    [Tooltip("The RectTransform component for proper UI positioning")]
    private RectTransform rectTransform;

    [Header("Color Transition Settings")]
    [Tooltip("Color when gauge is full (safe)")]
    public Color fullColor = Color.white;

    [Tooltip("Color when gauge is empty (danger)")]
    public Color emptyColor = Color.red;

    [Tooltip("Use gradient color transition instead of linear interpolation")]
    public bool useGradient = false;

    [Tooltip("Optional gradient for more control over color transition")]
    public Gradient colorGradient;

    [Header("Shake Settings")]
    [Tooltip("Maximum shake intensity when gauge is empty (in pixels)")]
    public float maxShakeIntensity = 10f;

    [Tooltip("Minimum shake intensity when gauge is full")]
    public float minShakeIntensity = 0f;

    [Tooltip("How quickly the shake oscillates at maximum intensity (higher = faster)")]
    public float maxShakeFrequency = 20f;

    [Tooltip("Shake frequency when gauge is full")]
    public float minShakeFrequency = 0f;

    [Tooltip("Use Perlin noise for more organic shake (false = sine wave)")]
    public bool usePerlinNoise = true;

    [Tooltip("Random seed offset for Perlin noise variation")]
    public float noiseOffset = 0f;

    [Header("Animation Curves")]
    [Tooltip("Controls how shake intensity increases as gauge depletes (0-1 input, 0-1 output)")]
    public AnimationCurve shakeIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Controls how shake frequency increases as gauge depletes (0-1 input, 0-1 output)")]
    public AnimationCurve shakeFrequencyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // Internal state
    private Vector2 originalPosition;
    private float shakeTimer = 0f;

    void Awake()
    {
        eyeImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (eyeImage == null)
        {
            Debug.LogError("EyeBlinkGauge: No Image component found! Please attach this script to a UI Image.");
        }

        if (rectTransform == null)
        {
            Debug.LogError("EyeBlinkGauge: No RectTransform component found! UI elements must have RectTransform.");
        }

        // Initialize gradient if not set
        if (colorGradient == null)
        {
            colorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(fullColor, 0f);
            colorKeys[1] = new GradientColorKey(emptyColor, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            colorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Start()
    {
        // Store the original anchored position for shake calculations
        // Using anchoredPosition ensures proper UI positioning across different resolutions
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }

        // Generate random noise offset if not set
        if (noiseOffset == 0f)
        {
            noiseOffset = Random.Range(0f, 1000f);
        }
    }

    void Update()
    {
        if (manaController == null || eyeImage == null)
            return;

        // Calculate gauge percentage (0 = empty, 1 = full)
        float gaugePercent = Mathf.Clamp01(manaController.currentMana / manaController.maxMana);

        // Invert for danger level (0 = safe, 1 = danger)
        float dangerLevel = 1f - gaugePercent;

        // Update color
        UpdateColor(dangerLevel);

        // Update shake
        UpdateShake(dangerLevel);
    }

    /// <summary>
    /// Updates the eye color based on danger level
    /// </summary>
    private void UpdateColor(float dangerLevel)
    {
        if (useGradient && colorGradient != null)
        {
            eyeImage.color = colorGradient.Evaluate(dangerLevel);
        }
        else
        {
            eyeImage.color = Color.Lerp(fullColor, emptyColor, dangerLevel);
        }
    }

    /// <summary>
    /// Updates the shake effect based on danger level
    /// </summary>
    private void UpdateShake(float dangerLevel)
    {
        if (rectTransform == null) return;

        // Calculate shake intensity based on curve
        float intensityMultiplier = shakeIntensityCurve.Evaluate(dangerLevel);
        float currentIntensity = Mathf.Lerp(minShakeIntensity, maxShakeIntensity, intensityMultiplier);

        // Calculate shake frequency based on curve
        float frequencyMultiplier = shakeFrequencyCurve.Evaluate(dangerLevel);
        float currentFrequency = Mathf.Lerp(minShakeFrequency, maxShakeFrequency, frequencyMultiplier);

        // If no shake needed, reset to original position
        if (currentIntensity <= 0.001f)
        {
            rectTransform.anchoredPosition = originalPosition;
            return;
        }

        // Update shake timer
        shakeTimer += Time.deltaTime * currentFrequency;

        // Calculate shake offset (2D for UI)
        Vector2 shakeOffset;

        if (usePerlinNoise)
        {
            // Perlin noise for organic shake
            float xShake = (Mathf.PerlinNoise(shakeTimer + noiseOffset, 0f) - 0.5f) * 2f;
            float yShake = (Mathf.PerlinNoise(0f, shakeTimer + noiseOffset + 100f) - 0.5f) * 2f;
            shakeOffset = new Vector2(xShake, yShake) * currentIntensity;
        }
        else
        {
            // Sine wave for predictable shake
            float xShake = Mathf.Sin(shakeTimer) * currentIntensity;
            float yShake = Mathf.Cos(shakeTimer * 1.3f) * currentIntensity; // Different frequency for Y
            shakeOffset = new Vector2(xShake, yShake);
        }

        // Apply shake using anchoredPosition for proper UI behavior
        rectTransform.anchoredPosition = originalPosition + shakeOffset;
    }

    /// <summary>
    /// Call this when the gauge is refilled to reset shake state
    /// </summary>
    public void ResetShake()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
        shakeTimer = 0f;
    }

    /// <summary>
    /// Manually set the original position (useful if the UI element moves)
    /// </summary>
    public void SetOriginalPosition(Vector2 position)
    {
        originalPosition = position;
    }

    /// <summary>
    /// Update the original position to current anchored position
    /// Useful if UI layout changes during gameplay
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
    }

    // Debug visualization in editor
    void OnValidate()
    {
        // Ensure curves are initialized
        if (shakeIntensityCurve == null || shakeIntensityCurve.keys.Length == 0)
        {
            shakeIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        if (shakeFrequencyCurve == null || shakeFrequencyCurve.keys.Length == 0)
        {
            shakeFrequencyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }
}
