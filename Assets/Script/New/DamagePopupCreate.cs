using UnityEngine;

public class DamagePopupCreate : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("DamagePopupCreate Start method called.");
        DamagePopup.Create(Vector3.zero, 300);
    }
}
