using UnityEngine;

public class ShoulderRigFollowCamera : MonoBehaviour
{
    public Camera targetCamera;

    [Header("Shoulder POV Offset")]
    public Vector3 offset = new Vector3(0.2f, -0.15f, 0.35f);

    [Header("Smoothing")]
    public float positionSmooth = 12f;
    public float rotationSmooth = 12f;

    void LateUpdate()
    {
        if (!targetCamera)
            return;

        Vector3 targetPos = targetCamera.transform.TransformPoint(offset);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * positionSmooth
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetCamera.transform.rotation,
            Time.deltaTime * rotationSmooth
        );
    }
}
