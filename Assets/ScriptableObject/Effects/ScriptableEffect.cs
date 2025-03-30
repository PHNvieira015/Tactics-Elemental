using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Effect")]
public class ScriptableEffect : ScriptableObject
{
    public enum StatType
    {
        maxBaseHealth,
        currentHealth,
        maxMana,
        currentMana,
        Strength,
        Agility,
        Intellect,
        PhysicalDefense,
        MagicalDefense,
        MovementRange,
        AttackRange
    }

    [Header("Visuals")]
    public string effectName;
    public Sprite icon;
    public GameObject vfxPrefab;

    [Header("Duration")]
    public int duration = 1;

    [Header("Stat Modification")]
    public StatType statToModify; // The actual field storing which stat to modify
    public float modifierValue;

    [Header("Damage Over Time")]
    public int damagePerTurn;
    public CharacterStat.ElementType elementType;
}