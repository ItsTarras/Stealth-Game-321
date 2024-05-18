using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightChange : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject spotlightObject; // GameObject with the spotlight component

    private Camera secondaryCamera;
    private RenderTexture renderTexture;

    void Start()
    {
        // Create a Render Texture
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        int textureSize = 512; // Set the size of the Render Texture
        renderTexture = new RenderTexture(textureSize, textureSize, 24);
        renderTexture.Create();

        // Create a secondary camera
        secondaryCamera = gameObject.AddComponent<Camera>();
        secondaryCamera.CopyFrom(mainCamera);
        secondaryCamera.targetTexture = renderTexture;
        secondaryCamera.enabled = false; // Disable the secondary camera to prevent rendering to the screen

        // Apply the Render Texture to the spotlight's cookie texture
        spotlightObject.GetComponent<Light>().cookie = renderTexture;
    }
}

