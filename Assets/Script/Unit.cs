using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Unit : MonoBehaviour
{
    // Reference to CharacterStat MonoBehaviour for accessing stats and data
    public CharacterStat characterStats;  // Now a component of the same GameObject

    // Control player properties
    public int teamID; // Team 1 or 2
    public int playerOwner; // 1 for Player 1, 2 for Player 2
    public int Mitigation; // Damage mitigation
    public bool isAlive = true; // Default value for isAlive
    public bool isDown; // Whether the unit is down (incapacitated)
    public bool hasMoved; // Whether the unit has moved in this turn
    public bool hasAttacked; // Whether the unit has attacked in this turn
    public bool selected; // Track if the unit is selected
    public GameObject weaponIcon; // Icon over unit if it's attackable

    // AttackRange
    public int attackRange;

    List<Unit> enemiesInRange = new List<Unit>();  // Enemies in range

    private LevelSystem levelSystem;

    private UnitSkills unitSkills;
    public List<Ability> abilities;

    #region skills
    private void Awake()
    {
        // Get the CharacterStat component from the same GameObject this Unit is attached to
        characterStats = GetComponent<CharacterStat>();

        if (characterStats == null)
        {
            Debug.LogError("CharacterStat component is missing from this GameObject!");
        }
        else
        {
            unitSkills = new UnitSkills();
            unitSkills.OnSkillUnlocked += UnitSkills_OnSkillUnlocked;
            levelSystem = new LevelSystem(characterStats);
            levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
        }
    }

    private void UnitSkills_OnSkillUnlocked(object sender, UnitSkills.OnSkillUnlockedEventArgs e)
    {
        switch (e.skillType)
        {
            case UnitSkills.SkillType.Tier1_1:
                Debug.Log("MoveSpeed_1");
                break;
            case UnitSkills.SkillType.Tier2_1:
                Debug.Log("MoveSpeed_2");
                break;
            case UnitSkills.SkillType.Tier1_2:
                Debug.Log("HealthMax_1");
                break;
            case UnitSkills.SkillType.Tier2_2:
                Debug.Log("HealthMax_2");
                break;
            case UnitSkills.SkillType.Tier3_1:
                Debug.Log("Earthshatter");
                break;
            case UnitSkills.SkillType.Tier3_2:
                Debug.Log("EarthShatter2");
                break;
        }
    }
    #endregion

    #region levelsystem
    public void SetLevelSystem(LevelSystem levelSystem)
    {
        this.levelSystem = levelSystem;
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
    }

    private void LevelSystem_OnLevelChanged(object sender, EventArgs e)
    {
        Debug.Log("Personagem subiu de nivel!");
    }
    #endregion

    public OverlayTile standingOnTile;

    #region buff/debuff
    public List<Buff> buffs = new List<Buff>(); // List of buffs currently affecting the unit
    public List<Debuff> debuffs = new List<Debuff>(); // List of debuffs currently affecting the unit
    private int currentTurn = 0;
    #endregion

    #region turnstate
    public enum unitTurnState { Move, Attack, Skill, Idle }

    public void SetState(unitTurnState state)
    {
        switch (state)
        {
            case unitTurnState.Move:
                if (hasMoved == false)
                {
                    // EnableMovementUI();
                }
                break;

            case unitTurnState.Attack:
                if (hasAttacked == false)
                {
                    // EnableAttackUI();
                }
                break;

            case unitTurnState.Skill:
                // EnableSkillUI();
                break;

            case unitTurnState.Idle:
                SetFaceDirectionAtTurnEnd();
                break;
        }
    }
    #endregion

    private void Start()
    {
        if (buffs.Count > 0)
        {
            Debug.Log($"First Buff: {buffs[0].buffName}");
        }
    }

    #region TakeDamage
    public bool IsAlive()
    {
        return isAlive && !isDown; // Unit is alive if isAlive is true and isDown is false
    }

    public void TakeDamage(int damage)
    {
        int effectiveDamage = Mathf.Max(damage - Mitigation, 1);
        characterStats.currentHealth -= effectiveDamage;

        Debug.Log($"{gameObject.name} took {effectiveDamage} damage. Remaining HP: {characterStats.currentHealth}");

        if (characterStats.currentHealth <= 0)
        {
            isAlive = false;
            isDown = true;
            Debug.Log($"{gameObject.name} has been defeated!");
        }
    }
    #endregion

    #region Buff/Debuff
    public void ApplyBuff(Buff newBuff)
    {
        if (!buffs.Contains(newBuff)) // Prevent applying duplicates
        {
            buffs.Add(newBuff);
            Debug.Log($"Buff {newBuff.buffName} applied to {gameObject.name}");
        }
        else
        {
            Debug.Log($"Buff {newBuff.buffName} is already applied to {gameObject.name}");
        }
    }

    public void ApplyDebuff(Debuff newDebuff)
    {
        if (!debuffs.Contains(newDebuff)) // Prevent applying duplicates
        {
            debuffs.Add(newDebuff);
            Debug.Log($"Debuff {newDebuff.debuffName} applied to {gameObject.name}");
        }
        else
        {
            Debug.Log($"Debuff {newDebuff.debuffName} is already applied to {gameObject.name}");
        }
    }

    private void UpdateTurnBasedEffects()
    {
        foreach (var buff in buffs)
        {
            buff.duration--;
            if (buff.duration <= 0)
            {
                Debug.Log($"Buff {buff.buffName} has expired.");
            }
        }

        foreach (var debuff in debuffs)
        {
            debuff.duration--;
            if (debuff.duration <= 0)
            {
                Debug.Log($"Debuff {debuff.debuffName} has expired.");
            }
        }

        buffs.RemoveAll(buff => buff.duration <= 0);
        debuffs.RemoveAll(debuff => debuff.duration <= 0);
    }

    public void RefreshStatusEffects()
    {
        Debug.Log($"Start of turn {currentTurn} - Refreshing buffs and debuffs.");
        UpdateTurnBasedEffects();
    }
    #endregion

    #region FaceDirection
    private void SetFaceDirectionAtTurnEnd()
    {
        if (characterStats.faceDirection == CharacterStat.Direction.North)
        {
            Debug.Log("Unit is facing North.");
        }
        else if (characterStats.faceDirection == CharacterStat.Direction.East)
        {
            Debug.Log("Unit is facing East.");
        }
        else if (characterStats.faceDirection == CharacterStat.Direction.South)
        {
            Debug.Log("Unit is facing South.");
        }
        else if (characterStats.faceDirection == CharacterStat.Direction.West)
        {
            Debug.Log("Unit is facing West.");
        }
    }
#endregion

    public bool CanUseEarthShatter()
    {
        return unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Earthshatter);
    }

    public UnitSkills GetUnitSkills()
    {
        return unitSkills;
    }
}
