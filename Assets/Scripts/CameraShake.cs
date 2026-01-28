using System.Collections;
using UnityEngine;

/// <summary>
/// Handles camera shake effects with configurable intensity and duration.
/// Can be triggered by events like entering/exiting blink state.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The BlinkController to subscribe to for automatic shake triggers")]
    public BlinkController blinkController;

    [Header("Shake on Enter Blink")]
    [Tooltip("Enable screen shake when entering blink state")]
    public bool shakeOnEnterBlink = true;

    [Tooltip("Intensity of shake when entering blink (in units)")]
    public float enterBlinkIntensity = 0.3f;

    [Tooltip("Duration of shake when entering blink (in seconds)")]
    public float enterBlinkDuration = 0.2f;

    [Header("Shake on Exit Blink")]
    [Tooltip("Enable screen shake when exiting blink state")]
    public bool shakeOnExitBlink = true;

    [Tooltip("Intensity of shake when exiting blink (in units)")]
    public float exitBlinkIntensity = 0.25f;

    [Tooltip("Duration of shake when exiting blink (in seconds)")]
    public float exitBlinkDuration = 0.15f;

    [Header("Shake Behavior")]
    [Tooltip("Damping factor - how quickly shake reduces over time (higher = faster decay)")]
    [Range(0f, 10f)]
    public float dampingSpeed = 2f;

    [Tooltip("Use Perlin noise for smoother shake (false = random)")]
    public bool usePerlinNoise = false;

    [Tooltip("Random seed for Perlin noise variation")]
    private float noiseOffset;

    // Internal state
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private CellBasedCamera cellCamera;

    void Awake()
    {
        // Store original camera position
        originalPosition = transform.localPosition;

        // Generate random noise offset
        noiseOffset = Random.Range(0f, 1000f);

        // Check if CellBasedCamera is attached
        cellCamera = GetComponent<CellBasedCamera>();
    }

    void OnEnable()
    {
        // Subscribe to blink events if BlinkController is assigned
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

    void LateUpdate()
    {
        // Continuously update original position when not shaking
        // This allows CameraShake to track cell transitions from CellBasedCamera
        if (shakeCoroutine == null)
        {
            originalPosition = transform.localPosition;
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

        // Reset camera position
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// Called when entering blink state
    /// </summary>
    private void OnEnterBlink()
    {
        if (shakeOnEnterBlink)
        {
            TriggerShake(enterBlinkIntensity, enterBlinkDuration);
        }
    }

    /// <summary>
    /// Called when exiting blink state
    /// </summary>
    private void OnExitBlink()
    {
        if (shakeOnExitBlink)
        {
            TriggerShake(exitBlinkIntensity, exitBlinkDuration);
        }
    }

    /// <summary>
    /// Trigger a camera shake with specified intensity and duration
    /// </summary>
    public void TriggerShake(float intensity, float duration)
    {
        // Update original position to current position before shaking
        // This ensures shake starts from the correct cell position
        UpdateOriginalPosition();

        // Stop any existing shake
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        // Start new shake
        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    /// <summary>
    /// Coroutine that performs the shake effect
    /// </summary>
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Calculate current shake intensity with damping
            float currentIntensity = intensity * (1f - (elapsed / duration));
            currentIntensity = Mathf.Lerp(intensity, 0f, Mathf.Pow(elapsed / duration, dampingSpeed));

            // Calculate shake offset
            Vector3 offset;

            if (usePerlinNoise)
            {
                // Perlin noise for smoother shake
                float x = (Mathf.PerlinNoise(elapsed * 20f + noiseOffset, 0f) - 0.5f) * 2f;
                float y = (Mathf.PerlinNoise(0f, elapsed * 20f + noiseOffset + 100f) - 0.5f) * 2f;
                offset = new Vector3(x, y, 0f) * currentIntensity;
            }
            else
            {
                // Random shake for more chaotic effect
                offset = Random.insideUnitCircle * currentIntensity;
            }

            // Apply shake offset to camera
            transform.localPosition = originalPosition + offset;

            yield return null;
        }

        // Reset to original position
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    /// <summary>
    /// Manually set the original position (useful if camera moves)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Stop any active shake immediately
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        transform.localPosition = originalPosition;
    }
}
