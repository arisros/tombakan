using UnityEngine;

public class SpearThrower : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public Transform spearHolder;
    public GameObject spearPrefab;

    [Header("Settings")]
    public float throwForce = 15f;

    private GameObject currentSpear;

    void Start()
    {
        SpawnSpear();
    }

    void SpawnSpear()
    {
        currentSpear = Instantiate(spearPrefab, spearHolder);
        currentSpear.transform.localPosition = Vector3.zero;
        currentSpear.transform.localRotation = Quaternion.identity;

        // pastikan rigidbody non-aktif dulu
        Rigidbody rb = currentSpear.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
        }
    }

    public void ThrowSpear()
    {
        if (!currentSpear)
            return;

        currentSpear.transform.parent = null;

        Rigidbody rb = currentSpear.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.velocity = arCamera.transform.forward * throwForce;
        }

        // spawn tombak baru setelah lempar
        Invoke(nameof(SpawnSpear), 0.5f);
    }
}
