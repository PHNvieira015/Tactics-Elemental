using UnityEngine;

[CreateAssetMenu(fileName = "NewBuff", menuName = "Tactical RPG/Status/Buff")]
public class Buff : ScriptableObject
{
    public string buffName;      // Name of the buff
    public int duration;         // Duration in turns
    public int effectAmount;     // Amount of the effect (e.g., healing, damage increase)

    // Optionally, you can provide an initialization method to set default values.
    public void Initialize(string name, int dur, int effect)
    {
        buffName = name;
        duration = dur;
        effectAmount = effect;
    }

    // Add more logic specific to how buffs work in your game
}
