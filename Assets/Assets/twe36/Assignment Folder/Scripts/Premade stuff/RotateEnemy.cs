using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RotateEnemy : MonoBehaviour
{
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float rotationRange = 90f;

    private float initialYRotation;

    private void Start()
    {
        initialYRotation = transform.eulerAngles.y;
    }

    private void FixedUpdate()
    {
        if (rotate)
        {
            float newYRotation = initialYRotation + Mathf.Sin(Time.time * rotationSpeed) * rotationRange;
            transform.rotation = Quaternion.Euler(0f, newYRotation, 0f);
        }
    }
}