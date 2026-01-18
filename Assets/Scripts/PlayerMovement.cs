using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidBody;

    public bool onGround = false;

    private Vector2 moveInput;

    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    private BlinkController blink;


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

    public void FlipCharacter()
    {
        this.transform.rotation = new Quaternion(180, 0, 0, 0);
    }

    public void ResetCharacter()
    {
        this.transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    private void OnEnable()
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

    private void OnDisable()
    {
        blink.enterBlink -= FlipCharacter;
        blink.exitBlink -= ResetCharacter;
    }
}