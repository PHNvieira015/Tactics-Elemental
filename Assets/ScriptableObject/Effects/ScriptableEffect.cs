using UnityEngine;

// ScriptableEffects can be attached to both tiles and abilities.
[CreateAssetMenu(fileName = "ScriptableEffect", menuName = "Tactical RPG/ScriptableEffect")]
public class ScriptableEffect : ScriptableObject
{
    public float Duration; // How long the effect lasts
    public int Value; // The magnitude of the effect (e.g., damage amount, heal amount)

    // Apply the effect to a target
    public virtual void ApplyEffect(Unit caster, Unit target)
    {
        Debug.Log($"Applying effect {name} to {target?.unitName ?? "unknown target"}");
        // Base implementation does nothing. Override this in derived classes.
    }
}