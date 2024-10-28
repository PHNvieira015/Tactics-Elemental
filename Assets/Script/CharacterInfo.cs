using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    public OverlayTile standingOnTile;
    
    
    //movement atributes
    public float MoveSpeed;
    public bool hasMoved;

    //Attack atributes
    public int playerNumber;
    public float initiative;
    public int attackRange;
    private List<CharacterInfo> enemiesInRange = new List<CharacterInfo>();  //list of enemies in range
    public int attackDamage;
    public bool hasAttacked;

    public GameObject weaponIcon;

    //defense atributes
    public int health;
    public int defenseDamage;
    public int armor;


    
}
