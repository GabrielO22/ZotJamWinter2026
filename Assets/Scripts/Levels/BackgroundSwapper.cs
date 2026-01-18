using UnityEngine;

/// <summary>
/// Swaps background sprites when entering/exiting blink state
/// Similar to how enemies change from normal to ghost sprites
/// Attach to background GameObjects with SpriteRenderer components
/// </summary>
public class BackgroundSwapper : MonoBehaviour
{
    [Header("Background Sprites")]
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite blinkBackground;

    [Header("Optional Settings")]
    [SerializeField] private bool changeColorTint = false;
    [SerializeField] private Color normalTint = Color.white;
    [SerializeField] private Color blinkTint = Color.white;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Cache SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: BackgroundSwapper requires a SpriteRenderer component!");
        }

        // Validate sprite references
        if (normalBackground == null)
        {
            Debug.LogWarning($"{gameObject.name}: Normal background sprite not assigned!");
        }

        if (blinkBackground == null)
        {
            Debug.LogWarning($"{gameObject.name}: Blink background sprite not assigned!");
        }
    }

    void OnEnable()
    {
        // Subscribe to world state events
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;

            // Set initial background based on current world state
            if (WorldStateManager.Instance.IsBlinking)
            {
                SetBlinkBackground();
            }
            else
            {
                SetNormalBackground();
            }
        }
        else
        {
            // Delay subscription if WorldStateManager doesn't exist yet
            StartCoroutine(DelayedSubscribe());
        }
    }

    void OnDisable()
    {
        // Unsubscribe from world state events
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink -= HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink -= HandleExitBlink;
        }
    }

    private System.Collections.IEnumerator DelayedSubscribe()
    {
        yield return null; // Wait one frame

        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;

            // Set initial background
            if (WorldStateManager.Instance.IsBlinking)
            {
                SetBlinkBackground();
            }
            else
            {
                SetNormalBackground();
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: WorldStateManager not found! BackgroundSwapper won't work.");
        }
    }

    /// <summary>
    /// Handle entering blink state - switch to blink background
    /// </summary>
    private void HandleEnterBlink()
    {
        SetBlinkBackground();
    }

    /// <summary>
    /// Handle exiting blink state - switch back to normal background
    /// </summary>
    private void HandleExitBlink()
    {
        SetNormalBackground();
    }

    /// <summary>
    /// Set background to normal state
    /// </summary>
    private void SetNormalBackground()
    {
        if (spriteRenderer == null) return;

        if (normalBackground != null)
        {
            spriteRenderer.sprite = normalBackground;
        }

        if (changeColorTint)
        {
            spriteRenderer.color = normalTint;
        }
    }

    /// <summary>
    /// Set background to blink state
    /// </summary>
    private void SetBlinkBackground()
    {
        if (spriteRenderer == null) return;

        if (blinkBackground != null)
        {
            spriteRenderer.sprite = blinkBackground;
        }

        if (changeColorTint)
        {
            spriteRenderer.color = blinkTint;
        }
    }
}
