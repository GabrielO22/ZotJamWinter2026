using System.Collections;
using UnityEngine;

/// <summary>
/// Platform that crumbles when stepped on and re-forms when you blink
/// </summary>
public class CrumblingPlatform : MonoBehaviour
{
    [Header("Crumble Settings")]
    [SerializeField] private float crumbleDelay = 0.3f; // Time before platform crumbles
    [SerializeField] private bool oneTimeUse = false; // If true, doesn't reform on blink

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color crumblingColor = Color.red;
    [SerializeField] private Color crumbledColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D platformCollider;
    [SerializeField] private Rigidbody2D platformRigidbody;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isCrumbling = false;
    private bool hasCrumbled = false;
    private bool hasBeenUsed = false;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (platformCollider == null) platformCollider = GetComponent<Collider2D>();
        if (platformRigidbody == null) platformRigidbody = GetComponent<Rigidbody2D>();

        // Ensure platform has a Static Rigidbody2D
        if (platformRigidbody == null)
        {
            platformRigidbody = gameObject.AddComponent<Rigidbody2D>();
            platformRigidbody.bodyType = RigidbodyType2D.Static;
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Auto-added Static Rigidbody2D");
            }
        }
        else
        {
            // Ensure it's set to Static
            if (platformRigidbody.bodyType != RigidbodyType2D.Static)
            {
                platformRigidbody.bodyType = RigidbodyType2D.Static;
                if (showDebugLogs)
                {
                    Debug.Log($"{gameObject.name}: Set Rigidbody2D to Static");
                }
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: CrumblingPlatform initialized");
        }
    }

    void OnEnable()
    {
        // Subscribe to blink events to reform platform
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink += HandleBlink;
        }
    }

    void OnDisable()
    {
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.OnEnterBlink -= HandleBlink;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if player stepped on platform
        if (collision.gameObject.CompareTag("Player"))
        {
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Player collision detected (isCrumbling: {isCrumbling}, hasCrumbled: {hasCrumbled})");
            }

            if (!isCrumbling && !hasCrumbled)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"{gameObject.name}: Starting crumble sequence");
                }
                StartCoroutine(CrumbleSequence());
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Maintain collision for player jump detection
        // This is necessary for PlayerMovement's ground detection
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Player left platform");
        }
    }

    /// <summary>
    /// Crumble sequence: delay -> disable collision
    /// </summary>
    private IEnumerator CrumbleSequence()
    {
        isCrumbling = true;

        // Only mark as used if one-time use is enabled
        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }

        // Visual feedback: platform turns red
        if (spriteRenderer != null)
        {
            spriteRenderer.color = crumblingColor;
        }

        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Crumbling in {crumbleDelay}s... (Player can still jump during this time)");
        }

        // Wait before crumbling
        yield return new WaitForSeconds(crumbleDelay);

        // Crumble: disable collision
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Collider disabled");
            }
        }

        // Visual feedback: platform fades
        if (spriteRenderer != null)
        {
            spriteRenderer.color = crumbledColor;
        }

        hasCrumbled = true;
        isCrumbling = false;

        if (showDebugLogs)
        {
            Debug.Log($"{gameObject.name}: Fully crumbled!");
        }
    }

    /// <summary>
    /// Reform platform when player blinks (unless one-time use)
    /// </summary>
    private void HandleBlink()
    {
        if (oneTimeUse && hasBeenUsed)
        {
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: One-time use, not reforming");
            }
            return; // Don't reform if one-time use
        }

        if (hasCrumbled || isCrumbling)
        {
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Reforming platform...");
            }

            // Stop crumbling if in progress
            StopAllCoroutines();

            // Reform platform
            isCrumbling = false;
            hasCrumbled = false;

            // Re-enable collider
            if (platformCollider != null)
            {
                platformCollider.enabled = true;
                if (showDebugLogs)
                {
                    Debug.Log($"{gameObject.name}: Collider re-enabled");
                }
            }

            // Force physics update by disabling/re-enabling Rigidbody2D
            if (platformRigidbody != null)
            {
                platformRigidbody.bodyType = RigidbodyType2D.Kinematic;
                platformRigidbody.bodyType = RigidbodyType2D.Static;
                if (showDebugLogs)
                {
                    Debug.Log($"{gameObject.name}: Forced physics refresh");
                }
            }

            // Restore visual
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }

            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Platform fully reformed!");
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log($"{gameObject.name}: Blink triggered but platform is not crumbled, no action taken");
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = hasCrumbled ? Color.gray : (isCrumbling ? Color.yellow : Color.white);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
