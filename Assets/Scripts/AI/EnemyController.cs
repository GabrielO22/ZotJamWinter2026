using UnityEngine;

/// <summary>
/// Enemy AI controller with state machine
/// Normal world: Idle/Patrol (non-hostile)
/// Blink world: Chase player as ghost (phases through obstacles)
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D physicsCollider; // For colliding with platforms
    [SerializeField] private Collider2D triggerCollider; // For detecting player contact

    [Header("Enemy Settings")]
    [SerializeField] private EnemySpeed speedType = EnemySpeed.Medium;
    [SerializeField] private float slowSpeed = 0.5f;
    [SerializeField] private float mediumSpeed = 1f;
    [SerializeField] private float fastSpeed = 2f;

    [Header("Visual Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite ghostSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color ghostColor = new Color(0.2f, 0.2f, 0.2f, 0.7f); // Dark semi-transparent
    [SerializeField] private bool flipSpriteOnDirection = true;

    // State tracking
    private EnemyState currentState = EnemyState.Idle;
    private Vector3 spawnPosition;
    private float moveSpeed;
    private float lastXDirection = 1f; // Track last movement direction for sprite flipping

    void Awake()
    {
        // Cache components
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Auto-find colliders if not assigned
        if (physicsCollider == null || triggerCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();

            if (colliders.Length >= 2)
            {
                // First collider = physics (solid), second = trigger (player detection)
                physicsCollider = colliders[0];
                triggerCollider = colliders[1];
            }
            else if (colliders.Length == 1)
            {
                // Only one collider - use it for physics, warn about missing trigger
                physicsCollider = colliders[0];
                Debug.LogWarning($"{gameObject.name}: Only one collider found. Add a second collider for player detection!");
            }
            else
            {
                Debug.LogError($"{gameObject.name}: No colliders found! Add BoxCollider2D components.");
            }
        }

        // Store spawn position for reset
        spawnPosition = transform.position;

        // Set speed based on type
        moveSpeed = GetSpeedForType(speedType);

        // Initialize enemy as idle with gravity enabled (Dynamic body)
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f; // Enable gravity
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        }

        // Configure colliders
        if (physicsCollider != null)
        {
            physicsCollider.isTrigger = false; // Solid collider for platforms
        }

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true; // Trigger collider for player detection
            triggerCollider.enabled = false; // Start disabled - only active during blink
        }
    }

    void Start()
    {
        // Find player if not set
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning($"EnemyController on {gameObject.name}: Player not found!");
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to world state events after a frame delay to ensure WorldStateManager exists
        StartCoroutine(SubscribeToWorldStateManager());
    }

    private System.Collections.IEnumerator SubscribeToWorldStateManager()
    {
        // Wait one frame to ensure WorldStateManager is initialized
        yield return null;

        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;
            Debug.Log($"{gameObject.name} subscribed to WorldStateManager");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: WorldStateManager not found! Enemy won't respond to blinks.");
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

    void Update()
    {
        // Only chase in Chasing state AND when coffee power-up is not active
        if (currentState == EnemyState.Chasing && player != null)
        {
            // Don't chase if coffee power-up is active
            if (WorldStateManager.Instance != null && WorldStateManager.Instance.IsCoffeeActive)
            {
                // Enemy is in ghost form but not chasing during coffee mode
                return;
            }

            ChasePlayer();
        }
    }

    /// <summary>
    /// Handle entering blink state - become ghost and chase player
    /// </summary>
    private void HandleEnterBlink()
    {
        currentState = EnemyState.Chasing;

        // Visual transformation
        if (spriteRenderer != null)
        {
            // Change sprite to ghost form if available
            if (ghostSprite != null)
            {
                spriteRenderer.sprite = ghostSprite;
            }
            spriteRenderer.color = ghostColor;
        }

        // Become Kinematic ghost - no gravity, phase through obstacles
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        // Disable physics collider to phase through obstacles
        if (physicsCollider != null)
        {
            physicsCollider.enabled = false;
        }

        // Enable trigger collider to detect player during blink
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }

        Debug.Log($"{gameObject.name} transformed into ghost - chasing player");
    }

    /// <summary>
    /// Handle exiting blink state - return to normal office worker
    /// </summary>
    private void HandleExitBlink()
    {
        currentState = EnemyState.Idle;

        // Visual restoration
        if (spriteRenderer != null)
        {
            // Change sprite back to normal form if available
            if (normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
            spriteRenderer.color = normalColor;
        }

        // Return to Dynamic with gravity - normal physics-affected enemy
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
        }

        // Re-enable physics collider to collide with platforms
        if (physicsCollider != null)
        {
            physicsCollider.enabled = true;
        }

        // Disable trigger collider in normal mode (enemies are harmless)
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        Debug.Log($"{gameObject.name} returned to normal form");
    }

    /// <summary>
    /// Chase player logic - move toward player ignoring obstacles
    /// </summary>
    private void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Update sprite flipping based on movement direction
        if (flipSpriteOnDirection && Mathf.Abs(direction.x) > 0.01f)
        {
            UpdateSpriteFlip(direction.x);
        }
    }

    /// <summary>
    /// Update sprite flip based on X direction
    /// </summary>
    private void UpdateSpriteFlip(float xDirection)
    {
        if (spriteRenderer == null) return;

        // Only flip when direction actually changes
        if (xDirection < 0 && lastXDirection >= 0)
        {
            spriteRenderer.flipX = true;
            lastXDirection = xDirection;
        }
        else if (xDirection > 0 && lastXDirection <= 0)
        {
            spriteRenderer.flipX = false;
            lastXDirection = xDirection;
        }
    }

    /// <summary>
    /// Reset enemy to spawn position (called on player death)
    /// </summary>
    public void ResetToSpawn()
    {
        transform.position = spawnPosition;
        currentState = EnemyState.Idle;

        if (spriteRenderer != null)
        {
            // Restore normal sprite if available
            if (normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
            spriteRenderer.color = normalColor;
        }

        // Return to Dynamic with gravity - idle enemies are affected by physics
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.linearVelocity = Vector2.zero;
        }

        // Re-enable physics collider
        if (physicsCollider != null)
        {
            physicsCollider.enabled = true;
        }

        // Disable trigger collider (normal mode)
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        Debug.Log($"{gameObject.name} reset to spawn position");
    }

    /// <summary>
    /// Get speed value based on enemy type
    /// </summary>
    private float GetSpeedForType(EnemySpeed type)
    {
        switch (type)
        {
            case EnemySpeed.Slow: return slowSpeed;
            case EnemySpeed.Medium: return mediumSpeed;
            case EnemySpeed.Fast: return fastSpeed;
            default: return mediumSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Only damage player when chasing (in ghost form during blink) AND coffee power-up is not active
        if (currentState == EnemyState.Chasing)
        {
            // Don't damage player if coffee power-up is active
            if (WorldStateManager.Instance != null && WorldStateManager.Instance.IsCoffeeActive)
            {
                return; // Player is safe during coffee mode
            }

            if (collision.gameObject.CompareTag("Player"))
            {
                // Trigger player death
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Die();
                    Debug.Log($"{gameObject.name} killed player during blink state");
                }
                else
                {
                    Debug.LogWarning("Player doesn't have PlayerHealth component!");
                }
            }
        }
        // In normal state (Idle), player passes through harmlessly (no action needed)
    }

    // Gizmo for debugging
    void OnDrawGizmosSelected()
    {
        // Draw spawn position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPosition : transform.position, 0.5f);

        // Draw line to player when chasing
        if (currentState == EnemyState.Chasing && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}

/// <summary>
/// Enemy state machine states
/// </summary>
public enum EnemyState
{
    Idle,       // Standing still in normal world
    Patrolling, // Moving around in normal world (future feature)
    Chasing,    // Pursuing player in blink world
    Disabled    // Inactive (between stages, etc.)
}

/// <summary>
/// Enemy speed variants
/// </summary>
public enum EnemySpeed
{
    Slow,
    Medium,
    Fast
}
