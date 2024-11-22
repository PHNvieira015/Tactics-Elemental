using System;
using UnityEngine;

public class HealthSystem
{
    public event EventHandler OnHealthChange;
    public event EventHandler OnDead;

    private int health;
    public int healthMax;

    // Constructor with initial health and max health
    public HealthSystem(int initialHealth, int healthMax)
    {
        this.healthMax = healthMax;
        this.health = Mathf.Clamp(initialHealth, 0, healthMax);  // Ensure initial health is within bounds
    }


    public int GetHealth()
    {
        return health;
    }

    public float GetHealthPercent()
    {
        return (float)health / healthMax;
    }

    // Damage health, ensuring health doesn't go below 0
    public void Damage(int damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Max(health, 0);  // Clamp health to be at least 0
        OnHealthChanged();  // Trigger event when health changes
    }
    // Heal health, ensuring health doesn't exceed max health
    public void Heal(int healAmount)
    {
        health += healAmount;
        health = Mathf.Min(health, healthMax);  // Clamp health to be at most healthMax
        OnHealthChanged();  // Trigger event when health changes
    }

    // Set health to the maximum value
    public void SetHealthMax()
    {
        health = healthMax;
        OnHealthChanged();  // Trigger event when health changes
    }

    // Method to invoke the OnHealthChange event
    private void OnHealthChanged()
    {
        OnHealthChange?.Invoke(this, EventArgs.Empty);
    }
    public void Die()
    {
        if (OnDead != null) OnDead(this, EventArgs.Empty);
    }

    public bool IsDead()
    {
        return health <= 0;
    }

}