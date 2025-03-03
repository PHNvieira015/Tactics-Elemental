using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public List<Unit> units;  // List of all units
    private Unit activeUnit;  // Active unit in the game
    private GameMaster gameMaster;  // Reference to GameMaster

    public void SetActiveUnit(Unit unit)
    {
        activeUnit = unit;
    }

    public Unit GetActiveUnit()
    {
        return activeUnit;
    }
}
