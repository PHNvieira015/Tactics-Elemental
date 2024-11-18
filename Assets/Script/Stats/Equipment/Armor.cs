using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Armor", menuName = "Tactical RPG/Equipment/Armor")]
public class Armor : Equipment
{

    public ItemClass armor;

    [Header("Defensive Stats")]
    [SerializeField] public int maxHealthBonus;
    [SerializeField] public int maxManaBonus;
    [SerializeField] public int ArmorBonus;
    [SerializeField] public int PhysicalDefenseBonus;
    [SerializeField] public int MagicalDefenseBonus;
    public enum ArmorType { Plate, Mail, Leather, Cloth }
    // For armor, this would add defense and categorize the armor
    [SerializeField] public ArmorType armorType;


}
