using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private CharacterInfo _characterInfo;

    private void Awake()
    {
        _characterInfo = GetComponent<CharacterInfo>();

    }

    // Control player properties, probably gonna move to unit script

    public int Mitigation; //damage mitigation
    public bool isAlive = true; // Default value for isAlive
    public bool isDown;
    public bool hasMoved;
    public bool hasAttacked;
    public bool hasPlayed;

    public OverlayTile standingOnTile;

    // Assuming you have a method to check if the unit is alive
    public bool IsAlive()
    {
        return isAlive && !isDown; // Example logic for alive status
    }



    public int attackPower; // Add this to store attack power
    public float health = 100; // Initial health
}
