using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class provides implements a basic "Red Light, Green Light"
 * player controller, that will move when the player presses a defined key
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class RedLightPlayer : MonoBehaviour
{
    // The key which will control our player
    public KeyCode moveKey;

    // The speed that the player is currently moving
    public float currentSpeed = 0;

    // The players max speed
    public float maxSpeed = 10f;

    // The speed the player accelerates at when we press the key
    public float accelRate = 10f;

    // The speed the player slows down at when we release the key
    public float deccelRate = 3f;

    // The direction our player will move
    public Vector3 moveDirection = Vector3.right;

    // Store the frustum cull script
    FrustumCull frustumCull;

    // Can the player move?
    bool canMove = true;

    AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        // Get the frustum cull script
        frustumCull = FindObjectOfType<FrustumCull>();

        audio = gameObject.GetComponent<AudioSource>();
        // Set the player to be able to move in the first instance
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        // If we can't move, return
        if (canMove)
        {
            // If the player is pressing the move key
            if (Input.GetKey(moveKey))
            {
                if(!audio.isPlaying)
                {
                    audio.Play();
                }
                
                // Accelerate our current speed towards the max speed
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, accelRate);
            }
            else
            {
                // If the player is releasing the move key
                // Decelerate our current speed towards 0
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deccelRate);
                audio.Stop();
            }
                

            transform.Translate(moveDirection * currentSpeed * Time.deltaTime);

            // Try and get the visibility sphere which is attached to this player
            if (transform.Find("VisibilitySphere") != null)
            {
                // If we've found it, get its material
                Material sphereMaterial = transform.Find("VisibilitySphere").GetComponent<Renderer>().material;

                if (sphereMaterial != null)
                {
                    int i = 0;
                    foreach (GameObject go in frustumCull.gameObjectsInFrustum)
                    {
                        //If we are in view of the camera.
                        if (go == gameObject)
                        {
                            //If we are moving.
                            if (currentSpeed != 0)
                            {
                                sphereMaterial.color = Color.grey;
                            }

                            //If we are not moving.
                            else
                            {
                                sphereMaterial.color = Color.red;
                            }
                        }
                        else
                        {
                            sphereMaterial.color = Color.blue;
                        }
                    }

                }
                /*****
                 * TODO: Check to see this gameobject is in the FrustumCull's list of objects in view and 
                 * whether the player is moving (i.e. have a current speed > 0). 
                 * If in view AND moving, set sphere material to gray and disable can move
                 * If in view AND NOT moving, set sphere material to red
                 * If not in view, set sphere material to blue
                 *****/
            }
        }
    }

    
}
