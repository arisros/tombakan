using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public Transform waterPlane;

    public int fishCount = 5;
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
            Color fishColor = (i == correctIndex) ? targetColor : RandomOtherColor(targetColor);

            ApplyColor(fish, fishColor);

            // kasih info ke fish
            FishTarget ft = fish.GetComponent<FishTarget>();

            if (ft != null)
                ft.fishColor = fishColor;

            spawnedFish[i] = fish;
        }
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

    Color RandomOtherColor(Color target)
    {
        Color[] colors = { Color.red, Color.green, Color.blue };
        Color c;
        do
        {
            c = colors[Random.Range(0, colors.Length)];
        } while (c == target);

        return c;
    }
}
