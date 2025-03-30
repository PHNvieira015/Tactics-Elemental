using System;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public BattleHandler battleHandler; // Reference to BattleHandler for handling battle logic
    public int baseDamage = 1;
    private UnitManager unitManager;
    private GameMaster gameMaster;
    [SerializeField] public Material flashMaterial;
    public float flashDuration = 0.1f;
    [SerializeField] public Color color = Color.white;
    public static DamageSystem Instance { get; private set; }
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

        baseDamage += Mathf.RoundToInt(baseDamage * 1f);
        baseDamage = Mathf.RoundToInt(baseDamage * 1 + attacker.characterStats.equippedWeapon.WeaponDamageModifier);

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

    // Calculate damage for a skill
    public int CalculateSkillDamage(Unit attacker, Unit target, Skill skill)
    {
        if (attacker == null || target == null || skill == null)
        {
            Debug.LogError("Attacker, target, or skill is null!");
            return 0;
        }

        // Start with the base damage from the skill
        int damage = skill.baseDamage;

        float statValue = 0f;
        switch (skill.scalingStat)
        {
            case Skill.StatType.Strength:
                statValue = attacker.characterStats.strength;
                break;
            case Skill.StatType.Agility:
                statValue = attacker.characterStats.agility;
                break;
            case Skill.StatType.Intellect:
                statValue = attacker.characterStats.intellect;
                break;
        }

        // Add the base damage from the attacker's stats (reusing CalculateDamage)
        //damage += CalculateDamage(attacker, target);
        // Apply stat scaling percentage
        damage += Mathf.RoundToInt(statValue * skill.statScalingPercentage);

        // Add target HP scaling if configured
        if (skill.targetHPScalingPercentage > 0f)
        {
            damage += Mathf.RoundToInt(target.characterStats.maxBaseHealth * skill.targetHPScalingPercentage);
        }

        // Apply attack type modifiers
        switch (skill.attackType)
        {
            case Skill.AttackType.Crush:
                damage += Mathf.RoundToInt(attacker.characterStats.strength * 0.25f);
                break;
            case Skill.AttackType.Slash:
                damage += Mathf.RoundToInt(attacker.characterStats.agility * 0.25f);
                break;
            case Skill.AttackType.Magic:
                damage += Mathf.RoundToInt(attacker.characterStats.intellect * 0.25f);
                break;
        }

        // Convert Skill.ElementType to CharacterStat.ElementType for comparison
        CharacterStat.ElementType convertedSkillElement = ConvertSkillElementToCharacterStatElement(skill.elementType);

        // Apply elemental modifiers
        if (convertedSkillElement == target.characterStats.weaknessElement)
        {
            damage = Mathf.RoundToInt(damage * 1.5f); // 50% bonus damage
        }
        else if (convertedSkillElement == target.characterStats.elementType)
        {
            damage = Mathf.RoundToInt(damage * 0.5f); // 50% reduced damage
        }

        return Mathf.Max(damage, 1); // Ensure damage is not negative
    }
    private CharacterStat.ElementType ConvertSkillElementToCharacterStatElement(Skill.ElementType skillElement)
    {
        // Assuming CharacterStat.ElementType and Skill.ElementType have similar values,
        // a simple cast might work if they are enums.
        return (CharacterStat.ElementType)skillElement;
    }
    public void Attack(Unit attacker, Unit target)
    {
        if (attacker == null || target == null)
        {
            Debug.LogError("Attacker or Target is null! Cannot perform attack.");
            return;
        }

        if (battleHandler == null)
        {
            Debug.LogError("BattleHandler is not assigned!");
            return;
        }

        battleHandler.attackerCharacterBattle = attacker.GetComponent<CharacterBattle>();
        battleHandler.targetCharacterBattle = target.GetComponent<CharacterBattle>();

        if (battleHandler.attackerCharacterBattle == null)
        {
            Debug.LogError("Attacker does not have a CharacterBattle component!");
            return;
        }

        if (battleHandler.targetCharacterBattle == null)
        {
            Debug.LogError("Target does not have a CharacterBattle component!");
            return;
        }

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
                //target.gameObject.SetActive(false);
                target.standingOnTile.ClearUnit();
                Destroy(target.gameObject);
                if (unitManager != null)
                {
                    unitManager.RemoveUnitFromTurnOrder(target);
                    GameMaster.RemoveUnit(target);
                }

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

}