using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
    public List<Unit> turnOrderList;
    [SerializeField]private Unit selectedUnit;
    private GameMaster gameMaster;

    [Header("UI References")]
    [SerializeField] private Button[] unitButtons;
    [SerializeField] private float cameraMoveDuration = 0.2f;

    [Header("Selected Unit UI")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private TMP_Text healthText_TMP;
    [SerializeField] private Image manaBarImage;
    [SerializeField] private TMP_Text manaText_TMP;
    [SerializeField] private TMP_Text characterName_TMP;
    [SerializeField] private TMP_Text LvL_NumberText_TMP;

    private void Awake()
    {
        gameMaster = FindObjectOfType<GameMaster>();
        turnOrderList = new List<Unit>();
    }

    private void Start()
    {
        SetTurnOrderList();
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (healthBarImage) healthBarImage.fillAmount = 1f;
        if (manaBarImage) manaBarImage.fillAmount = 1f;
        if (characterName_TMP) characterName_TMP.text = "";
        if (LvL_NumberText_TMP) LvL_NumberText_TMP.text = "";
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        Vector3 targetPosition = new Vector3(
            unit.transform.position.x,
            unit.transform.position.y,
            Camera.main.transform.position.z
        );
        Camera.main.transform.position = targetPosition;
        UpdateSelectedUnitUI(unit);
    }

    private void UpdateSelectedUnitUI(Unit unit)
    {
        if (unit == null) return;

        // Update character info
        if (characterName_TMP) characterName_TMP.text = unit.characterStats.CharacterName;
        if (LvL_NumberText_TMP) LvL_NumberText_TMP.text = unit.characterStats.CharacterLevel.ToString();

        // Update health UI
        UpdateHealthUI(unit.characterStats.currentHealth, unit.characterStats.maxBaseHealth);

        // Update mana UI
        UpdateManaUI(unit.characterStats.currentMana, unit.characterStats.maxMana);
    }

    private void UpdateHealthUI(float current, float max)
    {
        if (healthBarImage)
        {
            healthBarImage.fillAmount = current / max;
        }

        if (healthText_TMP)
        {
            healthText_TMP.text = $"{current}/{max}";
        }
    }

    private void UpdateManaUI(float current, float max)
    {
        if (manaBarImage)
        {
            manaBarImage.fillAmount = current / max;
        }

        if (manaText_TMP)
        {
            manaText_TMP.text = $"{current}/{max}";
        }
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
            Transform unitSpriteTransform = unit.transform.Find("UnitSprite");
            if (unitSpriteTransform != null && unitSpriteTransform.TryGetComponent<SpriteRenderer>(out var sr))
            {
                icon.sprite = sr.sprite;
                icon.preserveAspect = true;
            }
        }

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
        }
    }

    public void RemoveUnitFromTurnOrder(Unit unit)
    {
        if (turnOrderList.Remove(unit))
        {
            UpdateTurnOrderUI();
        }
    }
}