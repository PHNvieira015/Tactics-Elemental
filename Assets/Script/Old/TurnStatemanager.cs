using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnStateManager : MonoBehaviour
{
    public enum TurnState
    {
        Moving,
        Attacking,
        UsingSkill,
        Waiting
    }

    private Unit currentUnit;
    private TurnState currentTurnState;

    public void SetCurrentUnit(Unit unit)
    {
        currentUnit = unit;
        currentTurnState = TurnState.Waiting; // Default state at turn start
        Debug.Log($"Current unit set to {currentUnit.name}");
    }

    public void HandleTurnState(TurnState state)
    {
        if (currentUnit == null)
        {
            Debug.LogError("No unit selected for turn! Set a valid unit first.");
            return;
        }

        switch (state)
        {
            case TurnState.Moving:
                if (!currentUnit.hasMoved)
                {
                    Debug.Log($"{currentUnit.name} is moving...");
                    // Movement logic
                }
                else
                {
                    Debug.Log($"{currentUnit.name} has already moved.");
                }
                break;

            case TurnState.Attacking:
                if (!currentUnit.hasAttacked)
                {
                    Debug.Log($"{currentUnit.name} is attacking...");
                    // Attack logic
                }
                else
                {
                    Debug.Log($"{currentUnit.name} has already attacked.");
                }
                break;

            case TurnState.UsingSkill:
                Debug.Log($"{currentUnit.name} is using a skill...");
                // Skill usage logic
                break;

            case TurnState.Waiting:
                Debug.Log($"{currentUnit.name} is waiting...");
                currentUnit.RefreshStatusEffects();
                currentUnit.SetFaceDirectionAtTurnEnd();
                currentUnit.selected = false;
                break;

            default:
                Debug.LogWarning($"Unhandled turn state: {state}");
                break;
        }
    }
}
