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

    private LevelSystem levelSystem;

    private void Start()
    {
        // Initialize LevelSystem
        levelSystem = new LevelSystem();

        // Set up UI handlers
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

    private void SetExperienceBarSize(float experienceNormalized)
    {
        experienceBarImage.fillAmount = experienceNormalized;
        Debug.Log("Experience bar updated: " + experienceNormalized); // Debug log for experience bar update
    }

    private void SetLevelNumber(int levelNumber)
    {
        levelText.text = "Level\n" + (levelNumber + 1);
        Debug.Log("Level updated: " + (levelNumber + 1)); // Debug log for level update
    }

    public void SetLevelSystem(LevelSystem levelSystem)
    {
        // Ensure the level system is set
        if (levelSystem == null)
        {
            Debug.LogError("LevelSystem is not assigned to LevelWindow.");
            return;
        }

        this.levelSystem = levelSystem;

        // Update initial UI values
        SetLevelNumber(levelSystem.GetLelvelNumber());
        SetExperienceBarSize(levelSystem.GetexperienceNormalized());

        // Subscribe to events
        levelSystem.OnExperinceChanged += LevelSystem_OnExperinceChanged;
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;

        // Debugging the subscription
        Debug.Log("Subscribed to LevelSystem events.");
    }

    private void LevelSystem_OnExperinceChanged(object sender, System.EventArgs e)
    {
        Debug.Log("Experience Changed Event Triggered"); // Check if this event is triggered
        SetExperienceBarSize(levelSystem.GetexperienceNormalized());
    }

    private void LevelSystem_OnLevelChanged(object sender, System.EventArgs e)
    {
        Debug.Log("Level Changed Event Triggered"); // Check if this event is triggered
        SetLevelNumber(levelSystem.GetLelvelNumber());
    }
}
