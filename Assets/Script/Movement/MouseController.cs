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

    void LateUpdate()
    {
        // Check if the mouse is over a UI element (e.g., a button)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // Skip further processing if the mouse is over a UI element
        }

        if (currentUnit != null)
        {
            Xposition = currentUnit.Xposition;
            Yposition = currentUnit.Xposition;
        }

        if (isMoving)
        {
            if (path.Count > 0 && !_coroutineRunning)
            {
                StartCoroutine(MoveAlongPathCoroutine());
            }
            return; // Prevent further path recalculation if already moving
        }

        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
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

            if (rangeFinderTiles.Contains(tile) && !isMoving)
            {
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
        }

    }


    private IEnumerator MoveAlongPathCoroutine()
    {
        _coroutineRunning = true;

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
        GetInRangeTiles(); // Refresh range tiles after movement
    }

    private void PositionCharacterOnLine(OverlayTile tile)
    {
        // Correctly place the unit on the tile (centering it)
        currentUnit.transform.position = new Vector3(
            tile.transform.position.x,
            tile.transform.position.y + 0.0001f,  // Slightly adjust Y to avoid overlap
            0
        );

        // Set the correct sorting order for the unit based on the tile's sorting order
        currentUnit.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;

        // Update the unit's standing tile to the new tile
        currentUnit.standingOnTile = tile;

        // Debug to ensure the unit is placed on the correct tile
        Debug.Log($"{currentUnit.name} is now standing on tile: {tile.name}");
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
            Debug.LogError("CurrentUnit is null! Ensure SetUnit() is called before moving.");
            return;
        }

        if (currentUnit.standingOnTile == null)
        {
            Debug.LogError("CurrentUnit is not standing on a tile!");
            return;
        }

        rangeFinderTiles = rangeFinder.GetTilesInRange(
            new Vector2Int(
                currentUnit.standingOnTile.gridLocation.x,
                currentUnit.standingOnTile.gridLocation.y
            ),
            Mathf.RoundToInt(currentUnit.characterStats.movementRange)
        );
        // Only visualize the range if the turn state allows movement.
        if (turnStateManager != null && turnStateManager.currentTurnState == TurnState.Moving)
        {
            foreach (var tile in rangeFinderTiles)
            {
                tile.ShowTile(color, TileType.Movement); // Visualize the range
            }

            Debug.Log($"Highlighted {rangeFinderTiles.Count} tiles for movement.");
        }
    }

    public void SetUnit(Unit newUnit)
    {
        Debug.Log($"Assigning {newUnit.name} to currentUnit.");
        currentUnit = newUnit;
        GetInRangeTiles(); // Initialize range tiles for the new unit
    }
    // Call this method when the attack state is activated
    public void GetAttackRangeTiles()
    {
        if (currentUnit == null)
        {
            Debug.LogError("CurrentUnit is null! Cannot determine attack range.");
            return;
        }

        if (currentUnit.standingOnTile == null)
        {
            Debug.LogError("CurrentUnit is not standing on a tile!");
            return;
        }

        attackRangeTiles = rangeFinder.GetTilesInRange(
            new Vector2Int(
                currentUnit.standingOnTile.gridLocation.x,
                currentUnit.standingOnTile.gridLocation.y
            ),
            Mathf.RoundToInt(currentUnit.attackRange) // Use attackRange variable
        );

        // Only visualize if in Attack state
        if (turnStateManager != null && turnStateManager.currentTurnState == TurnState.Attacking)
        {
            foreach (var tile in attackRangeTiles)
            {
                tile.ShowTile(attackColor, TileType.AttackRangeColor); // Show red attack tiles
            }

            Debug.Log($"Highlighted {attackRangeTiles.Count} tiles for attack range.");
        }
    }

}
