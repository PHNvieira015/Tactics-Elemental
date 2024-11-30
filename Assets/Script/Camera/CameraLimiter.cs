using UnityEngine;

public class CameraLimit : MonoBehaviour
{
    public Camera mainCamera; // The camera to be controlled
    public Vector3 mapMin; // The minimum bounds of the camera movement (world space)
    public Vector3 mapMax; // The maximum bounds of the camera movement (world space)

    private Vector3 targetPosition;

    void Start()
    {
        // Initialize the camera's target position to its current position
        targetPosition = transform.position;
    }

    void Update()
    {
        // Step 2: Clamp the target position within the defined X and Y limits
        targetPosition.x = Mathf.Clamp(targetPosition.x, mapMin.x, mapMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, mapMin.y, mapMax.y);

        // Step 3: Apply the clamped target position back to the camera
        transform.position = targetPosition;
    }
}
