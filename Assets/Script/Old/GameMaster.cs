using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static TurnStateManager;

public class GameMaster : MonoBehaviour
{
    // Singleton instance
    public static GameMaster instance;

    // Teams and unit references
    public List<Unit> playerAvailableUnits = new List<Unit>();
    public List<Unit> enemyList = new List<Unit>();
    public List<Unit> playerList = new List<Unit>();
    public List<Unit> spawnedUnits = new List<Unit>();
    public Queue<Unit> turnQueue = new Queue<Unit>();
    public GameObject enemySpawner;
    public Unit currentUnit;
    public TurnStateManager turnStateManager;

    public static event Action<GameState> OnGameStateChanged;

    public enum GameState
    {
        SpawningUnits,
        GameRound,
        UnitTurn,
        Victory,
        Defeat
    }

    public GameState State;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateGameState(GameState.SpawningUnits);
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;
        Debug.Log($"Game state changed to: {newState}");

        switch (newState)
        {
            case GameState.SpawningUnits:
                SpawnEnemies();
                break;

            case GameState.GameRound:
                Debug.Log("unidades na lista: "+ spawnedUnits.Count);

                if (spawnedUnits.Count > 0 && enemyList.Count > 0)
                {
                    Debug.Log($"GameMaster active: {gameObject.activeSelf} during1...");

                    // Initialize the turn order once the teams are set
                    InitializeTurnOrder();
                    Debug.Log($"GameMaster active: {gameObject.activeSelf} during2...");


                    if (turnQueue.Count > 0)
                    {
                        currentUnit = turnQueue.Dequeue();  // Start with the first unit in the queue
                        UpdateGameState(GameState.UnitTurn);  // Proceed to the unit turn
                    }
                    else
                    {
                        Debug.LogError("TurnQueue is empty. Cannot start the game round.");
                    }
                }
                else
                {
                    Debug.LogError("Teams not initialized. Cannot start the game round.");
                }
                break;

            case GameState.UnitTurn:
                ProcessUnitTurn();
                break;

            case GameState.Victory:
                Debug.Log("Victory! The game is over.");
                break;

            case GameState.Defeat:
                Debug.Log("Defeat! Better luck next time.");
                break;

            default:
                Debug.LogWarning($"Unhandled game state: {newState}");
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    private void SpawnEnemies()
    {
        if (enemyList.Count > 0 && enemySpawner != null)
        {
            var enemySpawningTiles = enemySpawner.GetComponentsInChildren<Transform>().Skip(1).ToList(); // Skip the parent
            int spawnLimit = Mathf.Min(enemyList.Count, enemySpawningTiles.Count);

            // Create a list to track occupied tiles (to avoid overlap)
            List<Transform> availableTiles = new List<Transform>(enemySpawningTiles);

            for (int i = 0; i < spawnLimit; i++)
            {
                Unit enemyToSpawn = enemyList[i];

                // Check if there are any available tiles left
                if (availableTiles.Count == 0)
                {
                    Debug.LogError("No available tiles left for spawning.");
                    return;
                }

                // Randomly pick a tile from the available ones
                int randomTileIndex = UnityEngine.Random.Range(0, availableTiles.Count);
                Transform spawnTile = availableTiles[randomTileIndex];

                // Remove the used tile from the list of available tiles
                availableTiles.RemoveAt(randomTileIndex);

                if (!spawnedUnits.Contains(enemyToSpawn))
                {
                    // Spawn the unit on the selected tile
                    Unit spawnedUnit = Instantiate(enemyToSpawn, spawnTile.position, Quaternion.identity);
                    spawnedUnit.teamID = 2;
                    spawnedUnit.playerOwner = 2;
                    spawnedUnits.Add(spawnedUnit);

                    // Optionally, mark the tile as occupied if needed
                    // You can use a custom component to track tile occupation here if needed
                }
            }
        }
        else
        {
            Debug.LogError("No enemies or spawner available.");
        }

        SetTeams();
    }

    private void SetTeams()
    {
        var playerUnits = spawnedUnits.Where(unit => unit.teamID == 1).ToList();
        var enemies = spawnedUnits.Where(unit => unit.teamID == 2).ToList();

        // Keep both player and enemy units in spawnedUnits
        spawnedUnits = playerUnits.Concat(enemies).ToList();  // Ensure both teams are in the spawnedUnits list
        playerList = playerUnits;  // Store player units
        enemyList = enemies;  // Store enemies separately

        Debug.Log("Teams successfully set.");
    }

    public void InitializeTurnOrder()
    {
        Debug.Log($"Player count: {playerList.Count}, enemyList count: {enemyList.Count}");

        // Get all alive units
        var allUnits = new List<Unit>(playerList);  // Create a new list from playerList
        allUnits.AddRange(enemyList);  // Add all elements from enemyList to allUnits
        allUnits = allUnits.Where(unit => unit.isAlive).ToList();  // Optionally, filter out dead

        // Log each unit being added to the turn queue
        Debug.Log("Initializing turn order with the following units:");
        foreach (var unit in allUnits)
        {
            Debug.Log($"{unit.name} (Initiative: {unit.characterStats.initiative})");
        }

        // Order the units by initiative
        turnQueue = new Queue<Unit>(allUnits.OrderByDescending(unit => unit.characterStats.initiative));

        // Log turn order
        Debug.Log("Turn order initialized:");
        foreach (var unit in turnQueue)
        {
            Debug.Log($"{unit.name} (Initiative: {unit.characterStats.initiative})");
        }
    }

    private void ProcessUnitTurn()
    {
        if (currentUnit != null && !currentUnit.isAlive)
        {
            Debug.Log($"{currentUnit.name} is dead. Skipping turn.");
            currentUnit = null;
        }

        if (currentUnit == null)
        {
            FindNextLivingUnit();

            if (currentUnit != null)
            {
                currentUnit.Select();
                Debug.Log($"It's {currentUnit.name}'s turn!");
                // Start the unit's turn using the TurnStateManager
                turnStateManager.SetCurrentUnit(currentUnit);
                turnStateManager.ChangeState(TurnStateManager.TurnState.TurnStart);
            }
        }
    }

    private void FindNextLivingUnit()
    {
        while (turnQueue.Count > 0)
        {
            currentUnit = turnQueue.Peek();  // Get the first element without removing it.
            turnQueue.Dequeue();
            if (currentUnit.isAlive)
            {
                return;  // We found a living unit, exit the loop.
            }

            Debug.Log($"{currentUnit.name} is dead. Continuing to the next unit.");
        }

        // If no living units are left, set currentUnit to null (this will end the game or loop back).
        currentUnit = null;
    }

    public void HandleEndOfRound()
    {
        bool playersAlive = playerList.Any(unit => unit.isAlive);
        bool enemiesAlive = enemyList.Any(unit => unit.isAlive);

        if (playersAlive && !enemiesAlive)
        {
            UpdateGameState(GameState.Victory);
        }
        else if (enemiesAlive && !playersAlive)
        {
            UpdateGameState(GameState.Defeat);
        }
        else
        {
            UpdateGameState(GameState.GameRound);  // Continue with the next round
        }
    }

    private void OnEnable()
    {
        // Ensure we have a reference to the TurnStateManager from the child GameObject
        if (turnStateManager == null)
        {
            turnStateManager = GetComponentInChildren<TurnStateManager>();  // Automatically gets the TurnStateManager from children
        }

        if (turnStateManager != null)
        {
            turnStateManager.OnTurnStateChanged += HandleUnitTurnState;
        }
        else
        {
            Debug.LogError("TurnStateManager reference is missing. Make sure it's a child of the GameManager!");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the event
        if (turnStateManager != null)
        {
            turnStateManager.OnTurnStateChanged -= HandleUnitTurnState;
        }
    }

    private void HandleUnitTurnState(TurnStateManager.TurnState newState)
    {
        if (newState == TurnStateManager.TurnState.EndTurn)
        {
            Debug.Log($"Turn ended for {currentUnit.name}. Moving to the next unit.");

            // After this unit's turn ends, move to the next unit in the queue
            ProcessUnitTurn();  // This will either find the next living unit or handle the end of the round.
        }
    }
}
