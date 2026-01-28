using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform edgeCheck;   // empty child at front foot
    public Transform wallCheck;   // empty child at chest
    private SpriteChanger spriteChanger;

    private BlinkController blink;
    private Rigidbody2D rb;
    private Collider2D[] colliders;
    private CardFlipAnimation cardFlip;

    [Header("Chase")]
    public float enemySpeed = 1f;
    public bool isChasing = false;

    [Header("Patrol")]
    public float patrolSpeed = 1f;
    public float edgeRayLength = 0.4f;
    public float wallRayLength = 0.2f;
    public float turnCooldown = 0.2f;

    [Tooltip("Tag used for platforms/walls (must match exactly).")]
    public string floorTag = "Floor";
    public string playerTag = "Player";

    private int dir = 1; // 1 = right, -1 = left
    private float nextTurnTime = 0f;

    private Vector2 desiredVelocity;

    // Store initial position for reset functionality
    private Vector3 initialPosition;
    private int initialDir;
    private Vector3 initialScale;

    void Awake()
    {
        spriteChanger = GetComponent<SpriteChanger>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();
        cardFlip = GetComponent<CardFlipAnimation>();

        // Store initial position and orientation
        initialPosition = transform.position;
        initialDir = dir;
        initialScale = transform.localScale;

        // If you have a Rigidbody2D, these reduce wobble a lot
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    void OnEnable()
    {
        StartCoroutine(HookBlinkEvents());
    }

    IEnumerator HookBlinkEvents()
    {
        while (BlinkController.Instance == null)
            yield return null;

        blink = BlinkController.Instance;

        blink.enterBlink += StartChasing;
        if (spriteChanger != null) blink.enterBlink += spriteChanger.changeSprite;

        blink.exitBlink += StopChasing;
        if (spriteChanger != null) blink.exitBlink += spriteChanger.revertSprite;

        Debug.Log($"Enemy subscribed to BlinkController on {blink.gameObject.name}");
    }

    void OnDisable()
    {
        if (blink != null)
        {
            blink.enterBlink -= StartChasing;
            if (spriteChanger != null) blink.enterBlink -= spriteChanger.changeSprite;

            blink.exitBlink -= StopChasing;
            if (spriteChanger != null) blink.exitBlink -= spriteChanger.revertSprite;
        }
    }

    void Update()
    {
        // Decide what velocity we want this frame (movement happens in FixedUpdate)
        if (isChasing)
            ChaseMovement();
        else
            IdleMovement();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        rb.MovePosition(rb.position + desiredVelocity * Time.fixedDeltaTime);
    }

    private void ChaseMovement()
    {
        if (player == null)
        {
            desiredVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = (player.position - transform.position).normalized;
        desiredVelocity = toPlayer * enemySpeed;

        // Determine which direction enemy should face based on player position
        int targetDir = toPlayer.x >= 0 ? 1 : -1;

        // Flip if direction changed
        if (targetDir != dir)
        {
            TurnAround();
        }
    }

    private void IdleMovement()
    {
        // If checks aren't assigned, don't move (avoids null spam)
        if (edgeCheck == null || wallCheck == null)
        {
            desiredVelocity = Vector2.zero;
            return;
        }

        // Raycast down to check for ground ahead
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeRayLength);
        bool groundAhead = edgeHit.collider != null && edgeHit.collider.CompareTag(floorTag);

        // Raycast forward to check for wall ahead (only treat Floor-tagged things as walls)
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * dir, wallRayLength);
        bool wallAhead = wallHit.collider != null && (wallHit.collider.CompareTag(floorTag) || wallHit.collider.CompareTag(playerTag));

         

        // Turn around if at edge or hitting wall � but not every frame (prevents wobble/spin)
        if (Time.time >= nextTurnTime && (!groundAhead || wallAhead))
        {
            TurnAround();
            nextTurnTime = Time.time + turnCooldown;
        }

        // Patrol velocity
        desiredVelocity = new Vector2(dir * patrolSpeed, 0f);
    }

    private void StartChasing()
    {
        isChasing = true;
        SetGhostMode(true);
    }

    private void StopChasing()
    {
        isChasing = false;
        SetGhostMode(false);

        // Reset position if enabled in BlinkController
        if (blink != null && blink.resetEnemyPositionsOnExitBlink)
        {
            ResetToInitialPosition();
        }
    }

    private void ResetToInitialPosition()
    {
        // Reset position
        transform.position = initialPosition;

        // Reset velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Reset direction - use InstantFlip if CardFlipAnimation exists
        dir = initialDir;
        if (cardFlip != null)
        {
            cardFlip.InstantFlip(initialDir);
        }
        else
        {
            transform.localScale = initialScale;
        }

        // Reset velocity
        desiredVelocity = Vector2.zero;
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
            // Fallback: flip sprite visually (instant)
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            transform.localScale = s;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (edgeCheck != null)
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * edgeRayLength);

        if (wallCheck != null)
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * dir * wallRayLength);
    }
    private void SetGhostMode(bool ghost)
    {
        // ghost = true => pass through colliders
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].isTrigger = ghost;
    }

}
