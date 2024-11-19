using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{


public static GameManager instance;

    public static event Action<GameState> OnGameStateChanged;


    public GameState State;
    public enum GameState
    {
    SpawningUnits, //need to make a reference to this somewhere
    PlayerTurn,  //expand into the new turn model
    EnemyTurn, //expand into the new turn model
    Decide,
    Victory,    //handle victory and EXP
    Defeat,       //handle Losing and EXP


    }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
    UpdateGameState(GameState.SpawningUnits);
    }



    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.SpawningUnits:
                HandleSpawningUnits();

                break;
            case GameState.PlayerTurn:
                HandlePlayerTurn();
                break;
            case GameState.EnemyTurn:
                //HandleEnemyTurn();
                break;
            case GameState.Decide:
                break;
            case GameState.Victory:
                break;
            case GameState.Defeat:
                break;
        }

        OnGameStateChanged?.Invoke(newState);

    }

    private void HandleSpawningUnits()
    {
        throw new NotImplementedException();
    }
    private void HandlePlayerTurn()
    {
     
    }
    private void HandleDecide()
    {
        var units = FindObjectsOfType<Unit>();
        if (!units.Any(unit => unit.teamID == 2)) UpdateGameState(GameState.Victory);
        else if (!units.Any(unit => unit.teamID == 1)) UpdateGameState(GameState.Defeat);
        else UpdateGameState(GameState.PlayerTurn);
    }



}