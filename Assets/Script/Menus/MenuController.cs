using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [System.Serializable]
    public enum Screens
    {
        None,
        Main,
        Loading,
        NewGame,
        LoadGame,
        Options,
        Video,
        Audio,
        Controls,
        Credit
    }

    public Screens currentScreen;
    [SerializeField] private GameObject MainScreen;
    [SerializeField] private GameObject NewGameScreen;
    [SerializeField] private GameObject LoadGameScreen;
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private GameObject CreditScreen;
    [SerializeField] private GameObject OptionsScreen;
    [SerializeField] private GameObject VideoScreen;
    [SerializeField] private GameObject AudioScreen;
    [SerializeField] private GameObject ControlsScreen;
    [SerializeField] private GameObject MessageScreen;
    [SerializeField] private TextMeshProUGUI messageTXT;

    // You may add a loading bar for the Loading screen
    public UnityEngine.UI.Image LoadingBarFill;

    [HideInInspector] public Screens TargetScreen;

    private void Awake()
    {
        // Ensure there is only one instance of the MenuController
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keep across scenes if needed
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate if one already exists
            return;
        }

        // Explicitly set the MainScreen active at startup
        ShowScreen(Screens.Main);  // Show the Main Menu immediately when the scene starts
        //MessageScreen.SetActive(false); // Hide message screen by default
    }


    // Method to show or hide screens based on the current screen
    public void ShowScreen(Screens _screen)
    {
        Debug.Log("Showing screen: " + _screen.ToString());

        // Set the current screen
        currentScreen = _screen;

        // Deactivate all screens first
        MainScreen.SetActive(false);
        LoadingScreen.SetActive(false);
        NewGameScreen.SetActive(false);
        LoadGameScreen.SetActive(false);
        CreditScreen.SetActive(false);
        VideoScreen.SetActive(false);
        AudioScreen.SetActive(false);
        OptionsScreen.SetActive(false);
        ControlsScreen.SetActive(false);

        // Activate the desired screen based on the passed screen
        switch (currentScreen)
        {
            case Screens.None:
                MainScreen.SetActive(true);
                Debug.Log("Main screen activated");
                break;
            case Screens.Main:
                MainScreen.SetActive(true);
                Debug.Log("Main screen activated");
                break;
            case Screens.Loading:
                LoadingScreen.SetActive(true);
                break;
            case Screens.NewGame:
                NewGameScreen.SetActive(true);
                break;
            case Screens.LoadGame:
                LoadGameScreen.SetActive(true);
                break;
            case Screens.Credit:
                CreditScreen.SetActive(true);
                break;
            case Screens.Options:
                OptionsScreen.SetActive(true);
                break;
            case Screens.Video:
                VideoScreen.SetActive(true);
                break;
            case Screens.Audio:
                AudioScreen.SetActive(true);
                break;
            case Screens.Controls:
                ControlsScreen.SetActive(true);
                break;
        }
    }

    // Overloaded method to handle screen index
    public void ShowScreen(int screen)
    {
        ShowScreen((Screens)screen);
    }

    public void ShowMessage(string _message)
    {
        messageTXT.text = _message;
        MessageScreen.SetActive(true);
    }

    public void StartGame(int sceneId)
    {
        // Load the scene (change sceneId to load the desired scene)
        SceneManager.LoadScene(sceneId);
        ShowScreen(Screens.Loading); // Show loading screen during scene load
        Debug.Log("Game Started");
    }

    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }
}
