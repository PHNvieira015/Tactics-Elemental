using UnityEngine;
using UnityEngine.UI;

public class BattleOverWindow : MonoBehaviour
{
    // References to the text elements
    public GameObject WinnerText; // Assign in the inspector
    public GameObject LosingText; // Assign in the inspector
    public GameObject battleOverWindow; // Reference to the window itself
    [SerializeField]public bool playerWon;

    // Call this method when the battle ends to show the appropriate text
    public void ShowBattleResult(bool playerWon)
    {
        // Make sure the BattleOverWindow is active
        battleOverWindow.SetActive(true);

        // Toggle visibility of winner and loser text based on result
        if (playerWon)
        {//player wins
            WinnerText.gameObject.SetActive(true); // Show WinnerText
            LosingText.gameObject.SetActive(false); // Hide LosingText
        }
        else
        {//player loses
            WinnerText.gameObject.SetActive(false); // Hide WinnerText
            LosingText.gameObject.SetActive(true); // Show LosingText
        }
    }
}
