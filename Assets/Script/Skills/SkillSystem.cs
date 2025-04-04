using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkillSystem : MonoBehaviour
{
    public DamageSystem damageSystem;  // Assign this in the Inspector
    public static SkillSystem Instance { get; private set; }
    public bool skillused = false;

    private void Awake()
    {
        damageSystem = FindObjectOfType<DamageSystem>();
        if (damageSystem == null)
        {
            Debug.LogError("No DamageSystem found in the scene!");
        }
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    public bool CanUseSkill(Unit caster, Skill skill)
    {
        if (caster == null || skill == null)
        {
            Debug.LogError("Caster or skill is null!");
            return false;
        }

        // Check mana cost
        if (caster.characterStats.currentMana < skill.cost)
        {
            Debug.Log($"{caster.unitName} does not have enough mana to use {skill.Name}!");
            return false;
        }

        // Check cooldown
        if (caster.skillCooldowns.ContainsKey(skill) && caster.skillCooldowns[skill] > 0)
        {
            Debug.Log($"{skill.Name} is on cooldown!");
            return false;
        }

        // Check class requirements
        if (skill.classRequirement != Skill.ClassRequirements.None &&
            caster.characterStats.characterClass.ToString() != skill.classRequirement.ToString())
        {
            Debug.Log($"{caster.unitName} does not meet the class requirement for {skill.Name}!");
            return false;
        }

        if (skill.hasAttacked && caster.hasAttacked)
        {
            Debug.Log($"{caster.unitName} has already attacked this turn!");
            return false;
        }

        if (skill.hasMoved && caster.hasMoved)
        {
            Debug.Log($"{caster.unitName} has already moved this turn!");
            return false;
        }

        return true;
    }

    public void ExecuteSkill(Unit caster, Unit target, Skill skill)
    {
        if (caster == null)
        {
            Debug.LogError("Caster is null in ExecuteSkill!");
            return;
        }

        if (skill == null)
        {
            Debug.LogError("Skill is null in ExecuteSkill!");
            return;
        }

        if (!CanUseSkill(caster, skill))
        {
            Debug.Log($"{caster.unitName} cannot use {skill.Name}");
            return;
        }

        // Update action flags
        if (skill.hasAttacked) caster.hasAttacked = true;
        if (skill.hasMoved) caster.hasMoved = true;

        ApplySkillEffects(caster, target, skill);

        // Consume resources
        caster.characterStats.currentMana -= skill.cost;

        // Set cooldown
        if (skill.cooldown > 0)
        {
            if (!caster.skillCooldowns.ContainsKey(skill))
            {
                caster.skillCooldowns.Add(skill, skill.cooldown);
            }
            else
            {
                caster.skillCooldowns[skill] = skill.cooldown;
            }
        }

        Debug.Log($"{caster.unitName} used {skill.Name} on {target?.unitName ?? "themselves"}!");
    }

    private void ApplySkillEffects(Unit caster, Unit target, Skill skill)
    {
        if (skill.effects != null)
        {
            foreach (var effect in skill.effects)
            {
                if (effect != null)
                {
                    if (skill.targetType == Skill.TargetType.Enemy)
                    {
                        target?.ApplyDebuff(effect);
                    }
                    else
                    {
                        target?.ApplyBuff(effect);
                    }
                    Debug.Log($"Applied effect: {effect.effectName}");
                }
            }
        }

        // Apply direct damage if applicable
        if (skill.baseDamage > 0 && skill.targetType == Skill.TargetType.Enemy && target != null)
        {
            if (damageSystem != null)
            {
                int damage = damageSystem.CalculateSkillDamage(caster, target, skill);
                target.TakeDamage(damage, damageSystem);
                Debug.Log($"Dealt {damage} damage to {target.unitName}");

                // Update the health bar
                CharacterBattle targetBattle = target.GetComponent<CharacterBattle>();
                if (targetBattle != null)
                {
                    targetBattle.HealthSystem_OnHealthChanged(targetBattle, EventArgs.Empty);
                }

                // Handle death
                if (!target.IsAlive())
                {
                    Debug.Log($"{target.unitName} has been defeated by {skill.Name}!");

                    // Clear tile and destroy object
                    target.standingOnTile?.ClearUnit();
                    Destroy(target.gameObject);

                    // Remove from turn order
                    var unitManager = FindObjectOfType<UnitManager>();
                    if (unitManager != null)
                    {
                        unitManager.RemoveUnitFromTurnOrder(target);
                        GameMaster.RemoveUnit(target);
                    }
                }
            }
            else
            {
                Debug.LogError("damageSystem reference not assigned in SkillSystem!");
            }
        }
    }

    public void ReduceCooldowns(Unit unit)
    {
        foreach (var skill in unit.skillCooldowns.Keys.ToList())
        {
            if (unit.skillCooldowns[skill] > 0)
            {
                unit.skillCooldowns[skill]--;
            }
        }
    }
}