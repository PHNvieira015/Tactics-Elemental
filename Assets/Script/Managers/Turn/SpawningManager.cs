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
    public List<Unit> unplayableUnits;  // List of units that have already been placed
    public MouseController mouseController;  // Reference to the MouseController
    public Color spawnTileColor = Color.blue;  // Color for spawn tiles
    private GameObject unitPreview;  // Preview of the unit being placed

    void Awake()
    {
        unplayableUnits = new List<Unit>(); // Initialize the list of unplayable units

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
        // Transition to GameRound state when the Start button is clicked
        Debug.Log("Start button clicked. Transitioning to GameRound.");
        GameMaster.instance.spawnedUnits = unplayableUnits;
        GameMaster.instance.UpdateGameState(GameMaster.GameState.GameRound);
        startButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();

            // Show valid spawn tiles
            if (tile.GetComponent<SpawningTile>() != null && !tile.GetComponent<SpawningTile>().IsOccupied)
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
        if (playerAvailableUnits.Count > 0 && tile.GetComponent<SpawningTile>() != null)
        {
            // Ensure that the tile isn't already occupied
            if (tile.GetComponent<SpawningTile>().IsOccupied)
            {
                Debug.Log("Cannot spawn unit here, tile is already occupied.");
                return;
            }

            // Choose the first available unit from the list
            Unit selectedUnit = playerAvailableUnits[0];
            Unit unitInstance = Instantiate(selectedUnit.gameObject).GetComponent<Unit>();

            // Position the unit on the spawning tile
            unitInstance.transform.position = tile.transform.position;
            unitInstance.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
            unitInstance.standingOnTile = tile;

            // Set the unit as selected in MouseController
            mouseController.SetUnit(unitInstance);

            // Remove the unit from available units and add to unplayable units
            playerAvailableUnits.Remove(selectedUnit);
            unplayableUnits.Add(selectedUnit);

            // Mark the tile as occupied
            tile.GetComponent<SpawningTile>().IsOccupied = true;

            // Hide and destroy the preview unit after placement
            if (unitPreview != null)
            {
                unitPreview.SetActive(false);
                Destroy(unitPreview);  // Destroy the preview object after placement
            }

            // Print lists for debugging
            Debug.Log("Available Units after spawn:");
            foreach (var unit in playerAvailableUnits)
            {
                Debug.Log(unit.characterStats.CharacterName);
            }

            Debug.Log("Unplayable Units after spawn:");
            foreach (var unit in unplayableUnits)
            {
                Debug.Log(unit.characterStats.CharacterName);
            }

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
}
