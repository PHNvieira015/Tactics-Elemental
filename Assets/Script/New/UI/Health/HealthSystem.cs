using System;
using UnityEngine;

public class HealthSystem
{
    public event EventHandler OnHealthChange;
    public event EventHandler OnDead;

    private CharacterStat characterStats;  // Reference to CharacterStat
    public int health;
    public int healthMax;


    // Constructor: Initialize with characterStats and values
    public HealthSystem(int initialHealth, int healthMax, CharacterStat characterStats)
    {
        this.characterStats = characterStats;
        this.healthMax = healthMax;

        // Initialize health, clamping it between 0 and max health
        this.health = healthMax;

        // Sync with CharacterStat
        this.characterStats.currentHealth = this.health;
    }
    public int GetHealthMax()
    {
        return healthMax;
    }
    public int GetHealth()
    {
        return health;
    }

    public float GetHealthPercent()
    {
        return (float)health / healthMax;
    }

    // Damage health (update both HealthSystem and CharacterStat)
    public void Damage(int damageAmount)
    {
        if (health <= 0) return;  // If already dead, return

        health -= damageAmount;
        health = Mathf.Max(health, 0);  // Ensure health doesn't go below 0

        // Update characterStats' currentHealth
        characterStats.currentHealth = health;

        // Trigger health change event
        OnHealthChanged();
    }

    // Heal health (update both HealthSystem and CharacterStat)
    public void Heal(int healAmount)
    {
        if (health >= healthMax) return;  // Don't heal if already at max health

        health += healAmount;
        health = Mathf.Min(health, healthMax);  // Ensure health doesn't exceed max health

        // Update characterStats' currentHealth
        characterStats.currentHealth = health;

        // Trigger health change event
        OnHealthChanged();
    }

    public void SetHealthMax()
    {
        health = healthMax;
        characterStats.currentHealth = health;  // Sync with CharacterStat
        OnHealthChanged();
    }

    // Event trigger when health changes
    public void OnHealthChanged()
    {
        OnHealthChange?.Invoke(this, EventArgs.Empty);
    }

    public void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDead()
    {
        return health <= 0;
    }
}
