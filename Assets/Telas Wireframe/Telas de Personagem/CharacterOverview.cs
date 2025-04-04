using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterOverview : MonoBehaviour
{
    public Unit unit;
    public CharacterStat stat;
    public Transform skillSlotGameObject;
    public GameObject skillSlotPrefab;
    public TextMeshProUGUI weapon;
    public TextMeshProUGUI armor;
    public TextMeshProUGUI accessory1;
    public TextMeshProUGUI accessory2;
    public GameObject slotDescription;
    public Sprite[] elements;
    public Image element;
    public Sprite[] classes;
    public Image characterClass;
    public Image characterImage;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI characterDescription;
    public TextMeshProUGUI Level;
    public TextMeshProUGUI HP;
    public TextMeshProUGUI MP;
    public TextMeshProUGUI Str;
    public TextMeshProUGUI Int;
    public TextMeshProUGUI Agi;

    public void UpdateValues(Unit _unit, CharacterStat _stat, Image _image)
    {
        characterImage.sprite = _image.sprite;
        unit = _unit;
        stat = _stat;

        characterName.text = stat.CharacterName;
        characterDescription.text = stat.CharacterDescription;
        element.sprite = elements[GetCharacterElement(stat.elementType)];
        characterClass.sprite = classes[GetCharacterClass(stat.characterClass)];
        Level.text = $"Lv. {stat.CharacterLevel}";
        HP.text = $"HP {stat.maxBaseHealth}";
        MP.text = $"MP {stat.maxMana}";
        Str.text = $"Str {stat.strength}";
        Int.text = $"Int {stat.intellect}";
        Agi.text = $"Agi {stat.agility}";

        UpdateEquipmentSlot(weapon, stat.equippedWeapon);
        UpdateEquipmentSlot(armor, stat.equippedArmor);
        UpdateEquipmentSlot(accessory1, stat.accessory1);
        UpdateEquipmentSlot(accessory2, stat.accessory2);

        foreach (var skill in unit.skillslist)
        {
            GameObject newSkill = Instantiate(skillSlotPrefab, skillSlotGameObject);
            newSkill.transform.GetComponentInChildren<TextMeshProUGUI>().text = skill.Name;
            newSkill.GetComponent<Slot>().description = skill.Description;

            Image element = newSkill.transform.GetComponentInChildren<Image>();
            int elementIndex = GetElementIndex(skill.elementType);

            if (elementIndex == -1)
            {
                element.gameObject.SetActive(false);
            }
            else
            {
                element.sprite = elements[elementIndex];
            }
        }
    }

    private void UpdateEquipmentSlot(TextMeshProUGUI slot, Equipment equipment)
    {
        if (equipment == null)
        {
            slot.gameObject.SetActive(false);
            return;
        }

        slot.gameObject.GetComponent<Slot>().description = equipment.itemDescription;
        int maxStat = GetBiggerStat(equipment);

        slot.text = equipment.itemName;

        if (maxStat > 0)
        {
            slot.text += $" {StatType(equipment)} +{maxStat}";
        }

        if (string.IsNullOrEmpty(slot.text))
        {
            slot.gameObject.SetActive(false);
        }
    }

    private int GetCharacterElement(CharacterStat.ElementType elementType)
    {
        return elementType switch
        {
            CharacterStat.ElementType.Wind => 0,
            CharacterStat.ElementType.Thunder => 1,
            CharacterStat.ElementType.Fire => 2,
            CharacterStat.ElementType.Earth => 3,
            CharacterStat.ElementType.Water => 4,
            _ => -1
        };
    }

    private int GetCharacterClass(CharacterStat.CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterStat.CharacterClass.Warrior => 0,
            CharacterStat.CharacterClass.Archer => 1,
            CharacterStat.CharacterClass.Mage => 2,
            _ => -1
        };
    }

    private int GetElementIndex(Skill.ElementType elementType)
    {
        return elementType switch
        {
            Skill.ElementType.Wind => 0,
            Skill.ElementType.Thunder => 1,
            Skill.ElementType.Fire => 2,
            Skill.ElementType.Earth => 3,
            _ => -1
        };
    }

    public int GetBiggerStat(Equipment equipment)
    {
        return Mathf.Max(equipment.strengthBonus, equipment.intellectBonus, equipment.agilityBonus);
    }

    public string StatType(Equipment equipment)
    {
        int maxStat = Mathf.Max(equipment.strengthBonus, equipment.intellectBonus, equipment.agilityBonus);
        if (maxStat == equipment.strengthBonus)
        {
            return "STR";
        }
        else if (maxStat == equipment.intellectBonus)
        {
            return "INT";
        }
        else if(maxStat == equipment.agilityBonus)
        {
            return "AGI";
        }
        else
        {
            return "";
        }
    }
}
