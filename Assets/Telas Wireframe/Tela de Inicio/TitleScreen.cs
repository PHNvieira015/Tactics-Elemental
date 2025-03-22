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
    SceneLoader.Instance.LoadNextScene();
    }

    public void LoadGame()
    {
    SceneLoader.Instance.LoadSceneByName("Main Combat Scene");
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
