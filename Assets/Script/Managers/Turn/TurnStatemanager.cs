using System;
using System.Linq;
using System.Collections;
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
    public Skill selectedSkill; // Track selected skill

    public enum TurnState
    {
        None,
        TurnStart,
        Spawning,
        Moving,
        Attacking,
        AttackingAnimation,
        UsingSkill,
        SkillTargeting,
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
        currentTurnState = TurnState.Waiting; // Default state at waiting
        Debug.Log($"Current unit set to {currentUnit.name}");
        // Trigger the UI update after setting the current unit
        OnTurnStateChanged?.Invoke(currentTurnState);
        //Debug.Log("UI update triggered");
    }
    #endregion



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
                if (currentUnit.isAI=true)
                {
                    currentUnit.aiController.DecideAction();
                }
                // Clear existing path arrows first
                if (mouseController != null)
                {
                    mouseController.ClearPathArrows();
                    TurnStartingPosition = currentUnit.transform.position;
                    currentUnit.standingOnTile = currentUnit.GetTileUnderUnit();
                    // Clear previous tile references
                    if (currentUnit.standingOnTile != null)
                    {
                        currentUnit.standingOnTile.ClearUnit();
                    }
                }
                else
                {
                    Debug.LogWarning("MouseController reference missing - arrows not cleared");
                }

                //currentUnit.hasMoved = false;  // Reset movement status
                EnableUI_Action();
                currentUnit.GetTileUnderUnit();
                    {
        // Set camera to currentUnit's position
        Camera.main.transform.position = new Vector3(
            currentUnit.transform.position.x,
            currentUnit.transform.position.y,
            Camera.main.transform.position.z
        );
        Debug.Log($"Camera set to {currentUnit.name} at {currentUnit.transform.position}");

        // Initialize turn-specific logic
        currentUnit.standingOnTile = currentUnit.GetTileUnderUnit();
        mouseController.SetUnit(currentUnit);
        turnStarted = true;
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


    // After moving, update standingOnTile
    UpdateStandingOnTile(); // Update the tile after the unit has moved

    EnableUI_Action();

    uiActionBar.GameObjectButton_move.SetActive(false); // Deactivate Move button

                if (currentUnit.hasAttacked == false)
                {
                    uiActionBar.GameObjectButton_return.SetActive(true); // Activate Return button
                }
                
    break;
            #endregion

            #region Attacking
            case TurnState.Attacking:
                if (!currentUnit.hasAttacked)
                {
                    // Clear old attack range before showing new one
                    mouseController.ClearAttackRangeTiles(); // Add this line
                    mouseController.GetAttackRangeTiles();
                    DisableUI_Action();
                    Debug.Log($"{currentUnit.name} is attacking...");
                    // Attack logic
                    EnableUI_Action();
                    uiActionBar.GameObjectButton_attack.SetActive(false);
                    //Changing to AttackingAnimation on TriggerAttackAnimationnearattacker from mousecontroller
                }
                else
                {
                    Debug.Log($"{currentUnit.name} has already attacked.");
                    EnableUI_Action();
                }
                break;
            #endregion

            #region AttackingAnimation
            case TurnState.AttackingAnimation:
                StartCoroutine(WaitForAttackAnimation(currentUnit.characterStats.faceDirection));
//                ChangeState(TurnState.Waiting);
                break;
            #endregion

            #region UsingSkill
            case TurnState.UsingSkill:
                DisableUI_Action();
                Debug.Log($"{currentUnit.name} is using a skill...");
                // Skill usage logic
                EnableUI_Action();
                if (uiActionBar != null && uiActionBar.GameObject_SkillUIGroup != null)
                {
                    uiActionBar.GameObject_SkillUIGroup.SetActive(true);
                }
                else
                {
                    Debug.LogError("Skill UI Group reference is missing!");
                }
                //uiActionBar.GameObjectButton_return.SetActive(true); // Activate Return button
                uiActionBar.GameObjectButton_return.SetActive(false);
                uiActionBar.GameObjectButton_move.SetActive(false);
                uiActionBar.GameObjectButton_attack.SetActive(false);
                //uiActionBar.GameObjectButton_condition.SetActive(false);
                //uiActionBar.GameObjectButton_wait.SetActive(false);
                break;
            #endregion

            #region SkillTargeting
            case TurnState.SkillTargeting:
                mouseController.ShowSkillRange(selectedSkill);
                DisableUI_Action();
                break;
            #endregion

            #region Waiting
            case TurnState.Waiting:
                uiActionBar.GameObject_SkillUIGroup.SetActive(false);
                EnableUI_Action();
                Debug.Log($"{currentUnit.name} is waiting...");
                Camera.main.transform.position = new Vector3(
                    currentUnit.transform.position.x,
                    currentUnit.transform.position.y,
                    Camera.main.transform.position.z
                );
                break;
            #endregion

            #region endTurn
            case TurnState.EndTurn:
                DisableUI_Action();
                currentUnit.selected = false;

                // Reset flags
                turnStarted = false;
                currentUnit.hasMoved = false;
                currentUnit.hasAttacked = false;
                currentUnit.characterStats.EndTurnRoundInitiative();

                // Handle end-of-round logic
                if (gameMaster != null)
                {
                    gameMaster.HandleEndOfRound();
                }

                // Update turn order and start new turn
                unitManager.SetTurnOrderList();
                if (unitManager.turnOrderList.Count > 0)
                {
                    SetCurrentUnit(unitManager.turnOrderList[0]);
                }

                // Transition to TurnStart
                ChangeState(TurnState.TurnStart);
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
            RaycastHit2D[] hits = Physics2D.RaycastAll(currentUnit.transform.position, Vector2.down);

            if (hits.Length > 0)
            {
                // Find the tile with the highest Z-axis value
                OverlayTile standingTile = hits
                    .OrderByDescending(hit => hit.collider.transform.position.z)
                    .Select(hit => hit.collider.GetComponent<OverlayTile>())
                    .FirstOrDefault(tile => tile != null);

                if (standingTile != null)
                {
                    currentUnit.standingOnTile = standingTile;
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

    #region ChangeState
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
        if (UI_ActionBar != null)
        {
            UI_ActionBar.SetActive(true);
        }
        else
        {
            Debug.LogError("UI_ActionBar is not assigned!");
            return;
        }

        // Update Move button
        uiActionBar.GameObjectButton_move.SetActive(!currentUnit.hasMoved);

        // Update Attack button
        uiActionBar.GameObjectButton_attack.SetActive(!currentUnit.hasAttacked);

        // Update Return button: Show if either:
        // 1. The unit has moved but hasn't attacked, OR
        // 2. The unit is in the Attacking state and hasn't attacked yet
        bool showReturn = (currentUnit.hasMoved || currentTurnState == TurnState.Attacking) && !currentUnit.hasAttacked;
        uiActionBar.GameObjectButton_return.SetActive(showReturn);
    }
    #endregion


    #region backbutton
    public void OnBackButtonPressed()
    {
        if (currentUnit == null || !currentUnit.hasMoved)
            return;

        // Reset the hasMoved flag
        currentUnit.hasMoved = false;

        // Clear the current tile's unit reference
        OverlayTile currentTile = currentUnit.standingOnTile;
        if (currentTile != null)
        {
            currentTile.unitOnTile = null;
            currentTile.isBlocked = false;
        }

        // Find the original tile at TurnStartingPosition
        RaycastHit2D[] hits = Physics2D.RaycastAll(TurnStartingPosition, Vector2.zero);
        OverlayTile originalTile = hits
            .OrderByDescending(hit => hit.collider.transform.position.z)
            .Select(hit => hit.collider.GetComponent<OverlayTile>())
            .FirstOrDefault(tile => tile != null);

        if (originalTile != null)
        {
            // Move the unit back to the original tile's position
            currentUnit.transform.position = originalTile.transform.position;
            currentUnit.GetComponent<SpriteRenderer>().sortingOrder = originalTile.GetComponent<SpriteRenderer>().sortingOrder;

            // Update tile references
            originalTile.unitOnTile = currentUnit;
            originalTile.isBlocked = true;
            currentUnit.standingOnTile = originalTile;
        }

        // Refresh the UI
        EnableUI_Action();
        
        if (currentUnit.hasAttacked == false)
        {
            uiActionBar.GameObjectButton_attack.SetActive(true);
        }
        // Update Return button (show if ANY action was taken)


        // Reset the camera to the unit's position
        Camera.main.transform.position = new Vector3(
            currentUnit.transform.position.x,
            currentUnit.transform.position.y,
            Camera.main.transform.position.z
        );
    }
    #endregion


    private IEnumerator WaitForAttackAnimation(CharacterStat.Direction attackDirection)
    {
        //// Get the length of the attack animation dynamically
        //AnimatorScript animatorScript = currentUnit.GetComponentInChildren<AnimatorScript>();
        //if (animatorScript != null)
        //{
        //    float animationLength = animatorScript.GetAttackAnimationLength(attackDirection);
        //    yield return new WaitForSeconds(animationLength); // Wait for the exact duration of the animation
        //}
        //else
        //{
        //    Debug.LogWarning("AnimatorScript not found. Using default animation duration.");
        //    yield return new WaitForSeconds(1.0f); // Fallback to a default duration
        //}
        yield return new WaitForSeconds(1.0f); // Fallback to a default duration

        // Transition to Waiting state after the animation is complete
        ChangeState(TurnState.Waiting);
    }
    public void OnAttackAnimationStarted()
    {
        // No additional logic needed here unless you want to track animation state
    }
    public void ProcessAITurn()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();

        foreach (var unit in allUnits)
        {
            if (unit.isAI && unit.IsAlive())
            {
                unit.DecideAction();
            }
        }
    }

}