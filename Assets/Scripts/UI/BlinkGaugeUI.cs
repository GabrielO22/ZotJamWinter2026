using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI gauge that displays time until forced blink
/// Lerps from green to red and shakes more violently as it depletes
/// </summary>
public class BlinkGaugeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform gaugeContainer;

    [Header("Color Settings")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color emptyColor = Color.red;
    [SerializeField] private Gradient colorGradient; // Optional: Use gradient instead of simple lerp

    [Header("Shake Settings")]
    [SerializeField] private bool enableShake = true;
    [SerializeField] private float maxShakeIntensity = 10f;
    [SerializeField] private float shakeFrequency = 20f; // How fast it shakes
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("References")]
    [SerializeField] private WorldStateManager worldStateManager;

    private Vector3 originalPosition;
    private float currentShakeIntensity = 0f;

    void Awake()
    {
        // Cache original position for shake effect
        if (gaugeContainer != null)
        {
            originalPosition = gaugeContainer.localPosition;
        }

        // Create default gradient if none assigned
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
        // Auto-find WorldStateManager if not assigned
        if (worldStateManager == null)
        {
            worldStateManager = WorldStateManager.Instance;
        }

        if (worldStateManager == null)
        {
            Debug.LogWarning("BlinkGaugeUI: WorldStateManager not found!");
        }
    }

    void Update()
    {
        if (worldStateManager == null) return;

        // Get normalized time remaining (1 = full, 0 = empty)
        float normalizedTime = worldStateManager.GetNormalizedBlinkGaugeTime();

        // Update fill amount (invert so it drains)
        if (fillImage != null)
        {
            fillImage.fillAmount = normalizedTime;

            // Update color based on time remaining
            fillImage.color = colorGradient.Evaluate(1f - normalizedTime);
        }

        // Update shake intensity based on how empty the gauge is
        if (enableShake && gaugeContainer != null)
        {
            // Shake more violently as gauge depletes
            float shakeAmount = shakeCurve.Evaluate(1f - normalizedTime);
            currentShakeIntensity = shakeAmount * maxShakeIntensity;

            // Apply shake offset
            if (currentShakeIntensity > 0.01f)
            {
                float shakeX = Mathf.Sin(Time.time * shakeFrequency) * currentShakeIntensity;
                float shakeY = Mathf.Cos(Time.time * shakeFrequency * 1.3f) * currentShakeIntensity;
                gaugeContainer.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0f);
            }
            else
            {
                gaugeContainer.localPosition = originalPosition;
            }
        }
    }

    void OnDisable()
    {
        // Reset position when disabled
        if (gaugeContainer != null)
        {
            gaugeContainer.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// Manually set the gauge fill amount (for testing)
    /// </summary>
    public void SetFillAmount(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
            fillImage.color = colorGradient.Evaluate(1f - amount);
        }
    }
}
