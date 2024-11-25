using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UI_SkillTree : MonoBehaviour
{
    // Renamed button references to avoid invalid characters (hyphens)
    [SerializeField] public Button Talent1_1;
    [SerializeField] public Button Talent1_2;
    [SerializeField] public Button Talent2_1;
    [SerializeField] public Button Talent2_2;
    [SerializeField] public Button Skill1;
    private UnitSkills unitSkills;

    private void Start()
    {
        // Add listener to each button for unlocking skills
        Talent1_1.onClick.AddListener(OnTalent1_1Clicked);
        Talent1_2.onClick.AddListener(OnTalent1_2Clicked);
        Talent2_1.onClick.AddListener(OnTalent2_1Clicked);
        Talent2_2.onClick.AddListener(OnTalent2_2Clicked);


        // Add listener for activating skill
        Skill1.onClick.AddListener(OnSkill1Clicked);
    }

    // Handle Talent 1-1 button click (e.g., unlock Earthshatter skill)
    private void OnTalent1_1Clicked()
    {
        if (unitSkills != null)
        {
            unitSkills.TryUnlockSkill(UnitSkills.SkillType.Earthshatter);
        }
        else
        {
            Debug.LogWarning("UnitSkills is not assigned.");
        }
    }

    // Handle Talent 1-2 button click
    private void OnTalent1_2Clicked()
    {
        if (unitSkills != null)
        {
            unitSkills.TryUnlockSkill(UnitSkills.SkillType.Earthshatter);
        }
        else
        {
            Debug.LogWarning("UnitSkills is not assigned.");
        }
    }

    // Handle Talent 2-1 button click
    private void OnTalent2_1Clicked()
    {
        if (unitSkills != null)
        {
            unitSkills.TryUnlockSkill(UnitSkills.SkillType.Earthshatter);
        }
        else
        {
            Debug.LogWarning("UnitSkills is not assigned.");
        }
    }

    // Handle Talent 2-2 button click
    private void OnTalent2_2Clicked()
    {
        if (unitSkills != null)
        {
            unitSkills.TryUnlockSkill(UnitSkills.SkillType.Earthshatter);
        }
        else
        {
            Debug.LogWarning("UnitSkills is not assigned.");
        }
    }

    // Handle Skill1 button click (e.g., use Earthshatter skill)
    private void OnSkill1Clicked()
    {
        if (unitSkills != null)
        {
         unitSkills.TryUnlockSkill(UnitSkills.SkillType.Earthshatter);
        }
        else
        {
            Debug.LogWarning("UnitSkills is not assigned.");
        }
    }

    // Method to set the UnitSkills reference
    public void SetUnitSkills(UnitSkills unitSkills)
    {
        this.unitSkills = unitSkills;
    }
}
