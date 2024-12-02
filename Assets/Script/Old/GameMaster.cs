using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    // Singleton reference for GameMaster
    public static GameMaster instance;

    // List of available units and enemies
    public List<Unit> playerAvailableUnits = new List<Unit>();  // The list of units available to the player
    public List<Unit> enemyList = new List<Unit>();  // The list of enemies to spawn
    public GameObject enemySpawner;  // The EnemySpawner object (with child tiles)
    public List<Unit> spawnedUnits = new List<Unit>();  // List to track the spawned units


    // Queue to handle the turn order of units
    public Queue<Unit> turnQueue = new Queue<Unit>();

    // The unit whose turn it currently is
    public Unit currentUnit;

    // Event for game state changes
    public static event Action<GameState> OnGameStateChanged;

    // Enum for game states
    public enum GameState
    {
        SpawningUnits,  // State when units are being spawned
        GameRound,      // State when a game round is in progress
        UnitTurn,       // State for each unit's individual turn
        Victory,        // State when the victory condition is met
        Defeat          // State when the defeat condition is met
    }

    public GameState State;

    private void Awake()
    {
        // Ensure there's only one GameMaster instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);  // Destroy this object if there's already an instance
        }
    }

    private void Start()
    {
        // Start with the SpawningUnits state
        UpdateGameState(GameState.SpawningUnits);
    }

    // Method to update the game state and handle transitions
    public void UpdateGameState(GameState newState)
    {
        State = newState;

        // Print the current game state
        Debug.Log($"Game state changed to: {newState}");

        // Handle game state transitions
        switch (newState)
        {
            case GameState.SpawningUnits:
                Debug.Log("Spawning Units...");
                SpawnEnemies();
                break;

            case GameState.GameRound:
                // Only proceed to GameRound if teams have been set
                if (spawnedUnits.Count > 0 && enemyList.Count > 0)
                {
                    Debug.Log("Starting a new game round...");
                    InitializeTurnOrder();  // Initialize the turn order based on unit initiative
                    UpdateGameState(GameState.UnitTurn);  // Move to UnitTurn state
                }
                else
                {
                    Debug.LogError("Teams have not been initialized properly! Cannot start game round.");
                }
                break;

            case GameState.UnitTurn:
                //ProcessUnitTurn();
                break;

            case GameState.Victory:
                Debug.Log("Victory condition met!");
                break;

            case GameState.Defeat:
                Debug.Log("Defeat condition met!");
                break;

            default:
                Debug.LogWarning($"Unhandled game state: {newState}");
                break;
        }

        // Trigger the event for state change
        OnGameStateChanged?.Invoke(newState);
    }

    // Method to spawn the enemy units
    private void SpawnEnemies()
    {
        Debug.Log(enemyList.Count);
        if (enemyList.Count > 0 && enemySpawner != null)
        {
            // Gather all child tiles under the EnemySpawner
            List<Transform> enemySpawningTiles = new List<Transform>();
            foreach (Transform child in enemySpawner.transform)
            {
                enemySpawningTiles.Add(child);  // Add each child tile to the list
            }

            // Ensure we do not spawn more enemies than there are spawn tiles
            int spawnLimit = enemyList.Count;

            // Loop through the number of enemies to spawn based on available tiles
            for (int i = 0; i < spawnLimit; i++)
            {
                // Get the enemy unit to spawn
                Unit enemyToSpawn = enemyList[i];

                // Get the spawn tile to place the enemy on
                Transform spawnTile = enemySpawningTiles[i];

                // Check if this enemy has already been spawned
                if (!spawnedUnits.Contains(enemyToSpawn))
                {
                    // Instantiate the unit at the spawn tile's position
                    Unit spawnedUnit = Instantiate(enemyToSpawn, spawnTile.position, Quaternion.identity);

                    // Set the unit's teamID to 2 (team B)
                    spawnedUnit.teamID = 2;
                    spawnedUnit.playerOwner = 2;

                    // Add the spawned unit to the list
                    spawnedUnits.Add(spawnedUnit);

                    // Print the details of the newly spawned unit
                    Debug.Log($"Enemy {spawnedUnit.characterStats.CharacterName} has been spawned at position {spawnTile.position} with TeamID: {spawnedUnit.teamID}");
                }
                else
                {
                    Debug.Log($"Enemy {enemyToSpawn.characterStats.CharacterName} has already been spawned.");
                }
            }
        }
        else
        {
            Debug.Log("No enemies or enemy spawner available.");
        }

        // After spawning enemies, set teams
        SetTeams();
    }

    // Method to set teams for the game
   public void SetTeams()
{
    DebugTeamInitialization();
        
    // Assuming teamID = 1 for player units and teamID = 2 for enemy units
    List<Unit> playerUnits = spawnedUnits.Where(unit => unit.teamID == 1).ToList();  // Player units
    List<Unit> enemyUnits = spawnedUnits.Where(unit => unit.teamID == 2).ToList();   // Enemy units

        
    // Set the final teams
    spawnedUnits = playerUnits;  // Player units go into spawnedUnits (team A)
    enemyList = enemyUnits;      // Enemy units go into enemyList (team B)

    Debug.Log("Teams set successfully.");

    // Transition to the GameRound state
    UpdateGameState(GameState.GameRound);
}

    // Method to initialize the turn order based on unit initiative
    public void InitializeTurnOrder()
    {
        if (spawnedUnits.Count == 0 && enemyList.Count == 0)
        {
            Debug.LogError("No units found in teams! Turn order cannot be initialized.");
            return;
        }

        // Combine the teams and filter only alive units
        List<Unit> allUnits = new List<Unit>(spawnedUnits.Concat(enemyList)).Where(unit => unit.isAlive).ToList();

        // Sort the units by initiative (descending order) and enqueue them
        var sortedUnits = allUnits.OrderByDescending(unit => unit.characterStats.initiative).ToList();
        turnQueue = new Queue<Unit>(sortedUnits);

        Debug.Log("Turn order initialized based on initiative.");
        foreach (var unit in sortedUnits)
        {
            Debug.Log($"{unit.name} (Initiative: {unit.characterStats.initiative}) added to turn queue.");
        }
    }

    // Method to process the current unit's turn
    private void ProcessUnitTurn()
    {
        // Check if the current unit is dead, and skip it if necessary
        if (currentUnit != null && !currentUnit.isAlive)
        {
            Debug.Log($"{currentUnit.name} is dead, skipping this turn.");
            currentUnit = null;  // Set the current unit to null
        }

        // If there's no current unit or the current unit is dead, find the next living unit in the queue
        if (currentUnit == null)
        {
            // Find the next living unit
            FindNextLivingUnit();

            if (currentUnit != null)
            {
                currentUnit.Select();
                Debug.Log($"It's {currentUnit.name}'s turn!");
            }
            else
            {
                // If no units are alive, handle game over state
                HandleEndOfRound();
            }
        }
    }

    // Method to find the next living unit in the turn queue
    private void FindNextLivingUnit()
    {
        // Move through the turn queue until a living unit is found
        if (turnQueue.Count > 0)
        {
            // Check the next unit in the queue
            currentUnit = turnQueue.Dequeue();

            // If the unit is dead, skip to the next one
            if (!currentUnit.isAlive)
            {
                Debug.Log($"{currentUnit.name} is dead, skipping this unit.");
                // Recursively try to find the next living unit
                FindNextLivingUnit();
            }
        }
    }

    // Method to handle the end of the round and determine if the game is over
    private void HandleEndOfRound()
    {
        // Check if any team has living units left
        bool spawnedUnitsAlive = spawnedUnits.Any(unit => unit.isAlive);
        bool enemyListAlive = enemyList.Any(unit => unit.isAlive);

        if (spawnedUnitsAlive && !enemyListAlive)
        {
            Debug.Log("Team A wins!");
            UpdateGameState(GameState.Victory);
        }
        else if (enemyListAlive && !spawnedUnitsAlive)
        {
            Debug.Log("Team B wins!");
            UpdateGameState(GameState.Victory);
        }
        else
        {
            Debug.Log("No one wins yet, proceeding to the next round...");
            UpdateGameState(GameState.GameRound);
        }
    }

    // Debugging method to check if teams are initialized
    public void DebugTeamInitialization()
    {
        Debug.Log($"Team A has {spawnedUnits.Count} units.");
        Debug.Log($"Team B has {enemyList.Count} units.");
    }
}
