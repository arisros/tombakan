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
            FishHitBox fishHitBox = hit.GetComponentInParent<FishHitBox>();
            FishTarget target     = hit.GetComponentInParent<FishTarget>();

            if (fishHitBox != null && target != null)
            {
                hasHit = true;
                fishHitBox.OnHit(target.fishColor, target.speciesId, transform);
                break;
            }
        }
    }
}
