using static ArrowTranslator;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;
using System.Collections;

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
    private List<OverlayTile> rangeFinderTiles;
    public bool isMoving;
    public TurnStateManager turnStateManager;  // Reference to TurnStateManager

    public Vector3 TargetPosition { get; private set; } // Property to store the target position
    public Color color = Color.green;  // Default color for the tiles (you can change this)

    void Start()
    {
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
    }

    void LateUpdate()
    {
        if (currentUnit != null)
        {
            // Directly use currentUnit.standingOnTile (no need for a separate standingOnTile field)
            Debug.Log($"Unit Position: {currentUnit.gameObject.transform.position}, Standing on Tile: {currentUnit.standingOnTile?.name}");
        }

        if (isMoving)
        {
            if (path.Count > 0)
            {
                StartCoroutine(MoveAlongPathCoroutine());
            }
            return; // Prevent further path recalculation if already moving
        }

        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
            cursor.transform.position = tile.transform.position;
            cursor.gameObject.GetComponent<SpriteRenderer>().sortingOrder = tile.transform.GetComponent<SpriteRenderer>().sortingOrder;

            // Update TargetPosition
            TargetPosition = tile.transform.position;

            if (rangeFinderTiles.Contains(tile) && !isMoving)
            {
                // Only recalculate path if target tile has changed
                if (currentUnit.standingOnTile != tile)
                {
                    path = pathFinder.FindPath(currentUnit.standingOnTile, tile, rangeFinderTiles);

                    // Visualize the path using arrows
                    foreach (var item in rangeFinderTiles)
                    {
                        MapManager.Instance.map[item.grid2DLocation].SetSprite(ArrowDirection.None);
                    }

                    for (int i = 0; i < path.Count; i++)
                    {
                        var previousTile = i > 0 ? path[i - 1] : currentUnit.standingOnTile;
                        var futureTile = i < path.Count - 1 ? path[i + 1] : null;

                        var arrow = arrowTranslator.TranslateDirection(previousTile, path[i], futureTile);
                        path[i].SetSprite(arrow);
                    }
                }
            }

            if (Input.GetMouseButtonDown(0) && rangeFinderTiles.Contains(tile))
            {
                path = pathFinder.FindPath(currentUnit.standingOnTile, tile, rangeFinderTiles);
                isMoving = true; // Lock movement state
                Debug.Log($"Path calculated: {path.Count} tiles.");
            }
        }
    }

    private IEnumerator MoveAlongPathCoroutine()
    {
        foreach (var tile in path)
        {
            while (Vector2.Distance(currentUnit.transform.position, tile.transform.position) > 0.01f)
            {
                currentUnit.gameObject.transform.position = Vector2.MoveTowards(
                    currentUnit.gameObject.transform.position,
                    tile.transform.position,
                    speed * Time.deltaTime
                );
                yield return null;
            }
            PositionCharacterOnLine(tile);
        }

        isMoving = false; // End movement
        GetInRangeTiles(); // Refresh range tiles after movement
    }

    private void PositionCharacterOnLine(OverlayTile tile)
    {
        // Set the unit's position to the center of the tile
        currentUnit.gameObject.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z);

        // Update the sorting order based on the tile's sorting order
        currentUnit.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;

        // Update the unit's standing tile after movement
        currentUnit.standingOnTile = tile;

        // Debug to ensure correct tile assignment
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

        foreach (var tile in rangeFinderTiles)
        {
            tile.ShowTile(color, TileType.Movement); // Visualize the range
        }

        Debug.Log($"Highlighted {rangeFinderTiles.Count} tiles for movement.");
    }

    public void SetUnit(Unit newUnit)
    {
        Debug.Log($"Assigning {newUnit.name} to currentUnit.");
        currentUnit = newUnit;
        GetInRangeTiles(); // Initialize range tiles for the new unit
    }
}
