using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] private TurnStateManager turnStateManager; // Reference to TurnStateManager
    [SerializeField] private Unit currentUnit;

    [Header("Basic UI reference")]
    public Transform UI_Health;
    public Transform ActionMenu;
    public Transform SkillListHUD;

    [Header("Skill UI")]
    [SerializeField] private Button[] SkillButtons; // Predefined skill buttons
    [SerializeField] private TMP_Text selectedSkillName; // UI for skill name
    [SerializeField] private TMP_Text selectedSkillDescription; // UI for skill description
    [SerializeField] private TMP_Text selectedSkillDetails; // UI for skill details (range, cost, cooldown)

    private void Start()
    {
        // Ensure turnStateManager is not null
        if (turnStateManager == null)
        {
            turnStateManager = FindObjectOfType<TurnStateManager>();
        }

        // Ensure OnTurnStateChanged is subscribed
        if (turnStateManager != null)
        {
            turnStateManager.OnTurnStateChanged += OnTurnStateChanged;
        }

        // Initialize skill buttons
        InitializeSkillButtons();
    }

    public void DisplayUnitInfo(Unit unit)
    {
        currentUnit = unit;
        UpdateSkillButtons();
    }

    private void OnTurnStateChanged(TurnStateManager.TurnState newState)
    {
        if (turnStateManager.currentUnit != null)
        {
            // Set currentUnit from turnStateManager
            currentUnit = turnStateManager.currentUnit;

            // Only update the UI if the currentUnit is not null
            DisplayUnitInfo(turnStateManager.currentUnit);
        }
        else
        {
            Debug.LogWarning("Current unit is still null in UI_Manager!");
        }
    }

    private void InitializeSkillButtons()
    {
        // Disable all skill buttons initially
        foreach (var button in SkillButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void UpdateSkillButtons()
    {
        // Disable all skill buttons initially
        foreach (var button in SkillButtons)
        {
            button.gameObject.SetActive(false);
        }

        // Check if currentUnit and skillsList are valid
        if (currentUnit == null || currentUnit.skillslist == null || currentUnit.skillslist.Count == 0)
        {
            Debug.Log("No skills to display for the selected unit.");
            return;
        }

        // Update skill buttons based on the current unit's skills list
        for (int i = 0; i < currentUnit.skillslist.Count; i++)
        {
            if (i >= SkillButtons.Length)
            {
                Debug.LogWarning("Not enough skill buttons to display all skills!");
                break;
            }

            Skill skill = currentUnit.skillslist[i];
            Button skillButton = SkillButtons[i];

            // Enable the button and set its text to the skill name
            skillButton.gameObject.SetActive(true);
            TMP_Text buttonText = skillButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = skill.Name;
            }

            // Add a click listener to the button
            skillButton.onClick.RemoveAllListeners(); // Clear existing listeners
            skillButton.onClick.AddListener(() => OnSkillButtonClicked(skill));

            // Add event triggers for mouse enter and exit
            AddEventTriggers(skillButton, skill);
        }
    }

    private void OnSkillButtonClicked(Skill skill)
    {
        // Notify other systems that a skill button was clicked
        Debug.Log($"{currentUnit.unitName} clicked skill: {skill.Name}");
        // You can trigger an event or call a method here to handle the skill logic elsewhere
    }

    private void AddEventTriggers(Button skillButton, Skill skill)
    {
        // Add EventTrigger component if not already present
        EventTrigger trigger = skillButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = skillButton.gameObject.AddComponent<EventTrigger>();
        }

        // Mouse Enter: Show skill details
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnSkillButtonHover(skill); });
        trigger.triggers.Add(entryEnter);

        // Mouse Exit: Clear skill details
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnSkillButtonExit(); });
        trigger.triggers.Add(entryExit);
    }

    private void OnSkillButtonHover(Skill skill)
    {
        // Update the UI with skill details
        if (selectedSkillName != null) selectedSkillName.text = skill.Name;
        if (selectedSkillDescription != null) selectedSkillDescription.text = skill.Description;
        if (selectedSkillDetails != null) selectedSkillDetails.text = $"Range: {skill.range} | Cost: {skill.cost}";
        //if (selectedSkillDetails != null) selectedSkillDetails.text = $"Range: {skill.range} | Cost: {skill.cost} | Cooldown: {skill.cooldown}";
    }

    private void OnSkillButtonExit()
    {
        // Clear the skill details UI
        if (selectedSkillName != null) selectedSkillName.text = "";
        if (selectedSkillDescription != null) selectedSkillDescription.text = "";
        if (selectedSkillDetails != null) selectedSkillDetails.text = "";
    }
}