using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static ArrowTranslator;
using TacticsToolkit;

public class MouseController : MonoBehaviour
{
    public GameMaster gameMaster;  // Reference to the GameMaster
    public MapManager mapManager;
    public GameObject cursor;
    public float speed;
    public GameObject characterPrefab;  // Character prefab that should instantiate a Unit object
    private Unit unit;  // Now we're working with the Unit class

    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private ArrowTranslator arrowTranslator;
    private List<OverlayTile> path;
    private List<OverlayTile> rangeFinderTiles;
    private bool isMoving;

    public Color color = Color.green;  // Default color for the tiles (you can change this)

    public List<Unit> playerAvailableUnits;  // List of available units for the player

    private void Start()
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

        // Now you can access the available units like this:
        playerAvailableUnits = gameMaster.playerAvailableUnits;
    }

    void LateUpdate()
    {
        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
            cursor.transform.position = tile.transform.position;
            cursor.gameObject.GetComponent<SpriteRenderer>().sortingOrder = tile.transform.GetComponent<SpriteRenderer>().sortingOrder;

            if (rangeFinderTiles.Contains(tile) && !isMoving)
            {
                path = pathFinder.FindPath(unit.standingOnTile, tile, rangeFinderTiles);

                foreach (var item in rangeFinderTiles)
                {
                    MapManager.Instance.map[item.grid2DLocation].SetSprite(ArrowDirection.None);
                }

                for (int i = 0; i < path.Count; i++)
                {
                    var previousTile = i > 0 ? path[i - 1] : unit.standingOnTile;
                    var futureTile = i < path.Count - 1 ? path[i + 1] : null;

                    var arrow = arrowTranslator.TranslateDirection(previousTile, path[i], futureTile);
                    path[i].SetSprite(arrow);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Ensure ShowTile receives the correct TileType (use TileType instead of TileTypes)
                tile.ShowTile(color, TileType.Spawn);  // Assuming TileType is the correct enum type

                    if (gameMaster.playerAvailableUnits.Count > 0)
                    {
                    if (tile.GetComponent<SpawningTile>() != null)  // Check if the tile is a SpawningTile
                    {
                        SpawnUnitOnTile(tile);
                    }
                    else
                    {
                        Debug.Log("Cannot spawn unit here, tile is not a SpawningTile.");
                    }
                }
                else
                {
                    isMoving = true;
                    tile.gameObject.GetComponent<OverlayTile>().HideTile();
                }
            }
        }

        if (path.Count > 0 && isMoving)
        {
            MoveAlongPath();
        }
    }

    private void SpawnUnitOnTile(OverlayTile tile)
    {
        // Display the list of available units for the player to choose from
        if (playerAvailableUnits.Count > 0)
        {
            // Choose a unit (you can modify this based on your UI, or randomly select for now)
            Unit selectedUnit = playerAvailableUnits[0];  // Select the first unit as an example (modify this logic for selection)
            unit = Instantiate(selectedUnit.gameObject).GetComponent<Unit>();

            // Position the unit on the spawning tile
            PositionCharacterOnLine(tile);
            GetInRangeTiles();
        }
        else
        {
            Debug.Log("No available units to spawn.");
        }
    }

    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;

        float zIndex = path[0].transform.position.z;
        unit.transform.position = Vector2.MoveTowards(unit.transform.position, path[0].transform.position, step);
        unit.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y, zIndex);

        if (Vector2.Distance(unit.transform.position, path[0].transform.position) < 0.00001f)
        {
            PositionCharacterOnLine(path[0]);
            path.RemoveAt(0);
        }

        if (path.Count == 0)
        {
            GetInRangeTiles();
            isMoving = false;
        }
    }

    private void PositionCharacterOnLine(OverlayTile tile)
    {
        unit.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z);
        unit.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        unit.standingOnTile = tile;  // Set the unit's standing tile
    }

    private static RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    private void GetInRangeTiles()
    {
        rangeFinderTiles = rangeFinder.GetTilesInRange(new Vector2Int(unit.standingOnTile.gridLocation.x, unit.standingOnTile.gridLocation.y), 3);

        foreach (var item in rangeFinderTiles)
        {
            // Ensure you're passing the correct TileType here
            item.ShowTile(color, TileType.Movement);  // TileType instead of TileTypes
        }
    }
}
