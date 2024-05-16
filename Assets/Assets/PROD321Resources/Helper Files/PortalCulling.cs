using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class implements portal culling
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class PortalCulling : MonoBehaviour
{
    // The camera we will be generating portal frustums from
    public Camera portalCamera;

    // An array of all the regions which exist in our scene
    public Region[] regions;

    // The game objects which are within all frustums at any given frame
    public List<GameObject> gameObjectsInFrustum = new List<GameObject>();

    // The material to use for our debug frustum volume
    public Material frustumMaterial;
    // The material to use for our visibility spheres
    public Material visibilitySphereMaterial;

    // The Game Object transform that we will create our debug frustum
    // renderers as children of
    public Transform frustumContainer;

    // The frustum of our camera
    Frustum cameraFrustum;

    // Start is called before the first frame update
    void Start()
    {
        // If the occlusion camera hasn't been defined, try get the
        // camera tagged with MainCamera
        if (portalCamera == null)
            portalCamera = Camera.main;

        // Create a new GameObject for our camera frustum
        GameObject cameraFrustumGO = new GameObject("Camera Frustum");
        // Set it's layer to this layher
        cameraFrustumGO.layer = gameObject.layer;
        // Set it's parent to this gameobject (so it updates with the camera)
        cameraFrustumGO.transform.SetParent(transform, false);
        // Create the frustum for this camera
        cameraFrustum = Frustum.CreateFrustumFromCamera(portalCamera, cameraFrustumGO, frustumMaterial, Color.blue);
    }



    // Update is called once per frame
    void Update()
    {
        // Clear the list of game objects in the frustum
        gameObjectsInFrustum.Clear();

        // Calculate what region the camera is in
        Region cameraRegion = null;

        // Loop through each region
        foreach (Region region in regions)
        {
            // Reset the visibility spheres for all regions to "not visible"
            region.ResetVisibilitySpheres();
            // Reset all the portals to hide their frustums
            region.ResetPortals();

            // If the camera is inside a region, set that region as the
            // camera region
            if (region.containsPoint(portalCamera.transform.position))
                cameraRegion = region;
        }

        // If the camera is inside a region
        if (cameraRegion != null)
        {
            // Determine the visible objects in that region based on the camera frustum
            List<SphereCollider> visibleObjects = FindVisibleObjects(cameraRegion, cameraFrustum);

            // Loop through each visible object
            foreach (SphereCollider sphereCollider in visibleObjects)
            {
                // Get it's mesh renderer and set the color to blue
                MeshRenderer mr = sphereCollider.GetComponent<MeshRenderer>();
                mr.material.color = new Color(0, 0, 1, .5f);

                // Add the gameobject that this visible object is bounding
                gameObjectsInFrustum.Add(sphereCollider.transform.parent.gameObject);
            }
        }
    }

    // Find visible objects in a region, based on a frustum
    List<SphereCollider> FindVisibleObjects(Region region, Frustum frustum)
    {
        // Get the list of potentially visible objects in the region
        // (i.e. all the objects in the region which are tested for visibility)
        List<SphereCollider> potentiallyVisibleObjects = new List<SphereCollider>();
        potentiallyVisibleObjects.AddRange(region.visibilitySpheres);

        // Update the frustum for this frame
        frustum.UpdateFrustum();

        // Loop through each of the portals in this region
        foreach (Portal p in region.portals)
        {
            // If this portal is within this frustum
            if (frustum.containsPortal(p))
                // Get all the visible objects in the region that portal
                // links to, using the frustum created by that portal
                // relevant to the camera
                // Note: This is a recursive function, so will traverse room
                // by room based on which portals are visible in the current
                // frustum. The objects which are returned by this function
                // will be visible in all regions below this one (i.e. they
                // will be contained by every frustum below this one)
                potentiallyVisibleObjects.AddRange(FindVisibleObjects(p.nextRegion, p.GetUpdatedFrustum(portalCamera)));
        }

        // Create a list of objects which are actually visible
        List<SphereCollider> visibleObjects = new List<SphereCollider>();

        // Loop through all our potentially visible objects
        foreach (SphereCollider sphereCollider in potentiallyVisibleObjects)
        {
            // Get the position of this objects bounding sphere
            Vector3 spherePos = sphereCollider.transform.position;

            // Calculate the radius of the object's bounding sphere
            float bounds = sphereCollider.radius * sphereCollider.transform.localScale.x;

            // If the current frustum contains the object, it must be visible
            // (any object in a region below this one has already been determined
            // to be visible to all portal frustums below this)
            if (frustum.containsPoint(spherePos, bounds))
                visibleObjects.Add(sphereCollider);
        }

        // return the list of visible objects in this region, and all
        // other portal linked regions
        return visibleObjects;
    }

}