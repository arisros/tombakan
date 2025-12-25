using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpearLeash : MonoBehaviour
{
    [Header("References")]
    public Transform cameraAnchor; // Main Camera
    public Transform spearTip; // ujung tombak (projectile)

    [Header("Rope Shape")]
    [Range(8, 32)]
    public int segments = 24;

    [Tooltip("Seberapa melengkung talinya")]
    public float slack = 0.25f;

    [Header("Anchor Offset (Right Hand POV)")]
    public float forwardOffset = 0.25f;
    public float rightOffset = 0.18f;
    public float downOffset = -0.20f;

    [Header("Safety")]
    public float minCameraDistance = 0.15f;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        lr.enabled = false;
    }

    void LateUpdate()
    {
        if (!cameraAnchor || !spearTip)
        {
            lr.enabled = false;
            return;
        }

        lr.enabled = true;

        // === START POINT (tangan kanan imajiner) ===
        Vector3 start =
            cameraAnchor.position
            + cameraAnchor.forward * forwardOffset
            + cameraAnchor.right * rightOffset
            + cameraAnchor.up * downOffset;

        // safety: jangan tembus near clip
        if (Vector3.Distance(start, cameraAnchor.position) < minCameraDistance)
        {
            start = cameraAnchor.position + cameraAnchor.forward * minCameraDistance;
        }

        Vector3 end = spearTip.position;

        // arah "bawah" relatif kamera (lebih natural di AR)
        Vector3 down = -cameraAnchor.up;

        float distance = Vector3.Distance(start, end);
        float dynamicSlack = slack * Mathf.Clamp(distance, 0.8f, 3f);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);

            // posisi lurus
            Vector3 pos = Vector3.Lerp(start, end, t);

            // sag (melengkung di tengah)
            float sagAmount = Mathf.Sin(t * Mathf.PI) * dynamicSlack;
            pos += down * sagAmount;

            lr.SetPosition(i, pos);
        }
    }

    /// <summary>
    /// Dipanggil saat tombak dilepas
    /// </summary>
    public void AttachSpear(Transform spear)
    {
        spearTip = spear;
    }

    /// <summary>
    /// Dipanggil saat tombak hilang / reset
    /// </summary>
    public void DetachSpear()
    {
        spearTip = null;
        lr.enabled = false;
    }
}
