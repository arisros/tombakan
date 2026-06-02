using System.Collections;
using UnityEngine;

public class FishHitBox : MonoBehaviour
{
    public float hitRadius = 0.12f;
    public float destroyDelay = 1.2f;

    bool isHit;

    public void OnHit(Color fishColor, string speciesId, Transform spear)
    {
        if (isHit) return;
        isHit = true;

        GameManager.I.OnFishHit(fishColor, speciesId);

        StickSpear(spear);
        StartCoroutine(DestroyAfterDelay());
    }

    void StickSpear(Transform spear)
    {
        Rigidbody rb = spear.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        spear.SetParent(transform);
        spear.position = transform.position;
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(transform.root.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
