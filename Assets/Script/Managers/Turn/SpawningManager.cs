using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class SpawningManager : MonoBehaviour
{
    /*
    [SerializeField] private GameObject SpawningSelection;
    public List<Unit> playerAvailableUnits;  // List of available units for the player
    public MouseController mouseController;  // Reference to the MouseController
    public Color spawnTileColor = Color.blue;  // Color for spawn tiles

    void Awake()
    {
        playerAvailableUnits = new List<Unit>();

        // Subscribe to game state change event
        GameManager.OnGameStateChanged += GameManagerOnOnGameStateChanged;
    }
    void OnDestroy()
    {
        // Unsubscribe from game state change event
        GameManager.OnGameStateChanged -= GameManagerOnOnGameStateChanged;
    }

    private void GameManagerOnOnGameStateChanged(GameState state)
    {
        // Toggle SpawningSelection visibility based on game state
        SpawningSelection.SetActive(state == GameState.SpawningUnits);

        // If we are in the SpawningUnits state, update the game state to PlayerTurn
        if (state == GameState.SpawningUnits)
        {
            // Now we can safely access the singleton GameManager and update the state
            GameManager.instance.UpdateGameState(GameState.PlayerTurn);
        }
    }
    private void Update()
    {
RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();

            // Show valid spawn tiles
            if (tile.GetComponent<SpawningTile>() != null)
            {
                tile.ShowTile(spawnTileColor, TileType.Spawn);
            }

            if (Input.GetMouseButtonDown(0) && tile.GetComponent<SpawningTile>() != null)
            {
                // Spawn unit when player clicks on a spawning tile
                SpawnUnitOnTile(tile);
            }
        }
    }

    private void SpawnUnitOnTile(OverlayTile tile)
    {
        if (playerAvailableUnits.Count > 0)
        {
            // Choose the first unit from the list (you can modify this based on your UI or selection logic)
            Unit selectedUnit = playerAvailableUnits[0];
            Unit unitInstance = Instantiate(selectedUnit.gameObject).GetComponent<Unit>();

            // Position the unit on the spawning tile
            unitInstance.transform.position = tile.transform.position;
            unitInstance.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
            unitInstance.standingOnTile = tile;

            // Set the unit in the MouseController for movement control
            mouseController.SetUnit(unitInstance);

            Debug.Log("Unit spawned at: " + tile.transform.position);
        }
        else
        {
            Debug.Log("No available units to spawn.");
        }
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
    */
}