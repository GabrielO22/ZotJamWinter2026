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

    [Header("Statistics")]
    [SerializeField] private int blinkCount = 0;
    [SerializeField] private int coffeeBlinks = 3; // Coffee powerup uses

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

    // Visual effects tracking
    private SpriteRenderer[] allSprites;
    private Color[] originalSpriteColors;
    private bool spritesInitialized = false;
    private Vector3 cameraOriginalPosition;
    private float shakeTimer = 0f;

    // Events
    public event Action OnEnterBlink;
    public event Action OnExitBlink;
    public event Action<int> OnBlinkCountChanged;

    // Properties
    public WorldState CurrentState => currentState;
    public bool IsBlinking => isBlinking;
    public bool IsOnCooldown => isOnCooldown;
    public bool CanBlink => !isBlinking && !isOnCooldown;
    public int BlinkCount => blinkCount;
    public int CoffeeBlinks => coffeeBlinks;
    public float BlinkDuration => blinkDuration;
    public float BlinkCooldown => blinkCooldown;

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
    }

    void Update()
    {
        // Handle blink timer for auto-return to normal
        if (isBlinking)
        {
            currentBlinkTimer -= Time.deltaTime;
            if (currentBlinkTimer <= 0f)
            {
                ExitBlink();
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

        EnterBlink();
        return true;
    }

    /// <summary>
    /// Use coffee to blink without activating enemies
    /// </summary>
    public bool TryCoffeeBlink()
    {
        if (!CanBlink)
        {
            Debug.Log($"Cannot coffee blink: {(isBlinking ? "Already blinking" : "On cooldown")}");
            return false;
        }

        if (coffeeBlinks <= 0)
        {
            Debug.Log("No coffee blinks remaining!");
            return false;
        }

        coffeeBlinks--;
        EnterBlink();
        Debug.Log($"Coffee blink used! {coffeeBlinks} remaining");
        return true;
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
        OnBlinkCountChanged?.Invoke(blinkCount);
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
    /// Add a coffee powerup charge
    /// </summary>
    public void AddCoffeeBlink()
    {
        if (coffeeBlinks < 3)
        {
            coffeeBlinks++;
            Debug.Log($"Coffee collected! Blinks: {coffeeBlinks}/3");
        }
    }

    /// <summary>
    /// Reset all stats (for checkpoints/respawn)
    /// </summary>
    public void ResetStats()
    {
        blinkCount = 0;
        coffeeBlinks = 3;

        if (isBlinking)
        {
            ExitBlink();
        }

        Debug.Log("WorldStateManager stats reset");
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
