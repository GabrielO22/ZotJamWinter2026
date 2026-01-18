using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private PlayerControls inputActions;
    private SpriteRenderer spriteRenderer;

    private bool onGround = false;
    private Vector2 moveInput;
    private int gravityDirection = 1; // 1 = normal down, -1 = inverted up

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 2f;

    [Header("Physics Material")]
    public PhysicsMaterial2D zeroFrictionMaterial; // Assign in Inspector to prevent wall sticking

    [Header("Visual Settings")]
    public bool flipSpriteOnDirection = true;

    [Header("Debug")]
    public bool showGroundDebug = false;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Apply zero friction material to prevent wall sticking
        if (zeroFrictionMaterial == null)
        {
            // Create zero friction material if not assigned
            zeroFrictionMaterial = new PhysicsMaterial2D("ZeroFriction");
            zeroFrictionMaterial.friction = 0f;
            zeroFrictionMaterial.bounciness = 0f;
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.sharedMaterial = zeroFrictionMaterial;
        }

        // Initialize Input Actions
        inputActions = new PlayerControls();

        // Subscribe to input events
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.blink.performed += OnBlink; // Note: lowercase 'blink' in input actions
    }

    void OnEnable()
    {
        inputActions?.Player.Enable();

        // Subscribe to world state events
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink += HandleExitBlink;
        }
    }

    void OnDisable()
    {
        inputActions?.Player.Disable();

        // Unsubscribe from world state events
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink -= HandleEnterBlink;
            WorldStateManager.Instance.OnExitBlink -= HandleExitBlink;
        }
    }

    void Update()
    {
        // Check ground contact by detecting collision with top edge of any solid collider
        onGround = CheckGroundContact();

        if (showGroundDebug)
        {
            // Visual debug for ground contact
            Vector2 rayOrigin = (Vector2)transform.position + Vector2.down * 0.5f;
            Debug.DrawRay(rayOrigin, Vector2.down * gravityDirection * 0.2f, onGround ? Color.green : Color.red);
        }
    }

    /// <summary>
    /// Check if player is contacting the top edge of any solid collider
    /// </summary>
    private bool CheckGroundContact()
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return false;

        // Get the bounds of the player collider
        Bounds bounds = playerCollider.bounds;

        // Define bottom edge position based on gravity direction
        Vector2 bottomEdge;
        Vector2 checkDirection;

        if (gravityDirection == 1) // Normal gravity (falling down)
        {
            bottomEdge = new Vector2(bounds.center.x, bounds.min.y);
            checkDirection = Vector2.down;
        }
        else // Inverted gravity (falling up)
        {
            bottomEdge = new Vector2(bounds.center.x, bounds.max.y);
            checkDirection = Vector2.up;
        }

        // Cast a short ray slightly beyond the collider edge to detect ground
        float checkDistance = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(bottomEdge, checkDirection, checkDistance);

        if (hit.collider != null && hit.collider != playerCollider)
        {
            // Check if the hit is on the appropriate surface (top for normal, bottom for inverted)
            // by verifying the collision normal points opposite to gravity
            float dotProduct = Vector2.Dot(hit.normal, -checkDirection);
            return dotProduct > 0.7f; // Allow slight tolerance for angled surfaces
        }

        return false;
    }

    void FixedUpdate()
    {
        // Horizontal movement
        Vector2 velocity = rigidBody.linearVelocity;
        velocity.x = moveInput.x * moveSpeed;
        rigidBody.linearVelocity = velocity;

        // Flip sprite based on movement direction
        if (flipSpriteOnDirection && spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.01f)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }
    }

    /// <summary>
    /// Handle movement input
    /// </summary>
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handle jump input
    /// </summary>
    private void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (onGround)
        {
            // Reset vertical velocity and apply jump force
            Vector2 velocity = rigidBody.linearVelocity;
            velocity.y = 0f;
            rigidBody.linearVelocity = velocity;

            // Apply impulse force in gravity direction
            rigidBody.AddForce(Vector2.up * jumpForce * gravityDirection, ForceMode2D.Impulse);

            Debug.Log($"Player jumped! OnGround: {onGround}, Gravity: {gravityDirection}");
        }
    }

    /// <summary>
    /// Handle blink input
    /// </summary>
    private void OnBlink(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (WorldStateManager.Instance != null)
        {
            bool success = WorldStateManager.Instance.TryBlink();
            if (success)
            {
                Debug.Log("Player triggered blink");
            }
        }
    }

    // NOTE: Coffee power-up input not configured in PlayerControls.inputactions
    // To enable, add a "CoffeePowerUp" action to the Player action map and bind it to Q key
    // Then uncomment below and subscribe in Awake():
    
    private void OnCoffeePowerUp(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (WorldStateManager.Instance != null)
        {
            bool success = WorldStateManager.Instance.ActivateCoffeePowerUp();
            if (success)
            {
                Debug.Log("Player activated coffee power-up");
            }
        }
    }
    

    /// <summary>
    /// Handle entering blink state - flip gravity
    /// </summary>
    private void HandleEnterBlink()
    {
        gravityDirection = -1;
        rigidBody.gravityScale *= -1f;
        Debug.Log("Player gravity flipped (entering blink)");
    }

    /// <summary>
    /// Handle exiting blink state - restore gravity
    /// </summary>
    private void HandleExitBlink()
    {
        gravityDirection = 1;
        rigidBody.gravityScale *= -1f;
        Debug.Log("Player gravity restored (exiting blink)");
    }
}
