using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnStateManager;

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

    public void SetEnemies(List<Unit> enemies)
    {
        allEnemies = enemies;
    }

    public void StartTurn()
    {
        Debug.Log($"AI {unit.name} is starting its turn.");
        unit.hasMoved = false; // Reset movement flag
        unit.hasAttacked = false; // Reset attack flag
        StartCoroutine(RunTurn());
    }

    private IEnumerator RunTurn()
    {
        while (true)
        {
            DecideAction();

            // If the AI has moved and attacked, or has no actions left, end the turn
            if ((unit.hasMoved && unit.hasAttacked) || (unit.hasMoved && unit.enemiesInRange.Count == 0))
            {
                EndTurn();
                yield break; // Exit the coroutine
            }

            // Wait for the next frame to re-evaluate
            yield return null;
        }
    }

    public void DecideAction()
    {
        Debug.Log($"AI {unit.name} is deciding action. hasMoved: {unit.hasMoved}, hasAttacked: {unit.hasAttacked}");

        FindAllEnemies();
        FindAllAllies();
        CheckForEnemiesInRange(); // Update enemies in range

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
        Debug.Log("Checking for enemies in range");
        unit.enemiesInRange.Clear();
        foreach (var enemy in allEnemies)
        {
            float distance = Vector2.Distance(unit.transform.position, enemy.transform.position);
            if (distance <= unit.attackRange)
            {
                unit.enemiesInRange.Add(enemy);
                Debug.Log($"Enemy {enemy.name} is in range. Distance: {distance}, Attack Range: {unit.attackRange}");
            }
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
        float shortestDistance = float.MaxValue;

        // Find the nearest enemy
        foreach (var enemy in allEnemies)
        {
            float distance = Vector2.Distance(unit.transform.position, enemy.transform.position);
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
        float closestDistance = float.MaxValue;

        foreach (var tile in movementRangeTiles)
        {
            float distance = Vector2.Distance(tile.transform.position, nearestEnemy.transform.position);
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
        // Move the unit
        mouseController.HandleMovement(targetTile);

        // Wait for the movement to complete
        while (mouseController.isMoving)
        {
            yield return null;
        }

        // Mark the unit as having moved
        unit.hasMoved = true;
        Debug.Log($"{unit.name} has moved. hasMoved: {unit.hasMoved}");

        // Update enemies in range after moving
        CheckForEnemiesInRange();

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

        // Set the AI unit as the current unit in MouseController
        mouseController.SetUnit(unit);

        // Attack the enemy
        mouseController.HandleAttack(enemy.standingOnTile);

        // Mark the unit as having attacked
        unit.hasAttacked = true;
        Debug.Log($"{unit.name} has attacked. hasAttacked: {unit.hasAttacked}");

        // End the turn after attacking
        EndTurn();
    }

    private void EndTurn()
    {
        Debug.Log($"AI {unit.name} is ending its turn.");
        turnStateManager.ChangeState(TurnState.EndTurn);
    }
}