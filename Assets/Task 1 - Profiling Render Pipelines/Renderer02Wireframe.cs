using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class implements a renderering pipeline capable
 * of rendering a model in wireframe
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class Renderer02Wireframe : MonoBehaviour
{
    // Reference to the mesh filter we will be rendering
    public MeshFilter meshFilterToRender;

    // Reference to the camera which our scene will be rendered from
    public Camera renderingCamera;

    // Used to store the original vertices from the mesh filter
    Vector3[] originalMeshVertices;

    // Used to store the original vertices from the mesh filter
    int[] originalMeshTriangles;

    // The Raw Image UI Element which we will place our rendered scene texture in
    public RawImage renderedTextureUIContainer;

    // The size of the rendered scene texture we will create
    public Vector2Int renderedTextureSize = new Vector2Int(300, 300);

    // The rendered scene Texture2D
    Texture2D renderedTexture;

    // Set the clear colour to white. Alternately we can clear it
    // with an alpha of 0 to make the background transparent
    public Color TextureClearColour = Color.white;
    //public Color TextureClearColour = new Color(1, 1, 1, 0);

    // The colour to render our wireframe in
    public Color WireframeColour = Color.black;

    // The Text UI Element to write our time taken to
    public Text TimeText;

    /*****
     * TODO: Mark down your approximate time per frame 
     * for this pipeline in the Unity Inspector here
     *****/
    float finishedTime = 0f;
    [TextArea]
    [SerializeField]
    string ApproximateTimePerFrame = "Mark down your approximate time per frame for this pipeline in the Unity Inspector here";

    // Start is called before the first frame update
    void Start()
    {
        // Store a copy of the original mesh vertices from the meshFilterToRender
        originalMeshVertices = meshFilterToRender.sharedMesh.vertices;

        // Store a copy of the original mesh triangles from the meshFilterToRender
        originalMeshTriangles = meshFilterToRender.sharedMesh.triangles;

        // Create the rendered texture based on the defined size
        renderedTexture = new Texture2D(renderedTextureSize.x, renderedTextureSize.y);

        // Set the UI container's texture to be our rendered texture
        renderedTextureUIContainer.texture = renderedTexture;
    }

    // Do our scene rendering after Unity has done all it's transform updates
    void LateUpdate()
    {
        /*****
         * TODO: Add code to start timing here
         *****/
        float time = Time.realtimeSinceStartup;


        // Clear our rendered texture
        ClearTexture(TextureClearColour);

        // Calculate the model to world matrix for our mesh
        Matrix4x4 modelToWorldMatrix = CalculateModelToWorldMatrix(meshFilterToRender.transform);

        // Calculate the model to view matrix for our mesh based on the camera
        Matrix4x4 modelToViewMatrix = CalculateModelToViewMatrix(modelToWorldMatrix, renderingCamera);

        // Calculate the perspective projection matrix for our camera
        Matrix4x4 projectionMatrix = CalculatePerspectiveProjectionMatrix(renderingCamera);

        // Project our mesh vertices from model space to homogeneous clip space
        Vector3[] projectedVertices = ProjectModelSpaceVertices(originalMeshVertices, modelToViewMatrix, projectionMatrix);

        // Finally, render our projected mesh vertices in wireframe
        RenderProjectedVerticesAsWireframe(projectedVertices, originalMeshTriangles, WireframeColour);

        /*****
         * TODO: Add code to end timing here
         *****/

        if (finishedTime == 0f)
        {
            finishedTime = Time.realtimeSinceStartup - time;
        }

        TimeText.text = "Time: " + finishedTime;
    }

    // This function clears our rendered texture
    void ClearTexture(Color clearColour)
    {
        // Get all the pixels into an array
        Color[] c = renderedTexture.GetPixels();

        // Loop through each pixel and set it's color to the clear colour
        for (int i = 0; i < c.Length; i++) c[i] = clearColour;

        // Set the rendered textures pixel to whats in the array
        renderedTexture.SetPixels(c);
    }


    // This function calculates the model to world matrix for a transform
    Matrix4x4 CalculateModelToWorldMatrix(Transform initialTransform)
    {

        // This is where we will store the model to world matrix as we traverse
        // the hierarchy. Initialise it to the identity matrix
        Matrix4x4 modelToWorldMatrix = Matrix4x4.identity;

        // Start at the meshFilterToRenders transform
        Transform currentTransform = initialTransform;

        // As long as the current transform isn't null
        while (currentTransform != null)
        {
            // Multiply the existing modelToWorldMatrix by the current transform's
            // local position, local rotation and local scale (stored in matrix form)
            modelToWorldMatrix = Matrix4x4.TRS(currentTransform.localPosition, currentTransform.localRotation, currentTransform.localScale) * modelToWorldMatrix;

            // Update the current transform to it's parent - once we reach
            // root node, this will set current transform to null, exiting out
            // of our loop
            currentTransform = currentTransform.parent;
        }

        // return the calculated matrix
        return modelToWorldMatrix;
    }


    // This function calculates the Model to View Matrix (or ModelViewMatrix)
    Matrix4x4 CalculateModelToViewMatrix(Matrix4x4 modelToWorldMatrix, Camera camera)
    {
        // Note that camera space matches OpenGL convention: camera's forward
        // is the negative Z axis. This is different from Unity's convention,
        // where forward is the positive Z axis.
        Matrix4x4 unityWorldToCameraMatrix = Matrix4x4.Scale(new Vector3(1, 1, 1))
            * camera.worldToCameraMatrix;

        // The model view matrix is just the World to Camera matrix multiplied
        // by the model to world matrix
        return unityWorldToCameraMatrix * modelToWorldMatrix;
    }


    // This function calculates the perspective projection matrix for a camera
    Matrix4x4 CalculatePerspectiveProjectionMatrix(Camera camera)
    {
        // initialize our projection matrix to all zeros
        Matrix4x4 projectionMatrix = Matrix4x4.zero;

        // Get the camera vertical field of view in radians
        float rad_fovY = camera.fieldOfView * Mathf.Deg2Rad;

        // Manually calculate our projection matrix using the values:
        // a = vertical field of view, ar = aspect ratio
        // nz = near plane, fz = far plane

        // M_0,0 = 1 / ( ar * tan (a / 2) )
        projectionMatrix.m00 = 1f / (camera.aspect * Mathf.Tan(rad_fovY / 2));
        // M_1,1 = 1 / ( tan (a / 2) )
        projectionMatrix.m11 = 1f / (Mathf.Tan(rad_fovY / 2));
        // M_2,2 = -( (-nz - fz) / (nz - fz) )
        projectionMatrix.m22 = -((-camera.nearClipPlane - camera.farClipPlane) / (camera.nearClipPlane - camera.farClipPlane));
        // M_2,3 = (2 * fz * nz) / (nz - fz)
        projectionMatrix.m23 = (2 * camera.farClipPlane * camera.nearClipPlane) / (camera.nearClipPlane - camera.farClipPlane);
        // M_3,2 = -1
        projectionMatrix.m32 = -1;

        // Return the calculated projection matrix
        return projectionMatrix;
    }

    // This function projects vertices in model space into homogeneous clip space
    Vector3[] ProjectModelSpaceVertices(Vector3[] modelSpaceVertices, Matrix4x4 modelToViewMatrix, Matrix4x4 projectionMatrix)
    {

        // Calculate the model to homogeneous clip space matrix
        // by multiplying the projection matrix by the model to view matrix
        Matrix4x4 modelToHomogeneousMatrix = projectionMatrix * modelToViewMatrix;

        // Create an array to store our projected vertices
        Vector3[] projectedVertices = new Vector3[modelSpaceVertices.Length];
        // Loop through each vertex
        for (int i = 0; i < modelSpaceVertices.Length; i++)
        {
            // Convert it from a 3D vertex to a 4D homogenous vertex (with w = 1)
            Vector4 homogeneousVertex = new Vector4(modelSpaceVertices[i].x, modelSpaceVertices[i].y, modelSpaceVertices[i].z, 1);

            // Project the 4D vertex to Homogeneous Space
            Vector4 projectedHomogeneousVertex = modelToHomogeneousMatrix * homogeneousVertex;

            // Convert the projected vertex from 4D to 3D by dividing the
            // x, y and z values by the w value
            projectedVertices[i] = new Vector3(projectedHomogeneousVertex.x / projectedHomogeneousVertex.w,
                projectedHomogeneousVertex.y / projectedHomogeneousVertex.w,
                projectedHomogeneousVertex.z / projectedHomogeneousVertex.w);
        }

        // Return the array of projected vertices
        return projectedVertices;
    }

    // This function renders out projected vertices as a point cloud using a specific colour
    void RenderProjectedVerticesAsWireframe(Vector3[] projectedVertices, int[] meshTriangles, Color colour)
    {
        // Loop through each of the triplets of triangle indices in our mesh
        for (int i=0; i< meshTriangles.Length; i+=3) {

            // Get the projected vertices that make up the triangle based
            // on these indices
            Vector3 proj_v1 = projectedVertices[meshTriangles[i]];
            Vector3 proj_v2 = projectedVertices[meshTriangles[i+1]];
            Vector3 proj_v3 = projectedVertices[meshTriangles[i+2]];

            // If at least one of the vertices is in the homogeneous clip bounds
            // Render the triangle
            if ((proj_v1.x >= -1 && proj_v1.x <= 1 && proj_v1.y >= -1 && proj_v1.y <= 1) ||
                (proj_v2.x >= -1 && proj_v2.x <= 1 && proj_v2.y >= -1 && proj_v2.y <= 1) ||
                (proj_v3.x >= -1 && proj_v3.x <= 1 && proj_v3.y >= -1 && proj_v3.y <= 1))
            {
                // Normalize our projected vertices so that they are in the range
                // Between 0 and 1 (instead of -1 and 1)
                Vector2 normalized_v1 = new Vector2((proj_v1.x + 1) / 2f, (proj_v1.y + 1) / 2f);
                Vector2 normalized_v2 = new Vector2((proj_v2.x + 1) / 2f, (proj_v2.y + 1) / 2f);
                Vector2 normalized_vV3 = new Vector2((proj_v3.x + 1) / 2f, (proj_v3.y + 1) / 2f);

                // Multiply our normalized vertex positions by the texture size
                // to get their position in texture space (or if we were rendering
                // to the screen - screen space)
                Vector2 texturespace_v1 = new Vector2(normalized_v1.x * (float)renderedTexture.width, normalized_v1.y * (float)renderedTexture.height);
                Vector2 texturespace_v2 = new Vector2(normalized_v2.x * (float)renderedTexture.width, normalized_v2.y * (float)renderedTexture.height);
                Vector2 texturespace_v3 = new Vector2(normalized_vV3.x * (float)renderedTexture.width, normalized_vV3.y * (float)renderedTexture.height);

                // Draw lines which connect the three vertices in texture space
                DrawLine(renderedTexture, texturespace_v1, texturespace_v2, Color.black);
                DrawLine(renderedTexture, texturespace_v2, texturespace_v3, Color.black);
                DrawLine(renderedTexture, texturespace_v3, texturespace_v1, Color.black);
            }
        }

        // Apply the changed pixels to our rendered texture
        renderedTexture.Apply();
    }

    // This function was taken from the Unity wiki, but modified with a different
    // header (Vectors for the start and end positions rather than integers),
    // and also with the ability to disable pixel wrapping (where if we draw
    // past the right hand edge of the texture, we start drawing on the left,
    // etc)
    void DrawLine(Texture2D tex, Vector2 v1, Vector2 v2, Color col, bool wrap = false)
    {
        int x0 = (int)v1.x; int y0 = (int)v1.y;
        int x1 = (int)v2.x; int y1 = (int)v2.y;
        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        // Only draw if wrapping is on, or we are within the bounds of the texture
        if (wrap || (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height))
            tex.SetPixel(x0, y0, col);

        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;

                // Only draw if wrapping is on, or we are within the bounds of the texture
                if (wrap || (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height))
                    tex.SetPixel(x0, y0, col);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;

                // Only draw if wrapping is on, or we are within the bounds of the texture
                if (wrap || (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height))
                    tex.SetPixel(x0, y0, col);
            }
        }
    }
}
