using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform SelectedUnit;  // The unit the camera will follow
    public float stopThreshold;  // Distance threshold to determine if the unit is moving
    private Vector3 lastPosition;  // To track the unit's position from the previous frame
    private bool isInstantFollow = false; // Flag to control instant follow when mouse is clicked
    private float originalStopThreshold;  // Store the original value of stopThreshold
    public LayerMask gridLayer;  // The layer mask to define the grid object(s)
    [Serializefield] private float WaitTimeToMoveCamera;
    private void Start()
    {
        // Store the original value of stopThreshold
        originalStopThreshold = stopThreshold;

        // Initialize the camera position if a unit is selected at the start
        if (SelectedUnit != null)
        {
            lastPosition = SelectedUnit.position;
        }
        else
        {
            Debug.LogWarning("No unit selected for camera follow", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only proceed if a unit is selected
        if (SelectedUnit != null)
        {
            // Raycast from the mouse position to check if it's over the grid
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the raycast hits an object on the grid layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayer))
            {
                // Mouse click within the grid
                if (Input.GetMouseButtonDown(0))
                {
                    // Start the instant follow process and trigger the coroutine
                    isInstantFollow = true;
                    stopThreshold = 0;  // Temporarily set stopThreshold to 0 for instant follow
                    StartCoroutine(ResetFollowStateAfterDelay());  // Start the coroutine to reset follow state after delay
                }
            }

            // Get the desired position for the camera based on the selected unit's position
            Vector3 desiredPosition = SelectedUnit.position + new Vector3(0, 1, -5);

            // Calculate the distance moved by the unit
            float distanceMoved = Vector3.Distance(SelectedUnit.position, lastPosition);

            // If stopThreshold is 0, move the camera immediately
            if (isInstantFollow)
            {
                // Smoothly move the camera to follow the unit (instant update on click)
                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5f);
                lastPosition = SelectedUnit.position;  // Update the last known position
            }
            else if (distanceMoved > stopThreshold)
            {
                // Smoothly move the camera to follow the unit
                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5f);
                lastPosition = SelectedUnit.position;  // Update the last known position
            }
        }
    }

    // Coroutine to reset follow state after a delay
    private IEnumerator ResetFollowStateAfterDelay()
    {
        // Wait for 1 second before resetting
        yield return new WaitForSeconds(WaitTimeToMoveCamera);

        // After 1 second, reset the follow state
        isInstantFollow = false;
        stopThreshold = originalStopThreshold;  // Reset to the original stopThreshold value
    }
}

internal class SerializefieldAttribute : Attribute
{
}