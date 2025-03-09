using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CameraFollow cameraFollow;

    void Start()
    {
        // Set the camera to follow the mouse
        cameraFollow.Setup(() => Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    void Update()
    {
        // Update the camera to follow the mouse position in world space
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f; // Ensure that the z-coordinate remains constant

        // Update the camera follow position dynamically
        cameraFollow.SetGetCameraFollowPositionFunc(() => mouseWorldPosition);
    }
}
