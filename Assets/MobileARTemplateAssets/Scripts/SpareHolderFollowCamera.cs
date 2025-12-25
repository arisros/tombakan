using UnityEngine;

public class SpearHolderFollowCamera : MonoBehaviour
{
    public Camera targetCamera;

    [Header("Local Offset (relative to camera)")]
    public Vector3 positionOffset = new Vector3(0f, -0.15f, 0.35f);
    public Vector3 rotationOffset = Vector3.zero;

    void LateUpdate()
    {
        if (!targetCamera)
            return;

        // Position: always in front of camera
        transform.position = targetCamera.transform.TransformPoint(positionOffset);

        // Rotation: follow camera direction
        transform.rotation = targetCamera.transform.rotation * Quaternion.Euler(rotationOffset);
    }
}
