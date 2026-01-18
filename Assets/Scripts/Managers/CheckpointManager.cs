using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages checkpoints and player respawn
/// Resets player position, enemies, and world state on death
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Checkpoint Settings")]
    [SerializeField] private Vector3 currentCheckpoint;
    [SerializeField] private bool hasCheckpoint = false;

    [Header("Stats")]
    [SerializeField] private int deathCount = 0;

    // Enemy tracking
    private List<EnemyController> trackedEnemies = new List<EnemyController>();

    // Events
    public event Action OnCheckpointActivated;
    public event Action OnPlayerRespawn;

    // Properties
    public Vector3 CurrentCheckpoint => currentCheckpoint;
    public bool HasCheckpoint => hasCheckpoint;
    public int DeathCount => deathCount;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Find player if not set
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }

        // Subscribe to player death
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += HandlePlayerDeath;
        }

        // Set initial checkpoint to player's starting position
        if (!hasCheckpoint && player != null)
        {
            SetCheckpoint(player.position);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from player death
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= HandlePlayerDeath;
        }
    }

    /// <summary>
    /// Set a new checkpoint position
    /// </summary>
    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position;
        hasCheckpoint = true;

        Debug.Log($"Checkpoint set at {position}");
        OnCheckpointActivated?.Invoke();
    }

    /// <summary>
    /// Register an enemy to be tracked for respawn
    /// </summary>
    public void RegisterEnemy(EnemyController enemy)
    {
        if (!trackedEnemies.Contains(enemy))
        {
            trackedEnemies.Add(enemy);
            Debug.Log($"Enemy registered: {enemy.gameObject.name}");
        }
    }

    /// <summary>
    /// Unregister an enemy
    /// </summary>
    public void UnregisterEnemy(EnemyController enemy)
    {
        if (trackedEnemies.Contains(enemy))
        {
            trackedEnemies.Remove(enemy);
            Debug.Log($"Enemy unregistered: {enemy.gameObject.name}");
        }
    }

    /// <summary>
    /// Handle player death - respawn at checkpoint
    /// </summary>
    private void HandlePlayerDeath()
    {
        deathCount++;
        Debug.Log($"Player death #{deathCount}. Respawning at checkpoint...");

        // Small delay before respawn
        Invoke(nameof(RespawnPlayer), 0.5f);
    }

    /// <summary>
    /// Respawn player at checkpoint
    /// </summary>
    private void RespawnPlayer()
    {
        if (!hasCheckpoint || player == null) return;

        // Reset player position
        player.position = currentCheckpoint;

        // Restore player health
        if (playerHealth != null)
        {
            playerHealth.Respawn();
        }

        // Reset all enemies to spawn positions
        ResetAllEnemies();

        // Return to normal world if in blink
        if (WorldStateManager.Instance != null && WorldStateManager.Instance.IsBlinking)
        {
            // Force exit blink state (will be handled by WorldStateManager)
            Debug.Log("Forcing return to normal world after respawn");
        }

        Debug.Log($"Player respawned at {currentCheckpoint}");
        OnPlayerRespawn?.Invoke();
    }

    /// <summary>
    /// Reset all tracked enemies to their spawn positions
    /// </summary>
    private void ResetAllEnemies()
    {
        foreach (EnemyController enemy in trackedEnemies)
        {
            if (enemy != null)
            {
                enemy.ResetToSpawn();
            }
        }

        Debug.Log($"Reset {trackedEnemies.Count} enemies to spawn positions");
    }

    /// <summary>
    /// Reset all stats (for new game)
    /// </summary>
    public void ResetStats()
    {
        deathCount = 0;
        Debug.Log("CheckpointManager stats reset");
    }

    // Gizmo for debugging
    void OnDrawGizmos()
    {
        if (hasCheckpoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentCheckpoint, 1f);
            Gizmos.DrawLine(currentCheckpoint, currentCheckpoint + Vector3.up * 2f);
        }
    }
}
