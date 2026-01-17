using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidBody;

    private bool onGround = false;

    private Vector2 moveInput;

    public float moveSpeed = 2f;
    public float jumpForce = 2f;
    public float blinkTime = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
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
        if (!context.performed) return;

        if (onGround)
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0f);
            rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    public void OnBlink(InputAction.CallbackContext context)
    {
        rigidBody.linearVelocityY = rigidBody.linearVelocityY * -1;
        StartCoroutine(reverseGravity());
        
    }

    IEnumerator reverseGravity()
    {
        rigidBody.gravityScale *= -1;
        Debug.Log("Flip 1");
        yield return new WaitForSeconds(blinkTime);
        rigidBody.gravityScale *= -1;
        Debug.Log("Flip 2");
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
