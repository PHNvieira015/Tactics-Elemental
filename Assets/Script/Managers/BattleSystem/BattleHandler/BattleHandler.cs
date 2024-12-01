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

    // References to both character battle units (player and enemy)
    public CharacterBattle attackCharacterBattle;
    public CharacterBattle targetCharacterBattle;

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
        // Set the active character at the start (attacker will be the first active character)
        activeCharacterBattle = attackCharacterBattle;
        state = State.WaitingForPlayer;
    }

    private void Update()
    {
        if (state == State.WaitingForPlayer)
        {
            // Mouse click to select target
            if (Input.GetMouseButtonDown(0))  // Left mouse click
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    CharacterBattle selectedCharacter = hit.transform.GetComponent<CharacterBattle>();

                    if (selectedCharacter != null && selectedCharacter != activeCharacterBattle)
                    {
                        // If the selected character is not the active one, set it as the target
                        SetTargetCharacterBattle(selectedCharacter);
                        state = State.Busy;

                        // Attack the target
                        activeCharacterBattle.Attack(targetCharacterBattle, () =>
                        {
                            ChooseNextActiveCharacter();
                        });
                    }
                }
            }
        }
    }

    // Set the active character to attack
    private void SetActiveCharacterBattle(CharacterBattle characterBattle)
    {
        if (activeCharacterBattle != null)
        {
            activeCharacterBattle.HideSelectionCircle();
        }

        activeCharacterBattle = characterBattle;
        activeCharacterBattle.ShowSelectionCircle();
    }

    // Set the selected target
    private void SetTargetCharacterBattle(CharacterBattle selectedTarget)
    {
        targetCharacterBattle = selectedTarget;
    }

    // Switch to the next active character after the attack
    private void ChooseNextActiveCharacter()
    {
        if (TestBattleOver())
        {
            return;
        }

        if (activeCharacterBattle == attackCharacterBattle)
        {
            SetActiveCharacterBattle(targetCharacterBattle);
            state = State.Busy;

            targetCharacterBattle.Attack(attackCharacterBattle, () =>
            {
                ChooseNextActiveCharacter();
            });
        }
        else
        {
            SetActiveCharacterBattle(attackCharacterBattle);
            state = State.WaitingForPlayer;
        }
    }

    // Test if the battle is over
    private bool TestBattleOver()
    {
        if (attackCharacterBattle.IsDead())
        {
            // Player dead, enemy wins
            battleOverWindow.ShowBattleResult(false); // Show losing message
            return true;
        }
        if (targetCharacterBattle.IsDead())
        {
            // Enemy dead, player wins
            battleOverWindow.ShowBattleResult(true); // Show winner message
            return true;
        }

        return false;
    }
}
