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
    public List<Unit> playedUnits;  // List of player units that have already been placed
    public List<Unit> enemyList;  // List of enemy units that have been placed
    public MouseController mouseController;  // Reference to the MouseController
    public Color spawnTileColor = Color.blue;  // Color for spawn tiles
    private GameObject unitPreview;  // Preview of the unit being placed
    [SerializeField] private GameObject EnemySpawnerTiles;
    [SerializeField] private GameObject PlayerSpawningTiles;

    void Awake()
    {
        playedUnits = new List<Unit>();

        if (gameMaster != null)
        {
            playerAvailableUnits = new List<Unit>(gameMaster.playerAvailableUnits);
        }
        else
        {
            Debug.LogError("GameMaster reference is missing!");
        }

        GameMaster.OnGameStateChanged += GameManagerOnGameStateChanged;

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.interactable = true;
        }
        else
        {
            Debug.LogError("Start Button is not assigned!");
        }
    }

    void OnDestroy()
    {
        GameMaster.OnGameStateChanged -= GameManagerOnGameStateChanged;

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    private void GameManagerOnGameStateChanged(GameMaster.GameState state)
    {
        SpawningSelection.SetActive(state == GameMaster.GameState.SpawningUnits);
    }

    private void OnStartButtonClicked()
    {
        if (GameMaster.instance == null)
        {
            Debug.LogError("GameMaster instance is not available.");
            return;
        }

        if (playedUnits.Count == 0)
        {
            Debug.LogError("No units to spawn, spawn units to continue.");
            return;
        }

        // Assign player units to GameMaster
        GameMaster.instance.playerList = new List<Unit>(playedUnits);
        Debug.Log("Player units assigned to GameMaster.");

        // Assign enemy units to GameMaster if any are present
        if (enemyList != null && enemyList.Count > 0)
        {
            GameMaster.instance.enemyList = new List<Unit>(enemyList);
            Debug.Log("Enemy units assigned to GameMaster.");
        }

        GameMaster.instance.UpdateGameState(GameMaster.GameState.GameRound);

        startButton.gameObject.SetActive(false);
        //place tile
        Destroy(EnemySpawnerTiles.gameObject);
        Destroy(PlayerSpawningTiles.gameObject);

        // Update tile occupancy for player units:
        foreach (Unit unit in playedUnits)
        {
            if (unit.standingOnTile != null)
            {
                // Block the tile and assign the activeCharacter reference.
                unit.standingOnTile.isBlocked = true;
                unit.standingOnTile.activeCharacter = unit;
                unit.standingOnTile.unitOnTile = unit; // Set the unitOnTile during spawn

                // Also set the tile beneath (if exists)
                Collider2D[] colliders = Physics2D.OverlapPointAll(unit.standingOnTile.transform.position);
                foreach (var col in colliders)
                {
                    OverlayTile underlyingTile = col.GetComponent<OverlayTile>();
                    if (underlyingTile != null && underlyingTile != unit.standingOnTile)
                    {
                        underlyingTile.isBlocked = true;
                        underlyingTile.unitOnTile = unit;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Unit {unit.name} does not have a standingOnTile assigned!");
            }
        }

        // Update tile occupancy for enemy units:
        // Update tile occupancy for enemy units:
        if (enemyList != null)
        {
            foreach (Unit enemy in enemyList)
            {
                if (enemy.standingOnTile != null)
                {
                    // Assign the first tile
                    enemy.standingOnTile.SetUnit(enemy); // Ensure it calls SetUnit

                    // Raycast downward to find the second tile (if applicable)
                    RaycastHit2D hit = Physics2D.Raycast(enemy.standingOnTile.transform.position, Vector2.down, 0.5f);
                    if (hit.collider != null)
                    {
                        OverlayTile secondTile = hit.collider.GetComponent<OverlayTile>();
                        if (secondTile != null)
                        {
                            secondTile.SetUnit(enemy);
                            Debug.Log($"Second tile assigned for enemy {enemy.name} at {secondTile.transform.position}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No second tile found below enemy {enemy.name} at position {enemy.standingOnTile.transform.position}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Enemy {enemy.name} does not have a standingOnTile assigned!");
                }
            }
        }
    }




    private void Update()
    {
        RaycastHit2D? hit = GetFocusedOnTile();

        if (hit.HasValue)
        {
            OverlayTile tile = hit.Value.collider.gameObject.GetComponent<OverlayTile>();

            #region Spawning Logic for Mouse
            if (tile != null && tile.GetComponent<SpawningTile>() != null && !tile.GetComponent<SpawningTile>().IsOccupied)
            {
                tile.ShowTile(spawnTileColor, TileType.Spawn);

                if (unitPreview == null && playerAvailableUnits.Count > 0)
                {
                    Unit selectedUnit = playerAvailableUnits[0];
                    unitPreview = Instantiate(selectedUnit.gameObject);
                    unitPreview.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
                    unitPreview.transform.position = tile.transform.position;
                    unitPreview.SetActive(true);
                }
                else if (unitPreview != null)
                {
                    unitPreview.transform.position = tile.transform.position;
                    unitPreview.SetActive(true);
                }
            }
            else if (unitPreview != null)
            {
                unitPreview.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0) && tile.GetComponent<SpawningTile>() != null)
            {
                SpawnUnitOnTile(tile);
            }
            #endregion
        }
        else if (unitPreview != null)
        {
            unitPreview.SetActive(false);
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

        if (playerAvailableUnits.Count == 0)
        {
            Debug.LogError("No available units to spawn.");
            return;
        }

        if (spawningTile.IsOccupied)
        {
            return;
        }

        Unit selectedUnit = playerAvailableUnits[0];

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

        SpriteRenderer spriteRenderer = unitInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Unit does not have a SpriteRenderer.");
            return;
        }

        unitInstance.transform.position = tile.transform.position;
        spriteRenderer.sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        unitInstance.standingOnTile = tile;
        tile.unitOnTile = unitInstance;  // This should update unitOnTile properly

        if (mouseController != null)
        {
            mouseController.SetUnit(unitInstance);
        }
        else
        {
            Debug.LogError("MouseController is not assigned.");
        }

        playerAvailableUnits.Remove(selectedUnit);
        playedUnits.Add(unitInstance);

        spawningTile.IsOccupied = true;

        if (unitPreview != null)
        {
            unitPreview.SetActive(false);
            Destroy(unitPreview);
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
