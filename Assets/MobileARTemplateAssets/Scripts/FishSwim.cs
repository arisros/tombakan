using UnityEngine;

public class FishSwim : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 0.4f;
    public float turnSmoothness = 2f;
    public float verticalSpeed = 0.15f;

    [Header("Swim Area")]
    public float horizontalRadius = 1.5f;
    public float minDepth = -0.3f;
    public float maxDepth = -0.08f;

    private Vector3 swimCenter;
    private Vector3 velocity;
    private float verticalOffset;

    void Start()
    {
        swimCenter = transform.position;

        // random initial direction
        velocity = Random.onUnitSphere;
        velocity.y = Random.Range(-0.2f, 0.2f);
        velocity.Normalize();

        verticalOffset = Random.Range(minDepth, maxDepth);
    }

    void Update()
    {
        // subtle random steering (fish-like)
        Vector3 randomSteer = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.3f, 0.3f)
        );

        velocity = Vector3.Lerp(
            velocity,
            (velocity + randomSteer).normalized,
            Time.deltaTime * turnSmoothness
        );

        // move
        transform.position += velocity * speed * Time.deltaTime;

        // vertical swim (naik turun halus)
        float targetY = swimCenter.y + verticalOffset;
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * verticalSpeed),
            transform.position.z
        );

        // keep inside horizontal area
        Vector3 flatOffset = transform.position - swimCenter;
        flatOffset.y = 0;

        if (flatOffset.magnitude > horizontalRadius)
        {
            Vector3 backDir = (swimCenter - transform.position).normalized;
            velocity = Vector3.Lerp(velocity, backDir, Time.deltaTime * 2f);
        }

        // rotation follow velocity
        if (velocity.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 3f);
        }
    }
}
