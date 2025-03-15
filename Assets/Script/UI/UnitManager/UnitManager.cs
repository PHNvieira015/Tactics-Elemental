using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
    public List<Unit> turnOrderList;
    [SerializeField] private Unit selectedUnit;
    private GameMaster gameMaster;
    [SerializeField] private Camera mainCamera;

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

    [Header("Element & Class Icons")]
    [SerializeField] private Image elementIcon;
    [SerializeField] private Image classIcon;

    [System.Serializable]
    public struct ElementSpritePair
    {
        public CharacterStat.ElementType element;
        public Sprite sprite;
    }

    [System.Serializable]
    public struct ClassSpritePair
    {
        public CharacterStat.CharacterClass characterClass;
        public Sprite sprite;
    }

    [Header("Icon Mappings")]
    [SerializeField] private ElementSpritePair[] elementSprites;
    [SerializeField] private ClassSpritePair[] classSprites;

    private void Awake()
    {
        gameMaster = FindObjectOfType<GameMaster>();
        turnOrderList = new List<Unit>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        SetTurnOrderList();
        InitializeUI();
    }
    public void MoveCameraToUnit(Unit unit)
    {
        if (unit == null) return;

        Vector3 targetPosition = new Vector3(
            unit.transform.position.x,
            unit.transform.position.y,
            mainCamera.transform.position.z
        );
        mainCamera.transform.position = targetPosition;
        StartCoroutine(SmoothCameraMove(targetPosition));
    }
    private IEnumerator SmoothCameraMove(Vector3 targetPosition)
    {
        float duration = 0.5f;
        Vector3 startPosition = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsed / duration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
    }
    private void InitializeUI()
    {
        if (healthBarImage) healthBarImage.fillAmount = 1f;
        if (manaBarImage) manaBarImage.fillAmount = 1f;
        if (characterName_TMP) characterName_TMP.text = "";
        if (LvL_NumberText_TMP) LvL_NumberText_TMP.text = "";
        if (elementIcon) elementIcon.gameObject.SetActive(false);
        if (classIcon) classIcon.gameObject.SetActive(false);
    }

    #region TurnOrder/UnitSelection
    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        Vector3 targetPosition = new Vector3(
            unit.transform.position.x,
            unit.transform.position.y,
            Camera.main.transform.position.z
        );
        if (Input.GetMouseButtonDown(0))
        {
            Camera.main.transform.position = targetPosition;
        }
        UpdateSelectedUnitUI(unit);
    }

    public void SetTurnOrderList()
    {
        turnOrderList.Clear();
        turnOrderList.AddRange(GameMaster.instance.playerList);
        turnOrderList.AddRange(GameMaster.instance.spawnedUnits);
        turnOrderList.Sort((x, y) => y.characterStats.RoundInitiative.CompareTo(x.characterStats.RoundInitiative));
        UpdateTurnOrderUI();
    }

    public void RemoveUnitFromTurnOrder(Unit unit)
    {
        if (turnOrderList.Remove(unit))
        {
            // Find the index of the unit in the turnOrder list
            int unitIndex = turnOrderList.IndexOf(unit);

            // Hide or destroy the button associated with this unit
            if (unitIndex >= 0 && unitIndex < unitButtons.Length)
            {
                unitButtons[unitIndex].gameObject.SetActive(false); // or Destroy(unitButtons[unitIndex].gameObject);
            }

            // Update the UI to reflect the removal of the unit
            UpdateTurnOrderUI();
        }
    }
    #endregion

    #region UI
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

        // Update icons
        UpdateElementIcon(unit.characterStats.elementType);
        UpdateClassIcon(unit.characterStats.characterClass);
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

    void UpdateTurnOrderUI()
    {
        for (int i = 0; i < unitButtons.Length; i++)
        {
            Button btn = unitButtons[i];
            bool hasUnit = i < turnOrderList.Count;
            btn.gameObject.SetActive(hasUnit);

            if (!hasUnit)
            {
                // Hide and remove button for the unit if not present in the list
                btn.gameObject.SetActive(false);  // You can also destroy the button if required
                continue;
            }

            Unit unit = turnOrderList[i];
            if (unit == null)
            {
                // If the unit is null, remove the corresponding button from UI
                btn.gameObject.SetActive(false);
                continue;
            }

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
        {
            if (unitIndex < turnOrderList.Count)
            {
                Unit unit = turnOrderList[unitIndex];
                MoveCameraToUnit(unit);
            }
        }
    }

    private void UpdateElementIcon(CharacterStat.ElementType element)
    {
        if (elementIcon == null) return;

        foreach (var pair in elementSprites)
        {
            if (pair.element == element)
            {
                elementIcon.sprite = pair.sprite;
                elementIcon.gameObject.SetActive(true);
                return;
            }
        }
        elementIcon.gameObject.SetActive(false);
    }

    private void UpdateClassIcon(CharacterStat.CharacterClass characterClass)
    {
        if (classIcon == null) return;

        foreach (var pair in classSprites)
        {
            if (pair.characterClass == characterClass)
            {
                classIcon.sprite = pair.sprite;
                classIcon.gameObject.SetActive(true);
                return;
            }
        }
        classIcon.gameObject.SetActive(false);
    }
    #endregion
}