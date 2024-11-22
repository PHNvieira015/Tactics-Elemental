using System;
using UnityEngine;

public class LevelSystem
{
    public event EventHandler OnExperinceChanged;
    public event EventHandler OnLevelChanged;

    private int level;
    private int experience;
    private int experienceToNextLevel;

    public LevelSystem()
    {
        level = 0;
        experience = 0;
        experienceToNextLevel = 100;
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

            // Trigger Level Changed event and log it
            Debug.Log("Level Up! New Level: " + level);
            OnLevelChanged?.Invoke(this, EventArgs.Empty);  // Fire the event
        }

        // Trigger Experience Changed event and log it
        Debug.Log("Experience Changed: " + experience);
        OnExperinceChanged?.Invoke(this, EventArgs.Empty);  // Fire the event
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
