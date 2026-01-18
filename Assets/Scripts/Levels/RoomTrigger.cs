using UnityEngine;

/// <summary>
/// Trigger zone that transitions the camera to a new room
/// When player enters, camera smoothly moves to focus on the new room bounds
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomTrigger : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private string roomName = "Room 1";
    [SerializeField] private Vector3 cameraTargetPosition = Vector3.zero;
    [SerializeField] private float cameraSize = 5f; // Orthographic size for this room

    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 3f;
    [SerializeField] private bool lockPlayerDuringTransition = false;
    [SerializeField] private bool revertOnExit = true; // Revert camera when player exits

    [Header("Room Bounds (Optional)")]
    [SerializeField] private bool useRoomBounds = false;
    [SerializeField] private Bounds roomBounds = new Bounds(Vector3.zero, Vector3.one * 10f);

    private Collider2D triggerCollider;
    private bool isPlayerInRoom = false;
    private CameraController cameraController;

    // Store previous camera state
    private Vector3 previousCameraPosition;
    private float previousCameraSize;
    private bool hasPreviousState = false;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();

        // Ensure collider is a trigger
        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
            Debug.LogWarning($"{gameObject.name}: Collider was not set as trigger. Auto-fixed.");
        }
    }

    void Start()
    {
        // Cache camera controller reference
        cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogWarning($"{gameObject.name}: CameraController not found in scene!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EnterRoom();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ExitRoom();
        }
    }

    /// <summary>
    /// Handle player entering the room
    /// </summary>
    private void EnterRoom()
    {
        if (isPlayerInRoom) return;

        isPlayerInRoom = true;
        Debug.Log($"Player entered {roomName}");

        if (cameraController == null)
        {
            Debug.LogWarning($"{gameObject.name}: CameraController not found! Cannot transition camera.");
            return;
        }

        // Store previous camera state before transitioning
        if (!hasPreviousState)
        {
            previousCameraPosition = cameraController.transform.position;

            Camera cam = cameraController.GetComponent<Camera>();
            if (cam != null && cam.orthographic)
            {
                previousCameraSize = cam.orthographicSize;
            }
            else
            {
                previousCameraSize = 5f; // Default fallback
            }

            hasPreviousState = true;
            Debug.Log($"Stored previous camera state: Position {previousCameraPosition}, Size {previousCameraSize}");
        }

        // Transition to new room
        cameraController.TransitionToRoom(cameraTargetPosition, cameraSize, transitionSpeed);

        // Optionally lock player movement during transition
        if (lockPlayerDuringTransition)
        {
            // This would require a method in PlayerMovement to disable input
            // For now, just log
            Debug.Log("Lock player during transition (not implemented)");
        }
    }

    /// <summary>
    /// Handle player exiting the room
    /// </summary>
    private void ExitRoom()
    {
        isPlayerInRoom = false;
        Debug.Log($"Player exited {roomName}");

        // Revert to previous camera state if enabled
        if (revertOnExit && hasPreviousState && cameraController != null)
        {
            Debug.Log($"Reverting camera to previous state: Position {previousCameraPosition}, Size {previousCameraSize}");
            cameraController.TransitionToRoom(previousCameraPosition, previousCameraSize, transitionSpeed);

            // Reset state flag so it can be stored again on next entry
            hasPreviousState = false;
        }
    }

    /// <summary>
    /// Check if a position is within room bounds
    /// </summary>
    public bool IsPositionInRoom(Vector3 position)
    {
        if (!useRoomBounds) return true;
        return roomBounds.Contains(position);
    }

    // Gizmos for editor visualization
    void OnDrawGizmos()
    {
        // Draw trigger area
        Gizmos.color = Color.yellow;
        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D boxCol)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
        }

        // Draw camera target position (enter)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(cameraTargetPosition, 0.5f);
        Gizmos.DrawLine(transform.position, cameraTargetPosition);

        // Draw revert indicator if enabled
        if (revertOnExit && hasPreviousState)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(previousCameraPosition, 0.3f);
            Gizmos.DrawLine(transform.position, previousCameraPosition);
        }

        // Draw room bounds
        if (useRoomBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(roomBounds.center, roomBounds.size);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw labels
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(cameraTargetPosition + Vector3.up, $"{roomName}\nCamera Target (Enter)");

        if (revertOnExit && hasPreviousState)
        {
            UnityEditor.Handles.Label(previousCameraPosition + Vector3.up * 0.5f, "Previous Position (Exit)");
        }

        if (useRoomBounds)
        {
            UnityEditor.Handles.Label(roomBounds.center + Vector3.up * (roomBounds.extents.y + 0.5f), "Room Bounds");
        }
        #endif
    }
}
