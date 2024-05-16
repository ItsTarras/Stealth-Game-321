using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingPlaceCameraController : MonoBehaviour
{
    public float rotationSpeed = 5.0f;
    public float minAngle = -90.0f; // Minimum angle
    public float maxAngle = 90.0f; // Maximum angle 

    private float _currentRotation = 0.0f;

    void Update()
    {
        // Get mouse input for rotation
        float rotationInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

        // Calculate new rotation
        _currentRotation += rotationInput;

        // Apply rotation to the camera
        transform.localRotation = Quaternion.Euler(0, _currentRotation, 0);
    }
}
