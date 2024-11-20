using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private HealthSystem healthSystem;  // Declare healthSystem instance correctly
    private int healthMax;
    

    [SerializeField] public Image HealthBarImage;   // Fixed typo: Serializefield -> SerializeField
    [SerializeField] public TextMeshProUGUI healthText;   // Fixed typo: Serializefield -> SerializeField
    [SerializeField] public Button ButtonUI_Reset;
    [SerializeField] public Button ButtonUI_Damage;
    [SerializeField] public Button ButtonUI_Heal;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the health system with 100 health and max health of 100
        healthSystem = new HealthSystem(100, 100);
        Debug.Log("Health: " + healthSystem.GetHealth());
        
    // Setup button listeners
        ButtonUI_Reset.onClick.AddListener(ResetHealth);
        ButtonUI_Damage.onClick.AddListener(DamageHealth);
        ButtonUI_Heal.onClick.AddListener(HealHealth);

        // Subscribe to health change event
        healthSystem.OnHealthChange += HealthSystem_OnHealthChanged;

        // Initialize health UI on start (we can manually call the method to set the initial UI)
        HealthSystem_OnHealthChanged(this, System.EventArgs.Empty);
    }

    // Reset health to max (100 in this case)
    void ResetHealth()
    {
        healthSystem.SetHealthMax();  // Reset health to max value (100)
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

    // Method to update the health bar size based on normalized health value
    private void SetHealthBarSize(float healthNormalized)
    {
        HealthBarImage.fillAmount = healthNormalized;  // Update the health bar's fill amount
    }

    // Event handler for health changes
    private void HealthSystem_OnHealthChanged(object sender, System.EventArgs e)
    {
        // Update the health bar and health text when health changes
        SetHealthBarSize(healthSystem.GetHealthNormalized());
        SetHealthNumber(healthSystem.GetHealth(), healthSystem.healthMax);
    }

    // Method to update the health text (current health / max health)
    private void SetHealthNumber(int health, int healthMax)
    {
        // Display health in the format: "Health\nCurrentHealth/MaxHealth"
        healthText.text = "Health\n" + health + "/" + healthMax;
    }


}
