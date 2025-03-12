using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;

public class RangeFinder
{
    public List<OverlayTile> GetTilesInRange(
    Vector2Int location,
    int range,
    int teamID,  // Added teamID parameter
    bool ignoreObstacles = false
)
    {
        MapManager mapManager = GameObject.FindObjectOfType<MapManager>();

        if (mapManager == null)
        {
            Debug.LogError("MapManager not found in the scene.");
            return new List<OverlayTile>();
        }

        if (!mapManager.map.ContainsKey(location))
        {
            Debug.LogError($"No tile found at location {location}");
            return new List<OverlayTile>();
        }

        var startingTile = mapManager.map[location];
        var inRangeTiles = new List<OverlayTile> { startingTile };
        var tilesForPreviousStep = new List<OverlayTile> { startingTile };
        int stepCount = 0;

        while (stepCount < range)
        {
            var surroundingTiles = new List<OverlayTile>();

            foreach (var tile in tilesForPreviousStep)
            {
                var neighbours = mapManager.GetNeighbourTiles(tile, new List<OverlayTile>(), true);

                foreach (var neighbour in neighbours)
                {
                    // Skip obstacles unless ignoring them
                    if (!ignoreObstacles && neighbour.isBlockedByObstacle)
                        continue;

                    // Skip tiles with enemy units unless ignoring obstacles (for attack range)
                    if (!ignoreObstacles && neighbour.unitOnTile != null && neighbour.unitOnTile.teamID != teamID)
                        continue;

                    if (!inRangeTiles.Contains(neighbour))
                    {
                        surroundingTiles.Add(neighbour);
                        inRangeTiles.Add(neighbour);
                    }
                }
            }

            tilesForPreviousStep = surroundingTiles;
            stepCount++;
        }

        return inRangeTiles.Distinct().ToList();
    }
}
