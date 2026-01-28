using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a vignette overlay that responds to the blink gauge state.
/// Increases opacity as blink gauge depletes, quickly fades on blink entry.
/// </summary>
public class VignetteController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ManaController that tracks the blink gauge")]
    public ManaController manaController;

    [Tooltip("The BlinkController for blink state events")]
    public BlinkController blinkController;

    [Tooltip("The Image component for the vignette overlay")]
    private Image vignetteImage;

    [Header("Vignette Opacity Settings")]
    [Tooltip("Minimum opacity when gauge is full (safe)")]
    [Range(0f, 1f)]
    public float minOpacity = 0f;

    [Tooltip("Maximum opacity when gauge is empty (danger)")]
    [Range(0f, 1f)]
    public float maxOpacity = 0.7f;

    [Tooltip("Curve controlling how opacity increases as gauge depletes")]
    public AnimationCurve opacityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Blink Entry Animation")]
    [Tooltip("Enable quick fade animation when entering blink (eyes startled open)")]
    public bool animateOnBlinkEnter = true;

    [Tooltip("Duration of the fade animation when entering blink")]
    public float blinkEnterFadeDuration = 0.3f;

    [Tooltip("Target opacity during blink enter animation (brief flash)")]
    [Range(0f, 1f)]
    public float blinkEnterFlashOpacity = 0.9f;

    [Tooltip("How quickly opacity drops after the flash (higher = faster)")]
    public float blinkEnterFadeSpeed = 3f;

    [Header("Vignette Color")]
    [Tooltip("Color of the vignette effect")]
    public Color vignetteColor = Color.black;

    [Tooltip("Use gradient for color transition as danger increases")]
    public bool useColorGradient = false;

    [Tooltip("Gradient for vignette color (evaluated based on danger level)")]
    public Gradient colorGradient;

    // Internal state
    private Coroutine fadeCoroutine;
    private bool isAnimating = false;

    void Awake()
    {
        vignetteImage = GetComponent<Image>();

        if (vignetteImage == null)
        {
            Debug.LogError("VignetteController: No Image component found! Please attach this script to a UI Image.");
        }

        // Initialize color gradient if not set
        if (colorGradient == null)
        {
            colorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.black, 0f);
            colorKeys[1] = new GradientColorKey(Color.red * 0.5f, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            colorGradient.SetKeys(colorKeys, alphaKeys);
        }

        // Set initial color
        if (vignetteImage != null)
        {
            Color c = vignetteColor;
            c.a = minOpacity;
            vignetteImage.color = c;
        }
    }

    void OnEnable()
    {
        // Subscribe to blink events
        if (blinkController == null)
        {
            blinkController = BlinkController.Instance;
        }

        if (blinkController != null)
        {
            blinkController.enterBlink += OnEnterBlink;
            blinkController.exitBlink += OnExitBlink;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from events
        if (blinkController != null)
        {
            blinkController.enterBlink -= OnEnterBlink;
            blinkController.exitBlink -= OnExitBlink;
        }

        // Stop any active animation
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }

    void Update()
    {
        if (manaController == null || vignetteImage == null)
            return;

        // Don't update if currently animating blink entry
        if (isAnimating)
            return;

        // Calculate gauge percentage (0 = empty, 1 = full)
        float gaugePercent = Mathf.Clamp01(manaController.currentMana / manaController.maxMana);

        // Invert for danger level (0 = safe, 1 = danger)
        float dangerLevel = 1f - gaugePercent;

        // Update vignette
        UpdateVignette(dangerLevel);
    }

    /// <summary>
    /// Updates the vignette opacity and color based on danger level
    /// </summary>
    private void UpdateVignette(float dangerLevel)
    {
        // Calculate opacity using curve
        float curveValue = opacityCurve.Evaluate(dangerLevel);
        float targetOpacity = Mathf.Lerp(minOpacity, maxOpacity, curveValue);

        // Calculate color
        Color targetColor;
        if (useColorGradient && colorGradient != null)
        {
            targetColor = colorGradient.Evaluate(dangerLevel);
        }
        else
        {
            targetColor = vignetteColor;
        }

        // Apply opacity to color
        targetColor.a = targetOpacity;

        // Update vignette image
        vignetteImage.color = targetColor;
    }

    /// <summary>
    /// Called when entering blink state
    /// </summary>
    private void OnEnterBlink()
    {
        if (animateOnBlinkEnter && vignetteImage != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(BlinkEnterAnimation());
        }
    }

    /// <summary>
    /// Called when exiting blink state - reset to normal behavior
    /// </summary>
    private void OnExitBlink()
    {
        isAnimating = false;

        // Immediately update to current gauge state
        if (manaController != null && vignetteImage != null)
        {
            float gaugePercent = Mathf.Clamp01(manaController.currentMana / manaController.maxMana);
            float dangerLevel = 1f - gaugePercent;
            UpdateVignette(dangerLevel);
        }
    }

    /// <summary>
    /// Animation when entering blink - simulates eyes startled open
    /// Brief flash to high opacity, then quick fade to minimum
    /// </summary>
    private IEnumerator BlinkEnterAnimation()
    {
        isAnimating = true;

        // Brief flash to high opacity (eyes wide open effect)
        Color startColor = vignetteImage.color;
        Color flashColor = vignetteColor;
        flashColor.a = blinkEnterFlashOpacity;

        float elapsed = 0f;
        float flashDuration = blinkEnterFadeDuration * 0.2f; // Quick flash

        // Flash up
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            vignetteImage.color = Color.Lerp(startColor, flashColor, t);
            yield return null;
        }

        vignetteImage.color = flashColor;

        // Quick fade to minimum opacity
        Color targetColor = vignetteColor;
        targetColor.a = minOpacity;

        elapsed = 0f;
        float fadeDuration = blinkEnterFadeDuration * 0.8f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Pow(elapsed / fadeDuration, blinkEnterFadeSpeed);
            vignetteImage.color = Color.Lerp(flashColor, targetColor, t);
            yield return null;
        }

        vignetteImage.color = targetColor;

        isAnimating = false;
        fadeCoroutine = null;
    }

    /// <summary>
    /// Manually trigger the blink enter animation
    /// </summary>
    public void TriggerBlinkAnimation()
    {
        OnEnterBlink();
    }

    /// <summary>
    /// Force set vignette opacity (useful for testing)
    /// </summary>
    public void SetOpacity(float opacity)
    {
        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = Mathf.Clamp01(opacity);
            vignetteImage.color = c;
        }
    }
}
