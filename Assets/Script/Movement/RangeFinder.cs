using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;

public class RangeFinder
{
    public MapManager mapManager;
    public List<OverlayTile> GetTilesInRange(Vector2Int location, int range)
    {
        // Find MapManager instance using FindObjectOfType (works if there's only one MapManager in the scene)
        MapManager mapManager = GameObject.FindObjectOfType<MapManager>();

        if (mapManager == null)
        {
            Debug.LogError("MapManager not found in the scene.");
            return new List<OverlayTile>();
        }


        var startingTile = mapManager.map[location];  // Access map data directly
        var inRangeTiles = new List<OverlayTile>();
        int stepCount = 0;

        inRangeTiles.Add(startingTile);

        // Should contain the surroundingTiles of the previous step
        var tilesForPreviousStep = new List<OverlayTile>();
        tilesForPreviousStep.Add(startingTile);
        while (stepCount < range)
        {
            var surroundingTiles = new List<OverlayTile>();

            foreach (var item in tilesForPreviousStep)
            {
                surroundingTiles.AddRange(mapManager.GetNeighbourTiles(item, new List<OverlayTile>(), ignoreObstacles: false));
            }

            inRangeTiles.AddRange(surroundingTiles);
            tilesForPreviousStep = surroundingTiles.Distinct().ToList();
            stepCount++;
        }

        return inRangeTiles.Distinct().ToList();  // Return distinct tiles in range
    }
}
