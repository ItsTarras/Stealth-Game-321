using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace twe36
{

    public class EnemyMovement : MonoBehaviour
    {
        //Set up waypoints for the security to go between, and rotate slowly to each one.
        //At each end, stare off into the direction for a few seconds, then go back.
        [SerializeField] List<Transform> wayPoints = new List<Transform>();
        private Transform currentWaypointTarget;
        private Rigidbody rb;

        //Move forward, or backwards?

        private bool movingForward = true;
        private bool atWaypoint = false;
        public float waitTime = 2f;
        private float timer = 0f;
        [SerializeField] private bool debugDirection = false;
        public int currentWaypointIndex = 0;

        void Start()
        {
            rb = GetComponent<Rigidbody>();

            //Get the distance from this point.
            float wayPointDist = Mathf.Infinity;


            //Initialise the waypoints.
            for (int i = 0; i < wayPoints.Count; i++)
            {
                Transform t = wayPoints[i];

                //Calculate distance to the player.
                if (Vector3.Distance(transform.position, t.position) <= wayPointDist)
                {
                    wayPointDist = Vector3.Distance(transform.position, t.position);
                    currentWaypointTarget = t;
                    currentWaypointIndex = i;
                }
            }
        }

        void FixedUpdate()
        {
            if (atWaypoint)
            {
                timer += Time.deltaTime;
                if (timer >= waitTime || (currentWaypointIndex != 0 && currentWaypointIndex != wayPoints.Count - 1))
                {
                    timer = 0f;
                    SetNextWaypoint();
                    atWaypoint = false;
                }
            }
            else
            {
                MoveToWaypoint();
            }

            if (debugDirection)
            {
                DebugRaycastFromFace();
            }
        }


        //A function that moves the character to the next waypoint.
        void MoveToWaypoint()
        {
            // Calculate direction towards the current waypoint
            Vector3 direction = (currentWaypointTarget.position - transform.position).normalized;
            rb.MovePosition(transform.position + direction * Time.deltaTime * 3);

            //Update its rotation.
            // Smoothly rotate towards the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5);


            // Check if close enough to the waypoint
            if (Vector3.Distance(transform.position, wayPoints[currentWaypointIndex].position) <= 0.1f)
            {
                atWaypoint = true;
            }
        }


        void SetNextWaypoint()
        {
            //Check if we are at the end first.
            checkAtEnd();

            //Otherwwise, we set our next way point to one further down the line (based on a direction.)
            if (movingForward)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % wayPoints.Count;
            }

            else
            {
                if (currentWaypointIndex > 0)
                {
                    currentWaypointIndex = (currentWaypointIndex - 1);

                }
            }

            //Update the current target to always be the index.
            currentWaypointTarget = wayPoints[currentWaypointIndex];
        }


        void checkAtEnd()
        {
            //Checks if we are at the end or the start.
            if (currentWaypointIndex == wayPoints.Count - 1)
            {
                movingForward = false;
            }
            else if (currentWaypointIndex == 0)
            {
                movingForward = true;
            }
        }


        //Debugging function:
        void DebugRaycastFromFace()
        {
            // Calculate the direction the enemy is facing
            Vector3 forward = transform.forward;

            // Define the length of the raycast line
            float rayLength = 5f; // Adjust as needed

            // Cast a ray from the face of the enemy in the forward direction
            RaycastHit hit;
            if (Physics.Raycast(transform.position, forward, out hit, rayLength))
            {
                // If the ray hits something, draw a red line to visualize the raycast
                Debug.DrawRay(transform.position, forward * hit.distance, Color.red);
            }
            else
            {
                // If the ray doesn't hit anything, draw a green line to visualize the raycast
                Debug.DrawRay(transform.position, forward * rayLength, Color.green);
            }
        }
    }

}