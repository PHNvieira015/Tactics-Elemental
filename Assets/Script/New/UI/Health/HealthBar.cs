using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    private HealthSystem healthSystem;  // Declare healthSystem instance correctly
    private int healthMax;

    [SerializeField] public Image HealthBarImage;
    [SerializeField] public TextMeshProUGUI healthText;
    [SerializeField] public Button ButtonUI_Reset;
    [SerializeField] public Button ButtonUI_Damage;
    [SerializeField] public Button ButtonUI_Heal;

    // Start is called before the first frame update
    void Start()
    {
        // Setup button listeners
        ButtonUI_Reset.onClick.AddListener(ResetHealth);
        ButtonUI_Damage.onClick.AddListener(DamageHealth);
        ButtonUI_Heal.onClick.AddListener(HealHealth);
    }

    // Method to set up HealthSystem externally from CharacterBattle
    public void SetupHealthSystem(HealthSystem healthSystem)
    {
        this.healthSystem = healthSystem;

        // Subscribe to health change event
        this.healthSystem.OnHealthChange += HealthSystem_OnHealthChanged;

        // Update the health bar and health text initially
        HealthSystem_OnHealthChanged(this, System.EventArgs.Empty);
    }

    // Event handler for health changes
    public void HealthSystem_OnHealthChanged(object sender, System.EventArgs e)
    {
        // Update the health bar and health text when health changes
        SetHealthBarSize(healthSystem.GetHealthPercent());
        SetHealthNumber(healthSystem.GetHealth(), healthSystem.healthMax);
    }

    // Method to update the health bar size based on normalized health value
    public void SetHealthBarSize(float healthNormalized)
    {
        HealthBarImage.fillAmount = healthNormalized;  // Update the health bar's fill amount
    }

    // Method to update the health text (current health / max health)
    private void SetHealthNumber(int health, int healthMax)
    {
        healthText.text = "Health\n" + health + "/" + healthMax;
    }

    // Reset health to max (100 in this case)
    void ResetHealth()
    {
        healthSystem.SetHealthMax();  // Reset health to max value
        Debug.Log("Health Reset: " + healthSystem.GetHealth());
    }

    // Damage health by 10
    void DamageHealth()
    {
        healthSystem.Damage(10);
        Debug.Log("Health After Damage: " + healthSystem.GetHealth());
    }

    // Heal health by 10
    void HealHealth()
    {
        healthSystem.Heal(10);
        Debug.Log("Health After Heal: " + healthSystem.GetHealth());
    }
}
