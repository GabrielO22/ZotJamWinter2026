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

    [Header("Forced Blink Display")]
    [SerializeField] private BlinkGaugeUI blinkGaugeUI; // Optional: Reference to blink gauge UI
    [SerializeField] private TextMeshProUGUI forcedBlinkWarningText; // Optional: Warning text when gauge is low

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
            worldStateManager.OnForcedBlink += HandleForcedBlink;
            UpdateBlinkCount(); // Initial display
            UpdateCoffeePowerUpCount(); // Initial display
            UpdateCoffeeActiveDisplay(); // Initial display
        }
        else
        {
            Debug.LogWarning("HUDManager: WorldStateManager not found!");
        }

        // Initialize forced blink warning as hidden
        if (forcedBlinkWarningText != null)
        {
            forcedBlinkWarningText.gameObject.SetActive(false);
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
            worldStateManager.OnForcedBlink -= HandleForcedBlink;
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

        blinkCountText.text = $"Blinks (E): {current}";

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

        coffeePowerUpCountText.text = $"Coffee (Q): {current}/{max}";

        // Change color based on availability
        if (current == 0)
        {
            coffeePowerUpCountText.color = Color.white; // No coffee
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

    /// <summary>
    /// Handle forced blink event - show warning or feedback
    /// </summary>
    private void HandleForcedBlink()
    {
        if (forcedBlinkWarningText != null)
        {
            forcedBlinkWarningText.text = "FORCED BLINK!";
            forcedBlinkWarningText.color = Color.red;
            forcedBlinkWarningText.gameObject.SetActive(true);

            // Hide warning after a short delay
            StartCoroutine(HideForcedBlinkWarning());
        }

        Debug.Log("HUD: Forced blink warning displayed");
    }

    /// <summary>
    /// Hide forced blink warning after delay
    /// </summary>
    private System.Collections.IEnumerator HideForcedBlinkWarning()
    {
        yield return new WaitForSeconds(2f);

        if (forcedBlinkWarningText != null)
        {
            forcedBlinkWarningText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Optional: Show low gauge warning
        if (worldStateManager != null && forcedBlinkWarningText != null)
        {
            float normalizedTime = worldStateManager.GetNormalizedBlinkGaugeTime();

            // Show warning when gauge is below 25%
            if (normalizedTime < 0.25f && normalizedTime > 0f && !worldStateManager.IsBlinking)
            {
                if (!forcedBlinkWarningText.gameObject.activeSelf)
                {
                    forcedBlinkWarningText.text = "BLINK SOON!";
                    forcedBlinkWarningText.color = Color.yellow;
                    forcedBlinkWarningText.gameObject.SetActive(true);
                }
            }
            else if (normalizedTime >= 0.25f)
            {
                // Hide warning when gauge recovers
                if (forcedBlinkWarningText.gameObject.activeSelf && forcedBlinkWarningText.text == "BLINK SOON!")
                {
                    forcedBlinkWarningText.gameObject.SetActive(false);
                }
            }
        }
    }
}
