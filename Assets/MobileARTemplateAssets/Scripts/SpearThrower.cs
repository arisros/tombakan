using System.Collections;
using UnityEngine;

public class SpearThrower : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public GameObject spearFake;
    public GameObject spearProjectilePrefab;
    public Transform spearHolder;

    [Header("Holder Offset (POV Pundak)")]
    public Vector3 holderPositionOffset = new Vector3(0.2f, -0.15f, 0.35f);
    public Vector3 holderRotationOffset = Vector3.zero;

    [Header("Throw Settings")]
    public float throwForce = 15f;
    public float spearLifeTime = 2.5f;
    public float cooldown = 1.2f;

    bool canThrow = true;
    SpearLeash leash;

    void Awake()
    {
        leash = FindObjectOfType<SpearLeash>();
    }

    void LateUpdate()
    {
        if (!arCamera || !spearHolder)
            return;

        spearHolder.position = arCamera.transform.TransformPoint(holderPositionOffset);
        spearHolder.rotation = arCamera.transform.rotation * Quaternion.Euler(holderRotationOffset);
    }

    public void ThrowSpear()
    {
        if (!canThrow)
            return;
        canThrow = false;

        if (spearFake)
            spearFake.SetActive(false);

        GameObject spear = Instantiate(
            spearProjectilePrefab,
            arCamera.transform.position + arCamera.transform.forward * 0.4f,
            arCamera.transform.rotation
        );

        if (leash)
            leash.spearTip = spear.transform;

        Rigidbody rb = spear.GetComponent<Rigidbody>();
        if (rb)
            rb.velocity = arCamera.transform.forward * throwForce;

        Destroy(spear, spearLifeTime);

        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);

        if (spearFake)
            spearFake.SetActive(true);

        if (leash)
            leash.spearTip = null;

        canThrow = true;
    }
}
