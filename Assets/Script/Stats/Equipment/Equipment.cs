using UnityEngine;

public class Equipment : ScriptableObject
{
    [Header("Basic Information")]
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;

    [Header("Item Stats")]
    // Offensive stats
    public int strengthBonus;
    public int agilityBonus;
    public int intellectBonus;

    [Header("Movement Stats")]
    public int WeightBonus;
    public int MovementBonus;

    // Item Class
    public enum ItemClass { Weapon, Armor, Accessory }
    public ItemClass itemClass;

    public int value;


    public enum RequiredCharacterClass { Warrior, Mage, Archer }
    public RequiredCharacterClass requiredClass;
        
    // Function to equip or use the item
    public void Use()
    {
        // Define usage logic, like equipping the item or applying buffs
        Debug.Log($"{itemName} is now equipped!");
    }

    // Unity Editor Validation: Automatically set WeaponType or ArmorType based on the ItemClass

}
