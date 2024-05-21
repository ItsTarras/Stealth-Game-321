using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace twe36
{

    public class HidingScript : MonoBehaviour
    {

        public bool currentlyHiding = false;
        [SerializeField] List<GameObject> hidingPlaces = new List<GameObject>();
        //MeshRenderer meshRenderer;
        AudioSource audioSource;
        private Camera hidingCamera;
        [SerializeField] private Camera mainCamera;
        BasicCameraController controller;
        // Start is called before the first frame update
        void Start()
        {
            //meshRenderer = GetComponent<MeshRenderer>();
            controller = GetComponent<BasicCameraController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!currentlyHiding && controller.dead != true)
            {
                //Calculate the distance from the objects, and if we are close enough to one, we can push "E" to hide.
                //When we hide, just disable the mesh renderer, and move the camera to the object's camera.

                foreach (GameObject hidingSpot in hidingPlaces)
                {

                    float distanceToHidingSpot = Vector3.Distance(hidingSpot.transform.position, transform.position);

                    if (distanceToHidingSpot < 5)
                    {
                        //Allow us the option to hide.
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            currentlyHiding = true;
                            //meshRenderer.enabled = false;

                            //Play the hiding sound.
                            audioSource = hidingSpot.GetComponent<AudioSource>();
                            audioSource.pitch = Random.Range(0.8f, 1.1f);
                            audioSource.volume = Random.Range(0.8f, 1f);
                            audioSource.Play();

                            mainCamera.enabled = false;
                            hidingCamera = hidingSpot.GetComponentInChildren<Camera>();
                            hidingCamera.enabled = true;


                            Transform[] children = GetComponentsInChildren<Transform>();

                            // Iterate through each child
                            foreach (Transform child in children)
                            {
                                // Check if the child has a MeshRenderer component
                                SkinnedMeshRenderer childRenderer = child.GetComponent<SkinnedMeshRenderer>();

                                // If the child has a MeshRenderer, do something with it
                                if (childRenderer != null)
                                {
                                    childRenderer.enabled = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //If we want to reveal ourselves.
                if (Input.GetKeyDown(KeyCode.E))
                {
                    //Reset our camera to our original one.
                    if (hidingCamera != null)
                    {
                        hidingCamera.enabled = false;
                    }

                    mainCamera.enabled = true;
                    currentlyHiding = false;
                    //meshRenderer.enabled = true;

                    Transform[] children = GetComponentsInChildren<Transform>();

                    // Iterate through each child
                    foreach (Transform child in children)
                    {
                        // Check if the child has a MeshRenderer component
                        SkinnedMeshRenderer childRenderer = child.GetComponent<SkinnedMeshRenderer>();

                        // If the child has a MeshRenderer, do something with it
                        if (childRenderer != null)
                        {
                            childRenderer.enabled = true;
                        }
                    }

                }
            }
        }
    }

}