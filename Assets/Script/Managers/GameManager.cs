using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
//    // Singleton reference for GameManager
//    public static GameManager instance;

//    // Event for game state changes
//    public static event Action<GameState> OnGameStateChanged;

//    // The current game state
//    public GameState State;

//    // Teams for the game (player's team and enemy team)
//    [SerializeField] public List<Unit> teamA = new List<Unit>();  // List of team A's units (Player's team)
//    [SerializeField] public List<Unit> teamB = new List<Unit>();  // List of team B's units (Enemy team)

//    // Queue to handle the turn order of units
//    [SerializeField] public Queue<Unit> turnQueue = new Queue<Unit>();

//    // The unit whose turn it currently is
//    [SerializeField] public Unit currentUnit;

//    // Enum to define different game states
//    public enum GameState
//    {
//        SpawningUnits,    // State when units are being spawned
//        GameRound,        // State when a game round is in progress
//        UnitTurn,         // State for each unit's individual turn
//        Victory,          // State when the victory condition is met
//        Defeat            // State when the defeat condition is met
//    }

//    private void Awake()
//    {
//        // Ensure there's only one GameManager instance (singleton pattern)
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);  // Destroy this object if there's already an instance
//        }
//    }

//    private void Start()
//    {
//        // Start with the SpawningUnits state and wait for units to be added
//        UpdateGameState(GameState.SpawningUnits);
//    }

//    // Method to update the game state and handle transitions
//    public void UpdateGameState(GameState newState)
//    {
//        State = newState;

//        // Print the current game state
//        Debug.Log($"Game state changed to: {newState}");

//        // Handle game state transitions
//        switch (newState)
//        {
//            case GameState.SpawningUnits:
//                Debug.Log("Spawning Units...");
//                // Here you would spawn or initialize the units. After units are spawned, set teams.
//                break;

//            case GameState.GameRound:
//                // Only proceed to GameRound if teams have been set
//                if (teamA.Count > 0 && teamB.Count > 0)
//                {
//                    Debug.Log("Starting a new game round...");
//                    InitializeTurnOrder();  // Initialize the turn order based on unit initiative
//                    UpdateGameState(GameState.UnitTurn);  // Move to UnitTurn state
//                }
//                else
//                {
//                    Debug.LogError("Teams have not been initialized properly! Cannot start game round.");
//                }
//                break;

//            case GameState.UnitTurn:
//                ProcessUnitTurn();
//                break;

//            case GameState.Victory:
//                Debug.Log("Victory condition met!");
//                break;

//            case GameState.Defeat:
//                Debug.Log("Defeat condition met!");
//                break;

//            default:
//                Debug.LogWarning($"Unhandled game state: {newState}");
//                break;
//        }

//        // Trigger the event for state change
//        OnGameStateChanged?.Invoke(newState);
//    }

//    // Method to process the current unit's turn
//    private void ProcessUnitTurn()
//    {
//        // Check if the current unit is dead, and skip it if necessary
//        if (currentUnit != null && !currentUnit.isAlive)
//        {
//            Debug.Log($"{currentUnit.name} is dead, skipping this turn.");
//            currentUnit = null;  // Set the current unit to null
//        }

//        // If there's no current unit or the current unit is dead, find the next living unit in the queue
//        if (currentUnit == null)
//        {
//            // Find the next living unit
//            FindNextLivingUnit();

//            if (currentUnit != null)
//            {
//                currentUnit.Select();
//                Debug.Log($"It's {currentUnit.name}'s turn!");
//            }
//            else
//            {
//                // If no units are alive, handle game over state
//                HandleEndOfRound();
//            }
//        }
//    }

//    // Method to find the next living unit in the turn queue
//    private void FindNextLivingUnit()
//    {
//        // Move through the turn queue until a living unit is found
//        if (turnQueue.Count > 0)
//        {
//            // Check the next unit in the queue
//            currentUnit = turnQueue.Dequeue();

//            // If the unit is dead, skip to the next one
//            if (!currentUnit.isAlive)
//            {
//                Debug.Log($"{currentUnit.name} is dead, skipping this unit.");
//                // Recursively try to find the next living unit
//                FindNextLivingUnit();
//            }
//        }
//    }

//    // Method to handle the end of the round and determine if the game is over
//    private void HandleEndOfRound()
//    {
//        // Check if any team has living units left
//        bool teamAAlive = teamA.Any(unit => unit.isAlive);
//        bool teamBAlive = teamB.Any(unit => unit.isAlive);

//        if (teamAAlive && !teamBAlive)
//        {
//            Debug.Log("Team A wins!");
//            UpdateGameState(GameState.Victory);
//        }
//        else if (teamBAlive && !teamAAlive)
//        {
//            Debug.Log("Team B wins!");
//            UpdateGameState(GameState.Victory);
//        }
//        else
//        {
//            Debug.Log("No one wins yet, proceeding to the next round...");
//            UpdateGameState(GameState.GameRound);
//        }
//    }

//    // Method to initialize the turn order based on unit initiative
//    public void InitializeTurnOrder()
//    {
//        if (teamA.Count == 0 && teamB.Count == 0)
//        {
//            Debug.LogError("No units found in teams! Turn order cannot be initialized.");
//            return;
//        }

//        // Combine the teams and filter only alive units
//        List<Unit> allUnits = new List<Unit>(teamA.Concat(teamB)).Where(unit => unit.isAlive).ToList();

//        // Sort the units by initiative (descending order) and enqueue them
//        var sortedUnits = allUnits.OrderByDescending(unit => unit.characterStats.initiative).ToList();
//        turnQueue = new Queue<Unit>(sortedUnits);

//        Debug.Log("Turn order initialized based on initiative.");
//        foreach (var unit in sortedUnits)
//        {
//            Debug.Log($"{unit.name} (Initiative: {unit.characterStats.initiative}) added to turn queue.");
//        }
//    }

//    // Method to set the teams for the game
//    public void SetTeams(List<Unit> teamAUnits)
//    {
//        teamA = GameMaster.instance.spawnedUnits;

//        teamB = GameMaster.instance.enemyList;  // Copy enemyList to teamB
//        Debug.Log("Team B set from GameMaster's enemy list.");


//        Debug.Log("Teams set successfully.");

//        // After teams are set, transition to the GameRound state
//        UpdateGameState(GameState.GameRound);
//    }

//    // Debugging method to check if teams are initialized
//    public void DebugTeamInitialization()
//    {
//        Debug.Log($"Team A has {teamA.Count} units.");
//        Debug.Log($"Team B has {teamB.Count} units.");
//    }
}
