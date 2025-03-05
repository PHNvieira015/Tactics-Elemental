using static ArrowTranslator;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using static TurnStateManager;

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
                    // Debug.Log($"Tile {tile.name} is within range.");
                    if (currentUnit.standingOnTile != tile)
                    {
                        path = pathFinder.FindPath(currentUnit.standingOnTile, tile, rangeFinderTiles);

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
                                path[i].SetSprite(arrow);
                            }
                        }
                    }
                }

                // Move the unit to the clicked position if it's in range
                if (Input.GetMouseButtonDown(0) && turnStateManager.currentTurnState == TurnState.Moving && rangeFinderTiles.Contains(tile))
                {
                    if (rangeFinderTiles.Contains(tile))
                    {
                        path = pathFinder.FindPath(currentUnit.standingOnTile, tile, rangeFinderTiles);
                        if (path.Count > 0)
                        {
                            isMoving = true; // Lock movement state
                        }
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
                                CharacterBattle attackerBattle = currentUnit.GetComponent<CharacterBattle>();
                                CharacterBattle targetBattle = targetUnit.GetComponent<CharacterBattle>();

                                if (attackerBattle != null && targetBattle != null)
                                {
                                    attackerBattle.TriggerAttackAnimationnearattacker(targetBattle);
                                    Debug.Log("Attack animation triggered!");

                                    damageSystem.Attack(currentUnit, targetUnit);
                                    turnStateManager.ChangeState(TurnState.Waiting);
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
            currentUnit.standingOnTile.activeCharacter = null;  // Clear the reference
            currentUnit.standingOnTile.isBlocked = false;
        }
        for (int i = 0; i < path.Count; i++)
        {
            var tile = path[i];

            //error de posição aqui!!
            while (!Mathf.Approximately(Vector2.Distance(currentUnit.transform.position, tile.transform.position), 0))
            {
                // Move the unit towards the current tile
                currentUnit.transform.position = Vector2.MoveTowards(
                    currentUnit.transform.position,
                    tile.transform.position,
                    Mathf.Min(speed * Time.deltaTime, Vector2.Distance(currentUnit.transform.position, tile.transform.position))
                );


                yield return null;
            }

            // After reaching the tile, position the character correctly on top of the tile
            PositionCharacterOnLine(tile);
        }
        // Clear arrow sprites after movement
        foreach (var tile in path)
        {
            tile.SetSprite(ArrowDirection.None);
        }

        path.Clear();
        _coroutineRunning = false;
        isMoving = false; // End movement
        currentUnit.hasMoved = true;
        turnStateManager.ChangeState(TurnState.Waiting
            );
        //GetInRangeTiles(); // Refresh range tiles after movement   //possibily change it as we should not move twice in a turn.
        
        //Sethasmoved Only After movement is complete
        currentUnit.hasMoved = true;
    }

    private void PositionCharacterOnLine(OverlayTile newTile)
    {
        if (currentUnit.standingOnTile != null && currentUnit.standingOnTile != newTile)
        {
            currentUnit.standingOnTile.isBlocked = false;
            currentUnit.standingOnTile.activeCharacter = null;
            currentUnit.standingOnTile.ClearUnit();
        }

        currentUnit.transform.position = new Vector3(
            newTile.transform.position.x,
            newTile.transform.position.y + 0.0001f,
            0
        );

        currentUnit.GetComponent<SpriteRenderer>().sortingOrder = newTile.GetComponent<SpriteRenderer>().sortingOrder;
        currentUnit.standingOnTile = newTile;
        newTile.isBlocked = true;
        newTile.activeCharacter = currentUnit;
        newTile.unitOnTile = currentUnit;
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

        if (currentUnit == null)
        {
            //Debug.LogError("CurrentUnit is null! Ensure SetUnit() is called before moving.");
            return;
        }

        if (currentUnit.standingOnTile == null)
        {
            Debug.LogError("CurrentUnit is not standing on a tile!");
            return;
        }

        if (currentUnit != null)
        {

            rangeFinderTiles = rangeFinder.GetTilesInRange(
            new Vector2Int(
                currentUnit.standingOnTile.gridLocation.x,
                currentUnit.standingOnTile.gridLocation.y
            ),
            Mathf.RoundToInt(currentUnit.characterStats.movementRange),
            false // Do not ignore obstacles (movement is blocked by obstacles)
        );
        // Only visualize the range if the turn state allows movement.
             if (turnStateManager != null && turnStateManager.currentTurnState == TurnState.Moving)
            {
                foreach (var tile in rangeFinderTiles)
                {
                    tile.ShowTile(color, TileType.Movement); // Visualize the range
                }

                //Debug.Log($"Highlighted {rangeFinderTiles.Count} tiles for movement.");
            }
        }
    }

    public void SetUnit(Unit newUnit)
    {
        //Debug.Log($"Assigning {newUnit.name} to currentUnit.");
        currentUnit = newUnit;
        // Optionally, you can check or set the starting tile here:
        if (currentUnit.standingOnTile == null)
        {
            Debug.LogWarning("CurrentUnit.standingOnTile is null! Make sure to assign it.");
        }
        GetInRangeTiles(); // Initialize range tiles for the new unit
    }
    // Call this method when the attack state is activated
    public void GetAttackRangeTiles()
    {
        if (currentUnit == null || currentUnit.standingOnTile == null)
        {
            Debug.LogError("CurrentUnit is null or not standing on a tile! Cannot determine attack range.");
            return;
        }

        // Calculate attack range tiles
        attackRangeTiles = rangeFinder.GetTilesInRange(
            new Vector2Int(
                currentUnit.standingOnTile.gridLocation.x,
                currentUnit.standingOnTile.gridLocation.y
            ),
            Mathf.RoundToInt(currentUnit.attackRange),
            true // Ignore obstacles for attack range
        );

        // Only visualize attack range when in Attack state
        if (turnStateManager != null && turnStateManager.currentTurnState == TurnState.Attacking)
        {
            foreach (var tile in attackRangeTiles)
            {
                tile.ShowTile(attackColor, TileType.AttackRangeColor); // Show red attack tiles
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
}
