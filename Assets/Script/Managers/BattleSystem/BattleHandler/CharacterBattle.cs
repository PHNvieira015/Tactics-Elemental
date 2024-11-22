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
    private enum State
    {
        Iddle,
        Sliding,
        Busy

    }
        private void Awake()
    {
        unit = GetComponent<Unit>();  // Get the Unit component attached to this GameObject
        state = State.Iddle;
    }

    private void Start()
    {
        // Initialization or setup logic goes here if needed
    }

    // Method to set up the character for the battle
    public void Setup(bool isPlayerTeam)
    {
        if (isPlayerTeam)
        {
            // Setup logic for player team goes here
        }
        else
        {
            // Setup logic for enemy team goes here
        }
    }
    private void Update()
    {
        switch (state)
        {
            case State.Iddle:
            break;
            case State.Sliding:
                float slideSpeed = 10f;
            transform.position += (slideTargetPosition - GetPosition()) *slideSpeed * Time.deltaTime;

                float reachedDistance = 1f;
                if (Vector3.Distance(GetPosition(), slideTargetPosition) < reachedDistance)
                {//Arrived at Slide Target Position
                transform.position = GetPosition();
                onSlideComplete();
                }
                    break;
               
            case State.Busy:
            break;
        }
    }

    // Corrected typo 'publci' to 'public' here
    public Vector3 GetPosition()
    {
        return transform.position;  // Return the position of the character
    }

    // Attack method for attacking another CharacterBattle
    public void Attack(CharacterBattle targetCharacterBattle, Action onAttackComplete)
    {            // Do stuff for attack (e.g., apply damage, animation, etc.)
                 // For example:
                 // targetCharacterBattle.TakeDamage(damageAmount);
        Vector3 slideTargetPosition = (targetCharacterBattle.GetPosition()+ - targetCharacterBattle.GetPosition()).normalized *10f;
        Vector3 startingPosition = GetPosition();

        // Slide to Target 
        SlideToPosition(slideTargetPosition, () => 
        {//Arrived at Target, attacked him
            state = State.Busy;

        // Calculate attack direction based on the target's position
        Vector3 attackDir = (targetCharacterBattle.GetPosition() - GetPosition()).normalized;
            //Attack Completed, slide back
            SlideToPosition(startingPosition, () =>
            {//Slide back Completed, back to iddle
                state = State.Iddle;
                onAttackComplete();
            });
        });
    }
    private void SlideToPosition(Vector3 slideTargetPosition, Action onSlideComplete)
    {
        this.slideTargetPosition = slideTargetPosition;
        this.onSlideComplete = onSlideComplete;
    }
}
