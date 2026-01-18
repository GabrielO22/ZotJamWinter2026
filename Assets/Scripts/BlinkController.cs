using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class BlinkController : MonoBehaviour
{
    private bool canBlink = true;
    public GameObject player;
    private Rigidbody2D playerRigidBody;
    public float blinkTime;
    public float blinkCooldown;

    void Awake()
    {
        playerRigidBody = player.GetComponent<Rigidbody2D>();
    }

    public void OnBlink(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!canBlink) return;


        StartCoroutine(reverseGravity());
    }

    IEnumerator reverseGravity()
    {
        canBlink = false;
        playerRigidBody.gravityScale = -1;
        yield return new WaitForSecondsRealtime(blinkTime);
        playerRigidBody.gravityScale = -1;
        yield return new WaitForSecondsRealtime(blinkCooldown);
        canBlink = true;


    }

    public void forceBlink()
    {
        if (canBlink)
        {
            StartCoroutine(reverseGravity());
        }
    }
}
