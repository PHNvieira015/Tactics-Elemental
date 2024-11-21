using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPositionMovementHandler : MonoBehaviour {

    private const float speed = 40f;
    private Vector3 targetPosition;


    private void Start() {
        Transform bodyTransform = transform.Find("Body");
    }

    private void Update() {
        }

    private void HandleMovement() {
        if (Vector3.Distance(transform.position, targetPosition) > 1f) {
            Vector3 moveDir = (targetPosition - transform.position).normalized;

            float distanceBefore = Vector3.Distance(transform.position, targetPosition);
            transform.position = transform.position + moveDir * speed * Time.deltaTime;
        } 
    }


    public void SetTargetPosition(Vector3 targetPosition) {
        targetPosition.z = 0f;
        this.targetPosition = targetPosition;
    }

}