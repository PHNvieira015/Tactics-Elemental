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
        //indevelopment passable tile team1 or passable tile team2
     //   if (isBlocked = true)
     //   {
     //   if (activeCharacter.teamID ==1)
     //   {
     //       tileData.type = TileTypes.PlayerUnitBlocked;
     //    }
     //   if (activeCharacter.teamID == 2)
     //   {
     //       tileData.type = TileTypes.EnemyUnitBlocked;
     //   }
     //   else
     //       {
     //           return;
     //       }

     //}
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

    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }

    public void ShowTile(Color color, TileType type = TileType.Movement)
    {
        GetComponent<SpriteRenderer>().color = color;

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

    public void SetSprite(ArrowDirection d, bool highlight = false)
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

    public Unit GetUnit()
    {
        return FindObjectsOfType<Unit>().FirstOrDefault(unit => unit.standingOnTile == this);
    }

    public void SetUnit(Unit unit)
    {
        activeCharacter = unit;
        //isBlocked = true;

        if (unit.teamID == 1)
        {
            tileData.type = TileTypes.PlayerUnitBlocked;
        }
        else if (unit.teamID == 2)
        {
            tileData.type = TileTypes.EnemyUnitBlocked;
        }
    }

    public void ClearUnit()
    {
        activeCharacter = null;
        isBlocked = false;
        tileData.type = TileTypes.Traversable;
    }

    public bool CanMoveThrough(Unit unit)
    {
        if (isBlocked && activeCharacter != null)
        {
            return activeCharacter.teamID == unit.teamID;
        }
        return true;
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
