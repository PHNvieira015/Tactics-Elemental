using System;
using UnityEngine;

public class CharacterBattle : MonoBehaviour
{
    private Unit unit;
    private CharacterStat characterStat;
    private State state;
    private Vector3 slideTargetPosition;
    private Action onSlideComplete;
    [SerializeField] float slideSpeed = 5f;
    [SerializeField] float reachedDistance = 1f;
    private bool isPlayerTeam;
    private GameObject selectionCircle;
    private HealthSystem healthSystem;
    private HealthBar healthBar;
    public DamageSystem damageSystem;

    private enum State
    {
        Idle,
        Sliding,
        Busy
    }

    private void Awake()
    {
        unit = GetComponent<Unit>();
        characterStat = GetComponent<CharacterStat>();
        selectionCircle = transform.Find("SelectionCircle").gameObject;
        HideSelectionCircle();
        state = State.Idle;
        Setup(true);
    }

    public void Setup(bool isPlayerTeam)
    {
        this.isPlayerTeam = isPlayerTeam;

        int initialHealth = characterStat.currentHealth;
        int maxHealth = characterStat.maxBaseHealth;
        healthSystem = new HealthSystem(initialHealth, maxHealth, characterStat);

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

    public void Attack(CharacterBattle targetCharacterBattle, Action onAttackComplete)
    {
        if (this == null || targetCharacterBattle == null) return;
        if (targetCharacterBattle.healthSystem.IsDead()) return;

        int calculatedDamage = damageSystem.CalculateDamage(unit, targetCharacterBattle.unit);

        Vector3 targetPosition = targetCharacterBattle.GetPosition();
        Vector3 slideDirection = (targetPosition - GetPosition()).normalized;
        float slideDistance = Vector3.Distance(GetPosition(), targetPosition);
        Vector3 slideTargetPosition = GetPosition() + slideDirection * slideDistance;

        targetCharacterBattle.Damage(calculatedDamage);

        Vector3 startingPosition = GetPosition();

        SlideToPosition(slideTargetPosition, () =>
        {
            state = State.Busy;
            SlideToPosition(startingPosition, () =>
            {
                state = State.Idle;
                onAttackComplete?.Invoke();
            });
        });
    }

    public void Damage(int damageAmount)
    {
        if (this == null) return;

        healthSystem.Damage(damageAmount);
        DamagePopup.Create(GetPosition(), damageAmount);
        if (healthSystem.IsDead())
        {
            HandleDeath();
        }
    }

    public Vector3 GetPosition()
    {
        return this == null ? Vector3.zero : transform.position;
    }

    private void SlideToPosition(Vector3 slideTargetPosition, Action onSlideComplete)
    {
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

    public bool IsDead()
    {
        return healthSystem == null || healthSystem.IsDead();
    }

    public void HandleDeath()
    {
        if (this == null) return;

        Debug.Log($"{gameObject.name} has died.");
        gameObject.SetActive(false);
    }
}
