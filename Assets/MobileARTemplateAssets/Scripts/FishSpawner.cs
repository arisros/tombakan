using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public Transform waterPlane;

    public int fishCount = 5;
    public float spawnRadius = 1.5f;

    public Material correctFishMaterial;

    void Start()
    {
        SpawnFish();
    }

    void SpawnFish()
    {
        int correctIndex = Random.Range(0, fishCount);

        for (int i = 0; i < fishCount; i++)
        {
            Vector3 pos =
                waterPlane.position
                + new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    Random.Range(-0.3f, -0.1f), // depth
                    Random.Range(-spawnRadius, spawnRadius)
                );

            GameObject fish = Instantiate(fishPrefab, pos, Quaternion.identity);

            FishSwim swim = fish.AddComponent<FishSwim>();
            swim.horizontalRadius = spawnRadius;

            // tandai ikan benar
            if (i == correctIndex)
            {
                var renderer = fish.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material = correctFishMaterial;
                }

                fish.tag = "CorrectFish";
            }
        }
    }
}
