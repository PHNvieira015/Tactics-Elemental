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
        Debug.Log($"Starting turn for {unit.name}. Current unit: {turnStateManager.currentUnit.name}");
        if (unit == turnStateManager.currentUnit && turnStateManager.currentUnit.isAI==true)
        {
            StartCoroutine(RunTurn());
        }
        else
        {
            Debug.LogWarning($"{unit.name} is not the current unit. Current unit is {turnStateManager.currentUnit.name}");
        }
    }

    private IEnumerator RunTurn()
    {
        if (unit != turnStateManager.currentUnit)
        {
            Debug.LogWarning($"{unit.name} tried to act but is not the current unit!");
            yield break;
        }

        while (unit == turnStateManager.currentUnit)
        {
            Debug.Log($"AI {unit.name} is running its turn.");

            yield return new WaitForSeconds(0.5f);

            CheckForEnemiesInRange();
            DecideAction();

            if ((unit.hasMoved && unit.hasAttacked) || (unit.hasMoved && unit.enemiesInRange.Count == 0) || (unit.hasAttacked))
            {
                EndTurn();
                yield break;
            }
        }
    }


    public void DecideAction()
    {
        Debug.Log($"[{unit.name}] Deciding action. hasMoved: {unit.hasMoved}, hasAttacked: {unit.hasAttacked}");

        // Skip action if unit already moved or attacked
        if (unit.hasMoved && unit.hasAttacked)
        {
            Debug.Log($"[{unit.name}] Already moved and attacked, ending turn.");
            StartCoroutine(WaitAndEndTurn());
            return;
        }

        FindAllEnemies();
        FindAllAllies();
        CheckForEnemiesInRange();

        // If there are enemies in range and the unit hasn't attacked yet, prioritize attacking
        if (unit.enemiesInRange.Count > 0 && !unit.hasAttacked)
        {
            AttackEnemy(unit.enemiesInRange[0]);
        }
        // If there are no enemies in range, move the unit towards the nearest enemy, but only if it hasn't moved yet
        else if (!unit.hasMoved)
        {
            MoveTowardNearestEnemy();
        }
        else
        {
            // If no action is required, end the turn
            StartCoroutine(WaitAndEndTurn());
        }
    }



    public void MoveTowardNearestEnemy()
    {
        // Ensure this unit is the current unit
        if (turnStateManager.currentUnit != unit)
        {
            Debug.LogWarning($"{unit.name} is not the current unit. Skipping movement.");
            return;
        }

        if (unit.hasMoved)
        {
            Debug.LogWarning($"{unit.name} has already moved this turn!");
            return; // Avoid moving again if the unit has already moved
        }

        Unit nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            Debug.LogWarning($"{unit.name} found no enemies!");
            StartCoroutine(WaitAndEndTurn());
            return;
        }

        // Set the AI unit as the current unit in MouseController
        mouseController.SetUnit(unit);

        // Get the movement range tiles
        List<OverlayTile> movementRangeTiles = mouseController.GetInRangeTiles();

        // Find the closest valid tile to the enemy
        OverlayTile closestTile = FindClosestValidTile(nearestEnemy.standingOnTile, movementRangeTiles);

        if (closestTile == null)
        {
            Debug.LogWarning($"No valid tile found for {unit.name} to move toward {nearestEnemy.name}!");
            EndTurn(); // End turn if no valid tile is found
            return;
        }

        // Notify TurnStateManager of state change
        turnStateManager.ChangeState(TurnState.Moving);

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
        DecideAction(); // Re-decide action after moving
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

        Debug.Log($"{unit.name} has attacked. hasAttacked: {unit.hasAttacked}");

        // Wait for 2 seconds before re-evaluating or ending the turn
        StartCoroutine(WaitAfterAttack());
    }

    private IEnumerator WaitAfterAttack()
    {
        yield return new WaitForSeconds(3f); // Wait for 2 seconds after attacking

        // Re-evaluate after attacking (e.g., check if the unit can move or end the turn)
        DecideAction();
    }

    private IEnumerator WaitAndEndTurn()
    {
        yield return new WaitForSeconds(2f);
        // After waiting, explicitly set the end turn state
        EndTurn();
    }

    private void EndTurn()
    {
        if (turnStateManager.currentUnit != unit)
        {
            Debug.LogWarning($"{unit.name} tried to end turn but is not the current unit!");
            return;
        }

        Debug.Log($"Ending turn for {unit.name}.");

        unit.hasMoved = false;
        unit.hasAttacked = false;

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
    private OverlayTile FindClosestValidTile(OverlayTile targetTile, List<OverlayTile> movementRangeTiles)
    {
        Debug.Log($"[{unit.name}] Finding closest valid tile to {targetTile.gridLocation}.");

        OverlayTile closestTile = null;
        int closestDistance = int.MaxValue;

        foreach (var tile in movementRangeTiles)
        {
            // Skip occupied or blocked tiles
            if (tile.isBlocked || tile.unitOnTile != null)
            {
                Debug.Log($"Tile {tile.gridLocation} is occupied or blocked, skipping.");
                continue;
            }

            // Calculate Manhattan distance to the target tile
            int distance = Mathf.Abs(tile.gridLocation.x - targetTile.gridLocation.x) +
                           Mathf.Abs(tile.gridLocation.y - targetTile.gridLocation.y);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTile = tile;
            }
        }

        Debug.Log($"[{unit.name}] Closest valid tile: {closestTile?.gridLocation}");
        return closestTile;
    }
    private Unit FindNearestEnemy()
    {
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

        return nearestEnemy;
    }

}