using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Acessory", menuName = "Tactical RPG/Equipment/Acessory")]
public class Accessory : Equipment
{

    public ItemClass acessory;

    [Header("Defensive Stats")]
    [SerializeField] public int maxHealthBonus;
    [SerializeField] public int maxManaBonus;
    [SerializeField] public int ArmorBonus;
    [SerializeField] public int PhysicalDefenseBonus;
    [SerializeField] public int MagicalDefenseBonus;
}
