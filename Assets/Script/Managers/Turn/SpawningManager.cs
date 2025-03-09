using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
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
    [SerializeField] private UnitManager unitManager;
    private List<OverlayTile> allSpawningTiles = new List<OverlayTile>();
    private bool spawningPhaseActive;

    void Start()
    {
        playedUnits = new List<Unit>();
        enemyList = new List<Unit>(GameMaster.instance.spawnedUnits);
        InitializeSpawningTiles();

        if (gameMaster != null)
        {
            playerAvailableUnits = new List<Unit>(gameMaster.playerAvailableUnits);

            if (enemyList == null)
            {
                enemyList = new List<Unit>();
            }
            else
            {
                enemyList = new List<Unit>(enemyList);
            }
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
            Debug.Log("No units to spawn, spawn units to continue.");
            print("No units to spawn, spawn units to continue.");
            return;
        }

        // Assign player units to GameMaster
        GameMaster.instance.playerList = new List<Unit>(playedUnits);
        enemyList = new List<Unit>(GameMaster.instance.spawnedUnits);

        // Transfer enemy units to main grid tiles BEFORE destroying spawning tiles
        foreach (Unit enemy in enemyList)
        {
            // Find the main grid tile under the enemy's position
            RaycastHit2D[] hits = Physics2D.RaycastAll(enemy.transform.position, Vector2.zero);
            OverlayTile mainGridTile = hits
                .OrderByDescending(hit => hit.collider.transform.position.z) // Sort by Z-axis
                .Select(hit => hit.collider.GetComponent<OverlayTile>())
                .FirstOrDefault(tile => tile != null && tile.tileData != null && tile.tileData.type != TileTypes.Spawner);

            if (mainGridTile != null)
            {
                enemy.standingOnTile = mainGridTile;
                mainGridTile.isBlocked = true;
                mainGridTile.unitOnTile = enemy;
            }
        }

        GameMaster.instance.UpdateGameState(GameMaster.GameState.GameRound);

        startButton.gameObject.SetActive(false);
        Destroy(PlayerSpawningTiles.gameObject);
        Destroy(EnemySpawnerTiles.gameObject);
        unitManager.SetTurnOrderList();

        // Set the standingOnTile for all units
        foreach (Unit unit in enemyList)
        {
            SetStandingOnTile(unit);
        }

        foreach (Unit unit in playedUnits)
        {
            SetStandingOnTile(unit);
        }
    }

    private void SetStandingOnTile(Unit unit)
    {
        // Find the topmost tile under the unit's position
        RaycastHit2D[] hits = Physics2D.RaycastAll(unit.transform.position, Vector2.zero);
        OverlayTile topTile = hits
            .OrderByDescending(hit => hit.collider.transform.position.z) // Sort by Z-axis
            .Select(hit => hit.collider.GetComponent<OverlayTile>())
            .FirstOrDefault(tile => tile != null);

        if (topTile != null)
        {
            unit.standingOnTile = topTile;
            topTile.isBlocked = true;
            topTile.unitOnTile = unit;
        }
        else
        {
            Debug.LogWarning($"Unit {unit.name} does not have a standingOnTile assigned!");
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
                // Only highlight the topmost tile
                if (IsTopTile(tile))
                {
                    tile.ShowTile(Color.blue, TileType.Spawn); // Brighten hovered tile

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
            }
            else if (unitPreview != null)
            {
                unitPreview.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0) && tile.GetComponent<SpawningTile>() != null && IsTopTile(tile))
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

    private bool IsTopTile(OverlayTile tile)
    {
        Vector2 tilePosition = tile.transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(tilePosition, Vector2.zero);

        if (hits.Length > 0)
        {
            // Find the topmost tile
            OverlayTile topTile = hits
                .OrderByDescending(hit => hit.collider.transform.position.z) // Sort by Z-axis
                .Select(hit => hit.collider.GetComponent<OverlayTile>())
                .FirstOrDefault(t => t != null);

            return topTile == tile;
        }

        return false;
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
    private void InitializeSpawningTiles()
    {
        allSpawningTiles.Clear();

        // Get player spawning tiles
        foreach (Transform child in PlayerSpawningTiles.transform)
        {
            var tile = child.GetComponent<OverlayTile>();
            if (tile != null) allSpawningTiles.Add(tile);
        }

        // Get enemy spawning tiles
        foreach (Transform child in EnemySpawnerTiles.transform)
        {
            var tile = child.GetComponent<OverlayTile>();
            if (tile != null) allSpawningTiles.Add(tile);
        }
    }

}