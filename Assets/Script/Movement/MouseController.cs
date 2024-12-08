using static ArrowTranslator;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameMaster gameMaster;  // Reference to the GameMaster
    public MapManager mapManager;
    public GameObject cursor;
    public float speed;
    private Unit currentUnit;  // Now we're working with the Unit class

    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    private ArrowTranslator arrowTranslator;
    private List<OverlayTile> path;
    private List<OverlayTile> rangeFinderTiles;
    public bool isMoving;
    public TurnStateManager turnStateManager;  // Reference to TurnStateManager


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
        // Make sure the reference to TurnStateManager is set
        if (turnStateManager == null)
        {
            turnStateManager = FindObjectOfType<TurnStateManager>();  // Find the TurnStateManager if it's not assigned
        }
    }

    void LateUpdate()
    {
        // Ensure that the current state is "Moving" before allowing movement
        if (turnStateManager.currentTurnState != TurnStateManager.TurnState.Moving)
        {
            return;  // If we're not in the Moving state, exit the function
        }

        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();
            cursor.transform.position = tile.transform.position;
            cursor.gameObject.GetComponent<SpriteRenderer>().sortingOrder = tile.transform.GetComponent<SpriteRenderer>().sortingOrder;

            if (rangeFinderTiles.Contains(tile) && !isMoving)
            {
                path = pathFinder.FindPath(currentUnit.standingOnTile, tile, rangeFinderTiles);

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

            if (Input.GetMouseButtonDown(0))
            {
                // Handle the unit movement only (no spawning logic here)
                if (isMoving)
                {
                    tile.gameObject.GetComponent<OverlayTile>().HideTile();
                    isMoving = false; // Once the movement is completed, stop moving.
                }
            }
        }

        if (path.Count > 0 && isMoving)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;

        float zIndex = path[0].transform.position.z;
        currentUnit.transform.position = Vector2.MoveTowards(currentUnit.transform.position, path[0].transform.position, step);
        currentUnit.transform.position = new Vector3(currentUnit.transform.position.x, currentUnit.transform.position.y, zIndex);

        if (Vector2.Distance(currentUnit.transform.position, path[0].transform.position) < 0.00001f)
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
        currentUnit.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z);
        currentUnit.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        currentUnit.standingOnTile = tile;  // Set the unit's standing tile
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
        rangeFinderTiles = rangeFinder.GetTilesInRange(new Vector2Int(currentUnit.standingOnTile.gridLocation.x, currentUnit.standingOnTile.gridLocation.y), 3);

        foreach (var item in rangeFinderTiles)
        {
            // Ensure you're passing the correct TileType here
            item.ShowTile(color, TileType.Movement);  // TileType instead of TileTypes
        }
    }

    // SetUnit method to be used by SpawningManager
    public void SetUnit(Unit newUnit)
    {
        currentUnit = newUnit;
    }
}
