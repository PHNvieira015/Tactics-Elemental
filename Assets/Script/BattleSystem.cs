using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public Unit attacker;
    public Unit target;

    private void Start()
    {
        // Ensure attackers and targets are set in the Unity Inspector or programmatically.
    }

    private void Attack()
    {
        if (attacker.isAlive && target.isAlive)
        {
            // Perform the attack
            Damage();
         
            Debug.Log($"{attacker.name} attacks {target.name} for {attacker.attackPower} damage!");

            // Check if the target is defeated
            if (!target.IsAlive())
            {
                Debug.Log($"{target.name} has been defeated!");
            }
            else
            {
                Debug.Log($"{target.name} has {target.health} health remaining.");
            }
        }
        else
        {
            Debug.Log("One of the characters is not alive, cannot attack.");
        }
    }

    private void Damage()
    {
        /*
          Attacker:

         WeaponDamageModifier*   //depends on the weapon

         Attackerbasedamage* 
         (Mainstat  //mainstat=strength||aagility||intellect
          +WeaponPower
          +


         AttackerDamage *

         AffinityDamage *

         Defender:


         AffinityResistance

              */
        return;
    }
  
}
