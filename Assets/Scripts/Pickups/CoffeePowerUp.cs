using UnityEngine;

/// <summary>
/// Coffee power-up collectible that hovers in the world
/// Grants the player extended blink duration without enemy chase
/// </summary>
public class CoffeePowerUp : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite coffeeSprite;

    [Header("Hover Animation")]
    [SerializeField] private float hoverSpeed = 1f;
    [SerializeField] private float hoverHeight = 0.3f;

    [Header("Rotation Animation")]
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Pickup Settings")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPosition;
    private float hoverOffset = 0f;

    void Awake()
    {
        // Cache components
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Set coffee sprite if assigned
        if (coffeeSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = coffeeSprite;
        }

        // Store starting position for hover animation
        startPosition = transform.position;

        // Randomize hover offset for variation
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Hover animation
        float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed + hoverOffset) * hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation animation
        if (rotateObject)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if player picked up the coffee
        if (collision.CompareTag("Player"))
        {
            // Try to add coffee to player's inventory
            if (WorldStateManager.Instance != null)
            {
                bool success = WorldStateManager.Instance.AddCoffeePowerUp();

                if (success)
                {
                    // Play pickup sound
                    if (pickupSound != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(pickupSound);
                    }

                    Debug.Log("Player picked up coffee power-up!");

                    // Destroy or hide the coffee object
                    if (destroyOnPickup)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.Log("Coffee inventory full! Cannot pick up more coffee.");
                }
            }
            else
            {
                Debug.LogWarning("WorldStateManager not found! Cannot add coffee power-up.");
            }
        }
    }

    // Optional: Draw gizmo for visualization in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.3f, 0f, 0.5f); // Brown/coffee color
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw hover range
        Gizmos.color = Color.yellow;
        Vector3 pos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawLine(pos + Vector3.up * hoverHeight, pos - Vector3.up * hoverHeight);
    }
}
