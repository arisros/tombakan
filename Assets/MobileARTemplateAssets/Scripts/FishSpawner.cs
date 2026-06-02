using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public Transform waterPlane;

    [HideInInspector] public int fishCount = 3;
    public float spawnRadius = 1.5f;

    [Header("Species System (Phase 1)")]
    public FishCatalog catalog; // assign in Inspector; null = colour-only mode

    readonly float minDepth = -0.3f;
    readonly float maxDepth = -0.1f;

    GameObject[] spawnedFish;

    // --- Current target species (set per round when using catalog) ---
    public FishSpecies CurrentTargetSpecies { get; private set; }

    public void SpawnFish(Color targetColor, int activeColorCount)
    {
        ClearFish();

        // When a catalog is assigned, pick a target species from it
        FishSpecies targetSpecies = catalog != null ? catalog.PickRandom() : null;
        CurrentTargetSpecies = targetSpecies;

        Color resolvedTargetColor = targetSpecies != null ? targetSpecies.baseColor : targetColor;

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

            FishSpecies spawnSpecies;
            Color fishColor;

            if (i == correctIndex)
            {
                spawnSpecies = targetSpecies;
                fishColor    = resolvedTargetColor;
            }
            else if (catalog != null)
            {
                spawnSpecies = catalog.PickOther(targetSpecies);
                fishColor    = spawnSpecies != null ? spawnSpecies.baseColor
                                                    : FishPalette.RandomOther(resolvedTargetColor, activeColorCount);
            }
            else
            {
                spawnSpecies = null;
                fishColor    = FishPalette.RandomOther(targetColor, activeColorCount);
            }

            // Use species prefab if available, otherwise fall back to default fishPrefab
            GameObject prefabToUse = (spawnSpecies?.modelPrefab != null)
                ? spawnSpecies.modelPrefab
                : fishPrefab;

            GameObject fish = Instantiate(prefabToUse, pos, Quaternion.identity);
            fish.AddComponent<FishSwim>().horizontalRadius = spawnRadius;

            ApplyColor(fish, fishColor);

            FishTarget ft = fish.GetComponent<FishTarget>();
            if (ft != null)
            {
                ft.fishColor  = fishColor;
                ft.speciesId  = spawnSpecies != null ? spawnSpecies.id : "";
            }

            spawnedFish[i] = fish;
        }
    }

    /// <summary>Destroys every active fish. Called at end-of-game (BUG-04).</summary>
    public void ClearAll()
    {
        ClearFish();
        spawnedFish = null;
        CurrentTargetSpecies = null;
    }

    void ClearFish()
    {
        if (spawnedFish == null) return;
        foreach (var fish in spawnedFish)
            if (fish) Destroy(fish);
    }

    void ApplyColor(GameObject fish, Color color)
    {
        var renderer = fish.GetComponentInChildren<Renderer>();
        if (renderer)
            renderer.material.color = color;
    }
}
