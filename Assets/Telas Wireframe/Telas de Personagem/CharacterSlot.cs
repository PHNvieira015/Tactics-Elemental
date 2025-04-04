using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public bool isSelected;
    private GameObject sel;
    public Unit holdUnit;
    public CharacterStat holdingUnitStats;
    public Image highlight;

    private void Update()
    {
        /*sel = EventSystem.current.currentSelectedGameObject;
        if (sel != gameObject && isSelected)
        {
            HighlightedCharacter.instance.gameObject.SetActive(false);
            isSelected = false;
        }*/
    }

    public void UpdateHighlightedCharacter()
    {
        isSelected = true;
        HighlightedCharacter.instance.UpdateThings(transform.GetComponent<Image>(), holdingUnitStats, transform.GetComponentInChildren<TextMeshProUGUI>().text);
        HighlightedCharacter.instance.gameObject.SetActive(true);
        HighlightedCharacter.instance.unit = holdUnit;
        HighlightedCharacter.instance.stat = holdingUnitStats;
    }
}
