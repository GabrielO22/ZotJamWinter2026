using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 respawnPoint = new Vector2(0, 10);

    public ManaController manaController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (manaController == null)
            manaController = GetComponent<ManaController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            respawnPoint = other.transform.position;
            other.gameObject.SetActive(false);
            return;
        }

        // Works even if the collider is on a child object
        EnemyMovement enemy = other.GetComponentInParent<EnemyMovement>();

        if (enemy != null)
        {
            Debug.Log("Hit enemy. isChasing=" + enemy.isChasing);

            if (enemy.isChasing)
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = respawnPoint;

                if (manaController != null)
                    manaController.refill();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        EnemyMovement enemy = other.GetComponentInParent<EnemyMovement>();

        if (enemy != null)
        {
            Debug.Log("Hit enemy. isChasing=" + enemy.isChasing);

            if (enemy.isChasing)
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = respawnPoint;

                if (manaController != null)
                    manaController.refill();
            }
        }
    }
}
