using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighlightedCharacter : MonoBehaviour
{
    public static HighlightedCharacter instance;
    public TextMeshProUGUI LevelAndName;
    public TextMeshProUGUI HP;
    public TextMeshProUGUI SP;
    public Image sprite;
    public Unit unit;
    public CharacterStat stat;
    public GameObject partyOverview;
    public GameObject characterOverview;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void UpdateThings(Image image, CharacterStat characterStat, string name)
    {
        sprite.sprite = image.sprite;
        LevelAndName.text = "Lv." + characterStat.CharacterLevel + $" {name}";
        HP.text = $"HP {characterStat.maxBaseHealth}";
        SP.text = $"SP {characterStat.maxMana}";
    }

    public void CharacterOverviewVisible()
    {
        characterOverview.SetActive(true);
        partyOverview.SetActive(false);
        FindFirstObjectByType<CharacterOverview>().UpdateValues(unit, stat, sprite);
    }
}
