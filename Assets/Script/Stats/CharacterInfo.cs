using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    public OverlayTile standingOnTile;

    public bool selected;
    private GameMaster gm;

    //movement atributes
    public float initiative;
    public float MoveSpeed;
    public bool hasMoved;
    public int playerNumber;
    private List<CharacterInfo> enemiesInRange = new List<CharacterInfo>();  //list of enemies in range

    //Attack atributes
    public int attackRange;
    public int attackDamage;
    public bool hasAttacked;

    public GameObject weaponIcon;

    //defense atributes
    public int health;
    public int Defense;
    public int MagicDefense;

    //Magical Damage Types
    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Poison
    }
}
