using System.Collections;
using UnityEngine;

/// <summary>
/// Provides Paper Mario-style card flip animation for sprite direction changes.
/// Animates scale on X and/or Y axis while preserving collider bounds.
/// </summary>
public class CardFlipAnimation : MonoBehaviour
{
    [Header("Flip Animation Settings")]
    [Tooltip("Enable card flip animation when changing direction")]
    public bool enableFlipAnimation = true;

    [Tooltip("Duration of the flip animation in seconds")]
    [Range(0.05f, 1f)]
    public float flipDuration = 0.15f;

    [Tooltip("Animate flip on X axis (horizontal flips)")]
    public bool flipOnXAxis = true;

    [Tooltip("Animate flip on Y axis (vertical flips)")]
    public bool flipOnYAxis = false;

    [Header("Animation Style")]
    [Tooltip("Animation curve for the flip (controls easing)")]
    public AnimationCurve flipCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, -1f);

    [Tooltip("Overshoot effect - scale slightly beyond target for bounce effect")]
    [Range(0f, 0.3f)]
    public float overshoot = 0.1f;

    [Tooltip("Add squash and stretch effect during flip")]
    public bool useSquashAndStretch = true;

    [Tooltip("Amount of squash/stretch on perpendicular axis")]
    [Range(0f, 0.5f)]
    public float squashStretchAmount = 0.2f;

    [Header("Visual Elements")]
    [Tooltip("Transform that contains the visual sprite (leave empty to use this GameObject)")]
    public Transform visualTransform;

    [Tooltip("Sprite Renderer to flip (auto-detected if not set)")]
    public SpriteRenderer spriteRenderer;

    // Internal state
    private Vector3 baseScale;
    private Coroutine flipCoroutine;
    private int currentDirection = 1; // 1 = right/normal, -1 = left/flipped
    private bool isFlipping = false;

    void Awake()
    {
        // Auto-detect visual transform
        if (visualTransform == null)
        {
            visualTransform = transform;
        }

        // Auto-detect sprite renderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Store base scale
        baseScale = visualTransform.localScale;

        // Initialize curve if not set
        if (flipCurve == null || flipCurve.keys.Length == 0)
        {
            flipCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, -1f);
        }
    }

    /// <summary>
    /// Flip to a specific direction with animation
    /// </summary>
    /// <param name="direction">1 for right/normal, -1 for left/flipped</param>
    public void FlipToDirection(int direction)
    {
        // Normalize direction to 1 or -1
        direction = direction >= 0 ? 1 : -1;

        // Don't flip if already facing that direction
        if (direction == currentDirection && !isFlipping)
            return;

        // Don't flip if animation is disabled
        if (!enableFlipAnimation)
        {
            InstantFlip(direction);
            return;
        }

        // Start flip animation
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }

        flipCoroutine = StartCoroutine(FlipAnimationCoroutine(direction));
    }

    /// <summary>
    /// Instantly flip without animation
    /// </summary>
    /// <param name="direction">1 for right/normal, -1 for left/flipped</param>
    public void InstantFlip(int direction)
    {
        direction = direction >= 0 ? 1 : -1;
        currentDirection = direction;

        Vector3 scale = baseScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        visualTransform.localScale = scale;

        isFlipping = false;
    }

    /// <summary>
    /// Coroutine that performs the card flip animation
    /// </summary>
    private IEnumerator FlipAnimationCoroutine(int targetDirection)
    {
        isFlipping = true;

        int startDirection = currentDirection;
        currentDirection = targetDirection;

        float elapsed = 0f;

        // Store the starting scale
        Vector3 startScale = visualTransform.localScale;
        Vector3 targetScale = baseScale;
        targetScale.x = Mathf.Abs(targetScale.x) * targetDirection;

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;

            // Evaluate flip curve (should go from 1 to -1)
            float curveValue = flipCurve.Evaluate(t);

            // Calculate current scale
            Vector3 currentScale = baseScale;

            // Apply X-axis flip
            if (flipOnXAxis)
            {
                float xScale = curveValue * Mathf.Abs(baseScale.x);

                // Add overshoot at the end
                if (t > 0.5f)
                {
                    float overshootT = (t - 0.5f) * 2f;
                    float overshootMultiplier = 1f + (overshoot * Mathf.Sin(overshootT * Mathf.PI));
                    xScale *= overshootMultiplier;
                }

                currentScale.x = xScale * targetDirection;

                // Squash and stretch on Y axis
                if (useSquashAndStretch)
                {
                    float squashFactor = 1f + (squashStretchAmount * (1f - Mathf.Abs(curveValue)));
                    currentScale.y = baseScale.y * squashFactor;
                }
            }

            // Apply Y-axis flip (if enabled)
            if (flipOnYAxis)
            {
                float yScale = curveValue * Mathf.Abs(baseScale.y);

                if (t > 0.5f)
                {
                    float overshootT = (t - 0.5f) * 2f;
                    float overshootMultiplier = 1f + (overshoot * Mathf.Sin(overshootT * Mathf.PI));
                    yScale *= overshootMultiplier;
                }

                currentScale.y = yScale;

                // Squash and stretch on X axis (if X flip is not active)
                if (useSquashAndStretch && !flipOnXAxis)
                {
                    float squashFactor = 1f + (squashStretchAmount * (1f - Mathf.Abs(curveValue)));
                    currentScale.x = baseScale.x * squashFactor;
                }
            }

            visualTransform.localScale = currentScale;

            yield return null;
        }

        // Ensure final scale is exactly the target
        visualTransform.localScale = targetScale;

        isFlipping = false;
        flipCoroutine = null;
    }

    /// <summary>
    /// Get the current facing direction
    /// </summary>
    public int GetCurrentDirection()
    {
        return currentDirection;
    }

    /// <summary>
    /// Check if currently animating
    /// </summary>
    public bool IsFlipping()
    {
        return isFlipping;
    }

    /// <summary>
    /// Set the base scale (useful if sprite size changes)
    /// </summary>
    public void SetBaseScale(Vector3 newBaseScale)
    {
        baseScale = newBaseScale;
    }

    /// <summary>
    /// Get the base scale
    /// </summary>
    public Vector3 GetBaseScale()
    {
        return baseScale;
    }

    /// <summary>
    /// Stop any active flip animation and reset to current direction
    /// </summary>
    public void StopFlip()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }

        InstantFlip(currentDirection);
    }

    // Debug visualization
    void OnValidate()
    {
        // Ensure curve exists
        if (flipCurve == null || flipCurve.keys.Length == 0)
        {
            flipCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, -1f);
        }

        // Clamp duration
        flipDuration = Mathf.Clamp(flipDuration, 0.05f, 1f);
    }
}
