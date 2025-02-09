using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    //[SerializeField] private LevelWindow levelWindow;
    [SerializeField] private Unit Currentunit;
    //[SerializeField] private targetUnit Targettunit;
    public Transform UI_Health;
    public Transform UI_Level;
    public Transform ActionMenu;

    // Start is called before the first frame update
    private void Awake()
    {
        //LevelSystem levelSystem = new LevelSystem();
        //levelWindow.SetLevelSystem(levelSystem);
        //unit.SetLevelSystem(levelSystem);
    }

    private void Start()
    {
        HealthBar healthBar = UI_Health.GetComponent<HealthBar>();
        LevelWindow levelWindow = UI_Level.GetComponent<LevelWindow>();
        

    }
    public void DisplayUnitInfo(Unit unit)
    {
        Debug.Log("UI_Manager: Unit information updated for " + unit.name);
        // Here update your UI elements with properties from 'unit'
        // For example, update the health bar using the unit's current health
        HealthBar healthBar = UI_Health.GetComponent<HealthBar>();
        // healthBar.SetValue(unit.characterStats.currentHealth);
        // Similarly, update the level window and any other info from unit
    }
}
