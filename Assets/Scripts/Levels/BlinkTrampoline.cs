using UnityEngine;

/// <summary>
/// Trampoline that only bounces player when in Blink world
/// </summary>
public class BlinkTrampoline : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private bool onlyInBlinkWorld = true;

    [Header("Visual Settings")]
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color activeColor = new Color(1f, 0.5f, 0f, 1f); // Orange

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D trampolineCollider;

    private bool isActive = false;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (trampolineCollider == null) trampolineCollider = GetComponent<Collider2D>();

        // Make trigger so it doesn't block player
        if (trampolineCollider != null)
        {
            trampolineCollider.isTrigger = true;
        }
    }

    void OnEnable()
    {
        // Subscribe to world state events
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;

            // Set initial state
            UpdateState(WorldStateManager.Instance.CurrentState);
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

    private void HandleEnterBlink()
    {
        UpdateState(WorldState.Blink);
    }

    private void HandleExitBlink()
    {
        UpdateState(WorldState.Normal);
    }

    private void UpdateState(WorldState currentState)
    {
        if (onlyInBlinkWorld)
        {
            isActive = (currentState == WorldState.Blink);
        }
        else
        {
            isActive = true; // Always active if not blink-only
        }

        // Update visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isActive ? activeColor : inactiveColor;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Only bounce if active
        if (!isActive) return;

        // Check if player entered
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Apply bounce force opposite to gravity direction
                Vector2 bounceDirection = playerRb.gravityScale > 0 ? Vector2.up : Vector2.down;

                // Reset velocity and apply bounce
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
                playerRb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);

                Debug.Log($"{gameObject.name} bounced player with force {bounceForce}!");
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? new Color(1f, 0.5f, 0f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

            // Draw arrow showing bounce direction
            Vector3 center = col.bounds.center;
            Gizmos.DrawLine(center, center + Vector3.up * 2f);
            Gizmos.DrawLine(center + Vector3.up * 2f, center + Vector3.up * 1.5f + Vector3.left * 0.3f);
            Gizmos.DrawLine(center + Vector3.up * 2f, center + Vector3.up * 1.5f + Vector3.right * 0.3f);
        }
    }
}
