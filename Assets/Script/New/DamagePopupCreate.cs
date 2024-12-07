using UnityEngine;

public class DamagePopupCreate : MonoBehaviour
{
    private void Start()
    {
        DamagePopup.Create(Vector3.zero, 300);
    }
}
