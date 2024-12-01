using System;
using UnityEngine;

public class LevelSystem
{
    public event EventHandler OnExperinceChanged;
    public event EventHandler OnLevelChanged;

    private int level;
    private int experience;
    private int experienceToNextLevel;
    public CharacterStat characterStats;

    public LevelSystem(CharacterStat characterStat)
    {
        if (characterStat == null)
        {
            Debug.LogError("CharacterStat is not assigned!");
            return;
        }

        characterStats = characterStat;
        level = characterStats.CharacterLevel;
        experience = characterStats.experience;
        experienceToNextLevel = characterStats.requiredExperience;
    }

    // Method to add experience and check for level up
    // LevelSystem.cs
    public void AddExperience(int amount)
    {
        experience += amount;

        // Check if experience has surpassed the required threshold to level up
        if (experience >= experienceToNextLevel)
        {
            level++;
            experience -= experienceToNextLevel;
            characterStats.SetCharacterLevel(level);
            characterStats.experience = experience;  // Update the character's experience
            characterStats.requiredExperience = CalculateRequiredExperience(level);  // Adjust required experience for the next level

            // Trigger Level Changed event and log it
            Debug.Log("Level Up! New Level: " + level);
            OnLevelChanged?.Invoke(this, EventArgs.Empty);  // Fire the event
        }

        // Trigger Experience Changed event and log it
        Debug.Log("Experience Changed: " + experience);
        OnExperinceChanged?.Invoke(this, EventArgs.Empty);  // Fire the event
    }

    private int CalculateRequiredExperience (int currentLevel)
    {
        return currentLevel* 20;
    }
    public int GetLelvelNumber()
    {
        return level;
    }

    public float GetexperienceNormalized()
    {
        return (float)experience / experienceToNextLevel;
    }
}
