using UnityEngine;

public class FishSwim : MonoBehaviour
{
    public float speed = 0.2f;
    public float turnSpeed = 1.5f;
    public float swimRadius = 0.5f;

    private Vector3 center;
    private Vector3 target;

    void Start()
    {
        center = transform.position;
        PickNewTarget();
    }

    void Update()
    {
        Vector3 dir = target - transform.position;

        if (dir.magnitude < 0.05f)
        {
            PickNewTarget();
        }
        else
        {
            // Rotate
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                Time.deltaTime * turnSpeed
            );

            // Move forward
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void PickNewTarget()
    {
        Vector2 rand = Random.insideUnitCircle * swimRadius;
        target = center + new Vector3(rand.x, 0, rand.y);
    }
}
