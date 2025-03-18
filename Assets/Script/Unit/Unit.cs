using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static TurnStateManager;

[System.Serializable]
[RequireComponent(typeof(CharacterStat))]
public class Unit : MonoBehaviour
{
    public AnimatorScript animationScript;

    #region variables
    public string unitName; // Name of the unit
    public CharacterStat characterStats;  // Now a component of the same GameObject
    public TurnStateManager turnStateManager;  // Reference to TurnStateManager
    public GameObject unitGameObject;  // Add a reference to the GameObject if not already present
    public int teamID; // Team 1 or 2
    public int playerOwner; // 1 for Player 1, 2 for Player 2
    public int Mitigation; // Damage mitigation
    public bool isAlive = true; // Default value for isAlive
    public bool isDown; // Whether the unit is down (incapacitated)
    public bool hasMoved = false; // Whether the unit has moved in this turn
    public bool hasAttacked; // Whether the unit has attacked in this turn
    public bool selected; // Track if the unit is selected
    public GameObject weaponIcon; // Icon over unit if it's attackable
    public GameObject SelectionCircle;  // Add this declaration at the beginning of your class
    public bool isTarget; //is being target
    public int attackRange = 1; // AttackRange
    public int movementRange = 5; // Movement range
    public OverlayTile standingOnTile;
    public float Xposition;
    public float Yposition;
    public BattleHandler battleHandler;
    public DamageSystem damageSystem;
    [SerializeField] private SpriteRenderer unitSpriteRenderer;
    public List<Unit> enemiesInRange = new List<Unit>();  // Enemies in range (changed to public)

    private LevelSystem levelSystem;  //leveling system, stat growth

    public List<Buff> buffs = new List<Buff>(); // List of buffs currently affecting the unit
    public List<Debuff> debuffs = new List<Debuff>(); // List of debuffs currently affecting the unit
    private int currentTurn = 0; //buff duration to do

    private UnitSkills unitSkills;
    public List<Skill> skillslist;

    public DirectionHandler directionHandler;

    #region AI Variables
    public AIController aiController; // Reference to the AIController
    public bool isAI; // Whether this unit is controlled by AI
    private OverlayTile targetTile; // Tile the AI is moving toward
    public List<Unit> allEnemies; // List of all enemy units
    private PathFinder pathFinder; // Instance of PathFinder
    #endregion

    #region flasheffect Variables
    private Material originalMaterial;
    private Coroutine flashRoutine;
 

    #endregion



    #endregion

    #region skills
    private void Awake()
    {
 


        if (unitSpriteRenderer == null)
            unitSpriteRenderer = transform.Find("UnitSprite")?.GetComponent<SpriteRenderer>();
            originalMaterial = unitSpriteRenderer.material;

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

        if (isAI)
        {
            aiController = gameObject.AddComponent<AIController>();
        }

        // Initialize PathFinder
        pathFinder = new PathFinder();
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
        //flashing effect
        Flash(Color.white);

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
    public void SetFaceDirectionAtTurnEnd(CharacterStat.Direction newDirection)
    {
        // Set the new direction
        characterStats.faceDirection = newDirection;

        // Debug the new direction
        if (characterStats.faceDirection == CharacterStat.Direction.UpLeft)
        {
            Debug.Log("Unit is facing West.");
        }
        else if (characterStats.faceDirection == CharacterStat.Direction.UpRight)
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
            if (attackRange < 1)
            {
                attackRange = 1;
            }
        }
        else
        {
            Debug.LogWarning("CharacterStats is null. Cannot update attackRange.");
        }
    }

    #region AI Methods
    public void FindAllEnemies()
    {
        allEnemies = new List<Unit>();
        Unit[] allUnits = FindObjectsOfType<Unit>();

        foreach (var unit in allUnits)
        {
            if (unit.teamID != teamID && unit.IsAlive())
            {
                allEnemies.Add(unit);
            }
        }
    }

    public void CheckForEnemiesInRange()
    {
        enemiesInRange.Clear();
        foreach (var enemy in allEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= attackRange)
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    public void MoveTowardNearestEnemy()
    {
        Unit nearestEnemy = null;
        float shortestDistance = float.MaxValue;

        // Find the nearest enemy
        foreach (var enemy in allEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            // Use PathFinder to find a path to the nearest enemy
            targetTile = nearestEnemy.standingOnTile;
            List<OverlayTile> path = pathFinder.FindPath(standingOnTile, targetTile, GetMovementRangeTiles());

            if (path != null && path.Count > 0)
            {
                // Move along the path
                StartCoroutine(MoveAlongPath(path));
            }
        }
    }

    private List<OverlayTile> GetMovementRangeTiles()
    {
        List<OverlayTile> movementRangeTiles = new List<OverlayTile>();
        Vector2Int startLocation = standingOnTile.grid2DLocation;

        // Use a flood-fill or similar algorithm to find all tiles within movement range
        // For now, here's a placeholder implementation
        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                Vector2Int location = new Vector2Int(startLocation.x + x, startLocation.y + y);
                if (MapManager.Instance.map.ContainsKey(location))
                {
                    movementRangeTiles.Add(MapManager.Instance.map[location]);
                }
            }
        }

        return movementRangeTiles;
    }

    private IEnumerator MoveAlongPath(List<OverlayTile> path)
    {
        foreach (var tile in path)
        {
            // Move the unit to the next tile
            transform.position = tile.transform.position;
            standingOnTile = tile;
            yield return new WaitForSeconds(0.5f); // Adjust delay for animation or smooth movement
        }
    }

    public void AttackEnemy(Unit enemy)
    {
        if (enemiesInRange.Contains(enemy))
        {
            Debug.Log($"{unitName} is attacking {enemy.unitName}");
            // Use DamageSystem to calculate and apply damage
            DamageSystem.Instance.Attack(this, enemy);
            hasAttacked = true;
        }
    }

    public void DecideAction()
    {
        if (!isAI || !IsAlive() || hasMoved || hasAttacked) return;

        FindAllEnemies();
        CheckForEnemiesInRange();

        if (enemiesInRange.Count > 0)
        {
            // Attack the first enemy in range
            AttackEnemy(enemiesInRange[0]);
        }
        else
        {
            // Move toward the nearest enemy
            MoveTowardNearestEnemy();
        }
    }
    #endregion

    #region flash Method
    private IEnumerator FlashRoutine(Color color)
    {
        unitSpriteRenderer.material = damageSystem.flashMaterial;

        damageSystem.flashMaterial.color = color;

        yield return new WaitForSeconds(damageSystem.flashDuration);

        unitSpriteRenderer.material= originalMaterial;

        flashRoutine = null;

    }
    public void Flash(Color color)
    {
        if (flashRoutine != null)
        {
        StopCoroutine(flashRoutine);

        }
        flashRoutine=StartCoroutine(FlashRoutine(color));
    }
        #endregion
    }