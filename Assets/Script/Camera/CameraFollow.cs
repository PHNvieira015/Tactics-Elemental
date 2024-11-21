using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Func<Vector3> GetCameraFollowPosition;  // Delegate to get the follow position
    [SerializeField] private float moveThreshold = 1f;  // Dead zone threshold (adjust as needed)
    [SerializeField] private float cameraMoveSpeed = 2f;  // Adjustable camera move speed (new variable)
    private Vector3 centerPosition;  // The center position of the camera's view

    public void Setup(Func<Vector3> cameraFollowPosition)
    {
        this.GetCameraFollowPosition = cameraFollowPosition;
    }

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

        // If the mouse (or target position) is within the threshold, don't move the camera
        if (distanceFromCenter > moveThreshold)
        {
            // Move the camera smoothly towards the target position
            Vector3 cameraMoveDir = (cameraFollowPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, cameraFollowPosition);

            // Move the camera smoothly using Lerp
            Vector3 newCameraPosition = Vector3.Lerp(transform.position, cameraFollowPosition, cameraMoveSpeed * Time.deltaTime);

            // Check if the camera overshot the target and adjust if necessary
            if (Vector3.Distance(newCameraPosition, cameraFollowPosition) < 0.1f)
            {
                newCameraPosition = cameraFollowPosition;  // Adjust to exactly match the target position
            }

            transform.position = newCameraPosition;
        }
    }
}
