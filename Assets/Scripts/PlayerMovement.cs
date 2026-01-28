using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 moveInput;
    private bool jumpQueued;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public string floorTag = "Floor";
    public string enemyTag = "Enemy";
    public bool onGround;

    private BlinkController blink;
    private int dir = 1; // 1 = right, -1 = left
    private CardFlipAnimation cardFlip;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cardFlip = GetComponent<CardFlipAnimation>();
    }

    void FixedUpdate()
    {
        // 1) Update grounded state every physics step (stable)
        onGround = IsGrounded();

        // 2) Horizontal movement
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // 3) Handle sprite direction based on movement input
        if (moveInput.x > 0 && dir != 1)
        {
            TurnAround();
        }
        else if (moveInput.x < 0 && dir != -1)
        {
            TurnAround();
        }

        // 4) Jump (use queued input so it doesn't miss)
        if (jumpQueued && onGround)
        {
            jumpQueued = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else
        {
            // clear the queue if we can't use it soon (optional)
            jumpQueued = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        jumpQueued = true; // queue it; FixedUpdate will apply if grounded
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;

        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        return hit != null && (hit.CompareTag(floorTag) || hit.CompareTag(enemyTag));
    }

    // Visualize ground check circle
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // Blink visual flip: do NOT use quaternion raw values
    public void FlipCharacter()
    {
        transform.eulerAngles = new Vector3(180f, 0f, 0f);
    }

    public void ResetCharacter()
    {
        transform.eulerAngles = Vector3.zero;
    }

    private void TurnAround()
    {
        dir *= -1;

        // Use CardFlipAnimation if available, otherwise fallback to instant flip
        if (cardFlip != null)
        {
            cardFlip.FlipToDirection(dir);
        }
        else
        {
            // Fallback: Flip sprite visually (instant)
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            transform.localScale = s;
        }
    }

    void OnEnable()
    {
        StartCoroutine(CharacterBlinkRoutine());
    }

    private IEnumerator CharacterBlinkRoutine()
    {
        while (BlinkController.Instance == null)
            yield return null;

        blink = BlinkController.Instance;
        blink.enterBlink += FlipCharacter;
        blink.exitBlink += ResetCharacter;
    }

    void OnDisable()
    {
        if (blink != null)
        {
            blink.enterBlink -= FlipCharacter;
            blink.exitBlink -= ResetCharacter;
        }
    }
}
