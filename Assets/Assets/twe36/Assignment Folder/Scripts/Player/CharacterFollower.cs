using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject characterToFollow;
    [SerializeField] Vector3 offset = new Vector3(0f, 1f, -3f); // Offset from the character's position
    [SerializeField] float lerpSpeed = 5f;
    [SerializeField] float distanceAhead = 2f; // Distance in front of the character to look at
    [SerializeField] float heightOffset = 1f; // Height offset from the character's position


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Ensure characterToFollow is not null before accessing its transform
        if (characterToFollow != null)
        {
            // Calculate the desired position behind the character
            Vector3 desiredPosition = characterToFollow.transform.position
                                      + characterToFollow.transform.forward * offset.z
                                      + characterToFollow.transform.up * offset.y
                                      + characterToFollow.transform.right * offset.x;


            // Calculate the target position in front of the character
            Vector3 targetPosition = characterToFollow.transform.position
                                     + characterToFollow.transform.forward * distanceAhead
                                     + Vector3.up * heightOffset; // Apply height offset

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, lerpSpeed);

            // Set the camera's position to the desired position
            transform.position = smoothedPosition;

            // Make the camera look at the character
            transform.LookAt(targetPosition + characterToFollow.transform.forward * 2f); // Look slightly further ahead
        }
    }
}
