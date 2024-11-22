using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

[CreateAssetMenu(fileName = "New Character Stats", menuName = "Tactical RPG/Character Stats")]
public class CharacterStat : ScriptableObject
{
    public string CharacterName;
    public string CharacterDescription;
    public enum CharacterClass { Warrior, Mage, Archer }
    public CharacterClass characterClass;

    // References to other ScriptableObjects
    public LevelUpSystem levelUp;

    // **Explicit Equipment Slots**
    public Weapon equippedWeapon;  // 1 Weapon slot
    public Armor equippedArmor;    // 1 Armor slot
    public Accessory accessory1;   // 1st accessory slot
    public Accessory accessory2;   // 2nd accessory slot

    // Resources
    public int maxBaseHealth = 100;
    public int currentHealth;
    public int maxMana = 100;
    public int currentMana;

    // Editable stats (in Inspector)
    public int strength;
    public int agility;
    public int intellect;
    [HideInInspector] public AttackType PrimaryAttackType;

    // Defensive stats (Editable)
    public int armorValue;
    public int physicalDefense;
    public int magicalDefense;

    // Define element type and weakness
    public enum ElementType { Water, Fire, Wind, Earth, Thunder }
    public ElementType elementType; // The element type of the unit
    [HideInInspector] public ElementType weaknessElement; // Weakness based on the element type

    public enum ArmorType { Plate, Mail, Leather, Cloth }
    [HideInInspector] public ArmorType armorType;

    // Player direction
    public enum Direction { North, East, South, West }
    [HideInInspector] public Direction faceDirection; //define with movement

    // Movement stats (Editable)
    public int weight;
    public int initiative;
    public float movementRange;

    // Character Stats
    public int CharacterLevel;
    public int experience;
    public int requiredExperience;

    // Set Weakness based on Element Type
    public void SetWeakness()
    {
        switch (elementType)
        {
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
    public void Initialize()
    {
        // Initialize health and mana
        currentHealth = maxBaseHealth;
        currentMana = maxMana;

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

        // Calculate Movement Range
        CalculateMovementRange();
    }

    // Optional method to calculate armor value
    public void CalculateArmorValue()
    {
        armorValue = physicalDefense * 2; // Example calculation
    }

    // Calculate Movement Range
    public void CalculateMovementRange()
    {
        movementRange = initiative * 0.5f;  // Example calculation
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
}
