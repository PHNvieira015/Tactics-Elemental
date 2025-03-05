using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ActionBar : MonoBehaviour
{
    [SerializeField] public GameObject ActionBar;
    [SerializeField] private Button ButtonUI_ReturnTurnStart;
    [SerializeField] private Button ButtonUI_Move;
    [SerializeField] private Button ButtonUI_Attack;
    [SerializeField] private Button ButtonUI_Skill;
    [SerializeField] private Button ButtonUI_Condition;
    [SerializeField] private Button ButtonUI_Wait;
    [SerializeField] public GameObject GameObjectButton_return;
    [SerializeField] public GameObject GameObjectButton_move;
    [SerializeField] public GameObject GameObjectButton_attack;
    [SerializeField] public GameObject GameObjectButton_skill;
    [SerializeField] public GameObject GameObjectButton_condition;
    [SerializeField] public GameObject GameObjectButton_wait;

    private TurnStateManager turnStateManager;

    private void Start()
    {
        // Find the TurnStateManager instance in the scene
        turnStateManager = FindObjectOfType<TurnStateManager>();
        if (turnStateManager == null)
        {
            Debug.LogError("TurnStateManager not found in the scene!");
            return;
        }

        // Add listeners to buttons
        ButtonUI_ReturnTurnStart.onClick.AddListener(ReturnTurnStart);
        ButtonUI_Move.onClick.AddListener(OnMoveButtonClicked);
        ButtonUI_Attack.onClick.AddListener(OnAttackButtonClicked);
        ButtonUI_Skill.onClick.AddListener(OnSkillButtonClicked);
        ButtonUI_Condition.onClick.AddListener(OnConditionButtonClicked);
        ButtonUI_Wait.onClick.AddListener(OnEndTurnButtonClicked);

        ButtonUI_ReturnTurnStart.onClick.AddListener(() =>
        {
            TurnStateManager turnManager = FindObjectOfType<TurnStateManager>();
            if (turnManager != null) turnManager.OnBackButtonPressed();
        });
    }
        

    private void ReturnTurnStart()
    {
        //change position back to orignial indevelopment

        //add the go back to starting position
        Debug.Log("Returning to Turn Start...");
        turnStateManager.ChangeState(TurnStateManager.TurnState.Waiting);
        GameObjectButton_return.gameObject.SetActive(false);
        GameObjectButton_move.gameObject.SetActive(true);
        GameObjectButton_attack.gameObject.SetActive(true);
        GameObjectButton_skill.gameObject.SetActive(true);
    }

    private void OnMoveButtonClicked()
    {
        Debug.Log("Move button clicked.");
        turnStateManager.ChangeState(TurnStateManager.TurnState.Moving);
    }


    private void OnAttackButtonClicked()
    {
        //Debug.Log("Attack button clicked.");
        turnStateManager.ChangeState(TurnStateManager.TurnState.Attacking);
    }

    private void OnSkillButtonClicked()
    {
        Debug.Log("Skill button clicked.");
        turnStateManager.ChangeState(TurnStateManager.TurnState.UsingSkill);
    }

    private void OnConditionButtonClicked()
    {
        Debug.Log("Condition button clicked.");
        // Logic for condition button goes here
    }

    private void OnEndTurnButtonClicked()
    {
        Debug.Log("Wait button clicked.");
        turnStateManager.ChangeState(TurnStateManager.TurnState.EndTurn);
    }
}
