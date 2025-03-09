using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFollowUI : MonoBehaviour
{
    private RectTransform cursorRectTransform;
    private Canvas canvas;

    private void Start()
    {
        // Hide the system cursor
        Cursor.visible = false;

        // Get the RectTransform component of the cursor
        cursorRectTransform = GetComponent<RectTransform>();

        // Ensure the RectTransform's pivot is set to the center
        cursorRectTransform.pivot = new Vector2(0.5f, 0.5f);  // Centered pivot

        // Get the Canvas component to convert screen coordinates to canvas space
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        //transform.position = Input.mousePosition;
        // Get the mouse position in screen space
        Vector2 screenPos = Input.mousePosition;

        // Convert screen position to the canvas's local position
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, screenPos, canvas.worldCamera, out localPos);

        // Update the cursor's position within the local canvas space
        cursorRectTransform.localPosition = localPos;

    }
}
