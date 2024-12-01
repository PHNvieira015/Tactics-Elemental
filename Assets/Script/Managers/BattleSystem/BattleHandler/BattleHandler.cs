using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    public BattleOverWindow battleOverWindow; // Reference to BattleOverWindow
    private static BattleHandler instance;

    public static BattleHandler GetInstance()
    {
        return instance;
    }


    [SerializeField] private Transform pfCharacterBattle;
    public Transform SpawningPoint1;
    public Transform SpawningPoint2;


    private CharacterBattle attackerCharacterBattle;
    private CharacterBattle TargetCharacterBattle;
    private CharacterBattle activeCharacterBattle;
    private State state;

    private enum State
    {
        WaitingForPlayer,
        Busy,
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        attackerCharacterBattle = SpawnCharacter(true);
        TargetCharacterBattle = SpawnCharacter(false);

        SetActiveCharacterBattle(attackerCharacterBattle);
        state = State.WaitingForPlayer;
    }

    private void Update()
    {
        if (state == State.WaitingForPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                state = State.Busy;
                attackerCharacterBattle.Attack(TargetCharacterBattle, () => {
                    ChooseNextActiveCharacter();
                });
            }
        }
    }

    private CharacterBattle SpawnCharacter(bool isPlayerTeam)
    {
        Vector3 position;
        if (isPlayerTeam)
        {
            position = SpawningPoint1.position;
        }
        else
        {
            position = SpawningPoint2.position;        }
        Transform characterTransform = Instantiate(pfCharacterBattle, position, Quaternion.identity);
        CharacterBattle characterBattle = characterTransform.GetComponent<CharacterBattle>();
        characterBattle.Setup(isPlayerTeam);

        return characterBattle;
    }

    private void SetActiveCharacterBattle(CharacterBattle characterBattle)
    {
        if (activeCharacterBattle != null)
        {
            activeCharacterBattle.HideSelectionCircle();
        }

        activeCharacterBattle = characterBattle;
        activeCharacterBattle.ShowSelectionCircle();
    }

    private void ChooseNextActiveCharacter()
    {
        if (TestBattleOver())
        {
            return;
        }

        if (activeCharacterBattle == attackerCharacterBattle)
        {
            SetActiveCharacterBattle(TargetCharacterBattle);
            state = State.Busy;

            TargetCharacterBattle.Attack(attackerCharacterBattle, () => {
                ChooseNextActiveCharacter();
            });
        }
        else
        {
            SetActiveCharacterBattle(attackerCharacterBattle);
            state = State.WaitingForPlayer;
        }
    }

    private bool TestBattleOver()
    {
        if (attackerCharacterBattle.IsDead())
        {
            // Player dead, enemy wins
            battleOverWindow.ShowBattleResult(false); // Show losing message
            return true;
        }
        if (TargetCharacterBattle.IsDead())
        {
            // Enemy dead, player wins
            //CodeMonkey.CMDebug.TextPopupMouse("Player Wins!");
            battleOverWindow.ShowBattleResult(true); // Show winner message
            return true;
        }

        return false;
    }
}
