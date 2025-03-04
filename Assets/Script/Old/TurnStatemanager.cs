using System;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class TurnStateManager : MonoBehaviour
{
    #region Variables

    private bool turnStarted;
    [SerializeField] public MouseController mouseController; // Use MouseController instead of PathFinder
    [HideInInspector] public Vector3 TurnStartingPosition;
    private TurnStateManager turnStateManager;
    [SerializeField] private UnitManager unitManager;

    public enum TurnState
    {
        None,
        TurnStart,
        Spawning,
        Moving,
        Attacking,
        UsingSkill,
        Waiting,
        EndTurn
    }

    private PathFinder pathFinder;

    [SerializeField] public Unit currentUnit;
    [SerializeField] public GameObject currentUnitObject;
    public TurnState currentTurnState;
    public event Action<TurnState> OnTurnStateChanged;

    // Reference to UI_ActionBar and ActionBar
    public GameObject UI_ActionBar;
    public GameObject ActionBar;
    public UI_ActionBar uiActionBar;  // Reference to UI_ActionBar script
    [SerializeField] public GameMaster gameMaster;  // Direct reference to GameMaster
    public OverlayTile previousTile; // Store the tile before movement
    #endregion
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
    #region SetCurrentUnit
    public void SetCurrentUnit(Unit unit)
    {

        currentUnit = unit;
        currentUnitObject = unit.gameObject;
        currentTurnState = TurnState.Waiting; // Default state at turn start
        Debug.Log($"Current unit set to {currentUnit.name}");
        // Trigger the UI update after setting the current unit
        OnTurnStateChanged?.Invoke(currentTurnState);
        //Debug.Log("UI update triggered");
    }
    #endregion

    public void ChangeState(TurnState newState)
    {
        if (currentTurnState == newState)
        {
            return; // Avoid redundant state processing
        }
        if (currentUnit != null)
        {
            OverlayTile tileUnderUnit = currentUnit.GetTileUnderUnit();
            if (tileUnderUnit != null)
            {
                // Do something with the tile
                //Debug.Log($"Tile under the unit: {tileUnderUnit.name}");
            }
        }

        UI_ActionBar.SetActive(false); // Deactivate ActionBar (typically Move button will be hidden)
        currentTurnState = newState;
        //Debug.Log($"State changed to {currentTurnState}");

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

            case TurnState.Spawning:

                break;
            #region TurnStart
            case TurnState.TurnStart:
                currentUnit.hasMoved = false;  // Reset movement status
                EnableUI_Action();
                currentUnit.GetTileUnderUnit();
                Camera.main.transform.position = new Vector3(currentUnit.transform.position.x, currentUnit.transform.position.y, Camera.main.transform.position.z);
                
                //OverlayTile tileUnderUnit = currentUnit.TileUnderUnit();

                if (!turnStarted)
                {
                    currentUnit.standingOnTile = currentUnit.GetTileUnderUnit();  // Ensure tile is updated
                    TurnStartingPosition = currentUnit.transform.position;
                    mouseController.SetUnit(currentUnit);  // Assign unit to MouseController
                    Debug.Log($"Turn started for {currentUnit.name}! Standing on tile: {currentUnit.standingOnTile?.name}");
                    currentUnit.RefreshStatusEffects();
                    TurnStartingPosition = currentUnit.transform.position;
                    mouseController.SetUnit(currentUnit);
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

                    //deactivating butons
                    if(currentUnit.hasMoved==true)
                    {
                        uiActionBar.GameObjectButton_move.SetActive(false);
                    }
                    if (currentUnit.hasAttacked == true)
                    {
                        uiActionBar.GameObjectButton_attack.SetActive(false);
                    }
                }
                break;
            #endregion

            #region Moving
            case TurnState.Moving:

    OverlayTile previousTile = currentUnit.standingOnTile;  //old tile reference
    // Ensure that currentUnit's standingOnTile is updated when movement starts
    currentUnit.standingOnTile = currentUnit.GetTileUnderUnit();
    mouseController.SetUnit(currentUnit);  // Pass the unit reference to MouseController
    mouseController.currentUnit = currentUnit;
    mouseController.isMoving = false; // Enable movement logic
    mouseController.GetInRangeTiles(); // Highlight movement tiles

    // Log the coordinates of the standingOnTile
    if (currentUnit.standingOnTile != null)
    {
        Vector3 tilePosition = currentUnit.standingOnTile.transform.position;
        //Debug.Log($"Tile position: (X: {tilePosition.x}, Y: {tilePosition.y}, Z: {tilePosition.z})");
    }
    else
    {
        Debug.LogWarning("Current unit's standingOnTile is null!");
    }

    // Move the unit
    currentUnit.hasMoved = true;

    // After moving, update standingOnTile
    UpdateStandingOnTile(); // Update the tile after the unit has moved

    EnableUI_Action();
    uiActionBar.GameObjectButton_move.SetActive(false); // Deactivate Move button
    uiActionBar.GameObjectButton_return.SetActive(true); // Activate Return button
    //turnStateManager.ChangeState(TurnState.TurnStart);
    break;
            #endregion

            #region Attacking
            case TurnState.Attacking:
                if (!currentUnit.hasAttacked)
                {
                mouseController.GetAttackRangeTiles(); // Show attack range
                //Debug.Log($"{currentUnit.name} is attacking...");
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
                    uiActionBar.GameObjectButton_attack.SetActive(false); // Deactivate attack button
                    EnableUI_Action();
                }
                break;
            #endregion


            #region UsingSkill
            case TurnState.UsingSkill:
                DisableUI_Action();
                Debug.Log($"{currentUnit.name} is using a skill...");
                // Skill usage logic
                EnableUI_Action();
                uiActionBar.GameObjectButton_return.SetActive(true); // Activate Return button
                break;
            #endregion

            #region Waiting
            case TurnState.Waiting:
                DisableUI_Action();
                Debug.Log($"{currentUnit.name} is waiting...");
                currentUnit.SetFaceDirectionAtTurnEnd();
                currentUnit.selected = false;
                ChangeState(TurnState.EndTurn);
                uiActionBar.GameObjectButton_move.SetActive(true); // Deactivate Move button
                uiActionBar.GameObjectButton_attack.SetActive(true); // Deactivate Move button
                uiActionBar.GameObjectButton_return.SetActive(false); // Activate Return button
//                UpdateStandingOnTile();
                break;
            #endregion

            #region endTurn
            case TurnState.EndTurn:
                Debug.Log($"Turn ended for {currentUnit.name}.");
                turnStarted = false;
                currentUnit.hasMoved = false;  // Reset movement status
                currentUnit.hasAttacked = false;  // Reset attack status
                currentUnit.characterStats.EndTurnRoundInitiative();
                

                if (currentUnit.standingOnTile != null)
                {
                    if (currentUnit.standingOnTile.unitOnTile == currentUnit)
                    {
                        Debug.Log($"Clearing unitOnTile for {currentUnit.name} on {currentUnit.standingOnTile}");
                        currentUnit.standingOnTile.unitOnTile = null;
                    }
                }
                if (gameMaster != null)
                {
                    gameMaster.HandleEndOfRound();  // Handle end-of-round in GameMaster
                }
                else
                {
                    Debug.LogError("GameMaster is not assigned!");
                }
                unitManager.SetTurnOrderList();
                if (unitManager.turnOrderList.Count > 0)
                {
                    SetCurrentUnit(unitManager.turnOrderList[0]);
                }
                ChangeState(TurnState.TurnStart); // Start the new turn
                break;

            default:
                Debug.LogWarning($"Unhandled turn state: {state}");
                break;
        }
#endregion
    }


    #region StandingOnTile
    // The method to update standingOnTile
    private void UpdateStandingOnTile()
    {
        if (currentUnit != null)
        {
            // Log the position of the unit to verify it's being updated
            //Debug.Log($"Unit Position: {currentUnit.transform.position}");

            // Use raycasting or tile-based logic to determine the standing tile
            RaycastHit2D hit = Physics2D.Raycast(currentUnit.transform.position, Vector2.down);

            if (hit.collider != null)
            {
                // Assuming OverlayTile is attached to the tile's collider
                OverlayTile standingTile = hit.collider.gameObject.GetComponent<OverlayTile>();

                if (standingTile != null)
                {
                    // Update the standingOnTile in the Unit class
                    currentUnit.standingOnTile = standingTile;
                    //Debug.Log($"{currentUnit.name} is standing on tile: {standingTile.name}");
                }
                else
                {
                    Debug.LogWarning($"No OverlayTile found under {currentUnit.name}.");
                }
            }
            else
            {
                Debug.LogError("No tile detected under the unit.");
            }
        }
    }
    #endregion

    #region UI actionBar
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
    #endregion
}