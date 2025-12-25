using UnityEngine;

public class SpearHit : MonoBehaviour
{
    public LayerMask fishLayer;
    public float hitRadius = 0.1f;

    void Update()
    {
        CheckFishHit();
    }

    void CheckFishHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, fishLayer);

        foreach (var hit in hits)
        {
            FishHitBox fish = hit.GetComponentInParent<FishHitBox>();
            if (fish != null)
            {
                fish.OnHit(hit.GetComponentInParent<FishTarget>().fishColor);
                StickToFish(fish.transform);
                break;
            }
        }
    }

    void StickToFish(Transform fishTransform)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetParent(fishTransform);
        transform.position = fishTransform.position;
    }
}
