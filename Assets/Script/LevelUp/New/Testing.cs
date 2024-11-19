using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{

    [SerializeField] private LevelWindow levelWindow;
    [SerializeField] private Unit unit;

    // Start is called before the first frame update
private void Awake()
    {
    LevelSystem levelSystem = new LevelSystem();
        levelWindow.SetLevelSystem(levelSystem);
        unit.SetLevelSystem(levelSystem);


    }

}
