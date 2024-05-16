using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class represents a frustum, and has a bunch of functionality
 * which is useful for any sort of frustum culling - e.g. portals
 * and occlusion volumes
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class Frustum : MonoBehaviour
{
    // An array of the center points of each of the 6 sides of the frustum
    // Stored in model space
    Vector3[] frustumCenters = new Vector3[6];
    // An array of the normals for each of the 6 sides of the frustum
    // Stored in model space
    Vector3[] frustumNormals = new Vector3[6];

    // Arrays to store the frustum centers and normals transformed into
    // World space at each frame
    Vector3[] transformedFrustumCenters = new Vector3[6];
    Vector3[] transformedFrustumNormals = new Vector3[6];

    // This function creates a frustum based on a camera c, makes
    // it a child of the GameObject container, uses the material material
    // for the debug rendering, using the color color on the material
    public static Frustum CreateFrustumFromCamera(Camera c, GameObject container, Material material, Color color)
    {
        // Get the near plane, far plane, vertical field of view and aspect
        // ratio from the camera tagged with MainCamera
        float nearPlane = c.nearClipPlane;
        float farPlane = c.farClipPlane;
        float fovY = c.fieldOfView * Mathf.Deg2Rad;
        float aspect = c.aspect;

        // Calculate the width and height of the near plane of the camera
        // Since the centre of these planes will be at 0,0, we can just
        // Store half this value and -/+ the value to find the near plane
        // vertices
        float nearHalfHeight = Mathf.Tan(fovY / 2f) * nearPlane;
        float nearHalfWidth = nearHalfHeight * aspect;

        // As above, but for the far plane
        float farHalfHeight = Mathf.Tan(fovY / 2f) * farPlane;
        float farHalfWidth = farHalfHeight * aspect;

        // An array of the 8 vertices which make up the frustum
        Vector3[] fv = new Vector3[8];

        // Calculate the four vertices which make up the near plane
        fv[0] = new Vector3(-nearHalfWidth, -nearHalfHeight, nearPlane);
        fv[1] = new Vector3(nearHalfWidth, -nearHalfHeight, nearPlane);
        fv[2] = new Vector3(nearHalfWidth, nearHalfHeight, nearPlane);
        fv[3] = new Vector3(-nearHalfWidth, nearHalfHeight, nearPlane);

        // Calculate the four vertices which make up the far plane
        fv[4] = new Vector3(-farHalfWidth, -farHalfHeight, farPlane);
        fv[5] = new Vector3(farHalfWidth, -farHalfHeight, farPlane);
        fv[6] = new Vector3(farHalfWidth, farHalfHeight, farPlane);
        fv[7] = new Vector3(-farHalfWidth, farHalfHeight, farPlane);

        // Call the generic create frustum function with the vertices we've defined
        return CreateFrustum(fv, container, material, color);
    }

    // This function creates a frustum from a portal portal, given the position of
    // the camera c, makes it a child of the GameObject container, uses the 
    // material material for the debug rendering, using the color color on the material
    public static Frustum CreateFrustumFromPortal(Portal portal, Camera c, GameObject container, Material material, Color color)
    {
        // An array of the 8 vertices which make up the frustum
        Vector3[] fv = new Vector3[8];

        // Get the vertices which define the portal
        Vector3[] pv = portal.vertices.ToArray();

        // Calculate the four vertices which make up the near plane -
        // These will just be the vertices of the portal
        fv[0] = portal.transform.TransformPoint(pv[0]);
        fv[1] = portal.transform.TransformPoint(pv[1]);
        fv[2] = portal.transform.TransformPoint(pv[2]);
        fv[3] = portal.transform.TransformPoint(pv[3]);

        // Calculate the angle theta between the angles defined by a vector
        // going from the camera position to the top left corner of the portal
        // and a vector going from the camera position to the bottom right corner
        // of the portal
        float theta = Vector3.Angle((fv[0] - c.transform.position), (fv[2] - c.transform.position));

        // Calculate the length of the hypotenuse of the triangle defined by the distance 
        // to the camera's far plane (as the adjacent side) and half the angle theta
        // This will give us the distance from the camera to the vertex in the
        // top left and bottom right vertices of the frustum defined by the
        // portal
        float h = c.farClipPlane / Mathf.Cos((theta * Mathf.Deg2Rad) / 2);

        // Calculate the far plane vertices by taking the near plane vertices
        // and adding to them a vector which is defined as the direction of
        // the normalized vector which points from the near plane vertex to the
        // camera (the first half of the multiplication), multiplied by the
        // length of h - the length of the vector from the near plane vertex to
        // the camera.
        // More simplified, we get the direction of the vector from the camera
        // to the near plane vertex, make it's length the distance to the far
        // plane vertex (as defined by h - the length of the vector from the camera
        // to the near plane vertex), and add that vector to the original
        // near plane vertex.
        fv[4] = fv[0] + ((fv[0] - c.transform.position).normalized * (h - (fv[0] - c.transform.position).magnitude));
        fv[5] = fv[1] + ((fv[1] - c.transform.position).normalized * (h - (fv[1] - c.transform.position).magnitude));
        fv[6] = fv[2] + ((fv[2] - c.transform.position).normalized * (h - (fv[2] - c.transform.position).magnitude));
        fv[7] = fv[3] + ((fv[3] - c.transform.position).normalized * (h - (fv[3] - c.transform.position).magnitude));

        // Call the generic create frustum function with the vertices we've defined
        return CreateFrustum(fv, container, material, color);

    }

    // This function creates a frustum from a set of 8 vertices which define the
    // portal, the camera c, makes it a child of the GameObject container, uses the 
    // material material for the debug rendering, using the color color on the material
    public static Frustum CreateFrustum(Vector3[] fv, GameObject container, Material material, Color color)
    {
        // Check to see if the GameObject container already has a frustum class
        // on it - if not create one
        Frustum f = container.GetComponent<Frustum>();
        if (f == null)
            f = container.AddComponent<Frustum>();

        // Calculate the centres of the 6 planes that make up the frustum
        // (Just the average of the 4 points that make up the plane)
        f.frustumCenters[0] = (fv[0] + fv[1] + fv[2] + fv[3]) / 4; //Near
        f.frustumCenters[1] = (fv[4] + fv[5] + fv[6] + fv[7]) / 4; //Far
        f.frustumCenters[2] = (fv[4] + fv[0] + fv[3] + fv[7]) / 4; //Left
        f.frustumCenters[3] = (fv[1] + fv[5] + fv[6] + fv[2]) / 4; //Right
        f.frustumCenters[4] = (fv[7] + fv[6] + fv[2] + fv[3]) / 4; //Top
        f.frustumCenters[5] = (fv[4] + fv[5] + fv[1] + fv[0]) / 4; //Bottom

        // Calculate the normals of the 6 planes that make up the frustum
        // Take the cross product of two perpendicular edges of each plane
        f.frustumNormals[0] = Vector3.Cross(fv[3] - fv[0], fv[1] - fv[0]).normalized; //Near
        f.frustumNormals[1] = Vector3.Cross(fv[5] - fv[4], fv[6] - fv[5]).normalized; //Far
        f.frustumNormals[2] = Vector3.Cross(fv[7] - fv[4], fv[0] - fv[4]).normalized; //Left
        f.frustumNormals[3] = Vector3.Cross(fv[2] - fv[1], fv[5] - fv[1]).normalized; //Right
        f.frustumNormals[4] = Vector3.Cross(fv[3] - fv[2], fv[6] - fv[2]).normalized; //Top
        f.frustumNormals[5] = Vector3.Cross(fv[1] - fv[0], fv[4] - fv[0]).normalized; //Bottom

        // An array to store the 12 triangles that make up the frustum
        // 6 sides with each side being made of 2 triangles, 3 vertices per triangle
        // = 6 * 2 * 3 = 36
        int[] t = new int[36];

        // Vertices for the triangles for the near plane
        t[0] = 2;
        t[1] = 1;
        t[2] = 0;
        t[3] = 3;
        t[4] = 2;
        t[5] = 0;

        // Vertices for the triangles for the far plane
        t[6] = 6;
        t[7] = 5;
        t[8] = 4;
        t[9] = 7;
        t[10] = 6;
        t[11] = 4;

        // Vertices for the triangles for the left plane
        t[12] = 0;
        t[13] = 4;
        t[14] = 7;
        t[15] = 0;
        t[16] = 7;
        t[17] = 3;

        // Vertices for the triangles for the right plane
        t[18] = 5;
        t[19] = 1;
        t[20] = 2;
        t[21] = 5;
        t[22] = 2;
        t[23] = 6;

        // Vertices for the triangles for the top plane
        t[24] = 3;
        t[25] = 7;
        t[26] = 6;
        t[27] = 3;
        t[28] = 6;
        t[29] = 2;

        // Vertices for the triangles for the bottom plane
        t[30] = 0;
        t[31] = 4;
        t[32] = 5;
        t[33] = 0;
        t[34] = 5;
        t[35] = 1;

        // Create a mesh from the vertices and triangles defined above
        Mesh mesh = new Mesh();
        mesh.vertices = fv;
        mesh.triangles = t;

        // Check to see if the container already has a mesh filter
        // if so use that, if not create a mesh filter for this mesh
        // and assign the mesh to it
        MeshFilter m = container.GetComponent<MeshFilter>();
        if (m == null) m = container.AddComponent<MeshFilter>();
        m.mesh = mesh;

        // Check to see if the container already has a mesh renderer
        // if so use that, if not create a mesh renderer and assign the
        // frustum material to it
        Renderer r = container.GetComponent<MeshRenderer>();
        if (r == null) r = container.AddComponent<MeshRenderer>();
        r.material = material;

        // Set the colors alpha to .5, set the material color to color
        // and enable the renderer
        color.a = .5f;
        r.material.color = color;
        r.enabled = true;

        // Return a reference to the frustum we created
        return f;
    }

    // This function will check to see if the frustum contains a point or bounding
    // sphere, and returns true if it does, or returns false if it doesn't
    public bool containsPoint(Vector3 point, float bounds)
    {
        // Calculate the h distance of the point relative to each of the frustum planes
        // the formula for h distance is h = (P - P_0) . N
        // Where P is the point we are wanting to check what side of the plane it lays on
        // P_0 is a point on the plane we're testing, and N is the planes normal
        float hNear = Vector3.Dot((point - transformedFrustumCenters[0]), transformedFrustumNormals[0]); //Test Near
        float hFar = Vector3.Dot((point - transformedFrustumCenters[1]), transformedFrustumNormals[1]); //Test Far
        float hLeft = Vector3.Dot((point - transformedFrustumCenters[2]), transformedFrustumNormals[2]); //Test Left
        float hRight = Vector3.Dot((point - transformedFrustumCenters[3]), transformedFrustumNormals[3]); //Test Right
        float hTop = Vector3.Dot((point - transformedFrustumCenters[4]), transformedFrustumNormals[4]); //Test Top
        float hBottom = Vector3.Dot((point - transformedFrustumCenters[5]), transformedFrustumNormals[5]); //Test Bottom

        // if h < bounds, it means that the point/sphere is on the opposite side
        // of the plane to the planes normal, or in the case of our frustum
        // it is on the "inside" of that plane. If the point/sphere is on the "inside"
        // of all the frustum planes, it must be inside the frustum
        if (hNear < bounds && hFar < bounds && hLeft < bounds && hRight < bounds && hTop < bounds && hBottom < bounds)
            // return true if inside
            return true;
        else
            // return false if not inside
            return false;
    }

    // This function will check to see if the frustum contains a portal.
    // We cheat a little bit here - portals are flat rectangles in this code,
    // So rather than creating a bounding sphere, etc, we just sample a bunch
    // of points in a grid within the boundary of the portal, and if any of them
    // are inside the portal, we assume the portal is in the frustum.
    // It's not a perfect solution, but it's a simple one which works 99% of the
    // time
    public bool containsPortal(Portal portal)
    {


        if (portal.vertices.Count == 0) return false;

        // Get an array of the vertices which define the portal
        Vector3[] pv = portal.vertices.ToArray();

        // If the portal is facing the opposite direction of the frustum, we
        // consider it to not be in the frustum. This means that portals are one
        // directional - if we want a two directional portal (like is typical in
        // the real world), we need to have two portals back to back, facing opposite
        // directions. A bit more work for us, but simplifies other calculations
        if (Vector3.Dot(transformedFrustumNormals[1], portal.transform.forward) < -0.5)
            return false;

        // Rather than properly determining if a portal is in the frustum, lets just sample some points
        // which are on the portal plane and see if any of them are in the frustum
        // We'll do 10x10 increments across the portal in a grid
        for (float y = 0; y <= 1; y += .1f)
        {
            for (float x = 0; x <= 1; x += .1f)
            {
                // Get a new test point by lerping between the top left and bottom
                // right corners at x by y intervals
                Vector3 testPoint = new Vector3(Mathf.Lerp(pv[0].x, pv[2].x, x), Mathf.Lerp(pv[0].y, pv[2].y, y), pv[0].z);

                // If any point is inside the frustum, we immediately say the
                // portal is inside the frustum
                if (containsPoint(portal.transform.TransformPoint(testPoint), 0))
                    return true;
            }
        }

        // If no points were inside the frustum, we say the portal is not
        // inside the frustum
        return false;
    }

    // Update the frustum's transformed frustum centers and normals, and
    // Draw the normals on the frustum
    public void UpdateFrustum()
    {
        // Loop through all 6 of the frustum planes
        for (int i = 0; i < 6; i++)
        {
            // And update the transformed frustum centers and transformed frustum normals
            transformedFrustumCenters[i] = transform.TransformPoint(frustumCenters[i]);
            transformedFrustumNormals[i] = transform.TransformDirection(frustumNormals[i]);
        }

        // Call the draw normals method
        DrawNormals();
    }

    // Draw the normals at each side of the frustum
    void DrawNormals()
    {
        float normalLength = 5;
        // Draw a ray from the centre of the side of the frustum, in the direction of the normal * by the normal length
        // User a different colour for each normal
        Debug.DrawRay(transformedFrustumCenters[0], transformedFrustumNormals[0] * normalLength, Color.blue); // Near Plane Normal
        Debug.DrawRay(transformedFrustumCenters[1], transformedFrustumNormals[1] * normalLength, Color.red); // Far Plane Normal
        Debug.DrawRay(transformedFrustumCenters[2], transformedFrustumNormals[2] * normalLength, Color.cyan); // Left Plane Normal
        Debug.DrawRay(transformedFrustumCenters[3], transformedFrustumNormals[3] * normalLength, Color.yellow); // Right Plane Normal
        Debug.DrawRay(transformedFrustumCenters[4], transformedFrustumNormals[4] * normalLength, Color.black); // Top Plane Normal
        Debug.DrawRay(transformedFrustumCenters[5], transformedFrustumNormals[5] * normalLength, Color.white); // Bottom Plane Normal

    }

    // When we destroy our frustum
    private void OnDestroy()
    {
        // Also destroy the mesh filter and mesh renderer (if they exist)
        if (GetComponent<MeshFilter>()) Destroy(GetComponent<MeshFilter>());
        if (GetComponent<MeshRenderer>()) Destroy(GetComponent<MeshRenderer>());
    }
}