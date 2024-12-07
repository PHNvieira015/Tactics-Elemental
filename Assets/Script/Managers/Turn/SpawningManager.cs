using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;  // Required for UI components

public class SpawningManager : MonoBehaviour
{
    [SerializeField] private GameMaster gameMaster;  // Reference to the GameMaster
    [SerializeField] private GameObject SpawningSelection;
    [SerializeField] private Button startButton;  // Reference to the Start button
    public List<Unit> playerAvailableUnits;  // List of available units for the player
    public List<Unit> playedUnits;  // List of units that have already been placed
    public MouseController mouseController;  // Reference to the MouseController
    public Color spawnTileColor = Color.blue;  // Color for spawn tiles
    private GameObject unitPreview;  // Preview of the unit being placed
    [SerializeField] private GameObject EnemySpawnerTiles;
    [SerializeField] private GameObject PlayerSpawningTiles;


    void Awake()
    {
        playedUnits = new List<Unit>(); // Initialize the list of played units

        // Make sure we have a valid reference to GameMaster
        if (gameMaster != null)
        {
            // Copy the list from the GameMaster to the SpawningManager
            playerAvailableUnits = new List<Unit>(gameMaster.playerAvailableUnits);
        }
        else
        {
            Debug.LogError("GameMaster reference is missing!");
        }

        // Subscribe to game state change event
        GameMaster.OnGameStateChanged += GameManagerOnGameStateChanged;

        // Ensure Start Button is hooked up
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.interactable = true;  // Enable start button immediately
        }
        else
        {
            Debug.LogError("Start Button is not assigned!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from game state change event
        GameMaster.OnGameStateChanged -= GameManagerOnGameStateChanged;

        // Unsubscribe the button click listener
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    private void GameManagerOnGameStateChanged(GameMaster.GameState state)
    {
        // Toggle SpawningSelection visibility based on game state
        SpawningSelection.SetActive(state == GameMaster.GameState.SpawningUnits);
    }

    // Called when the player clicks the Start Button
    private void OnStartButtonClicked()
    {
        if (GameMaster.instance != null)
        {
            GameMaster.instance.playerList = new List<Unit>(playedUnits); // Ensure we send the list properly
            Debug.Log("Player units assigned to GameMaster.");
        }
        else
        {
            Debug.LogError("GameMaster instance is not available.");
        }

        Debug.Log("Starting Game Round.");
        GameMaster.instance.UpdateGameState(GameMaster.GameState.GameRound);

        // Disable the Start button to prevent multiple clicks
        startButton.gameObject.SetActive(false);
        EnemySpawnerTiles.gameObject.SetActive(false);
        PlayerSpawningTiles.gameObject.SetActive(false);
    }


    private void Update()
    {
        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();

            // Check if we got a valid tile and it's a spawnable tile
            if (tile != null && tile.GetComponent<SpawningTile>() != null && !tile.GetComponent<SpawningTile>().IsOccupied)
            {
                tile.ShowTile(spawnTileColor, TileType.Spawn);

                // If the preview doesn't exist yet, create it
                if (unitPreview == null && playerAvailableUnits.Count > 0)
                {
                    // Preview the first available unit
                    Unit selectedUnit = playerAvailableUnits[0];
                    unitPreview = Instantiate(selectedUnit.gameObject);
                    unitPreview.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
                    unitPreview.transform.position = tile.transform.position;
                    unitPreview.SetActive(true);  // Show the preview
                }
                else if (unitPreview != null)
                {
                    // Move the preview with the mouse position
                    unitPreview.transform.position = tile.transform.position;
                    unitPreview.SetActive(true);  // Ensure the preview stays visible
                }
            }
            else if (unitPreview != null)
            {
                // Hide the preview when hovering over invalid or occupied tiles
                unitPreview.SetActive(false);
            }

            // Place unit when clicked
            if (Input.GetMouseButtonDown(0) && tile.GetComponent<SpawningTile>() != null)
            {
                SpawnUnitOnTile(tile);
            }
        }
        else
        {
            // If the mouse is not over a tile, hide the preview
            if (unitPreview != null)
            {
                unitPreview.SetActive(false);
            }
        }
    }

    private void SpawnUnitOnTile(OverlayTile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Tile is null in SpawnUnitOnTile.");
            return;
        }

        SpawningTile spawningTile = tile.GetComponent<SpawningTile>();
        if (spawningTile == null)
        {
            Debug.LogError("Tile does not have a SpawningTile component.");
            return;
        }

        // Ensure we have an available unit to spawn
        if (playerAvailableUnits.Count == 0)
        {
            Debug.LogError("No available units to spawn.");
            return;
        }

        // Ensure the tile is not already occupied
        if (spawningTile.IsOccupied)
        {
            return;
        }

        // Select the first available unit
        Unit selectedUnit = playerAvailableUnits[0];

        // Instantiate the unit prefab
        Unit unitInstance = Instantiate(selectedUnit.gameObject).GetComponent<Unit>();
        if (unitInstance == null)
        {
            Debug.LogError("Failed to instantiate the unit.");
            return;
        }
        if (unitInstance.characterStats == null)
        {
            Debug.LogError($"CharacterStat is missing for unit: {unitInstance.name}");
        }
        // Ensure the SpriteRenderer exists and is not null
        SpriteRenderer spriteRenderer = unitInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Unit does not have a SpriteRenderer.");
            return;
        }

        // Position the unit on the spawning tile
        unitInstance.transform.position = tile.transform.position;
        spriteRenderer.sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;

        // Set the unit's standingOnTile field
        unitInstance.standingOnTile = tile;

        // If the MouseController is not null, assign the unit
        if (mouseController != null)
        {
            mouseController.SetUnit(unitInstance);
        }
        else
        {
            Debug.LogError("MouseController is not assigned.");
        }

        // Remove the unit from available units and add it to unplayable units
        playerAvailableUnits.Remove(selectedUnit);
        playedUnits.Add(selectedUnit);

        // Mark the tile as occupied
        spawningTile.IsOccupied = true;

        // If there's a unit preview, hide and destroy it
        if (unitPreview != null)
        {
            unitPreview.SetActive(false);
            Destroy(unitPreview);  // Destroy the preview object after placement
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
}
