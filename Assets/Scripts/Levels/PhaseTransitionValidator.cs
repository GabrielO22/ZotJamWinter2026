using UnityEngine;

/// <summary>
/// Validates player position during phase transitions to prevent getting stuck in walls
/// If player overlaps with solid geometry in target world state, finds nearest safe position
/// </summary>
public class PhaseTransitionValidator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float searchRadius = 5f; // Max distance to search for safe position
    [SerializeField] private float searchStep = 0.5f; // Grid resolution for search
    [SerializeField] private LayerMask normalSolidLayer;
    [SerializeField] private LayerMask blinkSolidLayer;

    private BoxCollider2D playerCollider;

    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("PhaseTransitionValidator requires a BoxCollider2D on the player!");
        }
    }

    void OnEnable()
    {
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += ValidateTransitionToBlink;
            WorldStateManager.Instance.OnExitBlink += ValidateTransitionToNormal;
        }
    }

    void OnDisable()
    {
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink -= ValidateTransitionToBlink;
            WorldStateManager.Instance.OnExitBlink -= ValidateTransitionToNormal;
        }
    }

    /// <summary>
    /// Check if transitioning to blink state would cause collision
    /// </summary>
    private void ValidateTransitionToBlink()
    {
        ValidateTransition(blinkSolidLayer);
    }

    /// <summary>
    /// Check if transitioning to normal state would cause collision
    /// </summary>
    private void ValidateTransitionToNormal()
    {
        ValidateTransition(normalSolidLayer);
    }

    /// <summary>
    /// Validate transition and teleport player if stuck
    /// </summary>
    private void ValidateTransition(LayerMask targetWorldLayer)
    {
        if (playerCollider == null) return;

        Vector2 playerCenter = playerCollider.bounds.center;
        Vector2 playerSize = playerCollider.bounds.size;

        // Check if player overlaps with target world solids
        Collider2D overlap = Physics2D.OverlapBox(playerCenter, playerSize * 0.9f, 0f, targetWorldLayer);

        if (overlap != null)
        {
            Debug.LogWarning($"Player stuck in wall after transition! Searching for safe position...");

            Vector2 safePosition = FindNearestSafePosition(playerCenter, playerSize, targetWorldLayer);

            if (safePosition != Vector2.zero)
            {
                transform.position = safePosition;
                Debug.Log($"Player teleported to safe position: {safePosition}");
            }
            else
            {
                Debug.LogError("Could not find safe position for player!");
            }
        }
    }

    /// <summary>
    /// Find nearest non-colliding position using radial search
    /// </summary>
    private Vector2 FindNearestSafePosition(Vector2 currentPos, Vector2 playerSize, LayerMask targetLayer)
    {
        // Search in expanding circles
        for (float radius = searchStep; radius <= searchRadius; radius += searchStep)
        {
            // Check 8 directions at this radius
            for (int angle = 0; angle < 360; angle += 45)
            {
                float radians = angle * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
                Vector2 testPos = currentPos + offset;

                // Test if this position is clear
                Collider2D overlap = Physics2D.OverlapBox(testPos, playerSize * 0.9f, 0f, targetLayer);

                if (overlap == null)
                {
                    return testPos;
                }
            }
        }

        // No safe position found
        return Vector2.zero;
    }

    // Gizmo for debugging
    void OnDrawGizmosSelected()
    {
        if (playerCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }
    }
}
