using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    //[SerializeField] private targetUnit Targettunit;
    [SerializeField] private TurnStateManager turnStateManager; // Reference to TurnStateManager
    [SerializeField] private Unit currentUnit;
    [Header("Basic UI reference")]
    public Transform UI_Health;
    public Transform ActionMenu;
    public Transform SkillListHUD;
    public List<ScriptableObject> SkillList;

    [SerializeField] private Button[] SkillButtons;

    [Header("Skill UI")]
    [SerializeField] private GameObject skillButtonPrefab;
    [SerializeField] private Transform skillButtonsParent;
    [SerializeField] private TMP_Text selectedSkillName;
    [SerializeField] private TMP_Text selectedSkillDescription;
    [SerializeField] private TMP_Text selectedSkillDetails;

    // Start is called before the first frame update
    private void Awake()
    {

    }

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
    }

    public void DisplayUnitInfo(Unit unit)
    {
        currentUnit = unit;
        //Debug.Log("UI_Manager: Unit information updated for " + unit.name);
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

}
