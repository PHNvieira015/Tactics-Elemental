using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFollowUI : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        // For UI: Directly use Input.mousePosition
        transform.position = Input.mousePosition;
    }
}
