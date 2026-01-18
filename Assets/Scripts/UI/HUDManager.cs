using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the HUD display for health, blink count, and other UI elements
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Health Display")]
    [SerializeField] private GameObject healthContainer;
    [SerializeField] private Image[] healthHearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Blink Display")]
    [SerializeField] private TextMeshProUGUI blinkCountText;

    [Header("Coffee Power-Up Display")]
    [SerializeField] private TextMeshProUGUI coffeePowerUpCountText;
    [SerializeField] private TextMeshProUGUI coffeeActiveText;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private WorldStateManager worldStateManager;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple HUDManagers detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Auto-find references if not set
        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }

        if (worldStateManager == null)
        {
            worldStateManager = WorldStateManager.Instance;
        }

        // Subscribe to events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay(); // Initial display
        }
        else
        {
            Debug.LogWarning("HUDManager: PlayerHealth not found!");
        }

        if (worldStateManager != null)
        {
            worldStateManager.OnBlinkCountChanged += UpdateBlinkCount;
            worldStateManager.OnCoffeeBlinkChanged += UpdateCoffeeActiveDisplay;
            worldStateManager.OnCoffeePowerUpChanged += UpdateCoffeePowerUpCount;
            UpdateBlinkCount(); // Initial display
            UpdateCoffeePowerUpCount(); // Initial display
            UpdateCoffeeActiveDisplay(); // Initial display
        }
        else
        {
            Debug.LogWarning("HUDManager: WorldStateManager not found!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }

        if (worldStateManager != null)
        {
            worldStateManager.OnBlinkCountChanged -= UpdateBlinkCount;
            worldStateManager.OnCoffeeBlinkChanged -= UpdateCoffeeActiveDisplay;
            worldStateManager.OnCoffeePowerUpChanged -= UpdateCoffeePowerUpCount;
        }
    }

    /// <summary>
    /// Update health heart display based on current/max health
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (playerHealth == null || healthHearts == null || healthHearts.Length == 0)
            return;

        int currentHealth = playerHealth.CurrentHealth;
        int maxHealth = playerHealth.MaxHealth;

        // Update each heart sprite
        for (int i = 0; i < healthHearts.Length; i++)
        {
            if (healthHearts[i] != null)
            {
                if (i < currentHealth)
                {
                    healthHearts[i].sprite = fullHeart;
                    healthHearts[i].enabled = true;
                }
                else if (i < maxHealth)
                {
                    healthHearts[i].sprite = emptyHeart;
                    healthHearts[i].enabled = true;
                }
                else
                {
                    // Hide hearts beyond max health
                    healthHearts[i].enabled = false;
                }
            }
        }

        Debug.Log($"HUD: Health updated to {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Update blink count display
    /// </summary>
    private void UpdateBlinkCount()
    {
        if (worldStateManager == null || blinkCountText == null)
            return;

        int current = worldStateManager.CurrentBlinkCount;
        int max = worldStateManager.MaxBlinkCount;

        blinkCountText.text = $"Blinks: {current}";

        // Change color based on blink availability
        if (current == 0)
        {
            blinkCountText.color = Color.red; // No blinks left
        }
        else if (current <= max / 3)
        {
            blinkCountText.color = Color.yellow; // Low blinks
        }
        else
        {
            blinkCountText.color = Color.white; // Normal
        }

        Debug.Log($"HUD: Blink count updated to {current}/{max}");
    }

    /// <summary>
    /// Update coffee power-up count display
    /// </summary>
    private void UpdateCoffeePowerUpCount()
    {
        if (worldStateManager == null || coffeePowerUpCountText == null)
            return;

        int current = worldStateManager.CoffeePowerUps;
        int max = worldStateManager.MaxCoffeePowerUps;

        coffeePowerUpCountText.text = $"Coffee: {current}/{max}";

        // Change color based on availability
        if (current == 0)
        {
            coffeePowerUpCountText.color = Color.gray; // No coffee
        }
        else if (current >= max)
        {
            coffeePowerUpCountText.color = new Color(1f, 0.8f, 0.2f); // Full - bright gold
        }
        else
        {
            coffeePowerUpCountText.color = new Color(0.8f, 0.6f, 0.3f); // Partial - coffee brown
        }

        Debug.Log($"HUD: Coffee power-up count updated to {current}/{max}");
    }

    /// <summary>
    /// Update coffee active status display
    /// </summary>
    private void UpdateCoffeeActiveDisplay()
    {
        if (worldStateManager == null || coffeeActiveText == null)
            return;

        bool isCoffeeActive = worldStateManager.IsCoffeeActive;

        if (isCoffeeActive)
        {
            coffeeActiveText.text = "COFFEE MODE ACTIVE!";
            coffeeActiveText.color = new Color(1f, 0.9f, 0.4f); // Bright coffee color
            coffeeActiveText.gameObject.SetActive(true);
        }
        else
        {
            coffeeActiveText.gameObject.SetActive(false);
        }

        Debug.Log($"HUD: Coffee active display updated (active: {isCoffeeActive})");
    }
}
