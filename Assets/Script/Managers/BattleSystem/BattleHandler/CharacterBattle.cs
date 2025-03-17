using System;
using System.Collections;
using UnityEngine;
using static TurnStateManager;

public class CharacterBattle : MonoBehaviour
{
    public Unit unit;
    private CharacterStat characterStat;
    public State state;
    private Vector3 slideTargetPosition;
    private Action onSlideComplete;
    [SerializeField] float slideSpeed = 1f;
    [SerializeField] float reachedDistance = 0.5f;
    private bool isPlayerTeam;
    private GameObject selectionCircle;
    public HealthSystem healthSystem;
    public HealthBar healthBar;
    //testing to make unit health decrease
    public GameObject damageManager;
    public DamageSystem damageSystem;
    public TurnStateManager turnStateManager;
    public AnimatorScript animatorScript;

    [SerializeField]private float WaitForSeconds =0f;

    public enum State
    {
        Idle,
        Sliding,
        Busy
    }

    private void Awake()
    {
        #region health decrease
        GameObject damageManager = GameObject.Find("DamageManager");

        if (damageManager != null)
        {
            DamageSystem damageSystem = damageManager.GetComponent<DamageSystem>();

            if (damageSystem != null)
            {
                // Successfully got the DamageSystem component
                Debug.Log("DamageSystem component found!");
            }
            else
            {
                Debug.LogWarning("DamageSystem component not found on DamageManager.");
            }
        }
        else
        {
            Debug.LogWarning("DamageManager GameObject not found.");
        }
        #endregion

        //damageSystem = GetComponent<DamageSystem>();
        unit = GetComponent<Unit>();
        characterStat = GetComponent<CharacterStat>();
        animatorScript = GetComponent<AnimatorScript>();
        turnStateManager = GetComponent<TurnStateManager>();
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

        healthBar = GetComponentInChildren<HealthBar>(); // Ensure this is correctly assigned
        if (healthBar != null)
        {
            healthBar.SetupHealthSystem(healthSystem);
        }
        else
        {
            Debug.LogError($"{gameObject.name} is missing a HealthBar component!");
        }

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

    public void UpdateHealthbar(int damageAmount)
    {
        if (this == null) return;

        healthSystem.Damage(damageAmount);

        // Immediately update health bar
        HealthSystem_OnHealthChanged(this, EventArgs.Empty);

        if (healthSystem.IsDead())
        {
            HandleDeath();
        }
    }


    public void Damage(int damageAmount)
    {
        if (this == null) return;

        healthSystem.Damage(damageAmount);

        // Immediately update health bar
        HealthSystem_OnHealthChanged(this, EventArgs.Empty);

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
    public void TriggerAttackAnimationtothetarget(CharacterBattle targetCharacterBattle)
    {
        // Calculate the target position and slide parameters
        Vector3 targetPosition = targetCharacterBattle.GetPosition();
        Vector3 slideDirection = (targetPosition - GetPosition()).normalized;
        float slideDistance = Vector3.Distance(GetPosition(), targetPosition);
        Vector3 slideTargetPosition = GetPosition() + slideDirection * slideDistance;
        Vector3 startingPosition = GetPosition();

        // Start sliding to the target position
        SlideToPosition(slideTargetPosition, () =>
        {
            // When reached, slide back to starting position
            SlideToPosition(startingPosition, () =>
            {
                state = State.Idle; // Reset state after sliding back
            });
        });
    }
    public void TriggerAttackAnimationnearattacker(CharacterBattle targetCharacterBattle)
    {
        if (animatorScript == null || turnStateManager == null)
        {
            Debug.LogError("AnimatorScript or TurnStateManager is null!", this);
            return;
        }

        // Get the attack direction from character stats
        CharacterStat.Direction attackDirection = unit.characterStats.faceDirection;

        // Play the attack animation
        animatorScript.PlayAttackAnimation(attackDirection);

        // Get attack animation length
        float attackAnimLength = animatorScript.GetAttackAnimationLength(attackDirection);

        // Start coroutine to wait for animation to finish
        StartCoroutine(WaitForAttackAnimation(attackAnimLength, targetCharacterBattle));
    }

    private IEnumerator WaitForAttackAnimation(float duration, CharacterBattle targetCharacterBattle)
    {
        // Ensure animation state is attacking
        turnStateManager.ChangeState(TurnState.AttackingAnimation);

        // Wait for the attack animation to finish
        yield return new WaitForSeconds(duration);

        // Apply damage after animation
        damageSystem.Attack(unit, targetCharacterBattle.unit);
        unit.hasAttacked = true;

        // Change turn state to Waiting
        turnStateManager.ChangeState(TurnState.Waiting);
    }

    public State GetState()
    {
        return state;
    }
    private IEnumerator SlideWithDelay(Vector3 slideTargetPosition, Vector3 startingPosition)
    {
        //turnStateManager.ChangeState(TurnState.AttackingAnimation);
        // Wait for a short delay to allow the attack animation to play
        yield return new WaitForSeconds(WaitForSeconds); // Adjust the delay as needed
        //animatorScript.PlayAttackAnimation(characterStat.faceDirection);
        // Start sliding to the target position
        SlideToPosition(slideTargetPosition, () =>
        {
            // When reached, slide back to starting position
            SlideToPosition(startingPosition, () =>
            {
                state = State.Idle; // Reset state after sliding back
            });
        });
    }
}