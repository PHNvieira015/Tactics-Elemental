using System;
using UnityEngine;

public class TurnStateManager : MonoBehaviour
{
    private bool turnStarted;
    [SerializeField] public MouseController mouseController; // Use MouseController instead of PathFinder
    [HideInInspector] public Vector3 TurnStartingPosition;

    public enum TurnState
    {
        None,
        TurnStart,
        Moving,
        Attacking,
        UsingSkill,
        Waiting,
        EndTurn
    }

    private PathFinder pathFinder;

    [SerializeField] public Unit currentUnit;

    public TurnState currentTurnState;
    public event Action<TurnState> OnTurnStateChanged;

    // Reference to UI_ActionBar and ActionBar
    public GameObject UI_ActionBar;
    public GameObject ActionBar;
    public UI_ActionBar uiActionBar;  // Reference to UI_ActionBar script
    [SerializeField] public GameMaster gameMaster;  // Direct reference to GameMaster

    private void Awake()
    {
        // Optionally find GameMaster if not assigned in the inspector
        if (gameMaster == null)
        {
            gameMaster = FindObjectOfType<GameMaster>();  // Find GameMaster in the scene
            if (gameMaster == null)
            {
                Debug.LogError("GameMaster not found in the scene!");
            }
        }

        // Ensure MouseController is assigned correctly
        if (mouseController == null)
        {
            mouseController = FindObjectOfType<MouseController>();  // Find MouseController in the scene
            if (mouseController == null)
            {
                Debug.LogError("MouseController not found in the scene!");
            }
        }
    }

    public void SetCurrentUnit(Unit unit)
    {
        currentUnit = unit;
        currentTurnState = TurnState.Waiting; // Default state at turn start
        Debug.Log($"Current unit set to {currentUnit.name}");
    }

    public void ChangeState(TurnState newState)
    {
        if (currentTurnState == newState)
        {
            return; // Avoid redundant state processing
        }
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
            case TurnState.None:
                break;

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
                        Debug.Log("ActionBar activated.");
                    }
                    else
                    {
                        Debug.LogError("UI_ActionBar or ActionBar is not assigned!");
                    }

                    turnStarted = true;
                }
                break;

            case TurnState.Moving:
                if (!currentUnit.hasMoved)
                {
                    DisableUI_Action();
                    Debug.Log($"{currentUnit.name} is moving...");
                    // Now trigger movement via MouseController
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

                EnableUI_Action();
                uiActionBar.GameObjectButton_move.SetActive(false); // Deactivate Move button
                uiActionBar.GameObjectButton_return.SetActive(true); // activate Return button
                break;

            case TurnState.Attacking:
                if (!currentUnit.hasAttacked)
                {
                    DisableUI_Action();
                    Debug.Log($"{currentUnit.name} is attacking...");
                    // Attack logic
                    EnableUI_Action();
                    uiActionBar.GameObjectButton_attack.SetActive(false); // Deactivate attack button
                    uiActionBar.GameObjectButton_return.SetActive(true); // Activate Return button
                }
                else
                {
                    Debug.Log($"{currentUnit.name} has already attacked.");
                }
                break;

            case TurnState.UsingSkill:
                DisableUI_Action();
                Debug.Log($"{currentUnit.name} is using a skill...");
                // Skill usage logic
                EnableUI_Action();
                uiActionBar.GameObjectButton_return.SetActive(true); // Activate Return button
                break;

            case TurnState.Waiting:
                DisableUI_Action();
                Debug.Log($"{currentUnit.name} is waiting...");
                currentUnit.SetFaceDirectionAtTurnEnd();
                currentUnit.selected = false;
                break;

            case TurnState.EndTurn:
                Debug.Log($"Turn ended for {currentUnit.name}.");
                turnStarted = false;
                currentUnit.hasMoved = false;  // Reset movement status
                currentUnit.hasAttacked = false;  // Reset attack status
                if (gameMaster != null)
                {
                    gameMaster.HandleEndOfRound();  // Handle end-of-round in GameMaster
                }
                else
                {
                    Debug.LogError("GameMaster is not assigned!");
                }
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
