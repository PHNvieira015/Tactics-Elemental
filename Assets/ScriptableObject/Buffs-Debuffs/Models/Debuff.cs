using UnityEngine;

[CreateAssetMenu(fileName = "NewDebuff", menuName = "Tactical RPG/Status/Debuff")]
public class Debuff : ScriptableObject
{
    public string debuffName; // Name of the buff
    public int duration;    // How long the buff lasts (in turns or time)
    public int effectAmount; // Amount of the effect (e.g., healing or damage increase)

    // Constructor for Buff (optional, can be set in the Inspector)
    public void Initialize(string name, int dur, int effect)
    {
        debuffName = name;
        duration = dur;
        effectAmount = effect;

    }

    // Add more logic specific to how buffs work in your game
}
