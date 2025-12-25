using UnityEngine;

public class FishHitBox : MonoBehaviour
{
    public float hitRadius = 0.12f;

    public void OnHit(Color fishColor)
    {
        GameManager.I.OnFishHit(fishColor);
        Destroy(transform.root.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
