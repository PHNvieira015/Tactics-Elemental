using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedData
{
    #region Variables

    [Header("Dialogue")]
    public string playerName;
    public bool playerGenderMF;
    public string playerScene;

    [Header("Player Stats")]
    public int playerLevel;
    public List<string> inventoryItems;
    public int playerGold;

    [Header("OtherUnits Stats")]
    public int[] units;
    public int[] unitsLevel;
    public int[] unitsSkillTreePoints;
    public int[] unitsequipment;
    public int[] unitsStat;

    #endregion

    // Default constructor
    public SavedData()
    {
        // Set default values
        playerName = "Player";
        playerGenderMF = true;
        playerScene = "Scene1";
        playerLevel = 1;
        inventoryItems = new List<string>();
        playerGold = 0;
        units = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        unitsLevel = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        unitsSkillTreePoints = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        unitsequipment = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        unitsStat = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }
}