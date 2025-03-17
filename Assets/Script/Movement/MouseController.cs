using static ArrowTranslator;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using static TurnStateManager;
using System;

public class MouseController : MonoBehaviour
{
    public GameMaster gameMaster;  // Reference to the GameMaster
    public MapManager mapManager;
    public GameObject cursor;
    public float speed;
    [SerializeField] public Unit currentUnit;  // Now we're working with the Unit class

    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private ArrowTranslator arrowTranslator;
    private List<OverlayTile> path;
    public List<OverlayTile> rangeFinderTiles;
    public bool isMoving = false;
    public TurnStateManager turnStateManager;  // Reference to TurnStateManager
    public float Xposition;
    public float Yposition;
    public DamageSystem damageSystem;
    public BattleHandler battleHandler;
    public UnitManager UnitManager;
    //AttackRange
    public List<OverlayTile> attackRangeTiles; // Store attack range tiles
    public Color attackColor = Color.red;  // Red color for attack range
    public GameObject SelectedUnitInfo;

    public Vector3 TargetPosition { get; private set; } // Property to store the target position
    public Color color = Color.blue;  // Default color for the tiles (you can change this)

    private bool _coroutineRunning;

    void Start()
    {
        //SelectedUnitInfo.SetActive(false);
        //Pathfinder
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        arrowTranslator = new ArrowTranslator();

        path = new List<OverlayTile>();
        isMoving = false;
        rangeFinderTiles = new List<OverlayTile>();

        if (gameMaster == null)
        {
            gameMaster = FindObjectOfType<GameMaster>();  // In case it's not assigned via Inspector
        }

        if (turnStateManager == null)
        {
            turnStateManager = FindObjectOfType<TurnStateManager>();  // Find the TurnStateManager if it's not assigned
        }

        // Make sure we update the movement range if needed
        if (currentUnit != null)
        {
            GetInRangeTiles(); // Initialize range tiles for the unit
        }
        //AttackRange
        attackRangeTiles = new List<OverlayTile>(); // Initialize attack range list
    }

    void Update()
    {
        //Check if the mouse is over a UI element (e.g., a button)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // Skip further processing if the mouse is over a UI element
        }

        if (currentUnit != null)
        {
            Xposition = currentUnit.Xposition;
            Yposition = currentUnit.Yposition;
        }

        if (isMoving)
        {
            if (path.Count > 0 && !_coroutineRunning)
            {
                StartCoroutine(MoveAlongPathCoroutine());
                currentUnit.hasMoved = true;

            }
            return; // Prevent further path recalculation if already moving
        }
        if (turnStateManager.currentTurnState == TurnState.Attacking)
        {
            GetAttackRangeTiles();
        }
        if (turnStateManager.currentTurnState == TurnState.SkillTargeting)
        {
            ShowSkillRange(turnStateManager.selectedSkill);
        }

        RaycastHit2D? hit = GetFocusedOnTile();

        // Check if a valid tile is hit
        if (hit.HasValue)
        {
            // Get the OverlayTile component
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
            if (tile != null)
            {
                // Get the unit from the tile
                Unit unitOnTile = tile.unitOnTile;
                Debug.Log("Unit on tile: " + (unitOnTile != null ? unitOnTile.name : "None"));


                #region movement
                if (unitOnTile != null)
                {
                    SelectedUnitInfo.SetActive(true);
                    UnitManager.SetSelectedUnit(unitOnTile);

                    // Found a Unit; now notify the UI_Manager.
                    UI_Manager uiManager = FindObjectOfType<UI_Manager>();
                    if (uiManager != null)
                    {
                        uiManager.DisplayUnitInfo(unitOnTile);
                    }
                    else
                    {
                        SelectedUnitInfo.SetActive(false);
                    }
                }

                cursor.transform.position = tile.transform.position;
                cursor.gameObject.GetComponent<SpriteRenderer>().sortingOrder = tile.transform.GetComponent<SpriteRenderer>().sortingOrder;

                // Update TargetPosition
                TargetPosition = tile.transform.position;
                GetInRangeTiles();
                if (currentUnit == null || currentUnit.standingOnTile == null || currentUnit.characterStats == null)
                {
                    return; // Exit early if unit, tile, or stats are not ready
                }
                if (rangeFinderTiles.Contains(tile))

                {
                    //Debug.Log($"Tile {tile.name} is within range.");
                    //Debug.Log($"Path Count Before Calculation: {path.Count}");
                    // Only recalculate path if the target tile is in range
                    if (currentUnit.standingOnTile != tile)
                    {
                        // Calculate the path from the current unit's standing tile to the clicked tile
                        path = pathFinder.FindPath(currentUnit.standingOnTile, tile, rangeFinderTiles);

                        // Visualize the path with arrows
                        foreach (var item in rangeFinderTiles)
                        {
                            MapManager.Instance.map[item.grid2DLocation].SetSprite(ArrowDirection.None);
                        }
                        if (turnStateManager.currentTurnState == TurnState.Moving)
                        {
                            for (int i = 0; i < path.Count; i++)
                            {
                                var previousTile = i > 0 ? path[i - 1] : currentUnit.standingOnTile;
                                var futureTile = i < path.Count - 1 ? path[i + 1] : null;

                                var arrow = arrowTranslator.TranslateDirection(previousTile, path[i], futureTile);
                                path[i].SetSprite((ArrowDirection)arrow); // Cast to ArrowDirection
                            }
                        }
                    }
                }

                // Move the unit to the clicked position if it's in range
                if (Input.GetMouseButtonDown(0) && turnStateManager.currentTurnState == TurnState.Moving)

                {
                    if (tile.unitOnTile == null)  // Prevent landing on occupied tiles
                    {
                        path = pathFinder.FindPath(currentUnit.standingOnTile, tile, rangeFinderTiles);
                        if (path.Count > 0) isMoving = true;
                    }
                    else
                    {
                        Debug.Log("Cannot move to occupied tile");
                    }
                }
                #endregion

                #region attack
                if (Input.GetMouseButtonDown(0) && turnStateManager.currentTurnState == TurnState.Attacking && attackRangeTiles.Contains(tile))
                {
                    if (attackRangeTiles.Contains(tile))
                    {
                        // Get the target unit from the tile
                        Unit targetUnit = tile.unitOnTile;
                        Debug.Log($"Attempting to attack. Target unit found: {targetUnit != null}");

                        if (targetUnit != null && targetUnit != currentUnit && !currentUnit.hasAttacked)
                        {
                            Debug.Log($"Attack conditions met! Current unit: {currentUnit.name}, Target: {targetUnit.name}");

                            if (targetUnit.playerOwner != currentUnit.playerOwner)
                            {
                                UpdateFaceDirection(targetUnit.standingOnTile);
                                CharacterBattle attackerBattle = currentUnit.GetComponent<CharacterBattle>();
                                CharacterBattle targetBattle = targetUnit.GetComponent<CharacterBattle>();

                                if (attackerBattle != null && targetBattle != null)
                                {
                                    turnStateManager.ChangeState(TurnState.AttackingAnimation);       //Attacking Animation
                                    //attackerBattle.TriggerAttackAnimationnearattacker(targetBattle);  //Sliding Animation
                                    Debug.Log("Attack animation triggered!");


                                    currentUnit.hasAttacked = true;
                                    turnStateManager.uiActionBar.GameObjectButton_attack.SetActive(false);
                                    StartCoroutine(WaitForAttackAndMovement(attackerBattle));
                                    damageSystem.Attack(currentUnit, targetUnit);
                                }
                                else
                                {
                                    Debug.LogWarning("CharacterBattle component not found on one of the units.");
                                }
                            }
                            else
                            {
                                Debug.Log("Cannot attack your own unit.");
                            }
                        }
                        else
                        {
                            Debug.Log($"Attack conditions not met: " +
                                      $"Target exists: {targetUnit != null}, " +
                                      $"Hasn't Attacked: {!currentUnit.hasAttacked}");
                        }
                    }
                }
                #endregion
            }
        }
    }



    private IEnumerator MoveAlongPathCoroutine()
    {
        _coroutineRunning = true;

        // Clear the unit from the previous tile before moving
        if (currentUnit.standingOnTile != null)
        {
            currentUnit.standingOnTile.activeCharacter = null;
            currentUnit.standingOnTile.isBlocked = false;
        }
        // Check if there are any tiles in the path
        if (path.Count > 0)
        {
            var firstTile = path[0];

            // Update the face direction for the first tile
            if (currentUnit.standingOnTile != firstTile)
            {
                UpdateFaceDirection(firstTile);

            }
        }

        for (int i = 0; i < path.Count; i++)
        {
            var tile = path[i];
            bool isFinalTile = (i == path.Count - 1);

            // Update unit facing direction BEFORE moving
            if (i < path.Count - 1)
            {
                // Calculate the direction to the next tile
                Vector2Int movementDirection = new Vector2Int(
                    path[i + 1].gridLocation.x - path[i].gridLocation.x,
                    path[i + 1].gridLocation.y - path[i].gridLocation.y
                );
                // Determine characterStat.Direction based on movement direction
                // Set the unit's facing direction based on movement direction
                if (movementDirection == new Vector2Int(0, 1))
                {
                    currentUnit.characterStats.faceDirection = CharacterStat.Direction.UpLeft;  // Moving North   UpRight
                }
                else if (movementDirection == new Vector2Int(0, -1))
                {
                    currentUnit.characterStats.faceDirection = CharacterStat.Direction.DownRight;  // Moving South  DownRight
                }
                else if (movementDirection == new Vector2Int(-1, 0))
                {
                    currentUnit.characterStats.faceDirection = CharacterStat.Direction.DownLeft;  // Moving West   UpLeft
                }
                else if (movementDirection == new Vector2Int(1, 0))
                {
                    currentUnit.characterStats.faceDirection = CharacterStat.Direction.UpRight;  // Moving East  DownLeft
                }
            }

        // Move the unit to the current tile
        while (!Mathf.Approximately(Vector2.Distance(currentUnit.transform.position, tile.transform.position), 0))
            {
                currentUnit.transform.position = Vector2.MoveTowards(
                    currentUnit.transform.position,
                    tile.transform.position,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            PositionCharacterOnLine(tile, isFinalTile); // Pass final tile status
        }

        // Clear arrows and finish movement
        foreach (var tile in path) tile.SetSprite(ArrowDirection.None);
        path.Clear();
        _coroutineRunning = false;
        isMoving = false;
        currentUnit.hasMoved = true;
        turnStateManager.ChangeState(TurnState.Waiting);
    }

    private void UpdateFaceDirection(OverlayTile targetTile)
    {
        Vector2Int movementDirection = new Vector2Int(
            targetTile.gridLocation.x - currentUnit.standingOnTile.gridLocation.x,
            targetTile.gridLocation.y - currentUnit.standingOnTile.gridLocation.y
        );
        if (movementDirection == new Vector2Int(0, 1))
        {
            currentUnit.characterStats.faceDirection = CharacterStat.Direction.UpLeft;  // Moving North   UpRight
        }
        else if (movementDirection == new Vector2Int(0, -1))
        {
            currentUnit.characterStats.faceDirection = CharacterStat.Direction.DownRight;  // Moving South  DownRight
        }
        else if (movementDirection == new Vector2Int(-1, 0))
        {
            currentUnit.characterStats.faceDirection = CharacterStat.Direction.DownLeft;  // Moving West   UpLeft
        }
        else if (movementDirection == new Vector2Int(1, 0))
        {
            currentUnit.characterStats.faceDirection = CharacterStat.Direction.UpRight;  // Moving East  DownLeft
        }

    }
    private void PositionCharacterOnLine(OverlayTile newTile, bool isFinalTile)
    {
        // Clear previous tile's references if moving from it
        if (currentUnit.standingOnTile != null && currentUnit.standingOnTile != newTile)
        {
            currentUnit.standingOnTile.isBlocked = false;
            currentUnit.standingOnTile.activeCharacter = null;

            // Only clear unitOnTile if it was set by this unit
            if (currentUnit.standingOnTile.unitOnTile == currentUnit)
            {
                currentUnit.standingOnTile.unitOnTile = null;
            }
        }

        // Update unit's position and tile reference
        currentUnit.transform.position = new Vector3(
            newTile.transform.position.x,
            newTile.transform.position.y + 0.0001f,
            0
        );

        currentUnit.GetComponent<SpriteRenderer>().sortingOrder = newTile.GetComponent<SpriteRenderer>().sortingOrder;
        currentUnit.standingOnTile = newTile;

        // Only block and claim the tile if it's the final destination
        if (isFinalTile)
        {
            newTile.isBlocked = true;
            newTile.activeCharacter = currentUnit;
            newTile.unitOnTile = currentUnit;
        }
    }

    private static RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        if (hits.Length > 0)
        {
            // Return the tile hit based on the highest sorting order (Z-axis)
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    public void GetInRangeTiles()
    {
        if (currentUnit == null || currentUnit.standingOnTile == null)
        {
            //Debug.LogError("CurrentUnit is null or not on a tile!");
            return;
        }

        rangeFinderTiles = rangeFinder.GetTilesInRange(
            new Vector2Int(
                currentUnit.standingOnTile.gridLocation.x,
                currentUnit.standingOnTile.gridLocation.y
            ),
            Mathf.RoundToInt(currentUnit.characterStats.movementRange),
            currentUnit.teamID,
            false // Pass false to respect obstacles (including enemy units)
        );

        // Visualize only if in Moving state
        if (turnStateManager != null && turnStateManager.currentTurnState == TurnState.Moving)
        {
            foreach (var tile in rangeFinderTiles)
            {
                if (tile.unitOnTile == null)
                {
                    tile.ShowTile(color, TileType.Movement);
                }
            }
        }
    }

    public void SetUnit(Unit newUnit)
    {
        Debug.Log($"Setting MouseController's currentUnit to: {newUnit?.name}");

        ClearPathArrows();
        ClearAttackRangeTiles(); // Clear old attack tiles

        currentUnit = newUnit;
        GetInRangeTiles(); // Refresh movement range

        // Initialize facing direction based on the unit's current position
        if (currentUnit != null && currentUnit.standingOnTile != null)
        {
            //currentUnit.characterStats.faceDirection = CharacterStat.Direction.UpRight; // Default direction
        }

        Debug.Log($"Attack range tiles after SetUnit: {attackRangeTiles?.Count}");
    }
    // Call this method when the attack state is activated

    public void GetAttackRangeTiles()
    {
        ClearAttackRangeTiles(); // Clear old tiles first
        Debug.Log($"Calculating attack range for: {currentUnit?.name} (Attack Range: {currentUnit?.attackRange})");

        if (currentUnit == null || currentUnit.standingOnTile == null)
        {
            Debug.LogError("CurrentUnit is null or not on a tile!");
            return;
        }

        attackRangeTiles = rangeFinder.GetTilesInRange(
            new Vector2Int(
                currentUnit.standingOnTile.gridLocation.x,
                currentUnit.standingOnTile.gridLocation.y
            ),
            Mathf.RoundToInt(currentUnit.attackRange), // Use attackRange instead of movementRange
            currentUnit.teamID,
            true // Pass true to ignore obstacles (including enemy units)
        );

        // Visualize only if in Attack state
        if (turnStateManager.currentTurnState == TurnState.Attacking)
        {
            foreach (var tile in attackRangeTiles)
            {
                tile.ShowTile(attackColor, TileType.AttackRangeColor);
            }
        }
    }
    public void ClearPathArrows()
    {
        foreach (var tile in path)
        {
            tile.SetSprite(ArrowDirection.None);
        }
        path.Clear();

        // Also clear any range highlights
        foreach (var tile in rangeFinderTiles)
        {
            tile.HideTile();
        }
        rangeFinderTiles.Clear();
    }
    public void ClearAttackRangeTiles()
    {
        if (attackRangeTiles != null)
        {
            foreach (var tile in attackRangeTiles)
            {
                tile.HideTile(); // Hide attack indicators
            }
            attackRangeTiles.Clear();
        }
    }
    public void ShowSkillRange(Skill skill)
    {
        ClearAttackRangeTiles();
        attackRangeTiles = rangeFinder.GetTilesInRange(
            currentUnit.standingOnTile.grid2DLocation,
            skill.range,  // Now using the parameter
            currentUnit.teamID,
            true
        );

        foreach (var tile in attackRangeTiles)
        {
            tile.ShowTile(Color.cyan, TileType.DamageSkillColor);
        }
    }
    private void UseSkillOnTarget(OverlayTile targetTile)
    {
        Unit targetUnit = targetTile.unitOnTile;
        Skill skill = turnStateManager.selectedSkill;

        // Apply skill effects
        if (skill.targetType == Skill.TargetType.Enemy && targetUnit != null)
        {
            damageSystem.CalculateDamage(currentUnit, targetUnit);
        }
        // Add other target types and effects as needed

        // Consume resources
        currentUnit.hasAttacked = true;
        turnStateManager.ChangeState(TurnStateManager.TurnState.Waiting);
        ClearAttackRangeTiles();
    }
    private IEnumerator WaitForAttackAndMovement(CharacterBattle attackerBattle)
    {
        // Wait for the attack animation to complete (adjust duration as needed)
        yield return new WaitForSeconds(1.0f); // Adjust based on your animation length

        // Wait for the movement to complete
        while (attackerBattle.GetState() != CharacterBattle.State.Idle)
        {
            yield return null; // Wait until the character stops moving
        }

        // Transition to Waiting state after both animation and movement are complete
        turnStateManager.ChangeState(TurnState.Waiting);
    }

}