using UnityEngine;

/// <summary>
/// Controls GameObject visibility based on world state
/// Allows objects to exist only in Normal world, Blink world, or both
/// </summary>
public class BlinkObject : MonoBehaviour
{
    [Header("Visibility Settings")]
    [SerializeField] private VisibilityMode visibilityMode = VisibilityMode.Always;
    [SerializeField] private bool disableOnHide = true; // SetActive(false) vs just disable renderer

    [Header("Optional Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D objectCollider;

    private bool isCurrentlyVisible = true;

    void Awake()
    {
        // Cache components
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (objectCollider == null) objectCollider = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        // Subscribe to world state events
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;

            // Set initial visibility based on current world state
            UpdateVisibility(WorldStateManager.Instance.CurrentState);
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
            UpdateVisibility(WorldStateManager.Instance.CurrentState);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: WorldStateManager not found! BlinkObject won't work.");
        }
    }

    /// <summary>
    /// Handle entering blink state
    /// </summary>
    private void HandleEnterBlink()
    {
        UpdateVisibility(WorldState.Blink);
    }

    /// <summary>
    /// Handle exiting blink state
    /// </summary>
    private void HandleExitBlink()
    {
        UpdateVisibility(WorldState.Normal);
    }

    /// <summary>
    /// Update visibility based on world state
    /// </summary>
    private void UpdateVisibility(WorldState currentState)
    {
        bool shouldBeVisible = ShouldBeVisible(currentState);

        if (shouldBeVisible == isCurrentlyVisible) return; // No change needed

        isCurrentlyVisible = shouldBeVisible;

        if (disableOnHide)
        {
            // Completely disable/enable the GameObject
            gameObject.SetActive(shouldBeVisible);
        }
        else
        {
            // Just toggle renderer and collider (keeps scripts running)
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = shouldBeVisible;
            }

            if (objectCollider != null)
            {
                objectCollider.enabled = shouldBeVisible;
            }
        }

        Debug.Log($"{gameObject.name} visibility: {(shouldBeVisible ? "VISIBLE" : "HIDDEN")} in {currentState} world");
    }

    /// <summary>
    /// Determine if object should be visible in given world state
    /// </summary>
    private bool ShouldBeVisible(WorldState state)
    {
        switch (visibilityMode)
        {
            case VisibilityMode.NormalOnly:
                return state == WorldState.Normal;

            case VisibilityMode.BlinkOnly:
                return state == WorldState.Blink;

            case VisibilityMode.Always:
                return true;

            default:
                return true;
        }
    }

    // Gizmo for debugging in editor
    void OnDrawGizmos()
    {
        // Color-code objects by visibility mode
        switch (visibilityMode)
        {
            case VisibilityMode.NormalOnly:
                Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.5f); // Blue
                break;
            case VisibilityMode.BlinkOnly:
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f); // Red
                break;
            case VisibilityMode.Always:
                Gizmos.color = new Color(1f, 1f, 1f, 0.3f); // White
                break;
        }

        // Draw wireframe around object
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw label showing visibility mode
        UnityEditor.Handles.Label(transform.position + Vector3.up, visibilityMode.ToString());
    }
}

/// <summary>
/// Defines when an object should be visible
/// </summary>
public enum VisibilityMode
{
    NormalOnly,  // Only visible in Normal world (disappears during blink)
    BlinkOnly,   // Only visible in Blink world (appears during blink)
    Always       // Always visible in both worlds
}
