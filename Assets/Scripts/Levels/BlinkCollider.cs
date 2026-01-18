using UnityEngine;

/// <summary>
/// Lightweight component that toggles collider based on world state
/// More efficient than BlinkObject when you only need to toggle collision
/// </summary>
public class BlinkCollider : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private VisibilityMode collisionMode = VisibilityMode.Always;
    [SerializeField] private Collider2D targetCollider;

    void Awake()
    {
        // Cache collider
        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider2D>();
        }

        if (targetCollider == null)
        {
            Debug.LogError($"{gameObject.name}: BlinkCollider requires a Collider2D component!");
        }
    }

    void OnEnable()
    {
        // Subscribe to world state events
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;

            // Set initial collision state
            UpdateCollision(WorldStateManager.Instance.CurrentState);
        }
        else
        {
            StartCoroutine(DelayedSubscribe());
        }
    }

    void OnDisable()
    {
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink -= HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink -= HandleExitBlink;
        }
    }

    private System.Collections.IEnumerator DelayedSubscribe()
    {
        yield return null;

        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;
            UpdateCollision(WorldStateManager.Instance.CurrentState);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: WorldStateManager not found!");
        }
    }

    private void HandleEnterBlink()
    {
        UpdateCollision(WorldState.Blink);
    }

    private void HandleExitBlink()
    {
        UpdateCollision(WorldState.Normal);
    }

    private void UpdateCollision(WorldState currentState)
    {
        if (targetCollider == null) return;

        bool shouldCollide = ShouldCollide(currentState);
        targetCollider.enabled = shouldCollide;

        Debug.Log($"{gameObject.name} collision: {(shouldCollide ? "ENABLED" : "DISABLED")} in {currentState} world");
    }

    private bool ShouldCollide(WorldState state)
    {
        switch (collisionMode)
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

    void OnDrawGizmos()
    {
        if (targetCollider == null) targetCollider = GetComponent<Collider2D>();
        if (targetCollider == null) return;

        // Color-code by collision mode
        switch (collisionMode)
        {
            case VisibilityMode.NormalOnly:
                Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.3f); // Blue
                break;
            case VisibilityMode.BlinkOnly:
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f); // Red
                break;
            case VisibilityMode.Always:
                Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.3f); // Green
                break;
        }

        Gizmos.DrawCube(targetCollider.bounds.center, targetCollider.bounds.size);
    }
}
