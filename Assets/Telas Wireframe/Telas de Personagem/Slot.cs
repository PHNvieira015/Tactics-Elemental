using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public string description;

    public void ShowDescription()
    {
        GameObject slotDescription = FindAnyObjectByType<CharacterOverview>().slotDescription;
        slotDescription.SetActive(true);
        slotDescription.GetComponentInChildren<TextMeshProUGUI>().text = description;
    }
}
