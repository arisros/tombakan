using UnityEngine;

public class SpearHit : MonoBehaviour
{
    public LayerMask fishLayer;
    public float hitRadius = 0.1f;

    bool hasHit;

    void Awake()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (fishLayer.value == 0)
            Debug.LogError(
                "[SpearHit] fishLayer is unassigned (mask value = 0). " +
                "Assign the Fish layer in the Inspector on this SpearHit component. " +
                "Falling back to an unmasked OverlapSphere so hits are still registered.",
                this);
#endif
    }

    void Update()
    {
        if (!hasHit)
            CheckFishHit();
    }

    void CheckFishHit()
    {
        // When fishLayer is unassigned (value == 0 means no layers selected),
        // fall back to an unmasked check so spears always register hits.
        Collider[] hits = fishLayer.value == 0
            ? Physics.OverlapSphere(transform.position, hitRadius)
            : Physics.OverlapSphere(transform.position, hitRadius, fishLayer);

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
