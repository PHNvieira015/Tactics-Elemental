using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/Tile/TileData")]
public class TileData : ScriptableObject
{
    public List<TileBase> baseTiles;

    public bool hasTooltip;
    public string tooltipName;
    [TextArea(3, 10)]
    public string tooltipDescription;
    public TileTypes type = TileTypes.Traversable;
    public int MoveCost = 1;
    public ScriptableEffect effect;
}