using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class implements a renderering pipeline capable
 * of rendering a model using Phong lit triangles
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class Renderer04LitTriangles : MonoBehaviour
{
    // Reference to the mesh filter we will be rendering
    public MeshFilter meshFilterToRender;

    // Reference to the camera which our scene will be rendered from
    public Camera renderingCamera;

    // Used to store the original vertices from the mesh filter
    Vector3[] originalMeshVertices;

    // Used to store the original vertex normals from the mesh filter
    Vector3[] originalMeshNormals;

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

    // The Model Material's Diffuse Colour, Specular Colour and Shininess values
    public Color Material_DiffuseColor = new Color(.8f, .8f, 0, 1);
    public Color Material_SpecularColor = new Color(1, 1, 0, 1);
    public float Material_Shininess = 0.8f;

    // The light source to use for lighting
    public Light lightSource;

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

        // Store a copy of the original mesh vertex normals from the meshFilterToRender
        originalMeshNormals = meshFilterToRender.sharedMesh.normals;

        // Store a copy of the original mesh triangles from the meshFilterToRender
        originalMeshTriangles = meshFilterToRender.sharedMesh.triangles;

        // Create the rendered texture based on the defined size
        renderedTexture = new Texture2D(renderedTextureSize.x, renderedTextureSize.y);

        // Set the UI container's texture to be our rendered texture
        renderedTextureUIContainer.texture = renderedTexture;

        // If the light source hasn't been set, find one
        if (lightSource==null)
            lightSource = FindObjectOfType<Light>();
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

        // Calculate the Vertex Colours for each vertex as a batch operation
        // Since we may use the same vertex for multiple triangles, this saves
        // Us computing the colour of the same vertices multiple times for each
        // triangle
        Color[] vertexColours = CalculateVertexColours(
            modelToWorldMatrix, originalMeshVertices, originalMeshNormals,
            renderingCamera.transform.position, lightSource,
            Material_DiffuseColor, Material_SpecularColor, Material_Shininess);

        // Finally, render our projected mesh vertices in wireframe
        RenderProjectedVerticesAsPhongLitTriangles(projectedVertices, originalMeshTriangles, vertexColours);

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
    void RenderProjectedVerticesAsPhongLitTriangles(Vector3[] projectedVertices, int[] meshTriangles, Color[] vertexColours)
    {
        // Loop through each of the triplets of triangle indices in our mesh
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {

            // Get the projected vertices that make up the triangle based
            // on these indices
            Vector3 proj_v1 = projectedVertices[meshTriangles[i]];
            Vector3 proj_v2 = projectedVertices[meshTriangles[i + 1]];
            Vector3 proj_v3 = projectedVertices[meshTriangles[i + 2]];

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
                Vector2 normalized_v3 = new Vector2((proj_v3.x + 1) / 2f, (proj_v3.y + 1) / 2f);

                // Multiply our normalized vertex positions by the texture size
                // to get their position in texture space (or if we were rendering
                // to the screen - screen space)
                Vector2 texturespace_v1 = new Vector2(normalized_v1.x * (float)renderedTexture.width, normalized_v1.y * (float)renderedTexture.height);
                Vector2 texturespace_v2 = new Vector2(normalized_v2.x * (float)renderedTexture.width, normalized_v2.y * (float)renderedTexture.height);
                Vector2 texturespace_v3 = new Vector2(normalized_v3.x * (float)renderedTexture.width, normalized_v3.y * (float)renderedTexture.height);

                // Draw the triangle interpolated between the three vertices,
                // using the colours calculated for these vertices based
                // on the triangle indices
                DrawInterpolatedTriangle(renderedTexture, texturespace_v1, texturespace_v2, texturespace_v3,
                        vertexColours[meshTriangles[i]], vertexColours[meshTriangles[i + 1]], vertexColours[meshTriangles[i + 2]]);
            }
        }

        // Apply the changed pixels to our rendered texture
        renderedTexture.Apply();
    }

    // This function draws a triangle to a texture renderTexture, based on
    // the texture space vertices v1, v2 and v3, which have the colours
    // v1Colour, v2Colour, v3Colour
    void DrawInterpolatedTriangle(Texture2D renderTexture,
        Vector2 v1, Vector2 v2, Vector2 v3,
        Color v1Colour, Color v2Colour, Color v3Colour)
    {
        // Calculate the bounding rectangle of the triangle based on the
        // three vertices
        RectInt triangleBoundingRect = new RectInt();
        triangleBoundingRect.xMin = (int)Mathf.Min(v1.x, Mathf.Min(v2.x, v3.x));
        triangleBoundingRect.xMax = (int)Mathf.Max(v1.x, Mathf.Max(v2.x, v3.x));
        triangleBoundingRect.yMin = (int)Mathf.Min(v1.y, Mathf.Min(v2.y, v3.y));
        triangleBoundingRect.yMax = (int)Mathf.Max(v1.y, Mathf.Max(v2.y, v3.y));

        // Cull the bounding rect to the size of the texture we're rendering to
        triangleBoundingRect.xMin = (int)Mathf.Max(triangleBoundingRect.xMin, 0);
        triangleBoundingRect.xMax = (int)Mathf.Min(triangleBoundingRect.xMax, renderTexture.width);
        triangleBoundingRect.yMin = (int)Mathf.Max(triangleBoundingRect.yMin, 0);
        triangleBoundingRect.yMax = (int)Mathf.Min(triangleBoundingRect.yMax, renderTexture.height);

        // Loop through every pixel in the bounding rect
        for (int y = triangleBoundingRect.yMin; y <= triangleBoundingRect.yMax; y++)
        {
            for (int x = triangleBoundingRect.xMin; x <= triangleBoundingRect.xMax; x++)
            {
                // Convert our integer x and y positions into a floating point
                // Vector 2 for floating point multiplication and ease use
                Vector2 p = new Vector2(x, y);

                // Calculate the weights w1, w2 and w3 for the barycentric
                // coordinates based on the positions of the three vertices
                float denom = (v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y);
                float w_v1 = ((v2.y - v3.y) * (p.x - v3.x) + (v3.x - v2.x) * (p.y - v3.y)) / denom;
                float w_v2 = ((v3.y - v1.y) * (p.x - v3.x) + (v1.x - v3.x) * (p.y - v3.y)) / denom;
                float w_v3 = 1 - w_v1 - w_v2;

                // If w1, w2 and w3 are >= 0, we are inside the triangle (or
                // on an edge, but either way, render the pixel)
                if (w_v1 >= 0 && w_v2 >= 0 && w_v3 >= 0)
                {
                    // Calculate the pixel colour based on the weighted vertex colours
                    Color pixelColour = v1Colour * w_v1 + v2Colour * w_v2 + v3Colour * w_v3;

                    // Set this pixel in the render texture
                    renderTexture.SetPixel(x, y, pixelColour);
                }
            }
        }
    }

    // This function calculates the colours of a range of vertices, based
    // on the vertex normals, camera world pos, light source, and material
    // diffuse colour, specular colour and shininess
    // Since we may use the same vertex for multiple triangles, it's faster
    // to computer the colours of all vertices at once, to save us computing
    // the colour of the same vertices multiple times for each triangle
    Color[] CalculateVertexColours(Matrix4x4 modelToWorldMatrix, Vector3[] verts, Vector3[] normals,
        Vector3 cameraWorldPos, Light lightSource,
        Color DiffuseColour, Color SpecularColour, float Shininess)
    {

        // Create a new array to keep track of all the vertex colours
        Color[] colours = new Color[verts.Length];

        // Calculate the light position and colour based on the light properties
        // LightW is set to 0 if the light is directional, and 1 otherwise
        float lightW; Vector3 lightPos; Color lightColour;
        float attenuation = 0;
        if (lightSource.type == LightType.Directional)
        {
            lightPos = -lightSource.transform.forward; lightW = 0;
            lightColour = lightSource.color * lightSource.intensity;
            attenuation = 1.0f;
        }
        else
        {
            lightPos = lightSource.transform.position; lightW = 1;
            // For non direcitonal lights, we'll figure out the light
            // Colour per vertex, based on the distance from the light
            // To the vertex
            lightColour = Color.black;
        }

        // Loop through each vertex
        for (int i = 0; i < verts.Length; i++)
        {

            // Calculate the vertices world position and world normal
            //o.posWorld = mul(unity_ObjectToWorld, v.vertex); //Calculate the world position for our point
            //o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz); //Calculate the normal
            Vector3 vertexWorldPosition = modelToWorldMatrix.MultiplyPoint3x4(verts[i]);
            Vector3 normalWorldDirection = modelToWorldMatrix.MultiplyVector(normals[i]);

            // Normalize the normal direction
            //float3 normalDirection = normalize(i.normal);
            Vector3 normalDirection = normalWorldDirection.normalized;

            // Calculate the normalized view direction (the camera position -
            // the vertex world position) 
            //float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
            Vector3 viewDirection = (cameraWorldPos - vertexWorldPosition).normalized;

            // if our light source is not directional
            if (lightSource.type != LightType.Directional)
            {
                // Calculate the distance from the light to the vertex, and 1/distance
                //float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
                Vector3 vert2LightSource = lightPos - vertexWorldPosition;
                //float oneOverDistance = 1.0 / length(vert2LightSource);
                //float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w); //Optimization for spot lights. This isn't needed if you're just getting started.
                attenuation = 1.0f / vert2LightSource.magnitude;

                // Calculate the colour based on the distance and lightsource range
                if (vert2LightSource.magnitude < lightSource.range)
                    lightColour = (lightSource.color * lightSource.intensity);
            }

            // Calculate light direction
            //float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;
            Vector3 lightDirection = lightPos - vertexWorldPosition * lightW;

            // Calculate Ambient Lighting
            //float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb; //Ambient component
            Color ambientLighting = RenderSettings.ambientLight * DiffuseColour;

            // Calculate Diffuse Lighting
            //float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection)); //Diffuse component
            Color diffuseReflection = attenuation * lightColour * DiffuseColour * Mathf.Max(0, Vector3.Dot(normalDirection, lightDirection));

            /*float3 specularReflection;
            if (dot(i.normal, lightDirection) < 0.0) //Light on the wrong side - no specular
            {
                specularReflection = float3(0.0, 0.0, 0.0);
            }
            else
            {
                //Specular component
                specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
            }*/

            // Calculate Specular reflection if the normal is pointing in the
            // Lights direction
            Color specularReflection;
            if (Vector3.Dot(normalWorldDirection, lightDirection) < 0.0)
            {
                specularReflection = Color.black;
            }
            else
            {
                specularReflection = attenuation * lightColour * SpecularColour * Mathf.Pow(Mathf.Max(0.0f, Vector3.Dot(Vector3.Reflect(-lightDirection, normalDirection), viewDirection)), Shininess);
            }

            // The final colour for this vertex is the sum of the ambient, diffuse and specular
            //float3 color = (ambientLighting + diffuseReflection) * tex2D(_Tex, i.uv) + specularReflection; //Texture is not applient on specularReflection
            colours[i] = (ambientLighting + diffuseReflection) + specularReflection;
        }

        // Return the array of vertex colours
        return colours;
    }
}
