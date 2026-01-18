using System;
using UnityEngine;

/// <summary>
/// Handles player health and death
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 1; // One-hit death for now
    [SerializeField] private int currentHealth = 1;

    [Header("Invincibility Settings")]
    [SerializeField] private bool hasInvincibility = false;
    [SerializeField] private float invincibilityDuration = 1f;
    private float invincibilityTimer = 0f;

    // Events
    public event Action OnPlayerDeath;
    public event Action OnHealthChanged; // For UI updates

    // Properties
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible => invincibilityTimer > 0f;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Update()
    {
        // Handle invincibility timer
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Take damage from enemy or hazard
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        if (!IsAlive || IsInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(); // Notify UI of health change

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (hasInvincibility)
        {
            invincibilityTimer = invincibilityDuration;
        }
    }

    /// <summary>
    /// Kill the player and trigger death event
    /// </summary>
    public void Die()
    {
        if (!IsAlive) return; // Already dead

        currentHealth = 0;
        Debug.Log("Player died!");

        OnPlayerDeath?.Invoke();
    }

    /// <summary>
    /// Restore player to full health (for respawn)
    /// </summary>
    public void Respawn()
    {
        currentHealth = maxHealth;
        invincibilityTimer = 0f;
        Debug.Log("Player respawned");

        OnHealthChanged?.Invoke(); // Notify UI of health restored
    }

    /// <summary>
    /// Heal the player
    /// </summary>
    public void Heal(int amount = 1)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed {amount}. Health: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(); // Notify UI of health increase
    }
}
