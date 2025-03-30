using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Tactical RPG/Character/Ability")]
public class Skill : ScriptableObject
{
    [Header("General")]
    public string Name;
    public string Description;

    [Header("Stats")]
    public TextAsset abilityShape;
    public int range;
    public int cooldown;
    public int cost;

    [Header("Effects")]
    public List<ScriptableEffect> effects = new List<ScriptableEffect>();

    [Header("Damage")]
    public int baseDamage; // Fixed base damage

    [Tooltip("Damage as percentage of caster's stat (1.0 = 100%)")]
    [Range(0f, 3f)]
    public float statScalingPercentage = 100f; // 0 means no scaling

    public StatType scalingStat = StatType.Strength; // Which stat to scale with

    [Tooltip("Damage as percentage of target's max health (0.1 = 10%)")]
    [Range(0f, 1f)]
    public float targetHPScalingPercentage = 0f; // 0 means no scaling

    public AttackType attackType;
    public ElementType elementType;

    [Header("Targeting")]
    public TargetType targetType = TargetType.Enemy;
    public bool includeOrigin;
    public bool requiresTarget;
    public bool hasAttacked;
    public bool hasMoved;
    public ClassRequirements classRequirement;

    public enum TargetType { None, Self, Ally, Enemy, AnyUnit, Ground, Area }
    public enum AttackType { None, Crush, Slash, Pierce, Magic }
    public enum ElementType { None, Water, Fire, Wind, Earth, Thunder }
    public enum ClassRequirements { None, Warrior, Mage, Archer }
    public enum StatType { Strength, Agility, Intellect }
}