using UnityEngine;

[CreateAssetMenu(fileName = "NewHealEffect", menuName = "Tactical RPG/Effects/HealEffect")]
public class HealEffect : ScriptableEffect
{
    public override void ApplyEffect(Unit caster, Unit target)
    {
        if (target != null)
        {
            int healAmount = Value; // Use the Value field from ScriptableEffect
            target.characterStats.currentHealth += healAmount;
            Debug.Log($"{caster.unitName} healed {target.unitName} for {healAmount} HP using {name}!");
        }
    }
}