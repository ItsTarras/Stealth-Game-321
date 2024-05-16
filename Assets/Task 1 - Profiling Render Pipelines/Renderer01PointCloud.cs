using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class implements a renderering pipeline capable
 * of rendering a model as a point cloud
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class Renderer01PointCloud : MonoBehaviour
{
    // Reference to the mesh filter we will be rendering
	public MeshFilter meshFilterToRender;

    // Reference to the camera which our scene will be rendered from
	public Camera renderingCamera;

    // Used to store the original vertices from the mesh filter
	Vector3[] originalMeshVertices;

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

    // The colour to render our point cloud in
    public Color PointCloudColour = Color.black;

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

    float sumTime = 0;
    int frameCount = 0;

    // Start is called before the first frame update
    void Start()
	{
        // Store a copy of the original mesh vertices from the meshFilterToRender
        originalMeshVertices = meshFilterToRender.sharedMesh.vertices;

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
        frameCount++;

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

        // Finally, render our projected vertices as a pointcloud
        RenderProjectedVerticesAsPointCloud(projectedVertices, PointCloudColour);

        /*****
         * TODO: Add code to end timing here
         *****/
        //if (finishedTime == 0f)
        {
            finishedTime = Time.realtimeSinceStartup - time;
        }
        sumTime += finishedTime;

        //TimeText.text = "Time: " + (sumTime/frameCount);
        TimeText.text = "FPS: " + 1f/(sumTime / frameCount);
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
    Matrix4x4 CalculateModelToWorldMatrix(Transform initialTransform) {

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
    Vector3[] ProjectModelSpaceVertices(Vector3[] modelSpaceVertices, Matrix4x4 modelToViewMatrix, Matrix4x4 projectionMatrix) {

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
    void RenderProjectedVerticesAsPointCloud(Vector3[] projectedVertices, Color colour) {     

        // Loop through each projected vertex
        for (int i=0; i< projectedVertices.Length; i++)
        {
            // Check that it isn't outside of the homogeneous clip bounds
            // -1 <= x <= 1 and -1 <= y <= 1
            if (projectedVertices[i].x >= -1 && projectedVertices[i].x <= 1 && projectedVertices[i].y >= -1 && projectedVertices[i].y <= 1)
            {
                // Normalize our projected vertex so that it is in the range
                // Between 0 and 1 (instead of -1 and 1)
                Vector2 normalizedVert = new Vector2((projectedVertices[i].x + 1) / 2f, (projectedVertices[i].y + 1) / 2f);

                // Multiply our normalized vertex position by the texture size
                // to get it's position in texture space (or if we were rendering
                // to the screen - screen space)
                Vector2 textureSpaceVert = new Vector2(normalizedVert.x * (float)renderedTexture.width, normalizedVert.y * (float)renderedTexture.height);

                // Set the pixel at the texture coordinates of the texture space
                // vertex to be the colour we defined before
                renderedTexture.SetPixel((int)textureSpaceVert.x, (int)textureSpaceVert.y, colour);
            }
        }

        // Apply the changed pixels to our rendered texture
        renderedTexture.Apply();
    }
}
