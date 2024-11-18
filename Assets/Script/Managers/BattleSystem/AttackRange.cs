using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public MapManager mapManager;
    public int maxRange;  // Max AttackRange
    public LayerMask terrainLayer;  // Layer mask for valid terrain tiles

    public void FindMovementRange(Vector3 startPosition)
    {
        Queue<Vector3> positionsToCheck = new Queue<Vector3>();
        positionsToCheck.Enqueue(startPosition);

        HashSet<Vector3> visited = new HashSet<Vector3>();
        visited.Add(startPosition);

        while (positionsToCheck.Count > 0)
        {
            Vector3 current = positionsToCheck.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3 neighbor = current + new Vector3(x, y, 0);

                    if (Vector3.Distance(startPosition, neighbor) <= maxRange &&
                        !visited.Contains(neighbor) &&
                        IsValidTerrain(neighbor))
                    {
                        visited.Add(neighbor);
                        positionsToCheck.Enqueue(neighbor);
                    }
                }
            }
        }

        // Display the range (e.g., using gizmos or instantiating markers)
        ShowRange(visited);
    }

    private bool IsValidTerrain(Vector3 position)
    {
        // Replace with your actual terrain validity check
        return Physics.CheckSphere(position, 0.1f, terrainLayer);
    }

    private void ShowRange(HashSet<Vector3> rangePositions)
    {
        foreach (var position in rangePositions)
        {
            // Visualize range (e.g., with gizmos or markers)
        }
    }
}
