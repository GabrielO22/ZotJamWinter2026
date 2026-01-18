using UnityEngine;
using UnityEditor;

/// <summary>
/// Platform that moves/shifts position when player blinks
/// IMPORTANT: This component moves the platform regardless of visibility.
/// If using with BlinkObject that has disableOnHide=true, the platform will be disabled and won't move.
/// Recommendation: Use BlinkObject with disableOnHide=false, or don't use BlinkObject at all for moving platforms.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("If true, uses the platform's starting position as normalPosition")]
    [SerializeField] private bool useStartingPositionAsNormal = true;
    [SerializeField] private Vector3 normalPosition = Vector3.zero;
    [SerializeField] private Vector3 blinkOffset = new Vector3(5f, 0f, 0f);
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Movement Type")]
    [SerializeField] private MovementType movementType = MovementType.Instant;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isInitialized = false;

    void Awake()
    {
        // Store starting position as normal position if configured to do so
        if (useStartingPositionAsNormal)
        {
            normalPosition = transform.position;
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Set normalPosition to starting position {normalPosition}");
            }
        }

        isInitialized = true;

        // Warn if BlinkObject with disableOnHide is attached
        BlinkObject blinkObj = GetComponent<BlinkObject>();
        if (blinkObj != null)
        {
            // We can't directly check disableOnHide since it's private, but we can warn
            Debug.LogWarning($"{gameObject.name}: MovingPlatform + BlinkObject detected. " +
                "If BlinkObject has 'Disable On Hide' enabled, the platform won't move when hidden. " +
                "Set 'Disable On Hide' to FALSE for proper movement.");
        }
    }

    void OnEnable()
    {
        // Subscribe to world state events with delayed subscription to avoid race conditions
        StartCoroutine(DelayedSubscribe());
    }

    private System.Collections.IEnumerator DelayedSubscribe()
    {
        yield return null; // Wait one frame for WorldStateManager to initialize

        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;

            // Set initial position based on current world state
            UpdatePosition(WorldStateManager.Instance.CurrentState, true);

            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Subscribed to WorldStateManager, current state: {WorldStateManager.Instance.CurrentState}");
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: WorldStateManager not found! MovingPlatform won't work.");
        }
    }

    void OnDisable()
    {
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink -= HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink -= HandleExitBlink;
        }

        // Stop any ongoing coroutines
        StopAllCoroutines();

        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: OnDisable called, unsubscribed from events");
        }
    }

    void Update()
    {
        // Smooth movement to target position
        if (isMoving && movementType == MovementType.Smooth)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);

            // Stop moving when close enough
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    private void HandleEnterBlink()
    {
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: HandleEnterBlink called");
        }
        UpdatePosition(WorldState.Blink, false);
    }

    private void HandleExitBlink()
    {
        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: HandleExitBlink called");
        }
        UpdatePosition(WorldState.Normal, false);
    }

    private void UpdatePosition(WorldState state, bool instant)
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"{gameObject.name}: UpdatePosition called before initialization!");
            return;
        }

        Vector3 newPosition = (state == WorldState.Normal) ? normalPosition : normalPosition + blinkOffset;

        if (movementType == MovementType.Instant || instant)
        {
            transform.position = newPosition;
            isMoving = false;

            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Instantly moved to {newPosition} (state: {state})");
            }
        }
        else
        {
            targetPosition = newPosition;
            isMoving = true;

            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Starting smooth movement to {newPosition} (state: {state})");
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw normal position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(normalPosition, Vector3.one);

        // Draw blink position
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(normalPosition + blinkOffset, Vector3.one);

        // Draw line between positions
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(normalPosition, normalPosition + blinkOffset);
    }

    void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        // Draw labels
        UnityEditor.Handles.Label(normalPosition + Vector3.up * 0.5f, "Normal Pos");
        UnityEditor.Handles.Label(normalPosition + blinkOffset + Vector3.up * 0.5f, "Blink Pos");
        #endif
    }
}

public enum MovementType
{
    Instant, // Teleports immediately
    Smooth   // Moves smoothly over time
}
