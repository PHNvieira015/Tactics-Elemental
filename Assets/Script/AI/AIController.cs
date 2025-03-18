using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Unit unit; // Reference to the Unit component
    public List<Unit> allEnemies; // List of all enemy units
    private OverlayTile targetTile; // Tile the AI is moving toward

    private PathFinder pathFinder; // Instance of your PathFinder

    private void Awake()
    {
        unit = GetComponent<Unit>();
        pathFinder = new PathFinder(); // Initialize the PathFinder
    }

    // Find all enemies on the opposing team
    public void FindAllEnemies()
    {
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

    // Check if any enemies are within attack range
    public void CheckForEnemiesInRange()
    {
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

    // Move toward the nearest enemy
    public void MoveTowardNearestEnemy()
    {
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

        if (nearestEnemy != null)
        {
            // Use your PathFinder to find a path to the nearest enemy
            targetTile = nearestEnemy.standingOnTile;
            List<OverlayTile> path = pathFinder.FindPath(unit.standingOnTile, targetTile, GetMovementRangeTiles());

            if (path != null && path.Count > 0)
            {
                // Move along the path
                unit.StartCoroutine(MoveAlongPath(path));
            }
        }
    }

    // Get all tiles within the unit's movement range
    private List<OverlayTile> GetMovementRangeTiles()
    {
        List<OverlayTile> movementRangeTiles = new List<OverlayTile>();
        Vector2Int startLocation = unit.standingOnTile.grid2DLocation;

        // Use a flood-fill or similar algorithm to find all tiles within movement range
        // For now, here's a placeholder implementation
        for (int x = -unit.movementRange; x <= unit.movementRange; x++)
        {
            for (int y = -unit.movementRange; y <= unit.movementRange; y++)
            {
                Vector2Int location = new Vector2Int(startLocation.x + x, startLocation.y + y);
                if (MapManager.Instance.map.ContainsKey(location))
                {
                    movementRangeTiles.Add(MapManager.Instance.map[location]);
                }
            }
        }

        return movementRangeTiles;
    }

    private IEnumerator MoveAlongPath(List<OverlayTile> path)
    {
        foreach (var tile in path)
        {
            // Move the unit to the next tile
            unit.transform.position = tile.transform.position;
            unit.standingOnTile = tile;
            yield return new WaitForSeconds(0.5f); // Adjust delay for animation or smooth movement
        }
    }

    // Attack an enemy if in range
    public void AttackEnemy(Unit enemy)
    {
        if (unit.enemiesInRange.Contains(enemy))
        {
            Debug.Log($"{unit.unitName} is attacking {enemy.unitName}");
            // Implement your attack logic here (e.g., deal damage, play animations)
            enemy.TakeDamage(unit.characterStats.attackDamage, unit.damageSystem);
            unit.hasAttacked = true;
        }
    }

    // AI decision-making process
    public void DecideAction()
    {
        if (!unit.IsAlive() || unit.hasMoved || unit.hasAttacked) return;

        FindAllEnemies();
        CheckForEnemiesInRange();

        if (unit.enemiesInRange.Count > 0)
        {
            // Attack the first enemy in range
            AttackEnemy(unit.enemiesInRange[0]);
        }
        else
        {
            // Move toward the nearest enemy
            MoveTowardNearestEnemy();
        }
    }

}