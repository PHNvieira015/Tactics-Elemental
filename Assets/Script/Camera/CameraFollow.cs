using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform SelectedUnit;  // The unit the camera will follow
    public float stopThreshold = 0.1f;  // Distance threshold to determine if the unit is moving
    private Vector3 lastPosition;  // To track the unit's position from the previous frame

    private void Start()
    {
        // Initialize the camera position if a unit is selected at the start
        if (SelectedUnit != null)
        {
            lastPosition = SelectedUnit.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only move the camera if the unit is selected
        if (SelectedUnit != null && SelectedUnit.GetComponent<Unit>().selected)
        {
            // Check if the unit has moved (not just a small movement)
            float distanceMoved = Vector3.Distance(SelectedUnit.position, lastPosition);

            if (distanceMoved > stopThreshold)
            {
                // Move the camera to follow the selected unit
                transform.position = SelectedUnit.position + new Vector3(0, 1, -5);
                lastPosition = SelectedUnit.position;  // Update the last known position
            }
            else if (SelectedUnit.GetComponent<Unit>().hasMoved)
            {
                // If the unit has moved and stopped, we stop updating the camera
                lastPosition = SelectedUnit.position;  // Final update to camera position
            }
        }
    }
}
