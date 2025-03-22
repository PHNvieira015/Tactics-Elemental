using static TurnStateManager;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    public Unit unit; // Reference to the Unit component
    public List<Unit> allEnemies; // List of all enemy units
    public List<Unit> allAllies; // List of all ally units
    private MouseController mouseController; // Reference to MouseController for movement and attack
    private TurnStateManager turnStateManager; // Reference to TurnStateManager

    private void Awake()
    {
        unit = GetComponent<Unit>();
        mouseController = FindObjectOfType<MouseController>(); // Get the MouseController instance
        turnStateManager = FindObjectOfType<TurnStateManager>(); // Get the TurnStateManager instance
    }

    public void StartTurn()
    {
        StartCoroutine(RunTurn());
    }

    private IEnumerator RunTurn()
    {
        while (true)
        {
            // Wait for 1 second before deciding action
            yield return new WaitForSeconds(0.5f);

            // Check for enemies in range and decide action
            CheckForEnemiesInRange();
            DecideAction();

            // If the AI has moved and attacked, or has no actions left, end the turn
            if ((unit.hasMoved && unit.hasAttacked) || (unit.hasMoved && unit.enemiesInRange.Count == 0))
            {
                EndTurn();
                yield break; // Exit the coroutine
            }
        }
    }

    public void DecideAction()
    {
        // Ensure this unit is the current unit
        if (turnStateManager.currentUnit != unit)
        {
            Debug.LogWarning($"{unit.name} is not the current unit. Skipping action.");
            return;
        }

        Debug.Log($"AI {unit.name} is deciding action. hasMoved: {unit.hasMoved}, hasAttacked: {unit.hasAttacked}");

        // Update enemies and allies
        FindAllEnemies();
        FindAllAllies();
        CheckForEnemiesInRange();

        if (unit.enemiesInRange.Count > 0 && !unit.hasAttacked)
        {
            // Attack the first enemy in range
            Debug.Log($"AI {unit.name} found an enemy in range, attacking...");
            AttackEnemy(unit.enemiesInRange[0]);
        }
        else if (!unit.hasMoved)
        {
            // Move toward the nearest enemy
            Debug.Log($"AI {unit.name} found no enemies in range, moving toward nearest enemy.");
            MoveTowardNearestEnemy();
        }
        else
        {
            // If no actions are possible, end the turn
            Debug.Log($"AI {unit.name} has no actions to perform, ending turn.");
            EndTurn();
        }
    }

    public void MoveTowardNearestEnemy()
    {
        if (unit.hasMoved)
        {
            Debug.LogWarning($"{unit.name} has already moved this turn!");
            return;
        }

        Unit nearestEnemy = null;
        int shortestDistance = int.MaxValue;

        // Find the nearest enemy using tile-based distance
        foreach (var enemy in allEnemies)
        {
            OverlayTile unitTile = unit.standingOnTile;
            OverlayTile enemyTile = enemy.standingOnTile;

            if (unitTile == null || enemyTile == null)
            {
                Debug.LogWarning("Unit or enemy tile is null!");
                continue;
            }

            // Calculate the Manhattan distance between tiles
            int distance = Mathf.Abs(unitTile.gridLocation.x - enemyTile.gridLocation.x) +
                           Mathf.Abs(unitTile.gridLocation.y - enemyTile.gridLocation.y);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy == null)
        {
            Debug.LogWarning($"{unit.name} found no enemies!");
            EndTurn(); // End turn if no enemies are found
            return;
        }

        // Set the AI unit as the current unit in MouseController
        mouseController.SetUnit(unit);

        // Get the movement range tiles
        List<OverlayTile> movementRangeTiles = mouseController.GetInRangeTiles();

        // Find the closest tile to the enemy within movement range
        OverlayTile closestTile = null;
        int closestDistance = int.MaxValue;

        foreach (var tile in movementRangeTiles)
        {
            OverlayTile enemyTile = nearestEnemy.standingOnTile;

            if (enemyTile == null)
            {
                Debug.LogWarning("Enemy tile is null!");
                continue;
            }

            // Calculate the Manhattan distance between the tile and the enemy
            int distance = Mathf.Abs(tile.gridLocation.x - enemyTile.gridLocation.x) +
                           Mathf.Abs(tile.gridLocation.y - enemyTile.gridLocation.y);

            if (distance < closestDistance && !tile.isBlocked)
            {
                closestDistance = distance;
                closestTile = tile;
            }
        }

        if (closestTile == null)
        {
            Debug.LogWarning($"No valid tile found for {unit.name} to move toward {nearestEnemy.name}!");
            EndTurn(); // End turn if no valid tile is found
            return;
        }

        // Move the unit to the closest tile
        StartCoroutine(MoveAndReevaluate(closestTile));
    }

    private IEnumerator MoveAndReevaluate(OverlayTile targetTile)
    {
        // Simulate player input by calling HandleMovement
        mouseController.HandleMovement(targetTile);

        // Wait for the movement to complete
        while (mouseController.isMoving)
        {
            yield return null;
        }

        // Mark the unit as having moved
        unit.hasMoved = true;
        Debug.Log($"{unit.name} has moved. hasMoved: {unit.hasMoved}");

        // Wait for 2 seconds before re-evaluating
        yield return new WaitForSeconds(2f);

        // Re-evaluate after moving
        DecideAction();
    }

    public void AttackEnemy(Unit enemy)
    {
        if (unit.hasAttacked)
        {
            Debug.LogWarning($"{unit.name} has already attacked this turn!");
            return;
        }

        // Simulate player input by calling HandleAttack
        mouseController.HandleAttack(enemy.standingOnTile);

        // Mark the unit as having attacked
        unit.hasAttacked = true;
        Debug.Log($"{unit.name} has attacked. hasAttacked: {unit.hasAttacked}");

        // Wait for 2 seconds before ending the turn
        StartCoroutine(WaitAndEndTurn());
    }

    private IEnumerator WaitAndEndTurn()
    {
        yield return new WaitForSeconds(2f);
        EndTurn();
    }

    private void EndTurn()
    {
        Debug.Log($"AI {unit.name} is ending its turn.");
        turnStateManager.ChangeState(TurnState.EndTurn);
    }
    public void FindAllEnemies()
    {
        Debug.Log("Finding all enemies");
        allEnemies = new List<Unit>();
        Unit[] allUnits = FindObjectsOfType<Unit>();

        foreach (var enemy in allUnits)
        {
            if (enemy.teamID != unit.teamID && enemy.IsAlive())
            {
                allEnemies.Add(enemy);
            }
        }
    }

    public void FindAllAllies()
    {
        Debug.Log("Finding all allies");
        allAllies = new List<Unit>();
        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (var ally in allUnits)
        {
            if (ally.teamID == unit.teamID && ally.IsAlive() && ally != unit)
            {
                allAllies.Add(ally);
            }
        }
    }

    public void CheckForEnemiesInRange()
    {
        Debug.Log("Checking for enemies in range using tile-based distance.");
        unit.enemiesInRange.Clear();

        foreach (var enemy in allEnemies)
        {
            // Get the tiles under the unit and the enemy
            OverlayTile unitTile = unit.standingOnTile;
            OverlayTile enemyTile = enemy.standingOnTile;

            if (unitTile == null || enemyTile == null)
            {
                Debug.LogWarning("Unit or enemy tile is null!");
                continue;
            }

            // Calculate the Manhattan distance between tiles
            int distance = Mathf.Abs(unitTile.gridLocation.x - enemyTile.gridLocation.x) +
                           Mathf.Abs(unitTile.gridLocation.y - enemyTile.gridLocation.y);

            // Check if the enemy is within attack range
            if (distance <= unit.attackRange)
            {
                unit.enemiesInRange.Add(enemy);
                Debug.Log($"Enemy {enemy.name} is in range. Distance: {distance}, Attack Range: {unit.attackRange}");
            }
        }
    }
}