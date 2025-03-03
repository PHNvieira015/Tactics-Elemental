using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
    public List<Unit> turnOrderList;
    private Unit activeUnit;
    private Unit selectedUnit;
    private GameMaster gameMaster;
    private Unit mouseOverUnit;

    [Header("UI References")]
    [SerializeField] private Button[] unitButtons;
    [SerializeField] private float cameraMoveDuration = 0.2f;

    private void Awake()
    {
        gameMaster = FindObjectOfType<GameMaster>();
        turnOrderList = new List<Unit>();
    }

    private void Start()
    {
        SetTurnOrderList();
    }

    private void SetActiveUnit(Unit unit)
    {
        activeUnit = unit;
        Debug.Log("Active Unit set to: " + unit.name);
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        Vector3 targetPosition = new Vector3(
            selectedUnit.transform.position.x,
            selectedUnit.transform.position.y,
            Camera.main.transform.position.z
        );
        Camera.main.transform.position = targetPosition;
    }

    public void SetTurnOrderList()
    {
        turnOrderList.Clear();
        turnOrderList.AddRange(GameMaster.instance.playerList);
        turnOrderList.AddRange(GameMaster.instance.spawnedUnits);
        turnOrderList.Sort((x, y) => y.characterStats.initiative.CompareTo(x.characterStats.initiative));
        UpdateTurnOrderUI();
    }

    void UpdateTurnOrderUI()
    {
        for (int i = 0; i < unitButtons.Length; i++)
        {
            Button btn = unitButtons[i];
            bool hasUnit = i < turnOrderList.Count;
            btn.gameObject.SetActive(hasUnit);

            if (!hasUnit) continue;

            Unit unit = turnOrderList[i];
            SetupButton(btn, unit, i);
        }
    }

    private void SetupButton(Button btn, Unit unit, int index)
    {
        Image icon = btn.GetComponent<Image>();
        if (icon != null)
        {
            // Get the UnitSprite child
            Transform unitSpriteTransform = unit.transform.Find("UnitSprite");
            if (unitSpriteTransform != null && unitSpriteTransform.TryGetComponent<SpriteRenderer>(out var sr))
            {
                SpriteRenderer spriteRenderer = unitSpriteTransform.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    icon.sprite = spriteRenderer.sprite;
                    icon.preserveAspect = true;
                }
                else
                {
                    Debug.LogWarning($"Unit {unit.name} has UnitSprite child but no SpriteRenderer");
                }
            }
            else
            {
                Debug.LogWarning($"Unit {unit.name} is missing UnitSprite child object");
            }
        }
        // Set unit name text
        TMP_Text buttonText = btn.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = unit.unitName;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnUnitButtonClick(index));
    }

    void OnUnitButtonClick(int unitIndex)
    {
        if (unitIndex < turnOrderList.Count)
        {
            Unit unit = turnOrderList[unitIndex];
            SetSelectedUnit(unit);
            Debug.Log($"Selected {unit.name} at position {unit.transform.position}");
        }
    }


    public void RemoveUnitFromTurnOrder(Unit unit)
    {
        if (turnOrderList.Remove(unit))
        {
            UpdateTurnOrderUI();
            Debug.Log($"Removed {unit.name} from turn order");
        }
    }
}