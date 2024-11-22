using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattle : MonoBehaviour
{
    private Unit unit;  // Reference to the Unit component
    private State state;
    private Vector3 slideTargetPosition;
    private Action onSlideComplete;
    [SerializeField] float slideSpeed = 5f;
    [SerializeField] float reachedDistance = 1f;
    private bool isPlayerTeam;
    private GameObject selectionCircle;
    private HealthSystem healthSystem;
    private HealthBar healthBar;  // Reference to the HealthBar script

    private enum State
    {
        Idle,  // Corrected spelling
        Sliding,
        Busy
    }

    private void Awake()
    {
        unit = GetComponent<Unit>();  // Get the Unit component attached to this GameObject
        selectionCircle = transform.Find("SelectionCircle").gameObject;
        HideSelectionCircle();
        state = State.Idle;
        Setup(true);
    }

    public void Setup(bool isPlayerTeam)
    {
        this.isPlayerTeam = isPlayerTeam;

        // Initialize health system with current health and max health
        int initialHealth = unit.characterStats.currentHealth;  // Get current health dynamically
        int maxHealth = unit.characterStats.maxBaseHealth;  // Max health is static (set once)
        healthSystem = new HealthSystem(initialHealth, maxHealth);

        // Get the HealthBar component attached to this character
        healthBar = GetComponentInChildren<HealthBar>();  // Ensure your HealthBar is a child of this object

        // Pass the HealthSystem to the HealthBar for synchronization
        healthBar.SetupHealthSystem(healthSystem);

        // Subscribe to health change event in CharacterBattle (if needed)
        healthSystem.OnHealthChange += HealthSystem_OnHealthChanged;
    }


    public void HealthSystem_OnHealthChanged(object sender, EventArgs e)
    {
        if (this == null) return;  // If the character is destroyed, do not proceed
        // Update the health bar whenever health changes
        if (healthBar != null)
        {
            float healthPercent = healthSystem.GetHealthPercent();
            healthBar.SetHealthBarSize(healthPercent);  // Update health bar based on health percentage
        }
    }

    private void Update()
    {
        if (this == null) return;  // Prevent errors if the object is destroyed
        switch (state)
        {
            case State.Idle:
                break;

            case State.Sliding:
                Vector3 direction = (slideTargetPosition - GetPosition());
                transform.position += direction * slideSpeed * Time.deltaTime;

                if (Vector3.Distance(GetPosition(), slideTargetPosition) < reachedDistance)
                {
                    transform.position = slideTargetPosition;  // Ensure we reach the target position
                    onSlideComplete?.Invoke();  // Null check before invoking
                }
                break;

            case State.Busy:
                break;
        }
    }

    public void Damage(int damageAmount)
    {
        healthSystem.Damage(damageAmount);
        Debug.Log("Hit! Health: " + healthSystem.GetHealthPercent());
        DamagePopup.Create(GetPosition(), damageAmount);
        if (healthSystem.IsDead())
        {
            Destroy(this.gameObject);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;  // Return the position of the character
    }

    public void Attack(CharacterBattle targetCharacterBattle, Action onAttackComplete)
    {
        if (this == null || targetCharacterBattle == null) return; // Prevent errors if the object is destroyed

        Vector3 targetPosition = targetCharacterBattle.GetPosition();

        Vector3 slideDirection = (targetPosition - GetPosition()).normalized;
        float slideDistance = Vector3.Distance(GetPosition(), targetPosition);
        Vector3 slideTargetPosition = GetPosition() + slideDirection * slideDistance;

        // Attack damage
        targetCharacterBattle.Damage(10);

        Vector3 startingPosition = GetPosition();

        SlideToPosition(slideTargetPosition, () =>
        {
            state = State.Busy;

            // Perform attack logic here, for example applying damage to the target.
            Vector3 attackDir = (targetCharacterBattle.GetPosition() - GetPosition()).normalized;

            SlideToPosition(startingPosition, () =>
            {
                state = State.Idle;
                onAttackComplete?.Invoke();  // Return to idle after the attack
            });
        });
    }

    private void SlideToPosition(Vector3 slideTargetPosition, Action onSlideComplete)
    {
        this.slideTargetPosition = slideTargetPosition;
        this.onSlideComplete = onSlideComplete;
        state = State.Sliding;
    }

    public void HideSelectionCircle()
    {
        selectionCircle.SetActive(false);
    }

    public void ShowSelectionCircle()
    {
        selectionCircle.SetActive(true);
    }
}
