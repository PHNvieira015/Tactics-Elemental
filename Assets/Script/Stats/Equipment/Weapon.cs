using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Tactical RPG/Equipment/Weapon")]
public class Weapon : Equipment
{


    [Header("Offensive Stats")]
    public ItemClass weapon;
    public float WeaponDamageModifier;
    public int WeaponRange;

    // Weapon and Armor Types
    public enum WeaponType { Sword, Bow, Staff, Axe }

    // For weapons, this would add attack power
    public WeaponType weaponType;
    public enum AttackType { Crush, Slash, Pierce, Magic }
    [HideInInspector] public AttackType PrimaryAttackType;

    public void SetPrimaryAttackType()
    {
        switch (weaponType)
        {
            case WeaponType.Sword:
                PrimaryAttackType = AttackType.Slash; // Sword = Slash
                break;
            case WeaponType.Bow:
                PrimaryAttackType = AttackType.Pierce; // Bow = Pierce
                break;
            case WeaponType.Staff:
                PrimaryAttackType = AttackType.Magic; // Staff = Magic
                break;
            case WeaponType.Axe:
                PrimaryAttackType = AttackType.Slash; // Axe = Slash
                break;
            default:
                PrimaryAttackType = AttackType.Slash; // Default to Slash if no match
                break;
        }
    }

    // Optional: Call this method during initialization or whenever the weapon is equipped
    private void OnValidate()
    {
        // Ensure PrimaryAttackType is set when you change weaponType in the editor
        SetPrimaryAttackType();
    }


}
