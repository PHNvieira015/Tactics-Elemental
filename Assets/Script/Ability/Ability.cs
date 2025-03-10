
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Tactical RPG/Character/Ability")]
public class Ability : ScriptableObject
{
    [Header("General")]
    public string Name;

    public string Description;

    [Header("Stats")]
    public TextAsset abilityShape;

    public List<ScriptableEffect> effects; //Buff or Debuff Effect

    public int range;

    public int cooldown;

    public int cost;

    [Header("Damage")]

    public int value;

    public AttackType attackType;

    public ElementType elementType;

    [Header("types")]
    public TargetType targetType;

    public bool includeOrigin;

    public bool requiresTarget;

    public bool hasAttacked;

    public bool hasMoved;

    public Classrequirements classRequirement;

    public enum TargetType
    {
        Ally,
        Enemy,
        All
    }
        public enum AttackType
    {
        None,
        Crush,
        Slash,
        Pierce,
        Magic
    }
    public enum ElementType
    {
        None,
        Water,
        Fire,
        Wind,
        Earth,
        Thunder
    }


    public enum Classrequirements
    {
        None,
        Warrior,
        Mage,
        Archer,
    }
}
