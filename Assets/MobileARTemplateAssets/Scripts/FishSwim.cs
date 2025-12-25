using UnityEngine;

public class FishSwim : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 0.4f;
    public float turnSmoothness = 1.5f;

    [Header("Swim Area")]
    public float horizontalRadius = 1.5f;
    public float minDepth = -0.3f;
    public float maxDepth = -0.08f;

    [Header("Depth")]
    public float depthLerp = 0.4f;
    public float depthChangeInterval = 2.5f;

    [Header("Turning Bank")]
    public float bankAngle = 12f;
    public float bankSmooth = 4f;

    private Vector3 swimCenter;
    private Vector3 direction;
    private Vector3 lastDirection;

    private float noiseOffset;
    private float targetDepth;
    private float nextDepthChange;

    private float currentBank;

    void Start()
    {
        swimCenter = transform.position;

        direction = Random.onUnitSphere;
        direction.y = 0f;
        direction.Normalize();

        speed *= Random.Range(0.85f, 1.15f);
        turnSmoothness *= Random.Range(0.8f, 1.2f);

        lastDirection = direction;

        noiseOffset = Random.Range(0f, 100f);
        targetDepth = Random.Range(minDepth, maxDepth);
        nextDepthChange = Time.time + Random.Range(1f, depthChangeInterval);
    }

    void Update()
    {
        float t = Time.time + noiseOffset;

        // --- Steering brain (Perlin Noise) ---
        Vector3 noiseDir = new Vector3(
            Mathf.PerlinNoise(t, 0f) - 0.5f,
            0f,
            Mathf.PerlinNoise(0f, t) - 0.5f
        );

        Vector3 desiredDir = (direction + noiseDir * 0.6f).normalized;

        direction = Vector3.Lerp(direction, desiredDir, Time.deltaTime * turnSmoothness).normalized;

        // --- Move forward ---
        transform.position += direction * speed * Time.deltaTime;

        // --- Depth retarget ---
        if (Time.time > nextDepthChange)
        {
            targetDepth = Random.Range(minDepth, maxDepth);
            nextDepthChange = Time.time + Random.Range(1.2f, depthChangeInterval);
        }

        float desiredY = swimCenter.y + targetDepth;
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Lerp(transform.position.y, desiredY, Time.deltaTime * depthLerp),
            transform.position.z
        );

        // --- Keep inside horizontal radius ---
        Vector3 flat = transform.position - swimCenter;
        flat.y = 0f;

        if (flat.magnitude > horizontalRadius)
        {
            Vector3 back = (swimCenter - transform.position);
            back.y = 0f;
            back.Normalize();

            direction = Vector3.Lerp(direction, back, Time.deltaTime * 2f).normalized;
        }

        // --- Root rotation + banking ---
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(direction);

            float signedTurn = Vector3.SignedAngle(lastDirection, direction, Vector3.up) / 90f;

            float targetBank = Mathf.Clamp(-signedTurn * bankAngle, -bankAngle, bankAngle);

            currentBank = Mathf.Lerp(currentBank, targetBank, Time.deltaTime * bankSmooth);

            Quaternion bankRot = Quaternion.AngleAxis(currentBank, Vector3.forward);

            Quaternion finalRot = look * bankRot;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                finalRot,
                Time.deltaTime * 6f
            );
        }

        lastDirection = direction;
    }

    // Optional helper for FishAnimate
    public float NormalizedSpeed => Mathf.Clamp01(speed / 1.2f);
}
