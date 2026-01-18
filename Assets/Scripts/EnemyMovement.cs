using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public float EnemySpeed = 1f;
    public bool isChasing = false;
    private SpriteChanger spriteChanger;

    private BlinkController blink;

    public float patrolSpeed = 1f;
    public Transform edgeCheck;
    public Transform wallCheck;
    private int dir = -1;
    public float edgeRayLength = 0.4f;
    public float wallRayLength = 0.2f;
    public LayerMask groundLayer;



    private void Awake()
    {
        spriteChanger = GetComponent<SpriteChanger>();
    }
    private void OnEnable()
    {
        StartCoroutine(HookBlinkEvents());
    }

    private IEnumerator HookBlinkEvents()
    {
        // wait until BlinkController.Istance exists
        while (BlinkController.Instance == null)
            yield return null;

        blink = BlinkController.Instance;
        blink.enterBlink += StartChasing;
        blink.enterBlink += spriteChanger.changeSprite;
        // change to evil sprite

        blink.exitBlink += StopChasing;
        blink.exitBlink += spriteChanger.revertSprite;
        // revert back to npc sprite

        Debug.Log($"Enemy subscribed to BlinkController on {blink.gameObject.name}");
    }

    private void OnDisable()
    {
        if (blink != null)
        {
            blink.enterBlink -= StartChasing;
            blink.enterBlink -= spriteChanger.changeSprite;

            blink.exitBlink -= StopChasing;
            blink.exitBlink -= spriteChanger.revertSprite;
        }
    }

    void Update()
    {
        if (player == null) return;

        if (!isChasing)
        {
            IdleMovement();
        }
        else
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * EnemySpeed * Time.deltaTime;
        }
        
    }

    private void StartChasing()
    {
        isChasing = true;
    }

    private void StopChasing()
    {
        isChasing = false;
    }
    private void IdleMovement()
    {
        // Move in current patrol direction
        transform.position += Vector3.right * dir * patrolSpeed * Time.deltaTime;

        // Raycast down to check for ground ahead
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeRayLength);

        bool groundAhead = edgeHit.collider != null && edgeHit.collider.CompareTag("Floor");

        // Raycast forward to check for wall
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * dir, wallRayLength);

        bool wallAhead = wallHit.collider != null && wallHit.collider.CompareTag("Floor");

        // Turn around if at edge or hitting wall
        if (!groundAhead || wallAhead)
            TurnAround();
    }


    private void TurnAround()
    {
        Debug.Log("SPIN");
        dir *= -1;

        // Optional: flip sprite
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        Debug.Log(edgeCheck);
        Debug.Log(wallCheck);
        if (edgeCheck != null)
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * edgeRayLength);

        if (wallCheck != null)
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * dir * wallRayLength);
    }

}
