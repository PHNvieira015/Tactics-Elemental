using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable] // Makes it visible in the Unity Inspector
public class TileData
{
    public List<TileBase> baseTiles; // Optional: If you need tile references
    public bool hasTooltip;
    public bool isTraversable;
    public string tooltipName;
    [TextArea(3, 10)]
    public string tooltipDescription;
    public TileTypes type = TileTypes.Traversable;
    public int MoveCost = 1;
    public ScriptableEffect effect; // Shared reference (if needed)
}