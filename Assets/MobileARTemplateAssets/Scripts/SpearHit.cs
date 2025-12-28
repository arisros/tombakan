using UnityEngine;

public class SpearHit : MonoBehaviour
{
    public LayerMask fishLayer;
    public float hitRadius = 0.1f;

    bool hasHit;

    void Update()
    {
        if (!hasHit)
            CheckFishHit();
    }

    void CheckFishHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, fishLayer);

        foreach (var hit in hits)
        {
            FishHitBox fish = hit.GetComponentInParent<FishHitBox>();
            FishTarget target = hit.GetComponentInParent<FishTarget>();

            if (fish != null && target != null)
            {
                hasHit = true;

                fish.OnHit(target.fishColor, transform);
                break;
            }
        }
    }
}
