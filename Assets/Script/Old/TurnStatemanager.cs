using System;
using UnityEngine;

public class TurnStateManager : MonoBehaviour
{
    private bool turnStarted;
    [HideInInspector] public Vector3 TurnStartingPosition;

    public enum TurnState
    {
        TurnStart,
        Moving,
        Attacking,
        UsingSkill,
        Waiting,
        EndTurn
    }

    private PathFinder pathFinder;

    [SerializeField]public Unit currentUnit;
    
    private TurnState currentTurnState;
    public event Action<TurnState> OnTurnStateChanged;

    // Reference to UI_ActionBar and ActionBar
    public GameObject UI_ActionBar;
    public GameObject ActionBar;
    public UI_ActionBar uiActionBar;  // Reference to UI_ActionBar script
    public void SetCurrentUnit(Unit unit)
    {
        currentUnit = unit;
        currentTurnState = TurnState.Waiting; // Default state at turn start
        Debug.Log($"Current unit set to {currentUnit.name}");
    }

    public void ChangeState(TurnState newState)
    {
        UI_ActionBar.SetActive(false); // Deactivate Move button
        currentTurnState = newState;
        Debug.Log($"State changed to {currentTurnState}");

        // Notify listeners of the state change
        OnTurnStateChanged?.Invoke(currentTurnState);

        // Execute state-specific logic
        HandleTurnState(newState);
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
            case TurnState.TurnStart:
                EnableUI_Action();
                if (!turnStarted)
                {
                    TurnStartingPosition = currentUnit.transform.position;
                    Debug.Log($"Turn started for {currentUnit.name}! Starting position set to {TurnStartingPosition}.");
                    currentUnit.RefreshStatusEffects();

                    // Deactivate the ActionBar
                    if (UI_ActionBar != null && ActionBar != null)
                    {
                        uiActionBar.ActionBar.SetActive(true); // Make sure the ActionBar is visible
                        Debug.Log("ActionBar deactivated.");
                    }
                    else
                    {
                        Debug.LogError("UI_ActionBar or ActionBar is not assigned!");
                    }

                    turnStarted = true;
                }
                else
                {
                    Debug.LogWarning("Turn has already started for this unit.");
                }
                break;

            case TurnState.Moving:
                if (!currentUnit.hasMoved)
                {
                    DisableUI_Action();
                    Debug.Log($"{currentUnit.name} is moving...");
                    // Now trigger movement via MouseController
                    // Now trigger movement via MouseController
                    MouseController mouseController = FindObjectOfType<MouseController>(); // Find MouseController in the scene
                    if (mouseController != null)
                    {
                        mouseController.isMoving = true; // Enable movement in MouseController
                    }
                    else
                    {
                        Debug.LogError("MouseController not found in the scene.");
                    }
                    // After movement ends, mark the unit as moved
                    currentUnit.hasMoved = true;



                }
                else
                {
                    Debug.Log($"{currentUnit.name} has already moved.");
                }

                    EnableUI_Action();
                    uiActionBar.GameObjectButton_move.SetActive(false); // Deactivate Move button
                    uiActionBar.GameObjectButton_return.SetActive(true); // activate Return button
                break;

            case TurnState.Attacking:
                if (!currentUnit.hasAttacked)
                {
                    DisableUI_Action();
                    //enable Attack UI if there is one
                    Debug.Log($"{currentUnit.name} is attacking...");
                    // Attack logic
                    EnableUI_Action();
                    uiActionBar.GameObjectButton_attack.SetActive(false);//deactivate attack button
                    uiActionBar.GameObjectButton_return.SetActive(true); // activate Return button

                }
                else
                {
                    Debug.Log($"{currentUnit.name} has already attacked.");
                }
                break;

            case TurnState.UsingSkill:
                DisableUI_Action();
                //enable Skill UI
                Debug.Log($"{currentUnit.name} is using a skill...");
                // Skill usage logic
                EnableUI_Action();
                uiActionBar.GameObjectButton_return.SetActive(true); // activate Return button
                break;

            case TurnState.Waiting:
                DisableUI_Action();
                Debug.Log($"{currentUnit.name} is waiting...");
                //enable waiting UI and confirmation after it it should also preview the selection
                currentUnit.SetFaceDirectionAtTurnEnd();
                currentUnit.selected = false;
                ChangeState(TurnState.EndTurn); // Automatically transition to EndTurn
                break;

            case TurnState.EndTurn:
                Debug.Log($"Turn ended for {currentUnit.name}.");
                turnStarted = false;
                // Notify GameMaster or other systems about the turn ending
                break;

            default:
                Debug.LogWarning($"Unhandled turn state: {state}");
                break;
        }
    }
    private void DisableUI_Action()
    {
        // Assuming UI_ActionBar is the GameObject, not the script itself
        if (UI_ActionBar != null)
        {
            UI_ActionBar.SetActive(false); // Deactivate ActionBar
        }
        else
        {
            Debug.LogError("UI_ActionBar is not assigned!");
        }
    }

    private void EnableUI_Action()
    {
        // Assuming UI_ActionBar is the GameObject, not the script itself
        if (UI_ActionBar != null)
        {
            UI_ActionBar.SetActive(true); // Activate ActionBar
        }
        else
        {
            Debug.LogError("UI_ActionBar is not assigned!");
        }
    }
}
