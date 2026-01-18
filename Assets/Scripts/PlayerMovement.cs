using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private InputSystem_Actions inputActions;

    private bool onGround = false;
    private Vector2 moveInput;
    private int gravityDirection = 1; // 1 = normal down, -1 = inverted up

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayers; // Set in Inspector to include all ground types

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        // Initialize Input Actions
        inputActions = new InputSystem_Actions();

        // Subscribe to input events
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Blink.performed += OnBlink;
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
        }
    }

    private void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector2(moveInput.x * moveSpeed, rigidBody.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (onGround)
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0f);
            // Jump opposite to gravity direction
            Vector2 jumpDirection = gravityDirection > 0 ? Vector2.up : Vector2.down;
            rigidBody.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
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
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check if collision layer is in groundLayers mask
        if (((1 << collision.gameObject.layer) & groundLayers) != 0)
        {
            onGround = false;
        }
    }
}
