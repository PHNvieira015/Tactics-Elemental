using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuTransition : MonoBehaviour
{
    [SerializeField] private MenuController.Screens targetScreen; // Target screen to show after button click
    private Button button;

    private void Start()
    {
        // Automatically get the Button component if it's on the same GameObject
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(SetTransition); // Add listener to button click
        }
        else
        {
            Debug.LogError("Button component not found on " + gameObject.name);
        }
    }

    private void SetTransition()
    {
        // Check if MenuController.instance is initialized properly
        if (MenuController.instance != null)
        {
            MenuController.instance.TargetScreen = targetScreen; // Set target screen
            Debug.Log("Transitioning to screen: " + targetScreen);
        }
        else
        {
            Debug.LogError("MenuController.instance is null. Make sure MenuController is in the scene.");
        }
    }
}
