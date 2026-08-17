using System.Collections;
using UnityEngine;

public class SpearThrower : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public GameObject spearFake;
    public GameObject spearProjectilePrefab;
    public Transform spearHolder;

    [Header("Skin Catalog (Phase 2)")]
    public SpearShopCatalog skinCatalog; // assign in Inspector; null = always use default prefab

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

    void Start()
    {
        if (skinCatalog != null)
            SpearStore.EnsureDefault(skinCatalog.DefaultSkin?.id);
    }

    void LateUpdate()
    {
        if (!arCamera || !spearHolder) return;

        spearHolder.position = arCamera.transform.TransformPoint(holderPositionOffset);
        spearHolder.rotation = arCamera.transform.rotation * Quaternion.Euler(holderRotationOffset);
    }

    public void ThrowSpear()
    {
        if (!canThrow) return;
        canThrow = false;

        TombakanOnboarding.I?.NotifyFirstThrow();

        if (spearFake) spearFake.SetActive(false);

        GameObject prefab = ResolveEquippedPrefab();
        GameObject spear  = Instantiate(
            prefab,
            arCamera.transform.position + arCamera.transform.forward * 0.4f,
            arCamera.transform.rotation
        );

        // Apply equipped skin material if overridden
        ApplyEquippedMaterial(spear);

        if (leash) leash.spearTip = spear.transform;

        Rigidbody rb = spear.GetComponent<Rigidbody>();
        if (rb) rb.velocity = arCamera.transform.forward * throwForce;

        Destroy(spear, spearLifeTime);
        StartCoroutine(CooldownRoutine());
    }

    GameObject ResolveEquippedPrefab()
    {
        if (skinCatalog == null) return spearProjectilePrefab;
        string equippedId = SpearStore.EquippedId();
        SpearSkin skin    = skinCatalog.GetById(equippedId);
        if (skin != null && skin.prefab != null) return skin.prefab;
        return spearProjectilePrefab;
    }

    void ApplyEquippedMaterial(GameObject spear)
    {
        if (skinCatalog == null) return;
        string equippedId = SpearStore.EquippedId();
        SpearSkin skin    = skinCatalog.GetById(equippedId);
        if (skin == null || skin.material == null) return;
        var renderer = spear.GetComponentInChildren<Renderer>();
        if (renderer) renderer.material = skin.material;
    }

    IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        if (spearFake) spearFake.SetActive(true);
        if (leash) leash.spearTip = null;
        canThrow = true;
    }

    public void LockThrow(float delay)
    {
        StopAllCoroutines();
        StartCoroutine(LockRoutine(delay));
    }

    IEnumerator LockRoutine(float delay)
    {
        canThrow = false;
        if (leash) leash.spearTip = null;
        if (spearFake) spearFake.SetActive(false);
        yield return new WaitForSeconds(delay);
        if (spearFake) spearFake.SetActive(true);
        canThrow = true;
    }
}
