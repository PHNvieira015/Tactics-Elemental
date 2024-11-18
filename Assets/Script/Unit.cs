using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    // Reference to CharacterStat ScriptableObject for accessing stats and data
    public CharacterStat characterStats;

    // Initialize the character stats (this is done by the Inspector in this case)
    private void Awake()
    {
        if (characterStats == null)
        {
            Debug.LogError("CharacterStats reference is missing!");
        }
    }

    // Control player properties
    public int teamID; //team 1 or 2
    public int playerOwner; // 1 for Player 1, 2 for Player 2
    public int Mitigation; // Damage mitigation
    public bool isAlive = true; // Default value for isAlive
    public bool isDown; // Whether the unit is down (incapacitated)
    public bool hasMoved; // Whether the unit has moved in this turn
    public bool hasAttacked; // Whether the unit has attacked in this turn
    public bool selected; // Add this to track if the unit is selected
    public GameObject weaponIcon; //icon over unit if it's attackable
    // AttackRange
    public int attackRange;
    List<Unit> enemiesInRange = new List<Unit>();  // Enemies in range
    public List<Ability> abilities;


    // Tile this unit is standing on
    public OverlayTile standingOnTile;

    // Buffs and debuffs
    public List<Buff> buffs = new List<Buff>(); // List of buffs currently affecting the unit
    public List<Debuff> debuffs = new List<Debuff>(); // List of debuffs currently affecting the unit

    // Track current turn for buffs
    private int currentTurn = 0;

    // Turn State
    public enum TurnState { Move, Attack, Skill, Idle }

    public void SetState(TurnState state)
    {
        switch (state)
        {
            case TurnState.Move:
                if (hasMoved == false)
                {
                    // EnableMovementUI();
                }
                break;

            case TurnState.Attack:
                if (hasAttacked == false)
                {
                    // EnableAttackUI();
                }
                break;

            case TurnState.Skill:
                // EnableSkillUI();
                break;

            case TurnState.Idle:
                // At the end of the turn, we set the faceDirection.
                SetFaceDirectionAtTurnEnd();
                break;
        }
    }

    private void Start()
    {
        // Example: Adding a buff/debuff manually (if needed)
        if (buffs.Count > 0)
        {
            Debug.Log($"First Buff: {buffs[0].buffName}");
        }
    }

    // Call this method to start a new turn
    public void StartNewTurn()
    {
        currentTurn++;
        UpdateTurnBasedEffects();
    }

    // Method to check if the unit is alive
    public bool IsAlive()
    {
        return isAlive && !isDown; // Unit is alive if isAlive is true and isDown is false
    }

    // Damage system
    public void TakeDamage(int damage)
    {
        int effectiveDamage = Mathf.Max(damage - Mitigation, 1); // Mitigation reduces damage, but minimum is 1
        characterStats.currentHealth -= effectiveDamage;

        Debug.Log($"{gameObject.name} took {effectiveDamage} damage. Remaining HP: {characterStats.currentHealth}");

        if (characterStats.currentHealth <= 0)
        {
            isAlive = false;
            isDown = true;
            Debug.Log($"{gameObject.name} has been defeated!");
        }
    }

    // Apply buffs
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

    // Apply debuffs
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

    // Method to update the buffs/debuffs each turn
    private void UpdateTurnBasedEffects()
    {
        // For Buffs: Decrease duration for each buff
        foreach (var buff in buffs)
        {
            buff.duration--; // Decrease duration by 1 turn
            if (buff.duration <= 0)
            {
                Debug.Log($"Buff {buff.buffName} has expired.");
            }
        }

        // For Debuffs: Decrease duration for each debuff
        foreach (var debuff in debuffs)
        {
            debuff.duration--; // Decrease duration by 1 turn
            if (debuff.duration <= 0)
            {
                Debug.Log($"Debuff {debuff.debuffName} has expired.");
            }
        }

        // Remove expired buffs and debuffs
        buffs.RemoveAll(buff => buff.duration <= 0);
        debuffs.RemoveAll(debuff => debuff.duration <= 0);
    }

    // This method will be called every time the turn starts to refresh status effects
    public void RefreshStatusEffects()
    {
        // Optional: Refresh buffs/debuffs based on turn
        Debug.Log($"Start of turn {currentTurn} - Refreshing buffs and debuffs.");
        UpdateTurnBasedEffects();
    }

    // This method sets the character's faceDirection at the end of their turn
    private void SetFaceDirectionAtTurnEnd()
    {
        // Logic to determine face direction at the end of the turn.
        // Assuming the unit's last action, or input decides the direction.

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

    // Placeholder method that might be used to gather enemies
    //void getenemies()
    //{
    //    enemiesInRange.Clear();
    //    foreach (Unit unit in FindObjectsOfType<Unit>())
    //    {
    //        if (Mathf.Abs(transform.position.x - unit.transform.position.x) < attackRange)
    //        {
    //            enemiesInRange.Add(unit);
    //        }
    //    }
    //}
}
