using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class SkillSystem : MonoBehaviour
{
    public static SkillSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    // Check if a skill can be used (cooldown, mana, range, etc.)
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
        if (skill.classRequirement != Skill.Classrequirements.None &&
            caster.characterStats.characterClass.ToString() != skill.classRequirement.ToString())
        {
            Debug.Log($"{caster.unitName} does not meet the class requirement for {skill.Name}!");
            return false;
        }

        return true;
    }

    // Execute a skill
    public void ExecuteSkill(Unit caster, Unit target, Skill skill)
    {
        if (!CanUseSkill(caster, skill))
        {
            return;
        }

        // Apply skill effects
        ApplySkillEffects(caster, target, skill);

        // Consume mana
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

    // Apply skill effects (damage, buffs, debuffs, etc.)
    private void ApplySkillEffects(Unit caster, Unit target, Skill skill)
    {
        if (skill.effects != null && skill.effects.Count > 0)
        {
            foreach (var effect in skill.effects)
            {
                if (effect != null)
                {
                    effect.ApplyEffect(caster, target);
                }
            }
        }

        // Apply damage if applicable
        if (skill.value > 0 && skill.targetType == Skill.TargetType.Enemy && target != null)
        {
            DamageSystem.Instance.CalculateSkillDamage(caster, target, skill);
        }
    }

    // Reduce cooldowns for a unit at the end of their turn
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