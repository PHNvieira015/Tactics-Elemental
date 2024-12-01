using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public List<Unit> playerAvailableUnits;  // The list of units available to the player
    public List<Unit> enemyList;             // The list of enemies to spawn (provided by you)
    public GameObject enemySpawner;         // The EnemySpawner object (with child tiles)

    private List<Unit> spawnedUnits = new List<Unit>();  // List to track the spawned units

    void Start()
    {
        // Check if the enemyList is not empty and the enemySpawner is assigned
        if (enemyList.Count > 0 && enemySpawner != null)
        {
            // Gather all child tiles under the EnemySpawner
            List<Transform> enemySpawningTiles = new List<Transform>();
            foreach (Transform child in enemySpawner.transform)
            {
                enemySpawningTiles.Add(child);  // Add each child tile to the list
            }

            // Ensure we do not spawn more enemies than there are spawn tiles
            int spawnLimit = Mathf.Min(enemyList.Count, enemySpawningTiles.Count);

            // Loop through the number of enemies to spawn based on available tiles
            for (int i = 0; i < spawnLimit; i++)
            {
                // Get the enemy unit to spawn
                Unit enemyToSpawn = enemyList[i];

                // Get the spawn tile to place the enemy on
                Transform spawnTile = enemySpawningTiles[i];

                // Check if this enemy has already been spawned
                if (!spawnedUnits.Contains(enemyToSpawn))
                {
                    // Instantiate the unit at the spawn tile's position
                    Unit spawnedUnit = Instantiate(enemyToSpawn, spawnTile.position, Quaternion.identity);

                    // Set the unit's teamID to 2
                    spawnedUnit.teamID = 2;
                    spawnedUnit.playerOwner = 2;

                    // Add the spawned unit to the list
                    spawnedUnits.Add(spawnedUnit);

                    // Print the details of the newly spawned unit
                    Debug.Log($"Enemy {spawnedUnit.characterStats.CharacterName} has been spawned at position {spawnTile.position} with TeamID: {spawnedUnit.teamID}");
                }
                else
                {
                    Debug.Log($"Enemy {enemyToSpawn.characterStats.CharacterName} has already been spawned.");
                }
            }
        }
        else
        {
            Debug.Log("No enemies or enemy spawner available.");
        }

        // Optionally print the list of spawned units (for debugging purposes)
        PrintUnitList();
    }

    // Method to print out the details of all spawned units
    void PrintUnitList()
    {
        // Check if the list is empty
        if (spawnedUnits.Count == 0)
        {
            Debug.Log("No units available.");
            return;
        }

        // Loop through the list and print each unit's details
        foreach (Unit unit in spawnedUnits)
        {
            Debug.Log($"Unit Name: {unit.characterStats.CharacterName}, Health: {unit.characterStats.currentHealth}, TeamID: {unit.teamID}");
        }
    }
}
