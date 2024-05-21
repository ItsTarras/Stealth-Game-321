using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace twe36
{

    public class RotateEnemy : MonoBehaviour
    {
        [SerializeField] private bool rotate = true;
        [SerializeField] private float rotationSpeed = 1f;
        [SerializeField] private float rotationRange = 90f;

        private Vector3 initialRotation;

        private void Start()
        {
            initialRotation = transform.eulerAngles;
        }

        private void FixedUpdate()
        {
            if (rotate)
            {
                float newYRotation = initialRotation.y + Mathf.Sin(Time.time * rotationSpeed) * rotationRange;
                transform.rotation = Quaternion.Euler(initialRotation.x, newYRotation, initialRotation.z);
            }
        }
    }

}