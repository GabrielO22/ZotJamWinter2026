using System.Collections;
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
    public LayerMask groundLayers; // Set in Inspector to include all ground types

    [Header("Visual Settings")]
    public bool flipSpriteOnDirection = true;

    [Header("Debug")]
    public bool showGroundDebug = false;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize Input Actions
        inputActions = new PlayerControls();

        // Subscribe to input events
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Blink.performed += OnBlink;
        inputActions.Player.CoffeePowerUp.performed += OnCoffeePowerUp;
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

    void OnDestroy()
    {
        // Unsubscribe from input events
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Jump.performed -= OnJump;
            inputActions.Player.Blink.performed -= OnBlink;
            inputActions.Player.CoffeePowerUp.performed -= OnCoffeePowerUp;
        }
    }

    private void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector2(moveInput.x * moveSpeed, rigidBody.linearVelocity.y);

        // Update sprite flipping based on movement direction
        if (flipSpriteOnDirection && spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.01f)
        {
            if (moveInput.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveInput.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (showGroundDebug)
        {
            Debug.Log($"Jump pressed! onGround: {onGround}");
        }

        if (onGround)
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0f);
            // Jump opposite to gravity direction
            Vector2 jumpDirection = gravityDirection > 0 ? Vector2.up : Vector2.down;
            rigidBody.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

            if (showGroundDebug)
            {
                Debug.Log($"Jump executed! Direction: {jumpDirection}, Force: {jumpForce}");
            }
        }
        else if (showGroundDebug)
        {
            Debug.Log($"Jump blocked - not on ground");
        }
    }

    public void OnBlink(InputAction.CallbackContext context)
    {
        // Delegate blink to WorldStateManager
        if (WorldStateManager.Instance != null)
        {
            bool success = WorldStateManager.Instance.TryBlink();
            if (success)
            {
                // Reverse Y velocity when entering blink
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y * -1);
            }
        }
        else
        {
            Debug.LogWarning("WorldStateManager not found! Cannot blink.");
        }
    }

    public void OnCoffeePowerUp(InputAction.CallbackContext context)
    {
        // Activate coffee power-up for extended blink without enemy chase
        if (WorldStateManager.Instance != null)
        {
            bool success = WorldStateManager.Instance.ActivateCoffeePowerUp();
            if (success)
            {
                // Reverse Y velocity when entering blink (if not already blinking)
                if (!WorldStateManager.Instance.IsBlinking)
                {
                    rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y * -1);
                }
                Debug.Log("Coffee power-up activated!");
            }
        }
        else
        {
            Debug.LogWarning("WorldStateManager not found! Cannot activate coffee power-up.");
        }
    }

    /// <summary>
    /// Handle entering blink state - flip gravity
    /// </summary>
    private void HandleEnterBlink()
    {
        rigidBody.gravityScale *= -1;
        gravityDirection *= -1;
        Debug.Log($"Player gravity flipped (direction: {gravityDirection})");
    }

    /// <summary>
    /// Handle exiting blink state - restore gravity
    /// </summary>
    private void HandleExitBlink()
    {
        rigidBody.gravityScale *= -1;
        gravityDirection *= -1;
        Debug.Log($"Player gravity restored (direction: {gravityDirection})");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collision layer is in groundLayers mask
        if (((1 << collision.gameObject.layer) & groundLayers) != 0)
        {
            onGround = true;
            if (showGroundDebug)
            {
                Debug.Log($"OnCollisionEnter2D: Ground contact with {collision.gameObject.name} (Layer: {collision.gameObject.layer}), onGround = true");
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Continuously verify ground contact (fixes crumbling platform issue)
        if (((1 << collision.gameObject.layer) & groundLayers) != 0)
        {
            onGround = true;
            if (showGroundDebug)
            {
                Debug.Log($"OnCollisionStay2D: Maintaining ground contact with {collision.gameObject.name}");
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check if collision layer is in groundLayers mask
        if (((1 << collision.gameObject.layer) & groundLayers) != 0)
        {
            if (showGroundDebug)
            {
                Debug.Log($"OnCollisionExit2D: Lost ground contact with {collision.gameObject.name}, checking for other contacts...");
            }

            // Don't immediately set to false - check if we have other ground contacts first
            CheckGroundContacts();
        }
    }

    void Update()
    {
        // Periodic ground check to catch edge cases
        // This runs LESS frequently to avoid overriding OnCollisionStay2D
        if (Time.frameCount % 10 == 0) // Only every 10 frames
        {
            if (onGround)
            {
                CheckGroundContacts();
            }
        }
    }

    /// <summary>
    /// Check all current contacts to verify ground status
    /// </summary>
    private void CheckGroundContacts()
    {
        bool hasGroundContact = false;
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int contactCount = rigidBody.GetContacts(contacts);

        if (showGroundDebug)
        {
            Debug.Log($"CheckGroundContacts: Found {contactCount} total contacts");
        }

        for (int i = 0; i < contactCount; i++)
        {
            if (contacts[i].collider != null)
            {
                int contactLayer = contacts[i].collider.gameObject.layer;
                bool isGroundLayer = ((1 << contactLayer) & groundLayers) != 0;

                if (showGroundDebug)
                {
                    Debug.Log($"  Contact {i}: {contacts[i].collider.gameObject.name} (Layer: {contactLayer}, IsGround: {isGroundLayer})");
                }

                if (isGroundLayer)
                {
                    hasGroundContact = true;
                    break;
                }
            }
        }

        if (!hasGroundContact && onGround)
        {
            onGround = false;
            if (showGroundDebug)
            {
                Debug.Log($"CheckGroundContacts: No ground contacts found, onGround = false");
            }
        }
        else if (hasGroundContact && !onGround)
        {
            onGround = true;
            if (showGroundDebug)
            {
                Debug.Log($"CheckGroundContacts: Ground contact found, onGround = true");
            }
        }
    }
}
