using System;
using System.Collections;
using System.Collections.Generic;
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

    public void AddExperience(int amount)
    {
        experience += amount;
        if (experience >= experienceToNextLevel)
            //Enough experience to level up
        {
            level++;
            experience -= experienceToNextLevel;
            if(OnLevelChanged !=null) OnLevelChanged(this, new EventArgs());
        }
        if(OnExperinceChanged != null) OnExperinceChanged(this, new EventArgs());
    }
    public int GetLelvelNumber()
    {
        return level;
    }
    public float GetexperienceNormalized()
    {
        return (float) experience / experienceToNextLevel;
    }



}
