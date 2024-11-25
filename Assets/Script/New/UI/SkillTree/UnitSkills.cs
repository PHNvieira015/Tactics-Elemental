using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSkills
{
    // Define an event that can be used to notify skill unlocking
    public event EventHandler<OnSkillUnlockedEventArgs> OnSkillUnlocked;

    // Event args class to pass the skill type that was unlocked
    public class OnSkillUnlockedEventArgs : EventArgs
    {
        public SkillType skillType;
    }

    // Enum to define all possible skills
    public enum SkillType
    {
        None,
        Earthshatter,
        Whirlwind,
        MoveSpeed_1,
        MoveSpeed_2,
        HealthMax_1,
        HealthMax_2,
        // Add more skills here as needed
    }

    // List to store the unlocked skills
    private List<SkillType> unlockedSkillTypeList;

    // Constructor to initialize the unlocked skills list
    public UnitSkills()
    {
        unlockedSkillTypeList = new List<SkillType>();
    }

    // Method to unlock a skill
    public void UnlockSkill(SkillType skilltype)
    {
        if (!IsSkillUnlocked(skilltype))
        {
            unlockedSkillTypeList.Add(skilltype);
            Debug.Log("Skill unlocked: " + skilltype.ToString());

            // Trigger the event that the skill has been unlocked
            OnSkillUnlocked?.Invoke(this, new OnSkillUnlockedEventArgs { skillType = skilltype });
        }
        else
        {
            Debug.Log("Skill " + skilltype.ToString() + " is already unlocked.");
        }
    }

    // Method to check if a skill is unlocked
    public bool IsSkillUnlocked(SkillType skilltype)
    {
        return unlockedSkillTypeList.Contains(skilltype);
    }

    // Method to activate the skill (only if it's unlocked)
    public void ActivateTalent(SkillType skilltype)
    {
        if (IsSkillUnlocked(skilltype))
        {
            // Logic for activating the skill (e.g., triggering effects)
            Debug.Log("Activating talent: " + skilltype.ToString());
            // Implement the actual effect of the talent here (e.g., applying buffs or abilities)
        }
        else
        {
            Debug.Log("Skill " + skilltype.ToString() + " is not unlocked yet.");
        }
    }

    // Method to get skill requirements
    public SkillType GetSkillRequirement(SkillType skilltype)
    {
        switch (skilltype)
        {
            case SkillType.HealthMax_2: return SkillType.HealthMax_1;
            case SkillType.MoveSpeed_2: return SkillType.MoveSpeed_1;
            case SkillType.Whirlwind: return SkillType.Earthshatter;
            default: return SkillType.None; // Return SkillType.None if no requirement
        }
    }

    // Method to try unlocking a skill based on its requirements
    public bool TryUnlockSkill(SkillType skillType)
    {
        SkillType skillRequirement = GetSkillRequirement(skillType);
        if (skillRequirement != SkillType.None) // Fixed missing parentheses
        {
            if (IsSkillUnlocked(skillRequirement))
            {
                UnlockSkill(skillType);
                return true; // Skill successfully unlocked
            }
            else
            {
                return false; // Skill cannot be unlocked, requirement not met
            }
        }
        else
        {
            UnlockSkill(skillType); // Unlock skill without any requirement
            return true; // Skill successfully unlocked
        }
    }

}
