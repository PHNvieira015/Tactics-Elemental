using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public BattleHandler battleHandler; // Reference to BattleHandler for handling battle logic

    public void Attack(Unit attacker, Unit target)
    {
        if (!IsWithinAttackRange(attacker, target))
        {
            Debug.Log($"{target.name} is out of attack range!");
            return;
        }

        if (attacker.IsAlive() && target.IsAlive())
        {
            int damageDealt = CalculateDamage(attacker, target);

            // Apply the damage to the target
            target.TakeDamage(damageDealt);

            Debug.Log($"{attacker.name} attacked {target.name} and dealt {damageDealt} damage!");

            if (!target.IsAlive())
            {
                Debug.Log($"{target.name} has been defeated!");
                // You might want to add death handling logic here
                target.gameObject.SetActive(false); // Or handle death in another way
            }
            else
            {
                Debug.Log($"{target.name} has {target.characterStats.currentHealth} HP remaining.");
            }
        }
    }

    private bool IsWithinAttackRange(Unit attacker, Unit target)
    {
        if (attacker.standingOnTile == null || target.standingOnTile == null)
        {
            return false;
        }

        Vector2Int attackerPos = attacker.standingOnTile.grid2DLocation;
        Vector2Int targetPos = target.standingOnTile.grid2DLocation;

        int distance = Mathf.Abs(attackerPos.x - targetPos.x) + Mathf.Abs(attackerPos.y - targetPos.y);
        return distance <= attacker.attackRange;
    }

    private int CalculateDamage(Unit attacker, Unit target)
    {
        int baseDamage = 0;

        // Calculate base damage based on class
        switch (attacker.characterStats.characterClass)
        {
            case CharacterStat.CharacterClass.Warrior:
                baseDamage = attacker.characterStats.strength;
                break;
            case CharacterStat.CharacterClass.Mage:
                baseDamage = attacker.characterStats.intellect;
                break;
            case CharacterStat.CharacterClass.Archer:
                baseDamage = attacker.characterStats.agility;
                break;
        }

        // Apply stat multiplier
        baseDamage += Mathf.RoundToInt(baseDamage * 2f);

        // Apply weapon damage modifier
        baseDamage = Mathf.RoundToInt(baseDamage * attacker.characterStats.equippedWeapon.WeaponDamageModifier);

        // Calculate elemental advantage/disadvantage
        float affinityDamageModifier = 1f;
        if (attacker.characterStats.elementType == target.characterStats.weaknessElement)
        {
            affinityDamageModifier = 1.5f;
            Debug.Log($"Elemental Advantage! Damage increased by 50%");
        }
        else if (target.characterStats.elementType == attacker.characterStats.weaknessElement)
        {
            affinityDamageModifier = 0.5f;
            Debug.Log($"Elemental Disadvantage! Damage reduced by 50%");
        }
        baseDamage = Mathf.RoundToInt(baseDamage * affinityDamageModifier);

        // Apply target's resistance
        int resistance = Mathf.RoundToInt(target.characterStats.magicalDefense * 0.2f);
        baseDamage -= resistance;

        // Ensure damage doesn't go below 0
        return Mathf.Max(baseDamage, 0);
    }
}