using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class CharacterStat : MonoBehaviour
{
    #region variables
    [Header("Name/Description")]
    public string CharacterName;
    public string CharacterDescription;
    public enum CharacterClass { None, Warrior, Mage, Archer }
    public CharacterClass characterClass;

    // References to other ScriptableObjects
    public LevelUpSystem levelUp;

    // **Explicit Equipment Slots**
    [Header("Equipemnt Settings")]
    public Weapon equippedWeapon;  // 1 Weapon slot
    public Armor equippedArmor;    // 1 Armor slot
    public Accessory accessory1;   // 1st accessory slot
    public Accessory accessory2;   // 2nd accessory slot

       // Character Stats
    [Header("LevelUp Stats")]
    public int CharacterLevel;
    public int experience;
    public int requiredExperience;

    // Resources
    [Header("Resources")]
    public int maxBaseHealth;
    public int currentHealth;
    public int maxMana;
    public int currentMana;

    // Editable stats (in Inspector)
    [Header("Damage Stats")]
    public int strength;
    public int agility;
    public int intellect;
    public AttackType PrimaryAttackType;
    public int attackRangeBonus;

    // Defensive stats (Editable)
    [Header("Defensive Stats")]
    public int armorValue;
    public int physicalDefense;
    public int magicalDefense;

    // Define element type and weakness
    public enum ElementType { None,Water, Fire, Wind, Earth, Thunder }
    public ElementType elementType; // The element type of the unit
    [HideInInspector] public ElementType weaknessElement; // Weakness based on the element type

    public enum ArmorType { None, Plate, Mail, Leather, Cloth }
    [HideInInspector] public ArmorType armorType;

    // Player direction
    [SerializeField]public enum Direction
    {
        UpRight,    // North
        UpLeft,     // West 
        DownRight,   // East
        DownLeft     // South
    }
    public Direction faceDirection; //define with movement

    // Movement stats (Editable)
    [Header("Movement/Initiative")]
    public int weight;
    public float initiative;
    [HideInInspector] public float RoundInitiative;
    public float movementRange;

 

    #endregion


        public void Initialize()
    {
        // Initialize health and mana
        currentHealth = maxBaseHealth;
        currentMana = maxMana;

        SetCharacterAttackRange(attackRangeBonus);

        // Optional method to calculate armor value (example)
        CalculateArmorValue();

        // Initialize equipment slots and log debug messages
        if (equippedWeapon != null)
        {
            Debug.Log("Equipped weapon: " + equippedWeapon.name);
        }
        else
        {
            Debug.Log("No weapon equipped.");
        }

        if (equippedArmor != null)
        {
            Debug.Log("Equipped armor: " + equippedArmor.name);
        }
        else
        {
            Debug.Log("No armor equipped.");
        }

        if (accessory1 != null)
        {
            Debug.Log("Equipped accessory 1: " + accessory1.name);
        }
        else
        {
            Debug.Log("No accessory 1 equipped.");
        }

        if (accessory2 != null)
        {
            Debug.Log("Equipped accessory 2: " + accessory2.name);
        }
        else
        {
            Debug.Log("No accessory 2 equipped.");
        }

    }
    #region methods
    // Set Weakness based on Element Type
    public void SetWeakness()
    {
        switch (elementType)
        {
            case ElementType.None:
                
                break;
            case ElementType.Water:
                weaknessElement = ElementType.Thunder; // Water is weak to Thunder
                break;
            case ElementType.Fire:
                weaknessElement = ElementType.Water; // Fire is weak to Water
                break;
            case ElementType.Wind:
                weaknessElement = ElementType.Earth; // Wind is weak to Earth
                break;
            case ElementType.Earth:
                weaknessElement = ElementType.Wind; // Earth is weak to Wind
                break;
            case ElementType.Thunder:
                weaknessElement = ElementType.Fire; // Thunder is weak to Fire
                break;
        }
    }
    public void SetCharacterAttackRange(int AttackRange)
    {
        this.attackRangeBonus = AttackRange+ equippedWeapon.WeaponRange;
        if (this.attackRangeBonus == 0)
        {
            attackRangeBonus += 1;
        }
    }

    // Set Character Level
    public void SetCharacterLevel(int newLevel)
    {
        CharacterLevel = newLevel;
    }

    // Set Max Health and Initialize
    public void SetMaxHealth(int maxHealth)
    {
        maxBaseHealth = maxHealth;
        currentHealth = maxBaseHealth; // Set initial health to max health
    }

    // Set Max Mana and Initialize
    public void SetMaxMana(int maxMana)
    {
        currentMana = maxMana; // Set initial mana to max mana
    }

    // Take damage (health)
    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0); // Prevent going below zero
    }

    // Restore health
    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxBaseHealth); // Prevent exceeding max health
    }

    // Use mana
    public void UseMana(int amount)
    {
        currentMana = Mathf.Max(currentMana - amount, 0); // Prevent going below zero
    }

    // Restore mana
    public void RestoreMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana); // Prevent exceeding max mana
    }

    // Initialization method


    // Optional method to calculate armor value
    public void CalculateArmorValue()
    {
        armorValue = physicalDefense * 2; // Example calculation
    }

    // Equip Weapon Method
    public void EquipWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
        Debug.Log("Weapon equipped: " + weapon.name);
    }

    // Equip Armor Method
    public void EquipArmor(Armor armor)
    {
        equippedArmor = armor;
        Debug.Log("Armor equipped: " + armor.name);
    }

    // Equip Accessory 1 Method
    public void EquipAccessory1(Accessory accessory)
    {
        accessory1 = accessory;
        Debug.Log("Accessory 1 equipped: " + accessory1.name);
    }

    // Equip Accessory 2 Method
    public void EquipAccessory2(Accessory accessory)
    {
        accessory2 = accessory;
        Debug.Log("Accessory 2 equipped: " + accessory2.name);
    }

    // Unequip Weapon Method
    public void UnequipWeapon()
    {
        equippedWeapon = null;
        Debug.Log("Weapon unequipped.");
    }

    // Unequip Armor Method
    public void UnequipArmor()
    {
        equippedArmor = null;
        Debug.Log("Armor unequipped.");
    }

    // Unequip Accessory 1 Method
    public void UnequipAccessory1()
    {
        accessory1 = null;
        Debug.Log("Accessory 1 unequipped.");
    }

    // Unequip Accessory 2 Method
    public void UnequipAccessory2()
    {
        accessory2 = null;
        Debug.Log("Accessory 2 unequipped.");
    }
    public void SetRoundInitiative()
    {
        RoundInitiative = initiative;  // Set the RoundInitiative to initiative value.
    }
    public void EndTurnRoundInitiative()
    {
        RoundInitiative=RoundInitiative/100;  // Divide RountInitiative by 100
    }


    public void CalculateBasicStats()
    {
        // Base stats based on class
        switch (characterClass)
        {
            case CharacterClass.Warrior:
                strength = 10 + (CharacterLevel * 3);  // Base strength + scaling with level
                agility = 5 + (CharacterLevel * 0);   // Base agility + scaling with level
                intellect = 3 + (CharacterLevel * 0); // Base intellect + no scaling for Warrior
                maxBaseHealth = 150 + (CharacterLevel * 10);  // Base health + scaling with level
                maxMana = 50 + (CharacterLevel * 4);   // Base mana + scaling with level for Warrior
                break;

            case CharacterClass.Mage:
                strength = 3 + (CharacterLevel * 0);   // Lower strength for Mage
                agility = 5 + (CharacterLevel * 0);    // Agility scaling for Mage
                intellect = 15 + (CharacterLevel * 3); // Higher intellect for Mage
                maxBaseHealth = 80 + (CharacterLevel * 4);  // Lower health scaling for Mage
                maxMana = 100 + (CharacterLevel * 10);  // Higher mana scaling for Mage
                break;

            case CharacterClass.Archer:
                strength = 7 + (CharacterLevel * 0);   // Base strength for Archer
                agility = 12 + (CharacterLevel * 3);   // Higher agility scaling for Archer
                intellect = 4 + (CharacterLevel * 0);  // Lower intellect for Archer
                maxBaseHealth = 100 + (CharacterLevel * 6); // Medium health scaling for Archer
                maxMana = 60 + (CharacterLevel * 6);  // Medium mana scaling for Archer
                break;

            default:
                strength = 5 + (CharacterLevel * 1);   // Default stat for unassigned class
                agility = 5 + (CharacterLevel * 1);
                intellect = 5 + (CharacterLevel * 1);
                maxBaseHealth = 70 + (CharacterLevel * 5);
                maxMana = 60 + (CharacterLevel * 5);
                break;
        }

        // After calculation, make sure to initialize health and mana
        currentHealth = maxBaseHealth;
        currentMana = maxMana;

        Debug.Log($"Calculated Stats for {CharacterName}: Strength = {strength}, Agility = {agility}, Intellect = {intellect}, Health = {currentHealth}, Mana = {currentMana}");
    }
    private CharacterStat.ElementType ConvertSkillElementToCharacterStatElement(Skill.ElementType skillElement)
    {
        switch (skillElement)
        {
            case Skill.ElementType.None:
                return CharacterStat.ElementType.None;
            case Skill.ElementType.Water:
                return CharacterStat.ElementType.Water;
            case Skill.ElementType.Fire:
                return CharacterStat.ElementType.Fire;
            case Skill.ElementType.Wind:
                return CharacterStat.ElementType.Wind;
            case Skill.ElementType.Earth:
                return CharacterStat.ElementType.Earth;
            case Skill.ElementType.Thunder:
                return CharacterStat.ElementType.Thunder;
            default:
                Debug.LogWarning($"Unknown Skill.ElementType: {skillElement}");
                return CharacterStat.ElementType.None;
        }
    }
}
#endregion
