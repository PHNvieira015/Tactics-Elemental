using System.Collections.Generic;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    private SavedData savedData;

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
    public int[] unitsStat; // Added to match SavedData

    #endregion

    private void Start()
    {
        // Load saved data when the game starts
        LoadData();
    }

    public void SaveData()
    {
        // Update savedData with current values before saving
        savedData = new SavedData
        {
            playerName = this.playerName,
            playerGenderMF = this.playerGenderMF,
            playerScene = this.playerScene,
            playerLevel = this.playerLevel,
            inventoryItems = this.inventoryItems,
            playerGold = this.playerGold,
            units = this.units,
            unitsLevel = this.unitsLevel,
            unitsSkillTreePoints = this.unitsSkillTreePoints,
            unitsequipment = this.unitsequipment,
            unitsStat = this.unitsStat // Added to match SavedData
        };

        // Save the data
        SaveSystem.Save(savedData);
        Debug.Log("Game Saved!");
    }

    public void LoadData()
    {
        // Load saved data
        savedData = SaveSystem.Load();

        if (savedData != null)
        {
            // Assign loaded data to variables
            playerName = savedData.playerName;
            playerGenderMF = savedData.playerGenderMF;
            playerScene = savedData.playerScene;
            playerLevel = savedData.playerLevel;
            inventoryItems = savedData.inventoryItems ?? new List<string>(); // Ensure inventory is not null
            playerGold = savedData.playerGold;
            units = savedData.units;
            unitsLevel = savedData.unitsLevel;
            unitsSkillTreePoints = savedData.unitsSkillTreePoints;
            unitsequipment = savedData.unitsequipment;
            unitsStat = savedData.unitsStat; // Added to match SavedData

            Debug.Log("Loaded Player Name: " + savedData.playerName);
            Debug.Log("Loaded Player Gold: " + savedData.playerGold);

            // Update game objects and systems
            //UpdatePlayer();
            //UpdateInventory();
            //UpdateUnits();
        }
        else
        {
            Debug.Log("No saved data found. Using default values.");
            SetDefaultValues();
        }
    }

    private void SetDefaultValues()
    {
        // Set default values if no saved data is found
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