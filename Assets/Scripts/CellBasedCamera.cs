using System.Collections;
using UnityEngine;

/// <summary>
/// Cell-based camera system that snaps to grid positions.
/// Camera only moves when player reaches screen edges, creating room-to-room transitions.
/// </summary>
public class CellBasedCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player transform to track")]
    public Transform player;

    [Header("Cell/Grid Settings")]
    [Tooltip("Width of each camera cell (horizontal room size)")]
    public float cellWidth = 20f;

    [Tooltip("Height of each camera cell (vertical room size)")]
    public float cellHeight = 11.25f;

    [Tooltip("Camera Z position (should be negative for 2D)")]
    public float cameraZ = -10f;

    [Header("Edge Detection")]
    [Tooltip("Distance from screen edge before triggering camera transition (in world units)")]
    public float edgeTriggerDistance = 1f;

    [Tooltip("Use viewport-based edge detection instead of fixed distance")]
    public bool useViewportEdgeDetection = true;

    [Tooltip("Viewport percentage for edge trigger (0.1 = 10% from edge)")]
    [Range(0.01f, 0.5f)]
    public float viewportEdgeThreshold = 0.1f;

    [Header("Transition Settings")]
    [Tooltip("Speed of camera pan transition between cells")]
    public float panSpeed = 8f;

    [Tooltip("Smoothing type for camera transitions")]
    public TransitionType transitionType = TransitionType.SmoothDamp;

    [Tooltip("Enable/disable transitions on each axis")]
    public bool enableHorizontalTransitions = true;
    public bool enableVerticalTransitions = true;

    [Header("Clamping")]
    [Tooltip("Enable camera bounds clamping")]
    public bool useWorldBounds = false;

    [Tooltip("Minimum world position for camera (bottom-left)")]
    public Vector2 worldBoundsMin = new Vector2(-100f, -100f);

    [Tooltip("Maximum world position for camera (top-right)")]
    public Vector2 worldBoundsMax = new Vector2(100f, 100f);

    [Header("Debug")]
    [Tooltip("Show debug gizmos for cells and edges")]
    public bool showDebugGizmos = true;

    public enum TransitionType
    {
        Linear,
        SmoothDamp,
        Exponential
    }

    // Internal state
    private Vector3 currentCellPosition;
    private Vector3 targetCellPosition;
    private bool isTransitioning = false;
    private Vector3 velocity = Vector3.zero; // For SmoothDamp
    private Camera cam;

    // Current cell indices
    private int currentCellX = 0;
    private int currentCellY = 0;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CellBasedCamera: No Camera component found!");
        }
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("CellBasedCamera: Player reference not set!");
            return;
        }

        // Initialize camera to player's starting cell
        SnapToPlayerCell();
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Check if player has crossed into a new cell
        if (!isTransitioning)
        {
            CheckCellTransition();
        }

        // Update camera position (handle transitions)
        UpdateCameraPosition();
    }

    /// <summary>
    /// Instantly snap camera to the cell containing the player
    /// </summary>
    private void SnapToPlayerCell()
    {
        // Calculate which cell the player is in
        currentCellX = Mathf.RoundToInt(player.position.x / cellWidth);
        currentCellY = Mathf.RoundToInt(player.position.y / cellHeight);

        // Calculate cell center position
        currentCellPosition = new Vector3(
            currentCellX * cellWidth,
            currentCellY * cellHeight,
            cameraZ
        );

        targetCellPosition = currentCellPosition;

        // Apply bounds if enabled
        if (useWorldBounds)
        {
            currentCellPosition = ClampToBounds(currentCellPosition);
            targetCellPosition = currentCellPosition;
        }

        // Snap camera immediately
        transform.position = currentCellPosition;
        isTransitioning = false;
    }

    /// <summary>
    /// Check if player has crossed an edge to trigger cell transition
    /// </summary>
    private void CheckCellTransition()
    {
        if (cam == null) return;

        // Get player position relative to camera
        Vector3 viewportPos = cam.WorldToViewportPoint(player.position);

        bool shouldTransition = false;
        int newCellX = currentCellX;
        int newCellY = currentCellY;

        if (useViewportEdgeDetection)
        {
            // Viewport-based edge detection (more reliable for different resolutions)
            if (enableHorizontalTransitions)
            {
                if (viewportPos.x < viewportEdgeThreshold)
                {
                    // Player near left edge
                    newCellX--;
                    shouldTransition = true;
                }
                else if (viewportPos.x > (1f - viewportEdgeThreshold))
                {
                    // Player near right edge
                    newCellX++;
                    shouldTransition = true;
                }
            }

            if (enableVerticalTransitions)
            {
                if (viewportPos.y < viewportEdgeThreshold)
                {
                    // Player near bottom edge
                    newCellY--;
                    shouldTransition = true;
                }
                else if (viewportPos.y > (1f - viewportEdgeThreshold))
                {
                    // Player near top edge
                    newCellY++;
                    shouldTransition = true;
                }
            }
        }
        else
        {
            // World distance-based edge detection
            float distanceX = Mathf.Abs(player.position.x - currentCellPosition.x);
            float distanceY = Mathf.Abs(player.position.y - currentCellPosition.y);

            if (enableHorizontalTransitions && distanceX > (cellWidth * 0.5f - edgeTriggerDistance))
            {
                newCellX = Mathf.RoundToInt(player.position.x / cellWidth);
                shouldTransition = true;
            }

            if (enableVerticalTransitions && distanceY > (cellHeight * 0.5f - edgeTriggerDistance))
            {
                newCellY = Mathf.RoundToInt(player.position.y / cellHeight);
                shouldTransition = true;
            }
        }

        // Trigger transition if cell changed
        if (shouldTransition && (newCellX != currentCellX || newCellY != currentCellY))
        {
            TransitionToCell(newCellX, newCellY);
        }
    }

    /// <summary>
    /// Initiate transition to a new cell
    /// </summary>
    private void TransitionToCell(int cellX, int cellY)
    {
        currentCellX = cellX;
        currentCellY = cellY;

        // Calculate target position
        targetCellPosition = new Vector3(
            cellX * cellWidth,
            cellY * cellHeight,
            cameraZ
        );

        // Apply bounds if enabled
        if (useWorldBounds)
        {
            targetCellPosition = ClampToBounds(targetCellPosition);
        }

        isTransitioning = true;
    }

    /// <summary>
    /// Update camera position with smooth transitions
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (!isTransitioning)
        {
            // Ensure camera stays at current cell position
            if (Vector3.Distance(transform.position, currentCellPosition) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, currentCellPosition, Time.deltaTime * panSpeed);
            }
            return;
        }

        // Perform transition based on type
        Vector3 newPosition = transform.position;

        switch (transitionType)
        {
            case TransitionType.Linear:
                newPosition = Vector3.MoveTowards(transform.position, targetCellPosition, panSpeed * Time.deltaTime);
                break;

            case TransitionType.SmoothDamp:
                newPosition = Vector3.SmoothDamp(transform.position, targetCellPosition, ref velocity, 1f / panSpeed);
                break;

            case TransitionType.Exponential:
                newPosition = Vector3.Lerp(transform.position, targetCellPosition, panSpeed * Time.deltaTime);
                break;
        }

        transform.position = newPosition;

        // Check if transition is complete
        if (Vector3.Distance(transform.position, targetCellPosition) < 0.01f)
        {
            transform.position = targetCellPosition;
            currentCellPosition = targetCellPosition;
            isTransitioning = false;
            velocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Clamp camera position to world bounds
    /// </summary>
    private Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, worldBoundsMin.x, worldBoundsMax.x),
            Mathf.Clamp(position.y, worldBoundsMin.y, worldBoundsMax.y),
            position.z
        );
    }

    /// <summary>
    /// Force camera to snap to player's current cell (useful after respawn)
    /// </summary>
    public void ForceSnapToPlayer()
    {
        SnapToPlayerCell();
    }

    /// <summary>
    /// Get current cell coordinates
    /// </summary>
    public Vector2Int GetCurrentCell()
    {
        return new Vector2Int(currentCellX, currentCellY);
    }

    /// <summary>
    /// Set cell size at runtime
    /// </summary>
    public void SetCellSize(float width, float height)
    {
        cellWidth = width;
        cellHeight = height;
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || player == null) return;

        // Draw current cell bounds
        Gizmos.color = Color.green;
        Vector3 cellCenter = new Vector3(currentCellX * cellWidth, currentCellY * cellHeight, 0f);
        Gizmos.DrawWireCube(cellCenter, new Vector3(cellWidth, cellHeight, 0f));

        // Draw player position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, 0.5f);

        // Draw edge trigger zones if using world distance
        if (!useViewportEdgeDetection && cam != null)
        {
            Gizmos.color = Color.yellow;
            float halfWidth = cellWidth * 0.5f;
            float halfHeight = cellHeight * 0.5f;

            // Left edge
            Gizmos.DrawLine(
                new Vector3(cellCenter.x - halfWidth + edgeTriggerDistance, cellCenter.y - halfHeight, 0f),
                new Vector3(cellCenter.x - halfWidth + edgeTriggerDistance, cellCenter.y + halfHeight, 0f)
            );

            // Right edge
            Gizmos.DrawLine(
                new Vector3(cellCenter.x + halfWidth - edgeTriggerDistance, cellCenter.y - halfHeight, 0f),
                new Vector3(cellCenter.x + halfWidth - edgeTriggerDistance, cellCenter.y + halfHeight, 0f)
            );

            // Top edge
            Gizmos.DrawLine(
                new Vector3(cellCenter.x - halfWidth, cellCenter.y + halfHeight - edgeTriggerDistance, 0f),
                new Vector3(cellCenter.x + halfWidth, cellCenter.y + halfHeight - edgeTriggerDistance, 0f)
            );

            // Bottom edge
            Gizmos.DrawLine(
                new Vector3(cellCenter.x - halfWidth, cellCenter.y - halfHeight + edgeTriggerDistance, 0f),
                new Vector3(cellCenter.x + halfWidth, cellCenter.y - halfHeight + edgeTriggerDistance, 0f)
            );
        }

        // Draw target position if transitioning
        if (isTransitioning)
        {
            Gizmos.color = Color.cyan;
            Vector3 targetCenter = new Vector3(currentCellX * cellWidth, currentCellY * cellHeight, 0f);
            Gizmos.DrawWireCube(targetCenter, new Vector3(cellWidth, cellHeight, 0f));

            // Draw arrow showing transition direction
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetCellPosition);
        }

        // Draw world bounds if enabled
        if (useWorldBounds)
        {
            Gizmos.color = Color.red;
            Vector3 boundsCenter = new Vector3(
                (worldBoundsMin.x + worldBoundsMax.x) * 0.5f,
                (worldBoundsMin.y + worldBoundsMax.y) * 0.5f,
                0f
            );
            Vector3 boundsSize = new Vector3(
                worldBoundsMax.x - worldBoundsMin.x,
                worldBoundsMax.y - worldBoundsMin.y,
                0f
            );
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}
