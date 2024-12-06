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
    public GameObject enemySpawner;
    public List<Unit> spawnedUnits = new List<Unit>();
    public Queue<Unit> turnQueue = new Queue<Unit>();
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
                if (spawnedUnits.Count > 0 && enemyList.Count > 0)
                {
                    Debug.Log("Starting a new game round...");
                    InitializeTurnOrder();
                    UpdateGameState(GameState.UnitTurn);
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

            for (int i = 0; i < spawnLimit; i++)
            {
                Unit enemyToSpawn = enemyList[i];
                Transform spawnTile = enemySpawningTiles[i];

                if (!spawnedUnits.Contains(enemyToSpawn))
                {
                    Unit spawnedUnit = Instantiate(enemyToSpawn, spawnTile.position, Quaternion.identity);
                    spawnedUnit.teamID = 2;
                    spawnedUnit.playerOwner = 2;
                    spawnedUnits.Add(spawnedUnit);

                    Debug.Log($"Enemy {spawnedUnit.characterStats.CharacterName} spawned at {spawnTile.position}.");
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

        Debug.Log($"Player Units: {playerUnits.Count}, Enemies: {enemies.Count}");

        spawnedUnits = playerUnits;  // Assign only player units to spawnedUnits
        enemyList = enemies;         // Assign only enemies to enemyList

        Debug.Log("Teams successfully set.");
    }

    public void InitializeTurnOrder()
    {
        var allUnits = spawnedUnits.Concat(enemyList).Where(unit => unit.isAlive).ToList();
        turnQueue = new Queue<Unit>(allUnits.OrderByDescending(unit => unit.characterStats.initiative));

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
            currentUnit = turnQueue.Dequeue();

            if (currentUnit.isAlive)
            {
                return;
            }

            Debug.Log($"{currentUnit.name} is dead. Continuing to the next unit.");
            break;
        }

        currentUnit = null;
    }

   public void HandleEndOfRound()
    {
        bool playersAlive = spawnedUnits.Any(unit => unit.isAlive);
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
            UpdateGameState(GameState.GameRound);
        }
        
    }
    private void CheckVictoryDefeat()
    {
        bool playersAlive = spawnedUnits.Any(unit => unit.isAlive);
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
