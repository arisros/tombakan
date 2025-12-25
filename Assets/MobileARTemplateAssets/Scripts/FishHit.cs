using UnityEngine;

public class FishHit : MonoBehaviour
{
    public bool isHit = false;

    public void OnHit(Transform spear)
    {
        if (isHit)
            return;
        isHit = true;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim)
            anim.enabled = false;
    }
}
