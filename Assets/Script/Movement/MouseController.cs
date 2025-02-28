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
    //AttackRange
    public List<OverlayTile> attackRangeTiles; // Store attack range tiles
    public Color attackColor = Color.red;  // Red color for attack range

    public Vector3 TargetPosition { get; private set; } // Property to store the target position
    public Color color = Color.blue;  // Default color for the tiles (you can change this)

    private bool _coroutineRunning;

    void Start()
    {
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

        RaycastHit2D? hit = GetFocusedOnTile();


        //check here if unit 
        if (hit.HasValue)
        {
            
            #region movement
            // Attempt to get the Unit component on the hit object
            Unit hitUnit = hit.Value.collider.gameObject.GetComponent<Unit>();
            
            if (hitUnit != null)
            {
                // Found a Unit; now notify the UI_Manager.
                UI_Manager uiManager = FindObjectOfType<UI_Manager>();
                Debug.Log("Unit name is" + hitUnit.name);
                if (uiManager != null)
                {
                    uiManager.DisplayUnitInfo(hitUnit);
                }
            }
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
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

            #region attack indevelopment
            // When the mouse button is clicked
            if (Input.GetMouseButtonDown(0) && turnStateManager.currentTurnState == TurnState.Attacking)
            {
                if (attackRangeTiles.Contains(tile))
                {
                    // Look for units at this position
                    Unit targetUnit = tile.unitOnTile;  // Directly access the unit from the tile

                    Debug.Log($"Attempting to attack. Target unit found: {targetUnit != null}");

                    // Check if there's a valid unit and it's not the current player unit
                    if (targetUnit != null && targetUnit != currentUnit && !currentUnit.hasAttacked)
                    {
                        Debug.Log($"Attack conditions met! Current unit: {currentUnit.name}, Target: {targetUnit.name}");

                        // Optionally check if it's an enemy unit (if you have a player-owner system)
                        if (targetUnit.playerOwner != currentUnit.playerOwner)
                        {
                            // Trigger the attack
                            damageSystem.Attack(currentUnit, targetUnit);
                            Debug.Log("Attack performed!");

                            // Clear attack range tiles after attack
                            foreach (var rangeTile in attackRangeTiles)
                            {
                                rangeTile.HideTile();
                            }

                            // Mark the current unit as having attacked
                            currentUnit.hasAttacked = true;

                            // Clear the attack range tiles
                            attackRangeTiles.Clear();
                        }
                        else
                        {
                            Debug.Log("Cannot attack your own unit.");
                        }
                    }
                    else
                    {
                        Debug.Log($"Attack conditions not met: " +
                                  $"Has Unit: {targetUnit != null}, " +
                                  $"Haven't Attacked: {!currentUnit.hasAttacked}");
                    }
                }
            }
            #endregion

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

        path.Clear();
        _coroutineRunning = false;
        isMoving = false; // End movement
        GetInRangeTiles(); // Refresh range tiles after movement   //possibily change it as we should not move twice in a turn.
    }

    private void PositionCharacterOnLine(OverlayTile newTile)
    {
        // Free the previous tile (if any) before moving
        if (currentUnit.standingOnTile != null && currentUnit.standingOnTile != newTile)
        {
            currentUnit.standingOnTile.isBlocked = false;
            currentUnit.standingOnTile.activeCharacter = null;
            currentUnit.standingOnTile.ClearUnit();  // <---- This method resets the tile data.
        }

        // Snap the unit to the new tile's position (with a slight Y adjustment)
        currentUnit.transform.position = new Vector3(
            newTile.transform.position.x,
            newTile.transform.position.y + 0.0001f,0  // Slight Y offset
        );

        // Update the sorting order based on the new tile
        currentUnit.GetComponent<SpriteRenderer>().sortingOrder = newTile.GetComponent<SpriteRenderer>().sortingOrder;

        // Set the new tile as the unit's current tile
        currentUnit.standingOnTile = newTile;

        // Mark the new tile as occupied by the unit
        newTile.isBlocked = true;
        newTile.activeCharacter = currentUnit;
        newTile.unitOnTile = currentUnit;
        //Debug.Log($"{currentUnit.name} is now standing on tile: {newTile.name}");
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


}
