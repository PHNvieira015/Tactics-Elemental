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
        if (isPlayerTeam)
        {
            //
        }
        healthSystem = new HealthSystem(100, 100);
    }


    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;

            case State.Sliding:
                //float slideSpeed = 5f;
                Vector3 direction = (slideTargetPosition - GetPosition());
                transform.position += direction * slideSpeed * Time.deltaTime;

                //float reachedDistance = 1f;
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
        Debug.Log("hit " + healthSystem.GetHealthPercent());
    }

    public Vector3 GetPosition()
    {
        return transform.position;  // Return the position of the character
    }

    public void Attack(CharacterBattle targetCharacterBattle, Action onAttackComplete)
    {
        // Calculate the target position based on the current position and the enemy's position
        Vector3 targetPosition = targetCharacterBattle.GetPosition();

        // Calculate the direction and target position to slide to (do not fix the distance, just move towards the target)
        Vector3 slideDirection = (targetPosition - GetPosition()).normalized;

        // Calculate the final target position based on a dynamic distance, for example, a small buffer distance for sliding.
        float slideDistance = Vector3.Distance(GetPosition(), targetPosition);
        Vector3 slideTargetPosition = GetPosition() + slideDirection * slideDistance;

        //Attack Damage
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
