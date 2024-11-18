using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ScriptableEffects can be attached to both tiles and abilities. 
[CreateAssetMenu(fileName = "ScriptableEffect", menuName = "Tactical RPG/ScriptableEffect")]
public class ScriptableEffect : ScriptableObject
{
    //public Stats statKey;
    //public Operation Operator;
    public float Duration;
    public int Value;

    //public Stats GetStatKey()
    //{
    //    return statKey;
    //}
}
