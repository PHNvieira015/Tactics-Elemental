using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Up System", menuName = "Tactical RPG/Systems/Level Up")]
public class LevelUpSystem : ScriptableObject
{
    // Method to level up the character
    public void CharacterLevelUp(CharacterStat characterStats)
    {
        if (characterStats == null)
        {
            Debug.LogError("CharacterStats is null.");
            return;
        }

        // Update character level
        characterStats.CharacterLevel += 1;

        // Update stats based on class
        switch (characterStats.characterClass)
        {
            case CharacterStat.CharacterClass.Warrior:
                characterStats.strength += 3; // Warrior gets more strength per level
                characterStats.maxBaseHealth += GetRandomStatIncrease(5, 10); // More health for warriors
                break;

            case CharacterStat.CharacterClass.Mage:
                characterStats.intellect += GetRandomStatIncrease(3, 6); // Mage gets more intellect per level
                characterStats.maxMana += GetRandomStatIncrease(3, 6); // More mana for mages
                break;

            case CharacterStat.CharacterClass.Archer:
                characterStats.agility += GetRandomStatIncrease(3, 6); // Archer gets more agility per level
                break;
        }

        // Generic stat increases for all classes
        characterStats.strength += GetRandomStatIncrease(1, 3);
        characterStats.agility += GetRandomStatIncrease(1, 3);
        characterStats.intellect += GetRandomStatIncrease(1, 3);

        // Increase health and mana
        characterStats.maxBaseHealth += GetRandomStatIncrease(15, 20);
        characterStats.maxMana += GetRandomStatIncrease(1, 3);

        // Log the level-up
        Debug.Log($"{characterStats.CharacterName} leveled up to level {characterStats.CharacterLevel}!");
    }

    // Helper method to get a random value between a min and max range
    private int GetRandomStatIncrease(int min, int max)
    {
        return Random.Range(min, max);
    }
}
