using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OcclusionCameraDebug : MonoBehaviour
{
    public Camera occlusionCamera; // Reference to the occlusion camera
    public RawImage debugImage; // Reference to the UI RawImage to display the debug view
    public int helpi = 0;
    private RenderTexture renderTexture; // Render Texture to render the occlusion camera's view

    private void Start()
    {
        // Create a Render Texture to render the occlusion camera's view
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        occlusionCamera.targetTexture = renderTexture;

        // Set the RawImage texture to the Render Texture
        debugImage.texture = renderTexture;
    }
}
