using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattle : MonoBehaviour
{
    private Unit unit;
    private State state;
    private Vector3 slideTargetPosition;
    private Action onSlideComplete;
    [SerializeField] float slideSpeed = 5f;
    [SerializeField] float reachedDistance = 1f;
    private bool isPlayerTeam;
    private GameObject selectionCircle;
    private HealthSystem healthSystem;
    private HealthBar healthBar;

    private enum State
    {
        Idle,
        Sliding,
        Busy
    }

    private void Awake()
    {
        unit = GetComponent<Unit>();
        selectionCircle = transform.Find("SelectionCircle").gameObject;
        HideSelectionCircle();
        state = State.Idle;
        Setup(true);
    }

    public void Setup(bool isPlayerTeam)
    {
        this.isPlayerTeam = isPlayerTeam;

        int initialHealth = unit.characterStats.currentHealth;
        int maxHealth = unit.characterStats.maxBaseHealth;
        healthSystem = new HealthSystem(initialHealth, maxHealth);

        healthBar = GetComponentInChildren<HealthBar>();

        healthBar.SetupHealthSystem(healthSystem);

        healthSystem.OnHealthChange += HealthSystem_OnHealthChanged;
    }

    public void HealthSystem_OnHealthChanged(object sender, EventArgs e)
    {
        if (this == null) return;
        if (healthBar != null)
        {
            float healthPercent = healthSystem.GetHealthPercent();
            healthBar.SetHealthBarSize(healthPercent);
        }
    }

    private void Update()
    {
        if (this == null) return;

        // If the object has been destroyed, don't proceed further in Update()
        if (healthSystem.IsDead()) return;

        switch (state)
        {
            case State.Idle:
                break;
            case State.Sliding:
                Vector3 direction = (slideTargetPosition - GetPosition());
                transform.position += direction * slideSpeed * Time.deltaTime;

                if (Vector3.Distance(GetPosition(), slideTargetPosition) < reachedDistance)
                {
                    transform.position = slideTargetPosition;
                    onSlideComplete?.Invoke();
                }
                break;
            case State.Busy:
                break;
        }
    }

    public void Damage(int damageAmount)
    {
        if (this == null) return; // Check if object is destroyed before applying damage

        healthSystem.Damage(damageAmount);
        DamagePopup.Create(GetPosition(), damageAmount);
        if (healthSystem.IsDead())  // Check if the character is dead
        {
            Destroy(this.gameObject); // Destroy the object if health is zero
        }
    }

    public Vector3 GetPosition()
    {
        // Ensure that the object is not destroyed
        if (this == null) return Vector3.zero;

        return transform.position;
    }

    public void Attack(CharacterBattle targetCharacterBattle, Action onAttackComplete)
    {
        if (this == null || targetCharacterBattle == null) return;

        // Check if either character is destroyed
        if (targetCharacterBattle == null || targetCharacterBattle.healthSystem.IsDead()) return;

        Vector3 targetPosition = targetCharacterBattle.GetPosition();
        Vector3 slideDirection = (targetPosition - GetPosition()).normalized;
        float slideDistance = Vector3.Distance(GetPosition(), targetPosition);
        Vector3 slideTargetPosition = GetPosition() + slideDirection * slideDistance;

        targetCharacterBattle.Damage(10);

        Vector3 startingPosition = GetPosition();

        SlideToPosition(slideTargetPosition, () =>
        {
            state = State.Busy;

            Vector3 attackDir = (targetCharacterBattle.GetPosition() - GetPosition()).normalized;

            SlideToPosition(startingPosition, () =>
            {
                state = State.Idle;
                onAttackComplete?.Invoke();
            });
        });
    }

    private void SlideToPosition(Vector3 slideTargetPosition, Action onSlideComplete)
    {
        // Check if the object is destroyed before starting the slide
        if (this == null) return;

        this.slideTargetPosition = slideTargetPosition;
        this.onSlideComplete = onSlideComplete;
        state = State.Sliding;
    }

    public void HideSelectionCircle()
    {
        if (selectionCircle != null)
        {
            selectionCircle.SetActive(false);
        }
    }

    public void ShowSelectionCircle()
    {
        if (selectionCircle != null)
        {
            selectionCircle.SetActive(true);
        }
    }

    // Add this method to check if the character is dead
    public bool IsDead()
    {
        if (healthSystem == null) return true;
        return healthSystem.IsDead();  // Checks the health system for death
    }
}
