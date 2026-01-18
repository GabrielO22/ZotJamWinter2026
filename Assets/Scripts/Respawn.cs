using UnityEngine;

/// <summary>
/// Handles player respawn on death triggers (falling, enemy contact)
/// Integrates with WorldStateManager for proper state reset
/// </summary>
public class Respawn : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 respawnPoint = new Vector2(0, 10);
    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            respawnPoint = other.transform.position;
            other.gameObject.SetActive(false);
            return;
        }

        // Check for death trigger (void, spikes, etc.)
        if (other.CompareTag("DeathTrigger") || other.CompareTag("Void"))
        {
            RespawnPlayer();
            return;
        }

        // Check for enemy collision - use EnemyController (not EnemyMovement)
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            // Enemy collision is now handled by EnemyController's OnTriggerEnter2D
            // which calls PlayerHealth.Die()
            // This is kept for backwards compatibility
            Debug.Log("Hit enemy via Respawn trigger");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Check for death trigger
        if (other.CompareTag("DeathTrigger") || other.CompareTag("Void"))
        {
            RespawnPlayer();
            return;
        }

        // Enemy collision handled by EnemyController
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            Debug.Log("Still touching enemy");
        }
    }

    /// <summary>
    /// Respawn player at checkpoint and reset forced blink timer
    /// </summary>
    private void RespawnPlayer()
    {
        // Reset velocity and position
        rb.linearVelocity = Vector2.zero;
        transform.position = respawnPoint;

        // Reset forced blink gauge via WorldStateManager
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.ResetStats();
        }
        
        Debug.Log($"Player respawned at {respawnPoint}");
    }

    /// <summary>
    /// Public method to set respawn point (called by CheckpointTrigger or other systems)
    /// </summary>
    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
        Debug.Log($"Respawn point updated to {respawnPoint}");
    }
}
