using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterList : MonoBehaviour
{
    public List<Unit> playerAvailableUnits;
    public GameObject prefab;
    public Vector3 vector3;

    private void Start()
    {
        foreach(var unit in playerAvailableUnits)
        {
            GameObject characterSlot = Instantiate(prefab, transform);
            characterSlot.transform.GetComponent<RectTransform>().anchoredPosition = vector3;
            characterSlot.transform.GetComponent<Image>().sprite = unit.transform.Find("UnitSprite")?.GetComponent<SpriteRenderer>().sprite;
            characterSlot.transform.GetComponentInChildren<TextMeshProUGUI>().text = unit.unitName;
            characterSlot.transform.GetComponent<CharacterSlot>().holdUnit = unit;
            characterSlot.transform.GetComponent<CharacterSlot>().holdingUnitStats = unit.GetComponent<CharacterStat>();
            vector3.x += 185;
        }
    }
}
