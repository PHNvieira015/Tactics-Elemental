using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LevelWindow : MonoBehaviour
{

    [Serializefield] public TextMeshProUGUI levelText;
    [Serializefield] public Image experinceBarImage;
    [Serializefield] public Button ButtonUI_5xp;
    [Serializefield] public Button ButtonUI_10xp;
    [Serializefield] public Button ButtonUI_50xp;

    private LevelSystem levelSystem;


private void Awake()
    {
        ButtonUI_5xp.onClick.AddListener(() => levelSystem.AddExperience(5));
        ButtonUI_10xp.onClick.AddListener(() => levelSystem.AddExperience(10));
        ButtonUI_50xp.onClick.AddListener(() => levelSystem.AddExperience(50));



    }

        private void SetExperinceBarSize(float experienceNormalized)
    {
        experinceBarImage.fillAmount= experienceNormalized;
    }
    private void SetLevelNumber (int levelNumber)
    {
        levelText.text = "Level\n" + (levelNumber+1);
    }
    public void SetLevelSystem(LevelSystem levelSystem)
    {
        //Set the LevelSystem object
        this.levelSystem = levelSystem;

        //Update the starting values
        SetLevelNumber(levelSystem.GetLelvelNumber());
        SetExperinceBarSize(levelSystem.GetexperienceNormalized());

        //subscribe to the changed events
        levelSystem.OnExperinceChanged += LevelSystem_OnExperinceChanged;
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;

    }
    private void LevelSystem_OnExperinceChanged(object sender, System.EventArgs e)
    {   //Experience changed, uipdate bar size
        SetExperinceBarSize(levelSystem.GetexperienceNormalized());
    }
    private void LevelSystem_OnLevelChanged(object sender, System.EventArgs e)
    {   //Level changed, update text
        SetLevelNumber(levelSystem.GetLelvelNumber());
    }

}
