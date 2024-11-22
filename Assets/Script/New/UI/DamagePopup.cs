using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private static DamagePopup Instance;
    private TextMeshPro textMesh;  // Already correctly declared
     private Color textColor;
    [SerializeField] private float disappearTimer;
    [SerializeField] private int popupSpeed;
   

    public static DamagePopup Create(Vector3 position, int damageAmount)
    {
        Debug.Log($"Creating DamagePopup at position: {position}, with damage: {damageAmount}");

        // Instantiate the DamagePopup prefab and get the transform.
        Transform damagePopupTransform = Instantiate(GameAssets.instance.DamagePopup, position, Quaternion.identity).transform;

        // Log the instantiated object
        Debug.Log("DamagePopup instantiated.");

        // Get the DamagePopup component.
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        if (damagePopup == null)
        {
            Debug.LogError("DamagePopup component is missing from the prefab.");
        }

        // Set up the damage amount.
        damagePopup.Setup(damageAmount);

        // Return the created DamagePopup.
        return damagePopup;
    }

    private void Awake()
    {
        // Corrected the type name to TextMeshPro with uppercase 'T' and 'M'.
        textMesh = transform.GetComponent<TextMeshPro>();

        if (textMesh == null)
        {
            Debug.LogError("TextMeshPro component is missing from the DamagePopup prefab.");
        }
        else
        {
            Debug.Log("TextMeshPro component found.");
        }
    }

    public void Setup(int damageAmount)
    {
        // Set the damage amount as text in the popup.
        Debug.Log($"Setting damage amount: {damageAmount}");
        textMesh.SetText(damageAmount.ToString());
        textColor = textMesh.color;
        disappearTimer = 1f; // You might want to tweak this value to change how long the popup stays visible.
    }

    private void Update()
    {
        // Corrected `Time.deltaTime` capitalization.
        float moveYSpeed = popupSpeed;
        transform.position += new Vector3(popupSpeed, popupSpeed) * Time.deltaTime;

        // Handle the disappearing of the text
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;  // Fading out the text
            textMesh.color = textColor;

            // Ensure the alpha value doesn't go below zero
            if (textColor.a < 0)
            {
                Destroy(gameObject);  // Destroy the DamagePopup when it's completely invisible
            }
        }
    }
}
