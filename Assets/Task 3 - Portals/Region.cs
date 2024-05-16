using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* This class represents a "region" for portal culling
 * A region is defined as a space which contains objects
 * to test for visibility, as well as portals which lead
 * out of the region into other regions. A region also
 * possibly contains the camera
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class Region : MonoBehaviour
{
    // An array of game objects to test for visibility
    public Renderer[] gameObjectsToTestForVisibility;

    // An array of visibility spheres for these game objects
    [HideInInspector]
    public SphereCollider[] visibilitySpheres;

    // An array of portals leading out of this region
    public Portal[] portals;

    // Start is called before the first frame update
    void Start()
    {
        // Get the portal culling script
        PortalCulling portalCulling = FindObjectOfType<PortalCulling>();

        // Instantiate the visibility spheres list based on the number of game
        // objects to test for visibility
        visibilitySpheres = new SphereCollider[gameObjectsToTestForVisibility.Length];

        // Loop through each game object to test for visibility
        for (int i = 0; i < gameObjectsToTestForVisibility.Length; i++)
        {
            // If the game object already has a visibility sphere
            if (gameObjectsToTestForVisibility[i].transform.Find("VisibilitySphere") != null)
            {
                // Lets just get use that one instead
                GameObject visibilitySphere = gameObjectsToTestForVisibility[i].transform.Find("VisibilitySphere").gameObject;

                // Store the collider for the sphere into the visibility spheres array
                visibilitySpheres[i] = visibilitySphere.GetComponent<SphereCollider>();
            }
            else
            {
                // Create a new sphere
                GameObject visibilitySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // Set the gameobjects name to "VisibilitySphere"
                visibilitySphere.name = "VisibilitySphere";
                // Set it's parent as the game object
                visibilitySphere.transform.SetParent(gameObjectsToTestForVisibility[i].transform, false);
                // Set it's size to the the game objects bounding box's extent magnitude * 2
                // (the magnitude is the half the size)
                visibilitySphere.transform.localScale = Vector3.one * gameObjectsToTestForVisibility[i].localBounds.extents.magnitude * 2;
                // Get the sphere's Renderer and update the material to our visibility sphere material, and turn off shadows
                Renderer renderer = visibilitySphere.GetComponent<Renderer>();
                renderer.material = portalCulling.visibilitySphereMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                // Store the collider for the sphere into the visibility spheres array
                visibilitySpheres[i] = visibilitySphere.GetComponent<SphereCollider>();
            }
        }
    }

    // Returns true if this region contains a point
    public bool containsPoint(Vector3 worldPoint)
    {
        // Get the mesh filter of this region
        MeshFilter mf = GetComponent<MeshFilter>();
        // return true if the point transformed into model space is inside
        // the bounds of the mesh attached to the mesh filter
        return mf.mesh.bounds.Contains(transform.InverseTransformPoint(worldPoint));
    }

    // Reset all our visibility spheres to not visible (red)
    public void ResetVisibilitySpheres()
    {
        // Loop through each visibility sphere
        foreach (SphereCollider sphereCollider in visibilitySpheres)
        {
            // Get it's mesh renderer
            MeshRenderer mr = sphereCollider.GetComponent<MeshRenderer>();
            // Set the materials colour to red
            mr.material.color = new Color(1, 0, 0, .5f);
        }
    }

    // Reset all our portal frustums
    public void ResetPortals()
    {
        // Loop through all the portals
        foreach (Portal p in portals)
            // Hide the frustum
            p.HideFrustum();
    }
}
