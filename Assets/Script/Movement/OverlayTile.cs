using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    public Unit unitOnTile;
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
            return tileData.MoveCost;
        }
        return 1;
    }

    public void Reset()
    {
        // Reset logic can go here if necessary.
    }

    // Check if this tile is the topmost tile at its position
    private bool IsTopTile()
    {
        Vector2 tilePosition = transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(tilePosition, Vector2.zero);

        if (hits.Length > 0)
        {
            // Find the topmost tile
            OverlayTile topTile = hits
                .OrderByDescending(hit => hit.collider.transform.position.z) // Sort by Z-axis
                .Select(hit => hit.collider.GetComponent<OverlayTile>())    // Get OverlayTile component
                .FirstOrDefault(tile => tile != null);                      // Select the first valid tile

            // If this tile is the topmost tile or a spawner tile, return true
            return topTile == this || tileData?.type == TileTypes.Spawner;
        }

        return false;
    }

    public void HideTile()
    {
        // Only hide the tile if it's the topmost tile or a spawner tile
        if (IsTopTile())
        {
            // Make sure tileData is not null and then check its type property
            if (tileData != null && tileData.type != TileTypes.Spawner)
            {
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            }
        }
    }

    public void ShowTile(Color color, TileType type = TileType.Movement)
    {
        // Only show the tile if it's the topmost tile or a spawner tile
        if (IsTopTile())
        {
            var renderer = GetComponent<SpriteRenderer>();
            renderer.color = color;

            switch (type)
            {
                case TileType.Movement:
                    break;
                case TileType.AttackRangeColor:
                    break;
                case TileType.AttackColor:
                    break;
                case TileType.Blocked:
                    break;
            }
        }
    }

    public void SetSprite(ArrowDirection d, bool highlight = false)
    {
        // Only set the sprite if it's the topmost tile or a spawner tile
        if (IsTopTile())
        {
            var arrowRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
            var tileRenderer = gameObject.GetComponent<SpriteRenderer>();

            if (d == ArrowDirection.None)
            {
                arrowRenderer.color = new Color(1, 1, 1, 0);
            }
            else
            {
                arrowRenderer.color = new Color(1, 1, 1, 1);
                arrowRenderer.sprite = arrows[(int)d];

                if (highlight)
                {
                    arrowRenderer.sortingLayerName = "Highlight";
                    arrowRenderer.sortingOrder = 1;
                }
                else
                {
                    arrowRenderer.sortingLayerName = tileRenderer.sortingLayerName;
                    arrowRenderer.sortingOrder = tileRenderer.sortingOrder;
                }
            }
        }
    }

    public Unit GetUnit()
    {
        // Only get the unit if this tile is the topmost tile or a spawner tile
        if (IsTopTile())
        {
            return FindObjectsOfType<Unit>().FirstOrDefault(unit => unit.standingOnTile == this);
        }
        return null;
    }

    public void SetUnit(Unit unit)
    {
        // Only set the unit if this tile is the topmost tile or a spawner tile
        if (IsTopTile())
        {
            if (unit == null) return; // Prevent null references

            if (tileData == null)
            {
                tileData = ScriptableObject.CreateInstance<TileData>();
            }

            unitOnTile = unit;
            unit.standingOnTile = this; // Ensure unit knows the tile it's standing on

            if (unit.teamID == 1 && tileData.type != TileTypes.Spawner)
            {
                tileData.type = TileTypes.PlayerUnitBlocked;
            }
            else if (unit.teamID == 2 && tileData.type != TileTypes.Spawner)
            {
                tileData.type = TileTypes.EnemyUnitBlocked;
            }

            //Debug.Log($"Unit {unit.name} is now on tile {grid2DLocation}.");
        }
    }

    public void ClearUnit()
    {
        // Only clear the unit if this tile is the topmost tile or a spawner tile
        if (IsTopTile())
        {
            activeCharacter = null;
            isBlocked = false;
            tileData.type = TileTypes.Traversable;
            unitOnTile = null;
        }
    }

    public bool CanMoveThrough(Unit unit)
    {
        // Only check movement if this tile is the topmost tile or a spawner tile
        if (IsTopTile())
        {
            if (isBlocked && activeCharacter != null)
            {
                return activeCharacter.teamID == unit.teamID;
            }
            return true;
        }
        return false;
    }
}

public enum TileType
{
    Movement,
    AttackRangeColor,
    AttackColor,
    Blocked,
    Spawn,
    EnemyunitBlocked,
    PlayerunitBlocked
}