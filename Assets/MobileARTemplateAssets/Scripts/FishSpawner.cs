using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public Transform waterPlane;
    public int fishCount = 5;
    public float spawnRadius = 1.5f;

    void Start()
    {
        SpawnFish();
    }

    void SpawnFish()
    {
        for (int i = 0; i < fishCount; i++)
        {
            Vector3 randomPos =
                waterPlane.position
                + new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    -0.05f, // sedikit di bawah air
                    Random.Range(-spawnRadius, spawnRadius)
                );

            GameObject fish = Instantiate(fishPrefab, randomPos, Quaternion.identity);

            fish.transform.LookAt(waterPlane.position + Random.insideUnitSphere);
            fish.AddComponent<FishSwim>();
        }
    }
}
