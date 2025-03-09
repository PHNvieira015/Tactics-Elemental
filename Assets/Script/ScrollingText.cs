using UnityEngine;
using TMPro;

public class ScrollingText : MonoBehaviour
{
    public RectTransform textRect;  // Assign the Text's RectTransform
    public float scrollSpeed = 30f;

    private float startX, endX;
    private bool isPaused = false;

    void Start()
    {
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        float parentWidth = parentRect.rect.width;
        float textWidth = textRect.rect.width;

        if (textWidth <= parentWidth)
        {
            Debug.LogWarning("Text is too small to scroll! Increase the text width.");
            enabled = false;
            return;
        }

        // Calculate positions for right-to-left scrolling
        startX = parentWidth;       // Start with text's left edge at parent's right
        endX = -textWidth;           // End when text's right edge reaches parent's left

        // Initialize position
        textRect.anchoredPosition = new Vector2(startX, textRect.anchoredPosition.y);
    }

    void Update()
    {
        if (isPaused) return;

        // Move text to the left
        Vector2 newPos = textRect.anchoredPosition;
        newPos.x -= scrollSpeed * Time.deltaTime;

        // Reset position when text fully exits to the left
        if (newPos.x <= endX)
        {
            newPos.x = startX;
        }

        textRect.anchoredPosition = newPos;
    }

    public void PauseScrolling() => isPaused = true;
    public void ResumeScrolling() => isPaused = false;
}