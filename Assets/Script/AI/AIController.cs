using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Unit unit; // Reference to the Unit component
    public List<Unit> allEnemies; // List of all enemy units
    public List<Unit> allAllies; // List of all Allies units
    private OverlayTile targetTile; // Tile the AI is moving toward
    private PathFinder pathFinder; // Instance of your PathFinder

    private void Awake()
    {
        unit = GetComponent<Unit>();
        pathFinder = new PathFinder(); // Initialize the PathFinder
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
            // Attack the first enemy in range
            Debug.Log($"AI {unit.name} found an enemy in range, attacking...");
            AIAttackMethod();            
            //AttackEnemy(unit.enemiesInRange[0]);
        }
        else
        {
            // Move toward the nearest enemy
            Debug.Log($"AI {unit.name} found no enemies in range, moving toward nearest enemy.");

            //MoveTowardNearestEnemy();
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

        unit.turnStateManager.ChangeState(TurnStateManager.TurnState.Moving);
        Debug.Log($"{unit.name} Moving toward nearest enemy");

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

        if (unit.standingOnTile == null)
        {
            Debug.LogError($"{unit.name} has NO standingOnTile! AI cannot move.");
            return;
        }

        if (nearestEnemy.standingOnTile == null)
        {
            Debug.LogError($"{nearestEnemy.name} has NO standingOnTile! AI cannot find path.");
            return;
        }

        Debug.Log($"{unit.name} pathfinding from {unit.standingOnTile.grid2DLocation} to {nearestEnemy.standingOnTile.grid2DLocation}");

        // Get all tiles within movement range
        List<OverlayTile> movementRangeTiles = GetMovementRangeTiles();

        if (movementRangeTiles.Count == 0)
        {
            Debug.LogError($"{unit.name} has no movement range tiles! AI is stuck.");
            return;
        }

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

        Debug.Log($"{unit.name} moving to tile at {closestTile.grid2DLocation}");

        // Call FindPath to calculate the path
        List<OverlayTile> path = pathFinder.FindPath(unit.standingOnTile, closestTile, movementRangeTiles);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"No path found from {unit.standingOnTile.grid2DLocation} to {closestTile.grid2DLocation}!");
            return;
        }

        Debug.Log($"Path found for {unit.name} with {path.Count} tiles");
        unit.StartCoroutine(MoveAlongPath(path));
        unit.hasMoved = true;
    }



    public List<OverlayTile> GetMovementRangeTiles()
    {
        List<OverlayTile> movementRangeTiles = new List<OverlayTile>();
        Vector2Int startLocation = unit.standingOnTile.grid2DLocation;

        // Use a flood-fill or similar algorithm to find all tiles within movement range
        for (int x = -unit.movementRange; x <= unit.movementRange; x++)
        {
            for (int y = -unit.movementRange; y <= unit.movementRange; y++)
            {
                Vector2Int location = new Vector2Int(startLocation.x + x, startLocation.y + y);
                if (MapManager.Instance.map.ContainsKey(location))
                {
                    OverlayTile tile = MapManager.Instance.map[location];
                    if (!tile.isBlocked) // Only add tiles that are not blocked
                    {
                        movementRangeTiles.Add(tile);
                    }
                }
            }
        }

        return movementRangeTiles;
    }

    public IEnumerator MoveAlongPath(List<OverlayTile> path)
    {
        Debug.Log("Starting movement along path");

        // Clear the unit's current tile
        if (unit.standingOnTile != null)
        {
            unit.standingOnTile.unitOnTile = null;
            unit.standingOnTile.isBlocked = false;
        }

        foreach (var tile in path)
        {
            Debug.Log($"Moving to tile at {tile.transform.position}");

            // Update the unit's position
            unit.transform.position = tile.transform.position;

            // Update the unit's standingOnTile
            unit.standingOnTile = tile;

            // Update the tile's unitOnTile and isBlocked status
            tile.unitOnTile = unit;
            tile.isBlocked = true;

            yield return new WaitForSeconds(0.5f); // Adjust delay for animation or smooth movement
        }

        Debug.Log("Movement complete");
        // Mark the unit as having moved
        unit.hasMoved = true;
    }

    public void AttackEnemy(Unit enemy)
    {
        if (unit.hasAttacked)
        {
            Debug.LogWarning($"{unit.name} has already moved this turn!");
            return;
        }

        unit.turnStateManager.ChangeState(TurnStateManager.TurnState.Attacking);
        Debug.Log($"Attempting to attack {enemy.unitName}");
        if (unit.enemiesInRange.Contains(enemy))
        {
            Debug.Log($"{unit.unitName} is attacking {enemy.unitName}");
            // Use DamageSystem to calculate and apply damage
            DamageSystem.Instance.Attack(unit, enemy);
            unit.hasAttacked = true;
        }
    }
    public void AIAttackMethod()
    {
        unit.turnStateManager.ChangeState(TurnStateManager.TurnState.Attacking);
        //targetTile.target //WIP work to get the enemy tile
    }
}