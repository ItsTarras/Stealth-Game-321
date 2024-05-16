using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class provides implements a basic "Red Light, Green Light"
 * camera controller, that looks towards and away from the players at
 * random intervals
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class RedLightCameraController : MonoBehaviour
{

    // The minimum time before the camera will turn
    public float minTimeBeforeTurn = .5f;

    // The maximum time before the camera will turn
    public float maxTimeBeforeTurn = 2f;

    // The speed that the camera will turn in degrees per second
    public float turnSpeed = 360;

    // The direction that the camera should point when "facing away" from the players
    public Vector3 facingAwayRotation = new Vector3(0,0,0);

    // The direction that the camera should point when "facing towards" from the players
    public Vector3 facingTowardsRotation = new Vector3(0, 180, 0);

    // How long until the next turn
    float timeNextTurn = 0;

    // How long the rotation will take
    float timeToRotate = 0;

    // Will the next turn be towards or away from the player?
    bool turnTowards = true;

    // Start is called before the first frame update
    void Start()
    {
        // Randomly generate a time for the next turn
        timeNextTurn = Time.realtimeSinceStartup + Random.Range(minTimeBeforeTurn, maxTimeBeforeTurn);

        // calculate how long it will take to finish the rotation
        timeToRotate = Quaternion.Angle(Quaternion.Euler(facingTowardsRotation), Quaternion.Euler(facingAwayRotation)) / turnSpeed;

        // Start facing away from the players
        transform.localRotation = Quaternion.Euler(facingAwayRotation);

        // Set the next turn to be towards the player
        turnTowards = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Don't execute if it's not time to turn yet
        if (Time.realtimeSinceStartup >= timeNextTurn)
        {
            // If we're turning towards the players
            if (turnTowards)
                // Rotate the camera towards the "facing towards" direction
                // at the turn speed per frame
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(facingTowardsRotation), Time.deltaTime * turnSpeed);
            else
                // Otherwise rotate the camera towards the "facing away" direction
                // at the turn speed per frame
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(facingAwayRotation), Time.deltaTime * turnSpeed);

            
            // If the current time is beyond the start turn time plus how long it will take to rotate
            if (Time.realtimeSinceStartup >= timeNextTurn + timeToRotate)
            {
                // Randomly generate a time for the next turn
                timeNextTurn = Time.realtimeSinceStartup + Random.Range(minTimeBeforeTurn, maxTimeBeforeTurn);

                // calculate how long it will take to finish the rotation
                timeToRotate = Quaternion.Angle(Quaternion.Euler(facingTowardsRotation), Quaternion.Euler(facingAwayRotation)) / turnSpeed;

                // Turn the opposite direction next time
                turnTowards = !turnTowards;
            }
        }
    }
}
