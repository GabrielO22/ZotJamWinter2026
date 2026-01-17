using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidBody;

    private bool onGround = false;
    private bool gravityReversed = false;

    private Vector2 moveInput;

    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public float blinkTime = 0.5f;
    public float blinkCooldownTime = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector2(moveInput.x * moveSpeed, rigidBody.linearVelocity.y);
        // Debug.Log(gravityReversed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (onGround)
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0f);
            rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    public void OnBlink(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        if (gravityReversed) return;
                
        StartCoroutine(reverseGravity());
    }

    IEnumerator reverseGravity()
    {
        gravityReversed = true;
        rigidBody.linearVelocityY *= -1;
        rigidBody.gravityScale *= -1;
        yield return new WaitForSecondsRealtime(blinkTime);
        rigidBody.gravityScale *= -1;
        Debug.Log("1");
        yield return new WaitForSecondsRealtime(blinkCooldownTime);
        gravityReversed = false;
        Debug.Log("2");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            onGround = false;
        }
    }
}
