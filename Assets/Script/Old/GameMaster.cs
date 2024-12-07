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
            // Get the list of tiles from the spawner (excluding the parent object)
            List<Transform> enemySpawningTiles = enemySpawner.GetComponentsInChildren<Transform>().Skip(1).ToList();

            // Debug: Log initial setup
            Debug.Log($"Tiles available: {enemySpawningTiles.Count}, Enemies to spawn: {enemyList.Count}");

            int spawnLimit = Mathf.Min(enemyList.Count, enemySpawningTiles.Count);

            for (int i = 0; i < spawnLimit; i++)
            {
                // Get the current enemy and corresponding tile
                Unit enemyToSpawn = enemyList[i];
                Transform selectedTile = enemySpawningTiles[i];

                // Debug: Log the assignment
                Debug.Log($"Assigning {enemyToSpawn.name} to tile {selectedTile.name} at position {selectedTile.position}");

                // Instantiate the enemy unit at the tile's position
                Unit spawnedUnit = Instantiate(enemyToSpawn, selectedTile.position, Quaternion.identity);
                spawnedUnit.teamID = 2; // Set enemy team ID
                spawnedUnit.playerOwner = 2; // Set enemy player owner ID
                spawnedUnits.Add(spawnedUnit);

                // Debug: Log the spawned unit
                Debug.Log($"Spawned {spawnedUnit.name} at position {spawnedUnit.transform.position}");

                // Remove the used tile from the list to prevent reuse
                enemySpawningTiles.RemoveAt(i);

                // Debug: Log the remaining tiles after each spawn
                Debug.Log($"Remaining tiles after spawn: {enemySpawningTiles.Count}");
            }
        }
        else
        {
            Debug.LogError("No enemies to spawn or no spawner available.");
        }

        // Assign the spawned units to their respective teams
        SetTeams();

        // Debug: Log all spawned units
        Debug.Log("Spawned Units:");
        foreach (var unit in spawnedUnits)
        {
            Debug.Log($"{unit.name} at position {unit.transform.position}");
        }
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
