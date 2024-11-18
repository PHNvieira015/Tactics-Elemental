using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceManager : MonoBehaviour
{
    [Header("Experience")]
    [SerializeField] AnimationCurve experienceCurve;

    int currentLevel, totalExperience;
    int previousLevelsExperience, nextLevelsExperience;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI experienceText;
    [SerializeField] Image experienceFill;

    void Start()
    {
        UpdateLevel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Simulate adding experience with the mouse click
        {
            AddExperience(5);  // Add 5 experience points for each click
        }
    }

    public void AddExperience(int amount)
    {
        totalExperience += amount;
        CheckForLevelUp();
        UpdateInterface();  // Update UI after adding experience
    }

    void CheckForLevelUp()
    {
        // Check if the player has gained enough experience to level up
        if (totalExperience >= nextLevelsExperience)
        {
            currentLevel++;  // Level up the player
            UpdateLevel();    // Update the experience values and thresholds for the new level
        }
    }

    void UpdateLevel()
    {
        // Calculate the experience required for the current level and the next level
        previousLevelsExperience = (int)experienceCurve.Evaluate(currentLevel);
        nextLevelsExperience = (int)experienceCurve.Evaluate(currentLevel + 1);

        // Debugging: Log the experience values to ensure they are being set correctly
        Debug.Log($"Level {currentLevel}: Previous XP = {previousLevelsExperience}, Next XP = {nextLevelsExperience}");

        UpdateInterface();  // Update the interface after updating the level
    }

    void UpdateInterface()
    {


        // Calculate experience progress for the current level
        int start = totalExperience - previousLevelsExperience;  // Experience earned this level
        int end = nextLevelsExperience - previousLevelsExperience;  // Total experience needed for the next level


        // Update the UI elements
        levelText.text = currentLevel.ToString();  // Display current level
        if (end <0)
        {
            end = 2;
        }
        if(start<0)
        {
            start = 1;
        }
        experienceText.text = start + " exp / " + end + " exp";  // Display current and required experience
        experienceFill.fillAmount = (float)start / (float)end;  // Update the progress bar based on current progress
        // Debugging: Log the start and end values
        Debug.Log($"Start XP = {start}, End XP = {end}");

    }
}
