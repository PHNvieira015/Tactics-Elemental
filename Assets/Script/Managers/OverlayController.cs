using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class OverlayController : MonoBehaviour
{
    private static OverlayController _instance;
    public static OverlayController Instance { get { return _instance; } }

    public Dictionary<Color, HashSet<OverlayTile>> coloredTiles; // Use HashSet to avoid duplicates
    public GameConfig gameConfig;

    // Colors to be used for specific purposes
    public Color AttackRangeColor;
    public Color MoveRangeColor;
    public Color BlockedTileColor;

    public enum TileColors
    {
        MovementColor,
        AttackRangeColor,
        AttackColor
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        coloredTiles = new Dictionary<Color, HashSet<OverlayTile>>();
        MoveRangeColor = gameConfig.MoveRangeColor;
        AttackRangeColor = gameConfig.AttackRangeColor;
        BlockedTileColor = gameConfig.BlockedTileColor;
    }

    // Remove colors from all tiles
    public void ClearTiles(Color? color = null)
    {
        if (color.HasValue)
        {
            if (coloredTiles.ContainsKey(color.Value))
            {
                var tiles = coloredTiles[color.Value];
                foreach (var tile in tiles)
                {
                    tile.HideTile();
                }
                coloredTiles.Remove(color.Value);
            }
        }
        else
        {
            foreach (var tiles in coloredTiles.Values)
            {
                foreach (var tile in tiles)
                {
                    tile.HideTile();
                }
            }
            coloredTiles.Clear();
        }
    }

    // Color tiles to a specific color and type
    public void ColorTiles(Color color, List<OverlayTile> overlayTiles, TileType type = TileType.Movement)
    {
        if (overlayTiles == null || overlayTiles.Count == 0) return;

        ClearTiles(color);  // Clear existing tile colors for this color

        var tileSet = new HashSet<OverlayTile>(); // Use HashSet to avoid duplicates

        foreach (var tile in overlayTiles)
        {
            if (tile == null) continue;

            if (tile.isBlocked)
            {
                tile.ShowTile(BlockedTileColor, TileType.Blocked);  // Override color for blocked tiles
            }
            else
            {
                tile.ShowTile(color, type);  // Pass the color and type
            }

            tileSet.Add(tile); // Add to the HashSet
        }

        coloredTiles[color] = tileSet; // Update the dictionary
    }

    // Color only one tile
    public void ColorSingleTile(Color color, OverlayTile tile, TileType type = TileType.Movement)
    {
        if (tile == null) return;

        if (tile.isBlocked)
        {
            tile.ShowTile(BlockedTileColor, TileType.Blocked);  // Override color for blocked tiles
        }
        else
        {
            tile.ShowTile(color, type);  // Pass the color and type
        }

        if (!coloredTiles.ContainsKey(color))
        {
            coloredTiles[color] = new HashSet<OverlayTile>();
        }

        coloredTiles[color].Add(tile); // Add to the HashSet
    }
}