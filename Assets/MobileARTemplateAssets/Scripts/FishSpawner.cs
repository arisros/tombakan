using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public Transform waterPlane;

    [HideInInspector] public int fishCount = 3;
    public float spawnRadius = 1.5f;

    private readonly float minDepth = -0.3f;
    private readonly float maxDepth = -0.1f;

    // simpan ikan aktif
    private GameObject[] spawnedFish;

    public void SpawnFish(Color targetColor)
    {
        ClearFish();

        spawnedFish = new GameObject[fishCount];
        int correctIndex = Random.Range(0, fishCount);

        for (int i = 0; i < fishCount; i++)
        {
            Vector3 pos =
                waterPlane.position
                + new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    Random.Range(minDepth, maxDepth),
                    Random.Range(-spawnRadius, spawnRadius)
                );

            GameObject fish = Instantiate(fishPrefab, pos, Quaternion.identity);

            // movement
            FishSwim swim = fish.AddComponent<FishSwim>();
            swim.horizontalRadius = spawnRadius;

            // warna
            Color fishColor = (i == correctIndex) ? targetColor : FishPalette.RandomOther(targetColor);

            ApplyColor(fish, fishColor);

            // kasih info ke fish
            FishTarget ft = fish.GetComponent<FishTarget>();

            if (ft != null)
                ft.fishColor = fishColor;

            spawnedFish[i] = fish;
        }
    }

    /// <summary>
    /// Destroys every active fish. Called at end-of-game so the last shoal does not
    /// keep swimming behind the result screen (Week 2 BUG-04).
    /// </summary>
    public void ClearAll()
    {
        ClearFish();
        spawnedFish = null;
    }

    void ClearFish()
    {
        if (spawnedFish == null)
            return;

        foreach (var fish in spawnedFish)
        {
            if (fish)
                Destroy(fish);
        }
    }

    void ApplyColor(GameObject fish, Color color)
    {
        var renderer = fish.GetComponentInChildren<Renderer>();
        if (renderer)
            renderer.material.color = color;
    }

}
