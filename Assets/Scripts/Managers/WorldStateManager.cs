using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Singleton manager that controls the world state (Normal vs Blink)
/// Handles blink toggling, collision layer swapping, and event broadcasting
/// </summary>
public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    [Header("World State")]
    [SerializeField] private WorldState currentState = WorldState.Normal;

    [Header("Blink Settings")]
    [SerializeField] private float blinkDuration = 1f;
    [SerializeField] private float blinkCooldown = 0.5f;

    [Header("Forced Blink Settings")]
    [SerializeField] private bool enableForcedBlink = true;
    [SerializeField] private float forcedBlinkTime = 10f; // Time until forced blink
    [SerializeField] private bool resetGaugeOnManualBlink = true; // Reset timer when player manually blinks

    [Header("Statistics")]
    [SerializeField] private int blinkCount = 0;
    [SerializeField] private int maxBlinkCount = 10; // Maximum blinks before depletion

    [Header("Coffee Power-Up")]
    [SerializeField] private int coffeePowerUps = 0; // Coffee items in inventory (max 3)
    [SerializeField] private int maxCoffeePowerUps = 3; // Maximum coffee items player can hold
    [SerializeField] private float coffeeDuration = 5f; // How long coffee blink lasts
    [SerializeField] private bool isCoffeeActive = false; // Currently using coffee power-up

    [Header("Layer References")]
    [SerializeField] private string normalSolidLayer = "NormalSolid";
    [SerializeField] private string blinkSolidLayer = "BlinkSolid";

    [Header("Visual Effects")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool enableColorInversion = true;
    [SerializeField] private Color normalWorldTint = Color.white;
    [SerializeField] private Color blinkWorldTint = new Color(0.7f, 0.7f, 1f); // Slight blue tint
    [SerializeField] private bool enableVignette = true;
    [SerializeField] private float vignetteIntensity = 0.3f;
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float shakeIntensity = 0.15f;
    [SerializeField] private float shakeDuration = 0.2f;

    // State tracking
    private bool isBlinking = false;
    private bool isOnCooldown = false;
    private float currentBlinkTimer = 0f;
    private float coffeeTimer = 0f;

    // Forced blink tracking
    private float forcedBlinkTimer = 0f;
    private bool wasManualBlink = false;

    // Visual effects tracking
    private SpriteRenderer[] allSprites;
    private Color[] originalSpriteColors;
    private bool spritesInitialized = false;
    private Vector3 cameraOriginalPosition;
    private float shakeTimer = 0f;

    // Events
    public event Action OnEnterBlink;
    public event Action OnExitBlink;
    public event Action OnBlinkCountChanged;
    public event Action OnCoffeeBlinkChanged;
    public event Action OnCoffeePowerUpChanged;
    public event Action OnForcedBlink; // Triggered when forced blink activates

    // Properties
    public WorldState CurrentState => currentState;
    public bool IsBlinking => isBlinking;
    public bool IsOnCooldown => isOnCooldown;
    public bool IsCoffeeActive => isCoffeeActive;
    public bool CanBlink => !isBlinking && !isOnCooldown;
    public int BlinkCount => blinkCount;
    public int CurrentBlinkCount => blinkCount; // Remaining blinks
    public int MaxBlinkCount => maxBlinkCount;
    public int CoffeePowerUps => coffeePowerUps;
    public int MaxCoffeePowerUps => maxCoffeePowerUps;
    public float BlinkDuration => blinkDuration;
    public float BlinkCooldown => blinkCooldown;
    public float CoffeeDuration => coffeeDuration;

    // Legacy property for compatibility
    public bool HasCoffeeBlink => coffeePowerUps > 0;

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

        // Find main camera if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            cameraOriginalPosition = mainCamera.transform.localPosition;
        }
    }

    void Start()
    {
        // Initialize sprite list for color effects
        InitializeSprites();

        // Initialize forced blink timer
        if (enableForcedBlink)
        {
            forcedBlinkTimer = forcedBlinkTime;
        }
    }

    void Update()
    {
        // Handle coffee power-up timer
        if (isCoffeeActive)
        {
            coffeeTimer -= Time.deltaTime;
            if (coffeeTimer <= 0f)
            {
                DeactivateCoffeePowerUp();
            }
        }

        // Handle blink timer for auto-return to normal
        if (isBlinking && !isCoffeeActive) // Don't auto-exit during coffee mode
        {
            currentBlinkTimer -= Time.deltaTime;
            if (currentBlinkTimer <= 0f)
            {
                ExitBlink();
            }
        }

        // Handle forced blink timer (only when not already blinking)
        if (enableForcedBlink && !isBlinking && !isOnCooldown)
        {
            forcedBlinkTimer -= Time.deltaTime;
            if (forcedBlinkTimer <= 0f)
            {
                // Force player to blink
                TriggerForcedBlink();
            }
        }

        // Handle screen shake
        if (shakeTimer > 0f && mainCamera != null)
        {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer > 0f)
            {
                // Random shake offset - consider using a pre-calculated array for better performance
                Vector2 shake2D = Random.insideUnitCircle * shakeIntensity;
                Vector3 shakeOffset = new Vector3(shake2D.x, shake2D.y, 0f);
                mainCamera.transform.localPosition = cameraOriginalPosition + shakeOffset;
            }
            else
            {
                // Reset camera when shake ends
                shakeTimer = 0f; // Clamp to prevent negative values
                mainCamera.transform.localPosition = cameraOriginalPosition;
            }
        }
    }

    void LateUpdate()
    {
        // Apply vignette effect (simple darkening at screen edges using camera background color)
        if (enableVignette && mainCamera != null)
        {
            float vignetteAmount = isBlinking ? vignetteIntensity : 0f;
            mainCamera.backgroundColor = Color.Lerp(Color.black, Color.clear, 1f - vignetteAmount);
        }
    }

    /// <summary>
    /// Attempt to enter blink state (triggers enemies)
    /// </summary>
    public bool TryBlink()
    {
        if (!CanBlink)
        {
            Debug.Log($"Cannot blink: {(isBlinking ? "Already blinking" : "On cooldown")}");
            return false;
        }

        wasManualBlink = true;
        EnterBlink();
        return true;
    }

    /// <summary>
    /// Trigger forced blink when timer runs out
    /// </summary>
    private void TriggerForcedBlink()
    {
        Debug.Log("Forced blink triggered!");
        wasManualBlink = false;
        OnForcedBlink?.Invoke();
        EnterBlink();
    }

    /// <summary>
    /// Activate coffee power-up for extended blink without enemy chase
    /// </summary>
    public bool ActivateCoffeePowerUp()
    {
        if (isCoffeeActive)
        {
            Debug.Log("Coffee power-up already active!");
            return false;
        }

        if (coffeePowerUps <= 0)
        {
            Debug.Log("No coffee power-ups available!");
            return false;
        }

        // Consume one coffee power-up
        coffeePowerUps = Mathf.Max(0, coffeePowerUps - 1);
        isCoffeeActive = true;
        coffeeTimer = coffeeDuration;

        Debug.Log($"Coffee power-up activated! Duration: {coffeeDuration}s, Remaining: {coffeePowerUps}/{maxCoffeePowerUps}");

        // Enter blink state if not already in it
        if (!isBlinking)
        {
            EnterBlink();
        }
        else
        {
            // Already blinking, just extend the duration
            currentBlinkTimer = coffeeDuration;
        }

        // Fire events
        OnCoffeePowerUpChanged?.Invoke();
        OnCoffeeBlinkChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// Deactivate coffee power-up and return to normal
    /// </summary>
    private void DeactivateCoffeePowerUp()
    {
        if (!isCoffeeActive) return;

        isCoffeeActive = false;
        coffeeTimer = 0f;

        Debug.Log("Coffee power-up duration ended");

        // Exit blink state
        if (isBlinking)
        {
            ExitBlink();
        }

        // Fire events
        OnCoffeeBlinkChanged?.Invoke();
    }

    /// <summary>
    /// Enter blink state
    /// </summary>
    private void EnterBlink()
    {
        if (isBlinking) return;

        isBlinking = true;
        currentState = WorldState.Blink;
        currentBlinkTimer = blinkDuration;
        blinkCount++;

        Debug.Log($"Entering Blink state (Count: {blinkCount})");

        // Apply visual effects
        ApplyVisualEffects(true);

        // Screen shake on transition
        if (enableScreenShake)
        {
            shakeTimer = shakeDuration;
        }

        OnEnterBlink?.Invoke();
        OnBlinkCountChanged?.Invoke();
    }

    /// <summary>
    /// Exit blink state and return to normal
    /// </summary>
    private void ExitBlink()
    {
        if (!isBlinking) return;

        isBlinking = false;
        currentState = WorldState.Normal;

        Debug.Log("Exiting Blink state - Returning to Normal");

        // Restore visual effects
        ApplyVisualEffects(false);

        // Screen shake on transition
        if (enableScreenShake)
        {
            shakeTimer = shakeDuration * 0.5f; // Smaller shake on exit
        }

        // Reset forced blink timer after any blink
        if (enableForcedBlink)
        {
            // Always reset if it was a manual blink
            // For forced blinks, reset unconditionally to prevent immediate re-trigger
            if (wasManualBlink && resetGaugeOnManualBlink)
            {
                forcedBlinkTimer = forcedBlinkTime;
            }
            else if (!wasManualBlink)
            {
                // Always reset after forced blink to prevent loop
                forcedBlinkTimer = forcedBlinkTime;
            }
        }

        OnExitBlink?.Invoke();

        // Start cooldown
        StartCoroutine(CooldownCoroutine());
    }

    /// <summary>
    /// Handle cooldown period after blink
    /// </summary>
    private System.Collections.IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        Debug.Log($"Cooldown started ({blinkCooldown}s)");

        yield return new WaitForSeconds(blinkCooldown);

        isOnCooldown = false;
        Debug.Log("Cooldown ended - Can blink again");
    }

    /// <summary>
    /// Add a coffee power-up to inventory (max 3)
    /// </summary>
    public bool AddCoffeePowerUp()
    {
        if (coffeePowerUps >= maxCoffeePowerUps)
        {
            Debug.Log($"Coffee inventory full! Cannot hold more than {maxCoffeePowerUps}");
            return false;
        }

        coffeePowerUps = Mathf.Min(coffeePowerUps + 1, maxCoffeePowerUps);
        Debug.Log($"Coffee power-up added! Count: {coffeePowerUps}/{maxCoffeePowerUps}");

        OnCoffeePowerUpChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Reset all stats (for checkpoints/respawn)
    /// </summary>
    public void ResetStats()
    {
        blinkCount = 0;
        coffeePowerUps = 0;
        isCoffeeActive = false;
        coffeeTimer = 0f;

        // Reset forced blink timer
        if (enableForcedBlink)
        {
            forcedBlinkTimer = forcedBlinkTime;
        }

        if (isBlinking)
        {
            ExitBlink();
        }

        Debug.Log("WorldStateManager stats reset");
        OnBlinkCountChanged?.Invoke();
        OnCoffeePowerUpChanged?.Invoke();
    }

    /// <summary>
    /// Update the cached camera position when the camera moves to a new room
    /// This ensures screen shake is relative to the current room's camera position
    /// </summary>
    public void UpdateCameraPosition()
    {
        if (mainCamera != null)
        {
            cameraOriginalPosition = mainCamera.transform.localPosition;
        }
    }

    /// <summary>
    /// Get normalized time remaining until forced blink (1 = full, 0 = empty)
    /// Used by BlinkGaugeUI to display the gauge
    /// </summary>
    public float GetNormalizedBlinkGaugeTime()
    {
        if (!enableForcedBlink || forcedBlinkTime <= 0f)
        {
            return 1f; // Always full if forced blink is disabled
        }

        return Mathf.Clamp01(forcedBlinkTimer / forcedBlinkTime);
    }

    /// <summary>
    /// Get remaining time until forced blink (in seconds)
    /// </summary>
    public float GetForcedBlinkTimeRemaining()
    {
        return enableForcedBlink ? forcedBlinkTimer : -1f;
    }

    /// <summary>
    /// Get layer mask for current active solids
    /// </summary>
    public LayerMask GetActiveSolidLayer()
    {
        if (currentState == WorldState.Normal)
        {
            return LayerMask.GetMask(normalSolidLayer);
        }
        else
        {
            return LayerMask.GetMask(blinkSolidLayer);
        }
    }

    /// <summary>
    /// Initialize all sprite renderers in the scene for color effects
    /// </summary>
    private void InitializeSprites()
    {
        if (!enableColorInversion) return;

        // Find all sprite renderers
        allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        originalSpriteColors = new Color[allSprites.Length];

        // Store original colors
        for (int i = 0; i < allSprites.Length; i++)
        {
            originalSpriteColors[i] = allSprites[i].color;
        }

        spritesInitialized = true;
        Debug.Log($"Initialized {allSprites.Length} sprites for color effects");
    }

    /// <summary>
    /// Apply visual effects based on world state
    /// </summary>
    private void ApplyVisualEffects(bool enteringBlink)
    {
        if (!enableColorInversion || !spritesInitialized) return;

        Color targetTint = enteringBlink ? blinkWorldTint : normalWorldTint;

        // Apply tint to all sprites
        for (int i = 0; i < allSprites.Length; i++)
        {
            if (allSprites[i] != null)
            {
                // Multiply original color with tint
                allSprites[i].color = originalSpriteColors[i] * targetTint;
            }
        }

        // Apply tint to camera background
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.Lerp(Color.black, targetTint, 0.3f);
        }

        Debug.Log($"Applied {(enteringBlink ? "blink" : "normal")} visual effects");
    }
}

/// <summary>
/// Enum representing the two world states
/// </summary>
public enum WorldState
{
    Normal,
    Blink
}
