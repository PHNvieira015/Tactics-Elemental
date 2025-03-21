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

    public void DecideAction()
    {
        Debug.Log($"AI {unit.name} is deciding action");

        FindAllEnemies();
        FindAllAllies();
        CheckForEnemiesInRange();

        if (unit.enemiesInRange.Count > 0)
        {
            if (unit.hasAttacked == false)
            {
                // Attack the first enemy in range
                Debug.Log($"AI {unit.name} found an enemy in range, attacking...");
                AttackEnemy(unit.enemiesInRange[0]);
            }
        }
        else
        {
            if (unit.hasMoved == false)
            {
                // Move toward the nearest enemy
                Debug.Log($"AI {unit.name} found no enemies in range, moving toward nearest enemy.");
                MoveTowardNearestEnemy();
            }

            // End the turn if:
            // 1. The AI has moved and there are no enemies in range, or
            // 2. The AI has moved and attacked
            if ((unit.hasMoved && unit.enemiesInRange.Count == 0) || (unit.hasMoved && unit.hasAttacked))
            {
                Debug.Log($"AI {unit.name} has completed its actions, ending turn.");
                turnStateManager.ChangeState(TurnState.EndTurn);
            }
            else
            {
                Debug.Log($"AI {unit.name} cannot end turn yet. hasMoved: {unit.hasMoved}, hasAttacked: {unit.hasAttacked}, enemiesInRange: {unit.enemiesInRange.Count}");
            }
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
            return;
        }

        // Move the unit to the closest tile
        mouseController.HandleMovement(closestTile);
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
    }
}