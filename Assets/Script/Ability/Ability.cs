
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Tactical RPG/Character/Ability")]
public class Ability : ScriptableObject
{
    [Header("General Stuff")]
    public string Name;

    public string Description;

    [Header("Ability Stuff")]
    public TextAsset abilityShape;

    public List<ScriptableEffect> effects;

    public int range;

    public int cooldown;

    public int cost;

    public int value;

    public AbilityTypes abilityType;

    public bool includeOrigin;

    public bool requiresTarget;

    public enum AbilityTypes
    {
        Ally,
        Enemy,
        All
    }

    //TODO damage types
}
