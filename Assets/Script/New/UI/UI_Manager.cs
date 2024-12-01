using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    //[SerializeField] private LevelWindow levelWindow;
    [SerializeField] private Unit unit;
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
}
