using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum gender { Masculino, Femenino }

public class NameSelector : MonoBehaviour
{
    public TextMeshProUGUI nameBox;
    public bool nameSelected;
    public bool genderSelected;
    public gender Gender;
    public Button Masculino;
    public Button Femenino;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (nameBox.text != null && nameSelected && genderSelected)
            {
                DialogueManager.ProtagonistName = nameBox.text;
                DialogueManager.ProtagonistGender = Gender;
                DialogueManager.SiblingGender = Gender == gender.Masculino ? gender.Femenino : gender.Masculino;
                SceneLoader.Instance.LoadNextScene();
            }
        }
    }

    public void ChangeGender(string gender)
    {
        if(gender == "Masculino")
        {
            Femenino.interactable = true;
            Masculino.interactable = false;
            Gender = global::gender.Masculino;
        }
        if(gender == "Femenino")
        {
            Masculino.interactable = true;
            Femenino.interactable = false;
            Gender = global::gender.Femenino;
        }
        genderSelected = true;
    }

    public void updateNameSelect()
    {
        if (nameSelected)
        {
            nameSelected = false;
        }
        if (!nameSelected)
        {
            nameSelected = true;
        }
    }
}