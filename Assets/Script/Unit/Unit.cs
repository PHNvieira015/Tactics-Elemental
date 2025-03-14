using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TurnStateManager;

[System.Serializable]
[RequireComponent(typeof(CharacterStat))]
public class Unit : MonoBehaviour
{
    #region variables
    public string unitName; // Name of the unit
    // Reference to CharacterStat MonoBehaviour for accessing stats and data
    public CharacterStat characterStats;  // Now a component of the same GameObject
    public TurnStateManager turnStateManager;  // Reference to TurnStateManager
    public GameObject unitGameObject;  // Add a reference to the GameObject if not already present
    // Control player properties
    public int teamID; // Team 1 or 2
    public int playerOwner; // 1 for Player 1, 2 for Player 2
    public int Mitigation; // Damage mitigation
    public bool isAlive = true; // Default value for isAlive
    public bool isDown; // Whether the unit is down (incapacitated)
    public bool hasMoved=false; // Whether the unit has moved in this turn
    public bool hasAttacked; // Whether the unit has attacked in this turn
    public bool selected; // Track if the unit is selected
    public GameObject weaponIcon; // Icon over unit if it's attackable
    public GameObject SelectionCircle;  // Add this declaration at the beginning of your class
    public bool isTarget; //is being target
    public int attackRange=1; // AttackRange
    public OverlayTile standingOnTile;
    public float Xposition;
    public float Yposition;
    public BattleHandler battleHandler;
    public DamageSystem damageSystem;
    [SerializeField] private SpriteRenderer unitSpriteRenderer;
    List<Unit> enemiesInRange = new List<Unit>();  // Enemies in range

    private LevelSystem levelSystem;  //leveling system, stat growth


    public List<Buff> buffs = new List<Buff>(); // List of buffs currently affecting the unit
    public List<Debuff> debuffs = new List<Debuff>(); // List of debuffs currently affecting the unit
    private int currentTurn = 0; //buff duration to do


    private UnitSkills unitSkills;
    public List<Skill> skillslist;

    private DirectionHandler directionHandler;

    #endregion

    #region skills
    private void Awake()
    {
        {
            if (unitSpriteRenderer == null)
                unitSpriteRenderer = transform.Find("UnitSprite")?.GetComponent<SpriteRenderer>();
        }
        directionHandler = GetComponent<DirectionHandler>();
        turnStateManager ??= FindObjectOfType<TurnStateManager>();
        characterStats = GetComponent<CharacterStat>();
        unitGameObject = gameObject;

        // Ensure damageSystem is assigned
        damageSystem ??= FindObjectOfType<DamageSystem>();
        battleHandler ??= FindObjectOfType<BattleHandler>();

        if (characterStats == null)
        {
            characterStats = gameObject.AddComponent<CharacterStat>();
            Debug.Log(characterStats.name + " CharacterStat was not found, so it has been added automatically.");
        }

        unitSkills = new UnitSkills();
        unitSkills.OnSkillUnlocked += UnitSkills_OnSkillUnlocked;
        levelSystem = new LevelSystem(characterStats);
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
        battleHandler = GetComponent<BattleHandler>();
        damageSystem ??= FindObjectOfType<DamageSystem>();
        characterStats.SetRoundInitiative();
        UpdateAttackRange();
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

    public void TakeDamage(int damageAmount, DamageSystem damageSystem)
    {
        CharacterBattle characterBattle = GetComponent<CharacterBattle>();
        if (characterBattle != null)
        {
            characterBattle.HealthSystem_OnHealthChanged(characterBattle, EventArgs.Empty);
        }

        if (characterBattle.healthSystem != null)
        {
            characterBattle.healthSystem.Damage(damageAmount); // Properly update health via HealthSystem
        }
        else
        {
            Debug.LogError($"HealthSystem is missing on {gameObject.name}");
        }
        // damage popup
        DamagePopup.Create(transform.position, damageAmount);
        //  Update health bar after taking damage
  
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
    public void SetFaceDirectionAtTurnEnd()
    {
        if (characterStats.faceDirection == CharacterStat.Direction.UpLeft)
        {
            Debug.Log("Unit is facing North.");
        }
        else if (characterStats.faceDirection == CharacterStat.Direction.DownRight)
        {
            Debug.Log("Unit is facing East.");
        }
        else if (characterStats.faceDirection == CharacterStat.Direction.DownLeft)
        {
            Debug.Log("Unit is facing South.");
        }
        else if (characterStats.faceDirection == CharacterStat.Direction.UpLeft)
        {
            Debug.Log("Unit is facing West.");
        }
    }
    #endregion

    #region useSkills
    public bool CanUseEarthShatter()
    {
        return unitSkills.IsSkillUnlocked(UnitSkills.SkillType.Earthshatter);
    }

    public UnitSkills GetUnitSkills()
    {
        return unitSkills;
    }
    #endregion

    #region Select and Deselect Methods

    public void Select()
    {
        if (SelectionCircle != null)
        {
            SelectionCircle.SetActive(true);
        }
    }

    public void Deselect()
    {
        if (SelectionCircle != null)
        {
            SelectionCircle.SetActive(false);
        }
    }

    public void TargetSelect()
    {
        if (SelectionCircle != null)
        {
            SelectionCircle.SetActive(true);
            SetSelectionCircleColor(Color.red); // Change SelectionCircle color to red
        }
    }

    public void TargetDeselect()
    {
        if (SelectionCircle != null)
        {
            SelectionCircle.SetActive(false);
            SetSelectionCircleColor(Color.white); // Reset SelectionCircle color to default
        }
    }

    private void SetSelectionCircleColor(Color color)
    {
        Renderer renderer = SelectionCircle.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
        else
        {
            Debug.LogError($"Renderer not found on SelectionCircle for {name}");
        }
    }

    #endregion

    // New method to get the tile under the unit
    public OverlayTile GetTileUnderUnit()
    {
        Vector2 unitPosition = unitGameObject.transform.position;  // World position of the unit

        // Use RaycastAll to get all tiles under the unit
        RaycastHit2D[] hits = Physics2D.RaycastAll(unitPosition, Vector2.down, 20f);

        if (hits.Length > 0)
        {
            // Find the tile with the highest Z-axis value
            OverlayTile tile = hits
                .OrderByDescending(hit => hit.collider.transform.position.z) // Sort by Z-axis
                .Select(hit => hit.collider.GetComponent<OverlayTile>())    // Get OverlayTile component
                .FirstOrDefault(tile => tile != null);                      // Select the first valid tile

            if (tile != null)
            {
                standingOnTile = tile;  // Set the standingOnTile
                tile.unitOnTile = this; // Set the unit on the tile
                standingOnTile.unitOnTile = this;
                return tile;
            }
        }

        return null;
    }
    // Helper property to access the sprite
    public Sprite UnitSprite
    {
        get
        {
            var sr = transform.Find("UnitSprite")?.GetComponent<SpriteRenderer>();
            return sr != null ? sr.sprite : null;
        }
    }
    public void UpdateAttackRange()
    {
        if (characterStats != null)
        {
            if (characterStats.equippedWeapon != null)
            {
                attackRange = characterStats.attackRangeBonus + characterStats.equippedWeapon.WeaponRange;
            }
            else
            {
                attackRange = characterStats.attackRangeBonus; // No weapon equipped
            }
            if (attackRange<1)
            {
                attackRange = 1;
            }
        }
        else
        {
            Debug.LogWarning("CharacterStats is null. Cannot update attackRange.");
        }
    }

}
