using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionFrustumDebugger : MonoBehaviour
{
    public Camera occlusionCamera; // Reference to the occlusion camera

    private void OnDrawGizmosSelected()
    {
        if (occlusionCamera == null)
            return;

        // Calculate the frustum corners
        Matrix4x4 viewProjectionMatrix = occlusionCamera.projectionMatrix * occlusionCamera.worldToCameraMatrix;
        Vector3[] frustumCorners = new Vector3[4];
        occlusionCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), occlusionCamera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        // Draw frustum planes
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(occlusionCamera.transform.position, frustumCorners[i], Color.red);
            Debug.DrawLine(occlusionCamera.transform.position, frustumCorners[(i + 1) % 4], Color.red);
            Debug.DrawLine(frustumCorners[i], frustumCorners[(i + 1) % 4], Color.red);
        }

        // Draw lines connecting frustum corners
        Debug.DrawLine(occlusionCamera.transform.position, frustumCorners[0], Color.green);
        Debug.DrawLine(occlusionCamera.transform.position, frustumCorners[1], Color.green);
        Debug.DrawLine(occlusionCamera.transform.position, frustumCorners[2], Color.green);
        Debug.DrawLine(occlusionCamera.transform.position, frustumCorners[3], Color.green);
    }
}
