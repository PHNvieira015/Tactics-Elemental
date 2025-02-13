using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public Unit selectedUnit; // Selected unit for the current turn

    public GameObject selectedUnitSquare; // Visual indicator for the selected unit
    public int playerTurn = 1; // 1 for Player 1, 2 for Player 2

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }

        if (selectedUnit != null)
        {
            if (selectedUnitSquare != null)
            {
                selectedUnitSquare.SetActive(true);
                selectedUnitSquare.transform.position = selectedUnit.transform.position;
            }
            else
            {
                Debug.LogWarning("selectedUnitSquare is not assigned. Please assign it in the Inspector.");
            }
        }
        else
        {
            if (selectedUnitSquare != null)
            {
                selectedUnitSquare.SetActive(false);
            }
        }
    }

    public void HandleTileClick(OverlayTile clickedTile)
    {
        Unit clickedUnit = clickedTile.GetComponentInChildren<Unit>();

        // Select attacker if not selected
        if (selectedUnit == null)
        {
            if (clickedUnit != null && clickedUnit.IsAlive() && clickedUnit.playerOwner == playerTurn)
            {
                selectedUnit = clickedUnit;
                selectedUnit.selected = true;
                Debug.Log($"{selectedUnit.name} selected as attacker.");
            }
        }
        else
        {
            // Select target and attack if in range
            if (clickedUnit != null && clickedUnit != selectedUnit && clickedUnit.IsAlive() && IsWithinAttackRange(selectedUnit, clickedUnit))
            {
                Attack(selectedUnit, clickedUnit);
            }
        }
    }

    private void EndTurn()
    {
        playerTurn = (playerTurn == 1) ? 2 : 1;

        if (selectedUnit != null)
        {
            selectedUnit.selected = false;
            selectedUnit = null;
        }

        ResetTiles();

        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            unit.hasMoved = false;
            unit.hasAttacked = false;
        }
    }

    private void ResetTiles()
    {
        foreach (OverlayTile tile in FindObjectsOfType<OverlayTile>())
        {
            tile.Reset();
        }
    }

    private bool IsWithinAttackRange(Unit attacker, Unit target)
    {
        Vector2Int attackerPos = attacker.standingOnTile.grid2DLocation;
        Vector2Int targetPos = target.standingOnTile.grid2DLocation;

        int distance = Mathf.Abs(attackerPos.x - targetPos.x) + Mathf.Abs(attackerPos.y - targetPos.y);
        return distance <= attacker.attackRange;
    }

    public void Attack(Unit attacker, Unit target)
    {
        if (attacker.IsAlive() && target.IsAlive())
        {
            int damageDealt = CalculateDamage(attacker, target);

            // Apply the damage to the target
            target.TakeDamage(damageDealt);

            Debug.Log($"{attacker.name} attacked {target.name} and dealt {damageDealt} damage!");

            if (!target.IsAlive())
            {
                Debug.Log($"{target.name} has been defeated!");
            }
            else
            {
                Debug.Log($"{target.name} has {target.characterStats.currentHealth} HP remaining.");
            }
        }
    }

    private int CalculateDamage(Unit attacker, Unit target)
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

        baseDamage += Mathf.RoundToInt(baseDamage * 2f); // Stat multiplier
        baseDamage = Mathf.RoundToInt(baseDamage * attacker.characterStats.equippedWeapon.WeaponDamageModifier);

        // Elemental damage modifier
        float affinityDamageModifier = 1f;
        if (attacker.characterStats.elementType == target.characterStats.weaknessElement)
        {
            affinityDamageModifier = 1.5f;
        }
        else if (target.characterStats.elementType == attacker.characterStats.weaknessElement)
        {
            affinityDamageModifier = 0.5f;
        }
        baseDamage = Mathf.RoundToInt(baseDamage * affinityDamageModifier);

        // Resistance calculation
        int resistance = Mathf.RoundToInt(target.characterStats.magicalDefense * 0.2f);
        baseDamage -= resistance;

        return Mathf.Max(baseDamage, 0);
    }
}
