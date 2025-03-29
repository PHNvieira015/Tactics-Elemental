using System;
using System.Linq;
using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class TurnStateManager : MonoBehaviour
{
    #region Variables

    [SerializeField] bool turnStarted;
    [SerializeField] public MouseController mouseController; // Use MouseController instead of PathFinder
    [HideInInspector] public Vector3 TurnStartingPosition;
    private TurnStateManager turnStateManager;
    [SerializeField] private UnitManager unitManager;
    public Skill selectedSkill; // Track selected skill
    private bool isEndTurnCoroutineRunning = false; // Add this flag
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
        SkillUsing,
        SkillAnimation,
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
                                              //Debug.Log($"Current unit set to {currentUnit.name}");
                                              // Trigger the UI update after setting the current unit

        Debug.Log($"Current unit set to {currentUnit.name}, AI: {currentUnit.isAI}");
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
                Debug.Log("Turn started");
                // Clear existing path arrows first
                if (mouseController != null)
                {
                    mouseController.ClearPathArrows();
                    TurnStartingPosition = currentUnit.transform.position;
                    currentUnit.standingOnTile = currentUnit.GetTileUnderUnit();

                    if (currentUnit.isAI == true)
                    {
                        AIController aiController = currentUnit.GetComponent<AIController>();
                        if (aiController != null)
                        {
                            aiController.StartTurn();
                        }
                    }
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
                EnableActionBarforPlayerUnit();

                currentUnit.GetTileUnderUnit();
                {
                    // Set camera to currentUnit's position
                    UpdateCameraPosition(currentUnit.transform.position);

                    //Debug.Log($"Camera set to {currentUnit.name} at {currentUnit.transform.position}");

                    // Initialize turn-specific logic
                    currentUnit.standingOnTile = currentUnit.GetTileUnderUnit();
                    mouseController.SetUnit(currentUnit);
                    turnStarted = true;
                }
                ChangeState(TurnState.Waiting);
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
                if(currentUnit.isAI==true)
                {
                    //currentUnit.aiController.DecideAction(); //Ai decision in Waiting
                }


                // After moving, update standingOnTile
                UpdateStandingOnTile(); // Update the tile after the unit has moved
                EnableActionBarforPlayerUnit();

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
                    EnableActionBarforPlayerUnit();
                    uiActionBar.GameObjectButton_attack.SetActive(false);
                    //Changing to AttackingAnimation on TriggerAttackAnimationnearattacker from mousecontroller
                }
                else
                {
                    Debug.Log($"{currentUnit.name} has already attacked.");
                    EnableActionBarforPlayerUnit();
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
                EnableActionBarforPlayerUnit();
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
                EnableActionBarforPlayerUnit();
                if (uiActionBar != null && uiActionBar.GameObject_SkillUIGroup != null)
                {
                    uiActionBar.GameObject_SkillUIGroup.SetActive(true);
                }
                else
                {
                    Debug.LogError("Skill UI Group reference is missing!");
                }
                uiActionBar.GameObjectButton_return.SetActive(true);
                uiActionBar.GameObjectButton_move.SetActive(false);
                uiActionBar.GameObjectButton_attack.SetActive(false);
                break;
            #endregion
            #region SkillAnimation
            case TurnState.SkillAnimation:
                StartCoroutine(WaitForSkillAnimation(currentUnit.characterStats.faceDirection));  //WIP
                //                ChangeState(TurnState.Waiting);
                break;
            #endregion
            #region Waiting
            case TurnState.Waiting:
 

                uiActionBar.GameObject_SkillUIGroup.SetActive(false);
                EnableActionBarforPlayerUnit();
                Debug.Log($"{currentUnit.name} is waiting...");
                UpdateCameraPosition(currentUnit.transform.position);
                if (currentUnit.isAI == true)
                {
                    if (currentUnit.aiController != null)
                    {
                        currentUnit.aiController.DecideAction(); //Ai decision in Waiting
                    }
                }
                break;
            #endregion

            #region endTurn
            case TurnState.EndTurn:
                StartCoroutine(EndTurnCoroutine());
                break; // Exit the function immediately to let coroutine handle the rest.

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
        // Ensure the current unit is valid
        if (currentUnit == null)
        {
            Debug.LogError("No current unit set. Cannot change state.");
            return;
        }
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

        uiActionBar.GameObjectButton_skill.SetActive(!currentUnit.usedSkill);

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
        UpdateCameraPosition(currentUnit.transform.position);
    }
    #endregion


    private IEnumerator WaitForAttackAnimation(CharacterStat.Direction attackDirection)
    {
        yield return new WaitForSeconds(1.0f); // Wait for the attack animation to complete

        // Ensure the state is updated after the animation
        if (currentTurnState == TurnState.AttackingAnimation)
        {
            ChangeState(TurnState.Waiting); // Transition to Waiting state
        }
    }
    private IEnumerator WaitForSkillAnimation(CharacterStat.Direction attackDirection)
    {
        yield return new WaitForSeconds(1.0f); // Wait for the attack animation to complete

        // Ensure the state is updated after the animation
        if (currentTurnState == TurnState.SkillAnimation)
        {
            ChangeState(TurnState.Waiting); // Transition to Waiting state
        }
    }
    public void OnAttackAnimationStarted()
    {
        // No additional logic needed here unless you want to track animation state
    }
    //public IEnumerator ProcessAITurn()
    //{
    //    Debug.Log("Processing AI turn");

    //    // Get all AI units
    //    Unit[] allUnits = FindObjectsOfType<Unit>();

    //    foreach (var unit in allUnits)
    //    {
    //        if (unit.isAI && unit.IsAlive())
    //        {
    //            Debug.Log($"Processing AI unit: {unit.name}");

    //            // Set the current unit
    //            SetCurrentUnit(unit);

    //            // Let the AI unit decide its action
    //            //unit.aiController.DecideAction();

    //            // Wait for the AI unit to finish its turn
    //            while (!unit.hasMoved && !unit.hasAttacked)
    //            {
    //                Debug.Log($"Waiting for {unit.name} to finish its turn...");
    //                yield return null; // Wait until the unit has moved and attacked
    //            }

    //            Debug.Log($"{unit.name} has finished its turn");
    //        }
    //    }

    //    Debug.Log("All AI units have finished their turns");

    //    // Transition to the player's turn
    //    ChangeState(TurnState.TurnStart);
    //}
    private void UpdateCameraPosition(Vector3 targetPosition)
    {
        Camera.main.transform.position = new Vector3(targetPosition.x, targetPosition.y, Camera.main.transform.position.z);
    }



    public void EnableActionBarforPlayerUnit()
    {
    if (currentUnit.isAI == false) //turn off UI if AI
        {
        EnableUI_Action();
        }
    }
    public void AIWaitTime(TurnState nextState)
    {
        if (currentUnit.isAI)
        {
            StartCoroutine(WaitAndDoAction(nextState));
        }
    }

    IEnumerator WaitAndDoAction(TurnState nextState)
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(5.0f);

        // Action to perform after waiting
        Debug.Log("2 seconds have passed!");
        ChangeState(nextState);
    }
    private IEnumerator EndTurnCoroutine()
    {
        Debug.Log("Waiting before processing EndTurn...");
        isEndTurnCoroutineRunning = true; // Set the flag
        yield return new WaitForSeconds(0.2f); // Wait for the full duration

        Debug.Log("EndTurn coroutine finished. Now executing EndTurn logic.");

        DisableUI_Action();
        currentUnit.selected = false;

        // Reset flags
        currentUnit.hasMoved = false;
        currentUnit.hasAttacked = false;
        currentUnit.usedSkill = false;
        currentUnit.characterStats.EndTurnRoundInitiative();

        // Handle end-of-round logic
        if (gameMaster != null)
        {
            gameMaster.HandleEndOfRound();
        }
        currentUnit.ReduceSkillCooldowns();
        // Update turn order and start new turn
        unitManager.SetTurnOrderList();
        if (unitManager.turnOrderList.Count > 0)
        {
            SetCurrentUnit(unitManager.turnOrderList[0]);
        }

        // Transition to TurnStart **AFTER** waiting
        ChangeState(TurnState.TurnStart);
        yield return new WaitForSeconds(0.2f); // Wait for the full duration
        isEndTurnCoroutineRunning = false; // Reset the flag
    }
    public void SelectSkill(Skill skill)
    {
        selectedSkill = skill;
        ChangeState(TurnState.SkillTargeting); // Transition to SkillTargeting state
    }

}