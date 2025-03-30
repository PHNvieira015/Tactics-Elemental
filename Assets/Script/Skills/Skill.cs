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
    public int value;
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
}