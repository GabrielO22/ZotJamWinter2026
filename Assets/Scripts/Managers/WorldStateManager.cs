using System;
using UnityEngine;

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

    // State tracking
    private bool isBlinking = false;
    private bool isOnCooldown = false;
    private float currentBlinkTimer = 0f;

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
}

/// <summary>
/// Enum representing the two world states
/// </summary>
public enum WorldState
{
    Normal,
    Blink
}
