using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidBody;

    private bool onGround = false;

    private Vector2 moveInput;

    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public float blinkTime = 0.5f;
    public float blinkCooldownTime = 2f;

    private bool canBlink = true;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            onGround = false;
        }
    }

    public void enterBlink(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!canBlink) return;

        StartCoroutine(blinkRoutine());
        // changes to players behavior in here
    }

    private IEnumerator blinkRoutine()
    {
        canBlink = false;

        WorldState.Instance.enteringBlink();
        rigidBody.gravityScale *= -1;

        yield return new WaitForSecondsRealtime(blinkTime);

        rigidBody.gravityScale *= -1;
        WorldState.Instance.exitingBlink();
        yield return new WaitForSecondsRealtime(blinkTime);
        canBlink = true;
    }
  
}
