using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Func<Vector3> GetCameraFollowPosition;  // Delegate to get the follow position
    [SerializeField] private float moveThreshold = 1f;  // Dead zone threshold (adjust as needed)
    [SerializeField] private float cameraMoveSpeed = 2f;  // Adjustable camera move speed
    private Vector3 centerPosition;  // The center position of the camera's view

    // Camera limits for X and Y movement (set in inspector)
    public Vector3 mapMin;
    public Vector3 mapMax;

    // Setup function to assign the follow target (delegate)
    public void Setup(Func<Vector3> cameraFollowPosition)
    {
        this.GetCameraFollowPosition = cameraFollowPosition;
    }

    // Set function to dynamically change the follow target
    public void SetGetCameraFollowPositionFunc(Func<Vector3> GetCameraFollowPosition)
    {
        this.GetCameraFollowPosition = GetCameraFollowPosition;
    }

    void Start()
    {
        if (GetCameraFollowPosition == null)
        {
            Debug.LogError("GetCameraFollowPosition is not assigned. Please set the camera follow position function.");
        }

        centerPosition = transform.position; // The center of the camera's view in world space
    }

    void Update()
    {
        if (GetCameraFollowPosition == null) return;

        // Get the target position (mouse or object to follow)
        Vector3 cameraFollowPosition = GetCameraFollowPosition();
        cameraFollowPosition.z = transform.position.z;  // Keep the camera's z-position the same

        // Calculate the distance from the center of the camera's view
        Vector3 screenCenter = transform.position; // This is where the camera is currently located
        float distanceFromCenter = Vector3.Distance(cameraFollowPosition, screenCenter);

        // If the target position is within the threshold, don't move the camera
        if (distanceFromCenter > moveThreshold)
        {
            // Step 1: Clamp the target position before moving the camera
            Vector3 clampedPosition = cameraFollowPosition;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, mapMin.x, mapMax.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, mapMin.y, mapMax.y);
            clampedPosition.z = transform.position.z;  // Keep the z-position the same

            // Step 2: Move the camera smoothly towards the clamped position
            Vector3 newCameraPosition = Vector3.Lerp(transform.position, clampedPosition, cameraMoveSpeed * Time.deltaTime);

            // Apply the final camera position (smoothly moved and clamped)
            transform.position = newCameraPosition;
        }
    }
}
