using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ArrowTranslator;

public class OverlayTile : MonoBehaviour
{
    public int G;
    public int H;
    public int F { get { return G + H; } }

    public bool isBlocked = false;
    public bool isLocked = false;
    public Unit activeCharacter;
    public TileData tileData;
    public int MoveCost => tileData != null ? tileData.MoveCost : 1;

    public OverlayTile Previous;
    public Vector3Int gridLocation;
    public Vector2Int grid2DLocation { get { return new Vector2Int(gridLocation.x, gridLocation.y); } }

    public List<Sprite> arrows;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HideTile();
        }
    }

    public int GetMoveCost()
    {
        if (tileData != null)
        {
            return tileData.MoveCost; // Assuming tileData references a TileData instance with MoveCost
        }
        return 1; // Default move cost if no data is available
    }

    public void Reset()
    {
        // Reset logic can go here if necessary.
    }

    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }

    public void ShowTile(Color color, TileType type = TileType.Movement)
    {
        //Debug.Log($"Setting tile {name} to color {color} for {type}"); // Debugging

        GetComponent<SpriteRenderer>().color = color; // Apply color

        switch (type)
        {
            case TileType.Movement:
                // Movement tile behavior
                break;
            case TileType.AttackRangeColor:
                // Attack range tile behavior
                break;
            case TileType.AttackColor:
                // Attack color tile behavior
                break;
            case TileType.Blocked:
                // Blocked tile behavior
                break;
        }
    }

    public void SetSprite(ArrowDirection d, bool highlight = false)
    {
        var arrowRenderer = GetComponentsInChildren<SpriteRenderer>()[1]; // Assuming this is the arrow sprite renderer
        var tileRenderer = gameObject.GetComponent<SpriteRenderer>();

        if (d == ArrowDirection.None)
        {
            arrowRenderer.color = new Color(1, 1, 1, 0); // Hide the arrow
        }
        else
        {
            arrowRenderer.color = new Color(1, 1, 1, 1); // Show the arrow
            arrowRenderer.sprite = arrows[(int)d];

            // Adjust sorting layer and order
            if (highlight)
            {
                arrowRenderer.sortingLayerName = "Highlight"; // Change to "Highlight" sorting layer
                arrowRenderer.sortingOrder = 1; // Optional: Set a consistent sorting order in "Highlight" layer
            }
            else
            {
                arrowRenderer.sortingLayerName = tileRenderer.sortingLayerName; // Match the tile's sorting layer
                arrowRenderer.sortingOrder = tileRenderer.sortingOrder; // Match the tile's sorting order
            }
        }
    }
    public Unit GetUnit()
    {
        // Assuming your units are NOT children of OverlayTile, we find the unit that is standing on this tile
        return FindObjectsOfType<Unit>().FirstOrDefault(unit => unit.standingOnTile == this);
    }
}

public enum TileType
{
    Movement,
    AttackRangeColor,
    AttackColor,
    Blocked,
    Spawn,

}
