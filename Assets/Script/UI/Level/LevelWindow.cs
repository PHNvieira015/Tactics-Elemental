using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelWindow : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI levelText;
    [SerializeField] public Image experienceBarImage;
    [SerializeField] public Button ButtonUI_5xp;
    [SerializeField] public Button ButtonUI_10xp;
    [SerializeField] public Button ButtonUI_50xp;

    private LevelSystem levelSystem;   // LevelSystem reference
    private Unit unit;  // Reference to the Unit script

    private void Start()
    {
        // Try to find the Unit component attached to the same GameObject or its parent
        unit = GetComponentInParent<Unit>();  // Get from parent or adjust if necessary

        // Ensure the Unit and CharacterStats are properly assigned
        if (unit == null || unit.characterStats == null)
        {
            Debug.LogError("Unit or CharacterStats is missing in LevelWindow.");
            return;
        }

        // Initialize LevelSystem with the characterStats from the Unit
        levelSystem = new LevelSystem(unit.characterStats);

        // Set up the experience button handlers
        ButtonUI_5xp.onClick.AddListener(() =>
        {
            Debug.Log("Button 5XP pressed");
            levelSystem.AddExperience(5); // Add 5 XP when pressed
        });

        ButtonUI_10xp.onClick.AddListener(() =>
        {
            Debug.Log("Button 10XP pressed");
            levelSystem.AddExperience(10); // Add 10 XP when pressed
        });

        ButtonUI_50xp.onClick.AddListener(() =>
        {
            Debug.Log("Button 50XP pressed");
            levelSystem.AddExperience(50); // Add 50 XP when pressed
        });

        // Set the initial level system for the window
        SetLevelSystem(levelSystem);
    }

    // Method to set experience bar size
    private void SetExperienceBarSize(float experienceNormalized)
    {
        experienceBarImage.fillAmount = experienceNormalized;
    }

    // Method to set level number
    private void SetLevelNumber(int levelNumber)
    {
        levelText.text = "Level\n" + (levelNumber + 1); // Display level number (adjust for 1-based index)
    }

    // Set LevelSystem and subscribe to events
    public void SetLevelSystem(LevelSystem levelSystem)
    {
        if (levelSystem == null)
        {
            Debug.LogError("LevelSystem is not assigned to LevelWindow.");
            return;
        }

        this.levelSystem = levelSystem;

        // Update UI with the initial level and experience values
        SetLevelNumber(levelSystem.GetLelvelNumber());
        SetExperienceBarSize(levelSystem.GetexperienceNormalized());

        // Subscribe to events
        levelSystem.OnExperinceChanged += LevelSystem_OnExperinceChanged;
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
    }

    // Event handler when experience changes
    private void LevelSystem_OnExperinceChanged(object sender, System.EventArgs e)
    {
        Debug.Log("Experience Changed Event Triggered");
        SetExperienceBarSize(levelSystem.GetexperienceNormalized());
    }

    // Event handler when level changes
    private void LevelSystem_OnLevelChanged(object sender, System.EventArgs e)
    {
        Debug.Log("Level Changed Event Triggered");
        SetLevelNumber(levelSystem.GetLelvelNumber());
    }
}
