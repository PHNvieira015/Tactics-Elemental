using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkilltreeSystem : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private UI_SkillTree uiSkillTree;


    private void Start()
    {
        uiSkillTree.SetUnitSkills(unit.GetUnitSkills());
    }

}
