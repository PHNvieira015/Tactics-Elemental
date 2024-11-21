using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CameraFollow cameraFollow;
    public Transform cursorTransform;
    public Transform selectedUnitTransform;
    // Start is called before the first frame update
    void Start()
    {
        cameraFollow.Setup(() => cursorTransform.position);
        //follow player instead of mouse, gonna need that for enemy turn and when a unit is selected
        //cameraFollow.SetGetCameraFollowPositionFunc(() => selectedUnitTransform.position);
    }
}