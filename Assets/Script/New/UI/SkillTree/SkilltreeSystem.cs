using UnityEngine;

public class SkilltreeSystem : MonoBehaviour
{
    [SerializeField] private UI_SkillTree uiSkillTree;
    private Unit unit;

    private void Start()
    {
        unit = FindActiveUnit();  // Call a method to find the active unit

        // If no unit is selected, just return and do nothing
        if (unit == null)
        {
            return;
        }

        // Set the unit's skills in the UI
        uiSkillTree.SetUnitSkills(unit.GetUnitSkills());
    }

    private Unit FindActiveUnit()
    {
        // Check for a unit with selected == true
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (u.selected)
            {
                return u;
            }
        }

        return null;  // Return null if no active unit is found
    }
}
