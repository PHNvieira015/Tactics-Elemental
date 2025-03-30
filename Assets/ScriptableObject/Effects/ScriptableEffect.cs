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

    public enum DamageCalculationType
    {
        FlatValue,
        TargetPercentage,
        CasterStatPercentage
    }

    [Header("Visuals")]
    public string effectName;
    public Sprite icon;
    public GameObject vfxPrefab;

    [Header("Duration")]
    public int duration = 1;

    [Header("Stat Modification")]
    public StatType statToModify;
    public float modifierValue;

    [Header("Damage Over Time")]
    public DamageCalculationType damageCalculationType = DamageCalculationType.FlatValue;
    public int damagePerTurn; // Used for FlatValue
    [Tooltip("Percentage (100 = 100%)")]
    [Range(0, 300)]
    public float percentageDamage; // Now shows as 0-300 in inspector (meaning 0%-300%)
    public StatType casterStatToUse; // Used for CasterStatPercentage
    public CharacterStat.ElementType elementType;
}