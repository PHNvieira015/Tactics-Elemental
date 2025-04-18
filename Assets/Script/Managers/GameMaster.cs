using System;
using System.Collections;
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
    public SpawningManager spawningManager;
    public UnitManager unitManager;
    public GameObject victoryCanvas;
    public GameObject defeatCanvas;

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
        //Debug.Log($"Game state changed to: {newState}");

        switch (newState)
        {
            case GameState.SpawningUnits:
                SpawnEnemies();
                break;

            case GameState.GameRound:
                Debug.Log("Preparing teams and initializing turn queue...");
                SetTeams();
                if (playerList.Count > 0 && enemyList.Count > 0)
                {
                    InitializeTurnOrder();
                    UpdateGameState(GameState.UnitTurn);
                }
                else
                {
                    Debug.LogError("Failed to initialize turn order. Player or enemy list is empty.");
                }
                break;

            case GameState.UnitTurn:
                if (turnStateManager != null && currentUnit != null)
                {
                    turnStateManager.SetCurrentUnit(currentUnit);
                    turnStateManager.ChangeState(TurnStateManager.TurnState.TurnStart);
                    currentUnit = turnStateManager.currentUnit;
                }
                else
                {
                    Debug.LogError("TurnStateManager or currentUnit is null, unable to start turn.");
                }
                break;

            case GameState.Victory:
                Debug.Log("Victory! You have defeated all enemies.");
                // Handle victory logic here (e.g., show victory screen)
                victoryCanvas.SetActive(true);
                StartCoroutine(WaitAndLoadNextScene(10f));
                StartNextScene();

                break;

            case GameState.Defeat:
                Debug.Log("Defeat! All your units have been defeated.");
                // Handle defeat logic here (e.g., show defeat screen)
                defeatCanvas.SetActive(true);
                StartCoroutine(WaitAndReloadScene(15f));
                SceneLoader.Instance.ReloadScene();
                break;

            default:
                Debug.LogWarning($"Unhandled game state: {newState}");
                break;
        }
    }

    private void SpawnEnemies()
    {
        if (enemyList.Count > 0 && enemySpawner != null)
        {
            List<Transform> enemySpawningTiles = enemySpawner.GetComponentsInChildren<Transform>().Skip(1).ToList();

            int spawnLimit = Mathf.Min(enemyList.Count, enemySpawningTiles.Count);

            for (int i = 0; i < spawnLimit; i++)
            {
                Unit enemyToSpawn = enemyList[i];
                Transform spawnPoint = enemySpawningTiles[i];

                Unit spawnedUnit = Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);
                spawnedUnit.teamID = 2;
                spawnedUnit.playerOwner = 2;

                // Raycast to find the tile under the spawn position
                RaycastHit2D[] hits = Physics2D.RaycastAll(spawnPoint.position, Vector2.zero);

                // Find the tile with the highest Z-axis value
                OverlayTile tileUnderEnemy = hits
                    .OrderByDescending(hit => hit.collider.transform.position.z)
                    .Select(hit => hit.collider.GetComponent<OverlayTile>())
                    .FirstOrDefault(tile => tile != null);

                if (tileUnderEnemy != null)
                {
                    spawnedUnit.standingOnTile = tileUnderEnemy;
                    tileUnderEnemy.SetUnit(spawnedUnit);
                    tileUnderEnemy.isBlocked = true;
                }
                else
                {
                    Debug.LogWarning($"No tile found under enemy spawn point at position {spawnPoint.position}");
                }

                if (tileUnderEnemy != null)
                {
                    SpriteRenderer spriteRenderer = spawnedUnit.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = tileUnderEnemy.GetComponent<SpriteRenderer>().sortingOrder;
                    }
                }

                spawnedUnits.Add(spawnedUnit);
                enemySpawningTiles.RemoveAt(i);
            }
        }
        else
        {
            Debug.LogError("No enemies to spawn or no spawner available.");
        }
    }

    private void SetTeams()
    {
        if (spawningManager == null)
        {
            Debug.LogError("SpawningManager is null. Cannot set teams.");
            return;
        }

        playerList = spawningManager.playedUnits;
        //Debug.Log("Player list is: " + spawningManager.playedUnits);
        enemyList = spawnedUnits.Where(unit => unit.teamID == 2).ToList();

        //Debug.Log($"Player units: {playerList.Count}, Enemy units: {enemyList.Count}");
    }

    public void InitializeTurnOrder()
    {
        //Debug.Log($"Player count: {playerList.Count}, enemyList count: {enemyList.Count}");

        // Combine all units into a single list
        //unitManager.turnOrderList.RemoveAll(unit => unit == null);
        var allUnits = new List<Unit>(playerList);
        allUnits.AddRange(enemyList);

        // Ensure no null elements are in the lists before proceeding
        allUnits.RemoveAll(unit => unit == null);
        playerList.RemoveAll(unit => unit == null);
        playerAvailableUnits.RemoveAll(unit => unit == null);
        playerList.RemoveAll(unit => unit == null);
        spawnedUnits.RemoveAll(unit => unit == null);

        //Debug.Log($"Total units before sorting and filtering: {allUnits.Count}");

        if (allUnits.Count == 0)
        {
            Debug.LogError("No units available to initialize turn order.");
            return;
        }

        // Ensure that every unit has a valid characterStats
        foreach (var unit in allUnits)
        {
            if (unit == null)
            {
                Debug.LogError("Unit is null!");
            }
            else if (unit.characterStats == null)
            {
                Debug.LogError($"Unit '{unit.name}' has no characterStats assigned!");
            }
            else
            {
                //Debug.Log($"Unit: {unit.name}, Initiative: {unit.characterStats.initiative}");
            }
        }

        // Initialize the turn queue, ordering by initiative (highest first)
        try
        {
            turnQueue = new Queue<Unit>(allUnits.OrderByDescending(unit => unit.characterStats.initiative));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred while sorting units: {ex}");
        }

        // Log the turn order after sorting
        //Debug.Log("Turn order initialized:");
        foreach (var unit in turnQueue)
        {
            //Debug.Log($"Unit in queue: {unit.name} with initiative {unit.characterStats.initiative}");
        }

        // Set the first unit as currentUnit
        if (turnQueue.Count > 0)
        {
            currentUnit = turnQueue.Dequeue(); // Dequeue the first unit
            //Debug.Log($"First unit in turn queue: {currentUnit.name}");
            // Proceed to the next game state
            UpdateGameState(GameState.UnitTurn);
        }
        else
        {
            Debug.LogError("Turn queue is empty after attempting to initialize.");
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

            if (currentUnit == null)
            {
                if (turnQueue.Count > 0)
                {
                    currentUnit = turnQueue.Dequeue(); // Dequeue the next unit
                    currentUnit.Select();
                    Debug.Log($"It's {currentUnit.name}'s turn!");
                    turnStateManager.SetCurrentUnit(currentUnit);
                    turnStateManager.ChangeState(TurnStateManager.TurnState.TurnStart);
                }
                else
                {
                    Debug.LogError("Turn queue is empty, no more units to process.");
                    HandleEndOfRound();
                }
            }
        }
    }

    private void EndUnitTurn()
    {
        // Log the turn queue before the current unit finishes their turn
        LogTurnQueueState("Before turn ends:");

        // After a unit finishes its turn, remove them from the queue
        if (currentUnit != null)
        {
            Debug.Log($"{currentUnit.name} has finished their turn. Removing from the queue.");

            // Log the current state of the queue after the turn
            LogTurnQueueState("After turn ends:");

            // Now we want to get the next unit in the queue
            if (turnQueue.Count > 0)
            {
                currentUnit = turnQueue.Dequeue();  // Get the next unit from the queue
                Debug.Log($"Next unit: {currentUnit.name} with initiative {currentUnit.characterStats.initiative}");
                // Update the game state or continue the game logic
                turnStateManager.SetCurrentUnit(currentUnit);
                turnStateManager.ChangeState(TurnStateManager.TurnState.TurnStart);
            }
            else
            {
                Debug.LogError("Turn queue is empty, no more units to process.");
                HandleEndOfRound();  // Check for victory or defeat conditions
            }
        }
    }

    private void LogTurnQueueState(string message)
    {
        Debug.Log(message);
        foreach (var unit in turnQueue)
        {
            Debug.Log($"Unit in queue: {unit.name} with initiative {unit.characterStats.initiative}");
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

        //Debug.Log($"Players alive: {playersAlive}, Enemies alive: {enemiesAlive}");

        if (playersAlive && !enemiesAlive)
        {
            Debug.Log("Victory!");
            UpdateGameState(GameState.Victory);
        }
        else if (enemiesAlive && !playersAlive)
        {
            Debug.Log("Defeat!");
            UpdateGameState(GameState.Defeat);
        }
        else
        {
            //Debug.Log("Continuing to the next round.");
            UpdateGameState(GameState.GameRound);  // Continue with the next round
        }
    }

    private void OnEnable()
    {
        if (turnStateManager == null)
        {
            turnStateManager = GetComponentInChildren<TurnStateManager>();
        }

        if (turnStateManager != null)
        {
            turnStateManager.OnTurnStateChanged += HandleUnitTurnState;
        }
        else
        {
            Debug.LogError("TurnStateManager reference is missing. Make sure it's a child of the GameManager!");
        }

        // Dynamically find the SpawningManager instance
        if (spawningManager == null)
        {
            spawningManager = FindObjectOfType<SpawningManager>();
            if (spawningManager == null)
            {
                Debug.LogError("SpawningManager could not be found in the scene. Ensure it exists.");
            }
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
            //Debug.Log($"Turn ended for {currentUnit.name}. Moving to the next unit.");

            // After this unit's turn ends, move to the next unit in the queue
            ProcessUnitTurn();  // This will either find the next living unit or handle the end of the round.
        }
    }
    public static void RemoveUnit(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError("Unit is null. Cannot remove from lists.");
            return;
        }

        // Log before removing
        Debug.Log($"Removing unit: {unit.name}");

        // Remove from the turn queue (if present)
        if (instance.turnQueue.Contains(unit))
        {
            // We need to create a new queue without the removed unit
            List<Unit> queueList = new List<Unit>(instance.turnQueue);
            queueList.Remove(unit);
            instance.turnQueue = new Queue<Unit>(queueList); // Reassign the updated queue
        }
        else
        {
            Debug.LogWarning($"Unit {unit.name} is not in the turn order queue.");
        }

        // Remove the unit from all lists
        RemoveUnitFromList(instance.playerList, unit);
        RemoveUnitFromList(instance.enemyList, unit);
        RemoveUnitFromList(instance.spawnedUnits, unit);

        // Clean up any null references that may remain after removal
        CleanUpNullEntries(instance.playerList);
        CleanUpNullEntries(instance.enemyList);
        CleanUpNullEntries(instance.spawnedUnits);

        // Clean up the turn queue to ensure no null entries are left
        CleanUpNullEntries(instance.turnQueue.ToList());

        // Log after removal
        Debug.Log($"{unit.name} has been successfully removed from all lists.");

        // Check for victory or defeat conditions after removing the unit
        instance.HandleEndOfRound();
    }

    private static void RemoveUnitFromList(List<Unit> unitList, Unit unit)
    {
        if (unitList.Contains(unit))
        {
            unitList.Remove(unit);
        }
    }

    private static void CleanUpNullEntries(List<Unit> unitList)
    {
        // Remove all null elements from the list
        unitList.RemoveAll(unit => unit == null);
    }
    void StartNextScene()
    {
        SceneLoader.Instance.LoadNextScene();
    }
    private IEnumerator WaitAndLoadNextScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextScene();
    }

    private IEnumerator WaitAndReloadScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneLoader.Instance.ReloadScene();
    }
}
