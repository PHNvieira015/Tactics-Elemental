using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using JetBrains.Annotations;
using TMPro;

public class UI_SkillTree : MonoBehaviour
{
    // Renamed button references to avoid invalid characters (hyphens)
    [SerializeField] private Material skillLockedMaterial;
    [SerializeField] private Material skillUnlockableMaterial;

    [SerializeField] public Button Talent1_1;
    [SerializeField] public Button Talent1_2;
    [SerializeField] public Button Talent2_1;
    [SerializeField] public Button Talent2_2;
    [SerializeField] public Button Talent3_1;
    [SerializeField] public Button Talent3_2;
    [SerializeField] public Button Skill_1;
    [SerializeField] public TMPro.TextMeshProUGUI TalentPointsText;
    private UnitSkills unitSkills;
    [SerializeField] private int TalentPoint;
    [SerializeField] public GameObject Unit;

    private void Start()
    {
        // Add listener to each button for unlocking skills
        UpdateTalentPointsDisplay();
        Talent1_1.onClick.AddListener(OnTalent1_1Clicked);
        Talent1_2.onClick.AddListener(OnTalent1_2Clicked);
        Talent2_1.onClick.AddListener(OnTalent2_1Clicked);
        Talent2_2.onClick.AddListener(OnTalent2_2Clicked);
        Talent3_1.onClick.AddListener(OnTalent3_1Clicked);
        Talent3_2.onClick.AddListener(OnTalent3_2Clicked);

        // Add listener for activating skill
        //Skill_1.onClick.AddListener(OnSkill_1Clicked);
    }

    #region UpdateUI
    private void UpdateTalentPointsDisplay()
    {
        // Find a Text component that shows talent points (you'll need to reference it in your UI)
        TMPro.TextMeshProUGUI talentPointsText = TalentPointsText; // Reference to the UI Text
        if (talentPointsText != null)
        {
            talentPointsText.text = "Talent Points: " + TalentPoint.ToString();
        }
    }
    #endregion

    #region Passive_Talents
    // Handle Talent 1-1 button click (e.g., unlock talent 1_1)
    private void UnlockTalent(UnitSkills.SkillType skillType, Button talentButton)
    {
        if (unitSkills != null)
        {
            // Check if the skill is already unlocked
            if (unitSkills.IsSkillUnlocked(skillType))
            {
                Debug.LogWarning("This talent is already unlocked. No need to spend talent points.");
                return; // Exit the method if the skill is already unlocked
            }

            // Check if there are enough talent points
            if (TalentPoint >= 1)
            {
                TalentPoint--; // Deduct a talent point

                // Try to unlock the skill
                unitSkills.TryUnlockSkill(skillType);

                // Find and destroy the lock icon as a child of the button
                Transform lockIconTransform = talentButton.transform.Find("LockIcon");
                if (lockIconTransform != null)
                {
                    Destroy(lockIconTransform.gameObject);
                }
                else
                {
                    Debug.LogWarning("LockIcon not found as a child of the button.");
                }

                // Update the Talent Points display
                UpdateTalentPointsDisplay();
            }
            else
            {
                Debug.LogWarning("Not enough talent points to unlock this talent.");
            }
        }
        else
        {
            Debug.LogWarning("UnitSkills is not assigned.");
        }
    }

    private void OnTalent1_1Clicked()
    {
        UnlockTalent(UnitSkills.SkillType.Tier1_1, Talent1_1);
    }

    // Handle Talent 1-2 button click
    private void OnTalent1_2Clicked()
    {
        UnlockTalent(UnitSkills.SkillType.Tier1_2, Talent1_2);
    }

    private void OnTalent2_1Clicked()
    {
        // Check if Talent 1-1 is unlocked before allowing investment in Talent 2-1
        if (unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier1_1))
        {
            UnlockTalent(UnitSkills.SkillType.Tier2_1, Talent2_1);
            Debug.Log("Tier2_1 unlocked: " + unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier2_1));
        }
        else
        {
            Debug.Log("Unlock Talent 1-1 first.");
        }
    }

    // Handle Talent 2-2 button click
    private void OnTalent2_2Clicked()
    {
        if (unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier1_1) || unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier1_2))
        {
            UnlockTalent(UnitSkills.SkillType.Tier2_2, Talent2_2);
            Debug.Log("Tier2_2 unlocked: " + unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier2_2));
        }
        else
        {
            Debug.Log("Unlock a tier 1 skill");
        }
    }

    // Handle Talent 3-1 button click
    private void OnTalent3_1Clicked()
    {
        // Check if either Talent 2-1 or Talent 2-2 is unlocked before allowing investment in Talent 3-1
        if (unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier2_1) || unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier2_2))
        {
            UnlockTalent(UnitSkills.SkillType.Tier3_1, Talent3_1);
        }
        else
        {
            Debug.Log("Unlock any Tier2");
        }
    }

    // Handle Talent 3-2 button click
    private void OnTalent3_2Clicked()
    {
        if (unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier2_1) || unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Tier2_2))
        {
            UnlockTalent(UnitSkills.SkillType.Tier3_2, Talent3_2);
        }
        else
        {
            Debug.Log("Unlock any Tier2 before 3-2.");
        }
    }
    #endregion

    // Handle Skill_1 button click (e.g., use Earthshatter skill)
    #region Skills
    private void OnSkill_1Clicked()
    {
        if (unitSkills != null)
        {
            // Implement the skill logic here, for example:
            Debug.Log("Using Earthshatter!");
        }
        else
        {
            Debug.LogWarning("UnitSkills is not assigned.");
        }
    }
    #endregion

    #region Active Skill
    // Method to set the UnitSkills reference
    public void SetUnitSkills(UnitSkills unitSkills)
    {
        this.unitSkills = unitSkills;

        unitSkills.OnSkillUnlocked += UnitSkills_OnSkillUnlocked;

        UpdateVisuals();
    }

    private void UnitSkills_OnSkillUnlocked(object sender, UnitSkills.OnSkillUnlockedEventArgs e)
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Update visuals based on unlocked skills
        if (unitSkills != null)
        {
            UpdateSkillButton(Skill_1, UnitSkills.SkillType.Earthshatter);
        }
    }

    private void UpdateSkillButton(Button skillButton, UnitSkills.SkillType skillType)
    {
        if (unitSkills.IsSkillUnlocked(skillType))
        {
            Image skillButtonImage = skillButton.GetComponent<Image>();
            if (skillButtonImage != null)
            {
                skillButtonImage.material = skillUnlockableMaterial;
            }
        }
        else
        {
            Image skillButtonImage = skillButton.GetComponent<Image>();
            if (skillButtonImage != null)
            {
                skillButtonImage.material = skillLockedMaterial;
            }
        }
    }
    #endregion
}
