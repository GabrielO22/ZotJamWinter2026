using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;


public class BlinkController : MonoBehaviour
{
    public bool canBlink = true;
    public bool isBlinking = false;
    public GameObject player;
    private Rigidbody2D playerRigidBody;
    public float blinkTime;
    public float blinkCooldown;
    public ManaController manaController;
    public GameObject normalplatform;
    public GameObject evilplatform;

    public event Action enterBlink;
    public event Action exitBlink;

    public static BlinkController Instance { get; private set; }

    void Awake()
    {
        playerRigidBody = player.GetComponent<Rigidbody2D>();
        if (manaController == null)
        {
            manaController = player.GetComponent<ManaController>();
        }
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnBlink(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!canBlink) return;


        StartCoroutine(blinkRoutine());
        
    }

    IEnumerator blinkRoutine()
    {
        if (manaController != null)
        {
            manaController.refill();
        }

        canBlink = false;
        enterBlink?.Invoke();
        playerRigidBody.gravityScale *= -0.3125f;
        yield return new WaitForSecondsRealtime(blinkTime);
        playerRigidBody.gravityScale *= -3.2f;
        exitBlink?.Invoke();
        yield return new WaitForSecondsRealtime(blinkCooldown);
        canBlink = true;
        
    }

    public void forceBlink()
    {
        if (canBlink)
        {
            StartCoroutine(blinkRoutine());
        }
    }
}
