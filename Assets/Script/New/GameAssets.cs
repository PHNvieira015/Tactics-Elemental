using UnityEngine;

public class GameAssets : MonoBehaviour
{
    // Private static field to hold the instance
    private static GameAssets _instance;

    // Public static property to access the instance
    public static GameAssets instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameAssets>(); // Find the instance of GameAssets in the scene

                if (_instance == null)
                {
                    Debug.LogError("GameAssets instance not found in the scene.");
                }
            }
            return _instance;
        }
    }

    // Reference to the DamagePopup prefab (assign in the Inspector)
    public Transform DamagePopup;

    // This is called when the GameAssets script is first initialized
    private void Start()
    {
        if (DamagePopup == null)
        {
            Debug.LogError("DamagePopup prefab is not assigned in GameAssets! Please assign it in the Inspector.");
        }
    }
}
