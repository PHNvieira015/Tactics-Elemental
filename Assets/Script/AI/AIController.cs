using static TurnStateManager;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;

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
        // Only proceed if this is an AI unit AND it's the current unit
        if (unit.isAI && unit == turnStateManager.currentUnit)
        {
            Debug.Log($"Starting AI turn for {unit.name}");
            StartCoroutine(RunTurn());
        }
        else
        {
            Debug.LogWarning($"{unit.name} is not an AI unit or not the current unit. Current unit is {turnStateManager.currentUnit?.name}");
        }
    }

    private IEnumerator RunTurn()
    {
        if (unit != turnStateManager.currentUnit && unit.isAI || !unit.isAI)
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
                mouseController.path.Clear();
                EndTurn();
                yield break;
            }
        }
    }


    public void DecideAction()
    {
        Debug.Log($"[{unit.name}] Deciding action. hasMoved: {unit.hasMoved}, hasAttacked: {unit.hasAttacked}");

        if (unit.hasMoved && unit.hasAttacked)
        {
            Debug.Log($"[{unit.name}] Already moved and attacked, ending turn.");
            StartCoroutine(WaitAndEndTurn());
            return;
        }

        FindAllEnemies();
        FindAllAllies();
        CheckForEnemiesInRange();

        // If enemies in range and hasn't attacked
        if (unit.enemiesInRange.Count > 0 && !unit.hasAttacked)
        {
            StartCoroutine(DelayedAttack(unit.enemiesInRange[0]));
        }
        // If no enemies in range and hasn't moved
         if (!unit.hasMoved)
        {

            StartCoroutine(MoveTowardNearestEnemy());
            mouseController.ClearAttackRangeTiles();
        }
        else
        {
            StartCoroutine(WaitAndEndTurn());
        }
    }

    private IEnumerator DelayedAttack(Unit enemy)
    {
        yield return new WaitForSeconds(0.3f); // Small delay before attacking
        AttackEnemy(enemy);
    }

    private IEnumerator MoveTowardNearestEnemy()
    {
        // Ensure this unit is the current unit
        if (turnStateManager.currentUnit != unit)
        {
            Debug.LogWarning($"{unit.name} is not the current unit. Skipping movement.");
            yield break;
        }

        if (unit.hasMoved)
        {
            Debug.LogWarning($"{unit.name} has already moved this turn!");
            yield break;
        }

        Unit nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            Debug.LogWarning($"{unit.name} found no enemies!");
            StartCoroutine(WaitAndEndTurn());
            yield break;
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
            EndTurn();
            yield break;
        }

        // Notify TurnStateManager of state change
        turnStateManager.ChangeState(TurnState.Moving);

        // Move the unit to the closest tile
        StartCoroutine(MoveAndReevaluate(closestTile));
    }


    private IEnumerator MoveAndReevaluate(OverlayTile targetTile)
    {
        // Clear any existing path before moving
        mouseController.path.Clear();

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

        // Clear the path after movement is complete
        mouseController.path.Clear();

        // Clear any movement range highlights
        foreach (var tile in mouseController.rangeFinderTiles)
        {
            tile.HideTile();
        }
        mouseController.rangeFinderTiles.Clear();

        // Wait before re-evaluating
        yield return new WaitForSeconds(0.5f);

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

        // Start the attack sequence with visualization
        StartCoroutine(AttackWithVisualization(enemy));
    }

    private IEnumerator AttackAfterDelay(Unit enemy)
    {
        // Wait for 0.2 seconds to show the attack range
        yield return new WaitForSeconds(0.2f);

        // Clear the attack range tiles
        mouseController.ClearAttackRangeTiles();

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

        unit.hasMoved = true;
        unit.hasAttacked = true;

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

        // Calculate the desired distance based on attack range
        int desiredDistance = unit.attackRange;

        // Find the closest valid tile in the movement range
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

            // Check if the tile is at the desired distance
            if (distance == desiredDistance)
            {
                // If the tile is at the desired distance, return it immediately
                Debug.Log($"[{unit.name}] Found a valid tile at the desired distance: {tile.gridLocation}.");
                return tile;
            }

            // If no tile is found at the desired distance, track the closest tile as a fallback
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTile = tile;
            }
        }

        // If no tile is found at the desired distance, return the closest valid tile as a fallback
        Debug.Log($"[{unit.name}] No tile found at the desired distance. Returning closest tile: {closestTile?.gridLocation}.");
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
    private IEnumerator AttackWithVisualization(Unit enemy)
    {
        // Verify again before each action
        if (!unit.isAI || turnStateManager.currentUnit != unit)
        {
            yield break;
        }
        // 1. Set up attack state
        turnStateManager.ChangeState(TurnState.Attacking);

        // 2. This will automatically show attack range through MouseController's Update
        yield return new WaitForSeconds(0.5f); // Let player see the range
       
        if (!unit.isAI || turnStateManager.currentUnit != unit)
        {
            turnStateManager.ChangeState(TurnState.Waiting);
            yield break;
        }
        // 3. Execute attack
        mouseController.HandleAttack(enemy.standingOnTile);
        Debug.Log($"{unit.name} has attacked. hasAttacked: {unit.hasAttacked}");

        // 4. Wait a moment after attacking
        yield return new WaitForSeconds(0.3f);

        // 5. Clear attack range through normal state change
        turnStateManager.ChangeState(TurnState.Waiting);

        // 6. Decide next action
        DecideAction();
    }

}