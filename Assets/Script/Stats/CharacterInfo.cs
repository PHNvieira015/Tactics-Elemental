using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    // Base Information
    public int CharacterID { get; private set; }
    public string CharacterName { get; private set; }
    public int CharacterLevel { get; private set; }

    // Resources
    public int MaxBaseHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int MaxMana { get; private set; }
    public int CurrentMana { get; private set; }

    // Enums for various character attributes
    public enum CharacterClass { Warrior, Mage, Archer }
    public enum ElementType { Water, Fire, Wind, Earth, Thunder }
    public enum AttackType { Crush, Slash, Pierce, Magic }
    public enum ArmorType { Plate, Mail, Leather, Cloth }
    public enum MovementType { Walking, Running, WaterWalk, LavaWalk, WaterSwimming, LavaSwimming, Levitate, Flying }

    // Main Stats related to the character's abilities
    [SerializeField] private int strength;  // Warrior main stat
    [SerializeField] private int agility;   // Archer main stat
    [SerializeField] private int intellect; // Mage main stat

    //derivative stats
    [SerializeField] private int physicalPower;  //based on main stat and weapon type
    [SerializeField] private int MagicalPower; //based on main stat and weapon type
    [SerializeField] private float Accuracy; //based on main stat

    // Secondary Stats for enhancing character performance
    [SerializeField] private int criticalChance; // Archer bonus
    [SerializeField] private int criticalDamageMultiplier; // Archer bonus
    [SerializeField] private int attackRange; // Range, melee is 1

    // Defense attributes to protect the character
    [SerializeField] private ArmorType armorType; // Armor type
    [SerializeField] private int armorValue;
    [SerializeField] private int physicalDefense;
    [SerializeField] private int magicalDefense;

    // Movement attributes to define character mobility
    [SerializeField] private int weight;
    [SerializeField] private int speed; // Check if different from initiative
    public float initiative; // Check if different from speed
    public float movementRange;

    // Description providing details about the character
    public string characterDescription;

    // Face Direction variable for character orientation
    [SerializeField] private int faceDirection;

    // Getters for main stats to encapsulate private fields
    public int Strength => strength;
    public int Agility => agility;
    public int Intellect => intellect;

    // You can add additional methods to handle character functionality here
}
