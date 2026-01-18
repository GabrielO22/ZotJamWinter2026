using UnityEngine;

/// <summary>
/// Camera controller that follows the player with smooth movement
/// Supports room transitions and orthographic size adjustments
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;        // Target to follow (player)
    public float followSpeed = 2f;
    public float fixedX;            // X position (if locked)
    public float offsetZ = -10f;
    public bool lockX = true;       // Lock X axis

    [Header("Room Transition")]
    private bool isTransitioning = false;
    private Vector3 transitionTarget;
    private float transitionSpeed;
    private float targetOrthographicSize;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (lockX)
        {
            fixedX = transform.position.x;      // Remember the starting x point
        }
    }

    void Update()
    {
        if (isTransitioning)
        {
            UpdateRoomTransition();
        }
        else
        {
            UpdatePlayerFollow();
        }
    }

    /// <summary>
    /// Standard player follow behavior
    /// </summary>
    private void UpdatePlayerFollow()
    {
        if (target == null) return;

        float newY = target.position.y;
        float newX = lockX ? fixedX : target.position.x;

        Vector3 cameraPosition = new Vector3(newX, newY, offsetZ);
        Vector3 nextPosition = Vector3.Lerp(transform.position, cameraPosition, followSpeed * Time.deltaTime);

        transform.position = nextPosition;
    }

    /// <summary>
    /// Room transition behavior - smoothly move to target position
    /// </summary>
    private void UpdateRoomTransition()
    {
        // Smooth position transition
        transform.position = Vector3.Lerp(transform.position, transitionTarget, transitionSpeed * Time.deltaTime);

        // Smooth orthographic size transition
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, transitionSpeed * Time.deltaTime);
        }

        // Check if transition is complete
        float distanceToTarget = Vector3.Distance(transform.position, transitionTarget);
        float sizeDifference = cam != null ? Mathf.Abs(cam.orthographicSize - targetOrthographicSize) : 0f;

        if (distanceToTarget < 0.1f && sizeDifference < 0.1f)
        {
            transform.position = transitionTarget;
            if (cam != null)
            {
                cam.orthographicSize = targetOrthographicSize;
            }

            isTransitioning = false;

            // Update WorldStateManager's cached camera position for blink shake effects
            if (WorldStateManager.Instance != null)
            {
                WorldStateManager.Instance.UpdateCameraPosition();
            }

            Debug.Log("Room transition complete");
        }
    }

    /// <summary>
    /// Transition camera to a new room position
    /// Called by RoomTrigger
    /// </summary>
    public void TransitionToRoom(Vector3 targetPosition, float targetSize, float speed)
    {
        isTransitioning = true;
        transitionTarget = new Vector3(targetPosition.x, targetPosition.y, offsetZ);
        transitionSpeed = speed;
        targetOrthographicSize = targetSize;

        Debug.Log($"Starting camera transition to {transitionTarget} with size {targetSize}");
    }

    /// <summary>
    /// Immediately snap camera to a position (no smooth transition)
    /// </summary>
    public void SnapToPosition(Vector3 position, float orthographicSize)
    {
        isTransitioning = false;
        transform.position = new Vector3(position.x, position.y, offsetZ);

        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = orthographicSize;
        }

        // Update WorldStateManager's cached camera position for blink shake effects
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.UpdateCameraPosition();
        }

        Debug.Log($"Camera snapped to {position}");
    }

    /// <summary>
    /// Resume following the player
    /// </summary>
    public void ResumeFollowingPlayer()
    {
        isTransitioning = false;
        Debug.Log("Camera resumed following player");
    }
}
