using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    //[SerializeField] private LevelWindow levelWindow;
    [SerializeField] private Unit unit;
    public Transform UI_Health;
    public Transform UI_Level;

    // Start is called before the first frame update
    private void Awake()
    {
        //LevelSystem levelSystem = new LevelSystem();
        //levelWindow.SetLevelSystem(levelSystem);
        //unit.SetLevelSystem(levelSystem);


    }
    private void Start()
    {
        //Instantiate(UI_Health, new Vector3(0, 10), Quaternion.identity);
        HealthBar healthBar = UI_Health.GetComponent<HealthBar>();
        //Instantiate(UI_Level, new Vector3(0, 10), Quaternion.identity);
        LevelWindow levelWindow = UI_Level.GetComponent<LevelWindow>();
    }
}
