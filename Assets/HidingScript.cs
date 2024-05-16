using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingScript : MonoBehaviour
{

    public bool currentlyHiding = false;
    [SerializeField] List<GameObject> hidingPlaces = new List<GameObject>();
    MeshRenderer meshRenderer;
    AudioSource audioSource;
    private Camera hidingCamera;
    [SerializeField] private Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentlyHiding)
        {
            //Calculate the distance from the objects, and if we are close enough to one, we can push "E" to hide.
            //When we hide, just disable the mesh renderer, and move the camera to the object's camera.
            
            foreach (GameObject hidingSpot in hidingPlaces)
            {

                float distanceToHidingSpot = Vector3.Distance(hidingSpot.transform.position, transform.position);

                if (distanceToHidingSpot < 5)
                {
                    //Allow us the option to hide.
                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        currentlyHiding = true;
                        meshRenderer.enabled = false;

                        //Play the hiding sound.
                        audioSource = hidingSpot.GetComponent<AudioSource>();
                        audioSource.Play();

                        mainCamera.enabled = false;
                        hidingCamera = hidingSpot.GetComponent<Camera>();
                        hidingCamera.enabled = true;
                    }
                }
            }
        }
        else
        {
            //If we want to reveal ourselves.
            if(Input.GetKeyDown(KeyCode.E))
            {
                //Reset our camera to our original one.
                if (hidingCamera != null)
                {
                    hidingCamera.enabled = false;
                }

                mainCamera.enabled = true;
                currentlyHiding = false;
                meshRenderer.enabled = true;
            }
        }
    }
}
