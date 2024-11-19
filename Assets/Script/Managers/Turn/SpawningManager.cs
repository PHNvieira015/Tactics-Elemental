using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class SpawningManager : MonoBehaviour
{
    [SerializeField] private GameObject SpawningSelection;
 

    private void Start()
    {
 
    }


    void Awake()
    {
        // Subscribe to game state change event
        GameManager.OnGameStateChanged += GameManagerOnOnGameStateChanged;
    }
    void OnDestroy()
    {
        // Unsubscribe from game state change event
        GameManager.OnGameStateChanged -= GameManagerOnOnGameStateChanged;
    }

    private void GameManagerOnOnGameStateChanged(GameState state)
    {
        // Toggle SpawningSelection visibility based on game state
        SpawningSelection.SetActive(state == GameState.SpawningUnits);

        // If we are in the SpawningUnits state, update the game state to PlayerTurn
        if (state == GameState.SpawningUnits)
        {
            // Now we can safely access the singleton GameManager and update the state
            GameManager.instance.UpdateGameState(GameState.PlayerTurn);
        }
        
    }
}
