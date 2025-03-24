using UnityEngine;

[CreateAssetMenu(fileName = "NewDamageEffect", menuName = "Tactical RPG/Effects/DamageEffect")]
public class DamageEffect : ScriptableEffect
{
    public override void ApplyEffect(Unit caster, Unit target)
    {
        if (target != null)
        {
            int damage = Value; // Use the Value field from ScriptableEffect
            target.TakeDamage(damage, DamageSystem.Instance);
            Debug.Log($"{caster.unitName} dealt {damage} damage to {target.unitName} using {name}!");
        }
    }
}