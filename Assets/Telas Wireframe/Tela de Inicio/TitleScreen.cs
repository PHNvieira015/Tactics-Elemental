using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    public GameObject[] clearOptions;
    public GameObject options;

    public void StartGame()
    {
        SceneManager.LoadScene("Dialogue/Sinar");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Dialogue/Sinar 1");
    }

    public void Options()
    {
        foreach(GameObject @object in clearOptions)
        {
            @object.SetActive(false);
        }
        options.SetActive(true);
    }

    public void Credits()
    {
        // SceneManager.LoadScene("Credits");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
