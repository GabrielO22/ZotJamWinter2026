using UnityEngine;

/// <summary>
/// Trigger zone that sets a checkpoint when the player enters it
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool oneTimeActivation = true;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // Offset from trigger position for spawn point

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer visualIndicator; // Optional sprite that changes color
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip activationSound; // Optional audio clip

    private bool hasBeenActivated = false;
    private Collider2D triggerCollider;
    private AudioSource audioSource;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();

        // Ensure collider is a trigger
        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
            Debug.LogWarning($"{gameObject.name}: Collider was not set as trigger. Auto-fixed.");
        }

        // Setup audio source if sound is provided
        if (activationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = activationSound;
            audioSource.playOnAwake = false;
        }

        // Set initial visual state
        if (visualIndicator != null)
        {
            visualIndicator.color = inactiveColor;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Only activate for player
        if (!collision.gameObject.CompareTag("Player"))
            return;

        // Skip if already activated and one-time only
        if (oneTimeActivation && hasBeenActivated)
            return;

        ActivateCheckpoint();
    }

    /// <summary>
    /// Activates the checkpoint and notifies CheckpointManager
    /// </summary>
    private void ActivateCheckpoint()
    {
        hasBeenActivated = true;

        // Calculate spawn position (trigger position + offset)
        Vector3 checkpointPosition = transform.position + spawnOffset;

        // Register checkpoint with manager
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetCheckpoint(checkpointPosition);
            Debug.Log($"Checkpoint activated at {checkpointPosition}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: CheckpointManager not found! Cannot set checkpoint.");
        }

        // Visual feedback
        if (visualIndicator != null)
        {
            visualIndicator.color = activeColor;
        }

        // Audio feedback
        if (audioSource != null && activationSound != null)
        {
            audioSource.Play();
        }

        // Disable trigger if one-time use
        if (oneTimeActivation)
        {
            triggerCollider.enabled = false;
        }
    }

    /// <summary>
    /// Reset checkpoint to inactive state (called on scene restart)
    /// </summary>
    public void ResetCheckpoint()
    {
        hasBeenActivated = false;
        triggerCollider.enabled = true;

        if (visualIndicator != null)
        {
            visualIndicator.color = inactiveColor;
        }
    }

    // Gizmo for editor visualization
    void OnDrawGizmos()
    {
        // Draw trigger area
        Gizmos.color = hasBeenActivated ? Color.green : Color.yellow;

        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D boxCol)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)boxCol.offset, boxCol.size);
        }
        else if (col is CircleCollider2D circleCol)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
        }

        // Draw spawn point
        Gizmos.color = Color.cyan;
        Vector3 spawnPoint = transform.position + spawnOffset;
        Gizmos.DrawWireSphere(spawnPoint, 0.3f);
        Gizmos.DrawLine(transform.position, spawnPoint);
    }

    void OnDrawGizmosSelected()
    {
        // Draw label for spawn point
        Vector3 spawnPoint = transform.position + spawnOffset;
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(spawnPoint + Vector3.up * 0.5f, "Spawn Point");
        #endif
    }
}
