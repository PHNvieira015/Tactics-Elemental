using UnityEngine;

public class SpawningTile : MonoBehaviour
{
    public bool IsOccupied { get; set; }
    public Unit unitOnTile;  // The unit that occupies this tile

    public void SetUnitOnTile(Unit unit)
    {
        unitOnTile = unit;
        IsOccupied = true;
    }

    public void ClearUnitOnTile()
    {
        unitOnTile = null;
        IsOccupied = false;
    }
}
