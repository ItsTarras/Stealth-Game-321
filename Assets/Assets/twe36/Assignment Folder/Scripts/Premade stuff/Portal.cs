using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class represents a one directional portal
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

namespace twe36
{


    public class Portal : MonoBehaviour
    {
        // The region that this portal links to
        public Region nextRegion;

        // The game object that will contain this portals frustrum
        [HideInInspector]
        public GameObject frustumGO;

        // The colour of this portals frustum
        public Color portalFrustumColour;

        // The vertices which define this portal
        [HideInInspector]
        public List<Vector3> vertices;

        // Start is called before the first frame update
        void Start()
        {
            // Get a reference to the portal culling class in the scene
            PortalCulling portalCulling = FindObjectOfType<PortalCulling>();

            // Create the frustum for this portal
            frustumGO = new GameObject("Portal Frustum : " + transform.parent.name + " - " + nextRegion.name);
            // Put it under the frustum container defined by the portal culling script
            frustumGO.transform.SetParent(portalCulling.frustumContainer, false);
            // Set it's render layer to the render laying of the portal culling gameobject
            frustumGO.layer = portalCulling.gameObject.layer;
            // Set it's position and rotation to the identity
            frustumGO.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (frustumGO)
            {
                //Debug.Log("WE HAVE A FRUSTUM");

            }
            // Get the mesh filter of this portal
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            // Get the vertices which define the outside edges of our portal
            vertices = ContinuousEdgeList.GetContinuousEdgeList(meshFilter.mesh.vertices, meshFilter.mesh.triangles, 0.0001f);

            // Calculate the normal of our portal based on two of the calculated
            // outside edges
            Vector3 normal = Vector3.Cross(vertices[1] - vertices[0], vertices[1] - vertices[2]);

            // If the normal is pointing opposite to the normal of the mesh
            // reverse the order of our calculated outside edges - this way we
            // know our calculated outside edges are in correct winding order
            if (Vector3.Dot(meshFilter.mesh.normals[0], normal) < 0)
                vertices.Reverse();
        }

        // Called if we want to hide the frustum
        public void HideFrustum()
        {
            // Get our frustum's mesh renderer
            if (frustumGO)
            {
                MeshRenderer mr = frustumGO.GetComponent<MeshRenderer>();
                // If it exists, disable it
                if (mr != null) mr.enabled = false;
            }

        }

        // Get an updated frustum for this portal
        public Frustum GetUpdatedFrustum(Camera c)
        {
            // Get our portal culling script
            PortalCulling portalCulling = FindObjectOfType<PortalCulling>();

            // Create a frustum from this portal, using the cameras position,
            // our frustum game object, the material defined in the portal culling script
            // and the colour of this portal's frustum
            return Frustum.CreateFrustumFromPortal(this, c, frustumGO, portalCulling.frustumMaterial, portalFrustumColour);
        }
    }

}