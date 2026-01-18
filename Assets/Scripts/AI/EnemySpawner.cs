using UnityEngine;

/// <summary>
/// Manages enemy spawning and registration with CheckpointManager
/// Place this on an empty GameObject and assign enemy prefabs or existing enemies
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy References")]
    [SerializeField] private EnemyController[] enemies;

    [Header("Auto-Find Settings")]
    [SerializeField] private bool autoFindEnemies = true;

    void Start()
    {
        // Auto-find all enemies in scene if enabled
        if (autoFindEnemies)
        {
            enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            Debug.Log($"EnemySpawner found {enemies.Length} enemies in scene");
        }

        // Register all enemies with CheckpointManager
        RegisterAllEnemies();
    }

    /// <summary>
    /// Register all enemies with the CheckpointManager
    /// </summary>
    private void RegisterAllEnemies()
    {
        if (CheckpointManager.Instance == null)
        {
            Debug.LogWarning("CheckpointManager not found! Enemies won't respawn properly.");
            return;
        }

        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                CheckpointManager.Instance.RegisterEnemy(enemy);
            }
        }

        Debug.Log($"Registered {enemies.Length} enemies with CheckpointManager");
    }

    void OnDestroy()
    {
        // Unregister all enemies
        if (CheckpointManager.Instance != null)
        {
            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null)
                {
                    CheckpointManager.Instance.UnregisterEnemy(enemy);
                }
            }
        }
    }

    // Gizmo for debugging
    void OnDrawGizmos()
    {
        if (enemies == null || enemies.Length == 0) return;

        Gizmos.color = Color.red;
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 0.3f);
            }
        }
    }
}
