using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    [SerializeField] private Transform pfCharacterBattle; // Prefab of the character's transform
    [SerializeField] Transform Spawningpoint1;           // Player spawn point
    [SerializeField] Transform Spawningpoint2;           // Enemy spawn point

    private CharacterBattle playerCharacterbattle;
    private CharacterBattle enemyCharacterbattle;
    private State state;

    private enum State
    {
        WaitingForPlayer,
        Busy,
    }

    private void Start()
    {
        // Spawn the player and enemy characters
        playerCharacterbattle = SpawnCharacter(true);
        enemyCharacterbattle = SpawnCharacter(false);

        // Set the spawn points
        Spawningpoint1.position = new Vector3(0.31f, -2.3f, 0f); // Player spawn position
        Spawningpoint2.position = new Vector3(5.7f, 0f, 0f);     // Enemy spawn position

        // Set the initial state
        state = State.WaitingForPlayer;
    }

    private void Update()
    {
        if (state == State.WaitingForPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space))  // Check for player input (space key)
            {
                // Change state to Busy when the player acts
                state = State.Busy;

                // Perform the player's attack and set callback to wait for the next player input
                playerCharacterbattle.Attack(enemyCharacterbattle, () =>
                {
                    state = State.WaitingForPlayer;  // Set state back to waiting for player input
                });
            }
        }
    }

    // Method to spawn a character
    private CharacterBattle SpawnCharacter(bool isPlayerTeam)
    {
        Vector3 position;
        if (isPlayerTeam)
        {
            position = Spawningpoint1.position; // Set spawn point for player
        }
        else
        {
            position = Spawningpoint2.position; // Set spawn point for enemy
        }

        // Instantiate the GameObject from the prefab and spawn it at the correct position
        GameObject characterObject = Instantiate(pfCharacterBattle.gameObject, position, Quaternion.identity);

        // Return the CharacterBattle component attached to the spawned GameObject
        return characterObject.GetComponent<CharacterBattle>();
    }
}