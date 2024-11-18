using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;
using static OverlayTile;

// Handles the coloring of tiles. 
public class OverlayController : MonoBehaviour
{
    private static OverlayController _instance;
    public static OverlayController Instance { get { return _instance; } }

    public Dictionary<Color, List<OverlayTile>> coloredTiles;
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

        coloredTiles = new Dictionary<Color, List<OverlayTile>>();
        MoveRangeColor = gameConfig.MoveRangeColor;
        AttackRangeColor = gameConfig.AttackRangeColor;
        BlockedTileColor = gameConfig.BlockedTileColor;
    }

    // Remove colours from all tiles
    public void ClearTiles(Color? color = null)
    {
        if (color.HasValue)
        {
            if (coloredTiles.ContainsKey(color.Value))
            {
                var tiles = coloredTiles[color.Value];
                coloredTiles.Remove(color.Value);
                foreach (var coloredTile in tiles)
                {
                    coloredTile.HideTile();

                    foreach (var usedColors in coloredTiles.Keys)
                    {
                        foreach (var usedTile in coloredTiles[usedColors])
                        {
                            if (coloredTile.grid2DLocation == usedTile.grid2DLocation)
                            {
                                coloredTile.ShowTile(usedColors);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            foreach (var item in coloredTiles.Keys)
            {
                foreach (var colouredTile in coloredTiles[item])
                {
                    colouredTile.HideTile();
                }
            }

            coloredTiles.Clear();
        }
    }

    // Color tiles to specific color and type
    public void ColorTiles(Color color, List<OverlayTile> overlayTiles, TileType type = TileType.Movement)
    {
        ClearTiles(color);  // Clear existing tile colors
        foreach (var tile in overlayTiles)
        {
            tile.ShowTile(color, type);  // Pass the color and type

            if (tile.isBlocked)
            {
                tile.ShowTile(BlockedTileColor, TileType.Blocked);  // Override color for blocked tiles
            }
        }

        coloredTiles.Add(color, overlayTiles);
    }

    // Color only one tile
    public void ColorSingleTile(Color color, OverlayTile tile, TileType type = TileType.Movement)
    {
        tile.ShowTile(color, type);  // Pass the color and type (defaults to TileType.Movement)

        if (tile.isBlocked)
        {
            tile.ShowTile(BlockedTileColor, TileType.Blocked);  // Override color for blocked tiles
        }

        var list = new List<OverlayTile> { tile };

        if (!coloredTiles.ContainsKey(color))
            coloredTiles.Add(color, list);
        else
            coloredTiles[color].AddRange(list);
    }
}