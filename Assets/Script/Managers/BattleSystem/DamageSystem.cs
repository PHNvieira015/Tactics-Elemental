using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public BattleHandler battleHandler; // Reference to BattleHandler for handling battle logic
    public int baseDamage = 1;

    public static DamageSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    public void Attack(Unit attacker, Unit target)
    {
        battleHandler.attackerCharacterBattle = attacker.GetComponent<CharacterBattle>();
        battleHandler.targetCharacterBattle = target.GetComponent<CharacterBattle>();

        if (!IsWithinAttackRange(attacker, target))
        {
            Debug.Log($"{target.name} is out of attack range!");
            return;
        }

        if (attacker.IsAlive() && target.IsAlive())
        {
            int damageDealt = CalculateDamage(attacker, target);

            // Apply the damage to the target
            target.TakeDamage(damageDealt, this);

            Debug.Log($"{attacker.name} attacked {target.name} and dealt {damageDealt} damage!");

            // Update the health bar after applying damage
            CharacterBattle targetBattle = target.GetComponent<CharacterBattle>();
            if (targetBattle != null)
            {
                targetBattle.HealthSystem_OnHealthChanged(targetBattle, EventArgs.Empty);
            }

            if (!target.IsAlive())
            {
                Debug.Log($"{target.name} has been defeated!");
                target.gameObject.SetActive(false);
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

    public int CalculateDamage(Unit attacker, Unit target)
    {
        int baseDamage = 0;

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

        baseDamage += Mathf.RoundToInt(baseDamage * 2f);
        baseDamage = Mathf.RoundToInt(baseDamage *1 + attacker.characterStats.equippedWeapon.WeaponDamageModifier);

        float affinityDamageModifier = 1f;
        if (attacker.characterStats.elementType == target.characterStats.weaknessElement)
        {
            affinityDamageModifier = 1.5f;
            Debug.Log($"Elemental Advantage! Magic Damage increased by 50%");
        }
        else if (target.characterStats.elementType == attacker.characterStats.weaknessElement)
        {
            affinityDamageModifier = 0.5f;
            Debug.Log($"Elemental Disadvantage! Magic Damage reduced by 50%");
        }
        baseDamage = Mathf.RoundToInt(baseDamage * affinityDamageModifier);

        int resistance = Mathf.RoundToInt(target.characterStats.magicalDefense * 0.2f);
        baseDamage -= resistance;

        battleHandler.attackerCharacterBattle = null;
        battleHandler.targetCharacterBattle = null;

        return Mathf.Max(baseDamage, 0);
    }
}
