using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public List<Unit> playerAvailableUnits;  // The list of units available to the player
    public Unit unitPrefab; // Drag your prefab into this field in the Inspector
    public CharacterStat defaultCharacterStat;  // Reference to a default CharacterStat ScriptableObject

    void Start()
    {
        // Instantiate a new Unit prefab
        Unit newUnit = Instantiate(unitPrefab);

        // Assign the default CharacterStat ScriptableObject to the new unit
        newUnit.characterStats = defaultCharacterStat;

        // Add the new unit to the list
        playerAvailableUnits.Add(newUnit);

        // Print the list of units to the console
        PrintUnitList();
    }

    // Method to print out the details of all units in the list
    void PrintUnitList()
    {
        // Check if the list is empty
        if (playerAvailableUnits.Count == 0)
        {
            Debug.Log("No units available.");
            return;
        }

        // Loop through the list and print each unit's details
        foreach (Unit unit in playerAvailableUnits)
        {
            Debug.Log($"Unit Name: {unit.characterStats.CharacterName}, Health: {unit.characterStats.currentHealth}");
        }
    }
}
