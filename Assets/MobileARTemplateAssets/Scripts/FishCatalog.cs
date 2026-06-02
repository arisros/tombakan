using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tombakan/Fish Catalog")]
public class FishCatalog : ScriptableObject
{
    public List<FishSpecies> species = new List<FishSpecies>();

    public FishSpecies GetById(string id) => species.Find(s => s.id == id);

    // Weighted random pick respecting rarity
    public FishSpecies PickRandom()
    {
        if (species == null || species.Count == 0) return null;

        int total = 0;
        foreach (var s in species)
            total += WeightFor(s.rarity);

        int roll = Random.Range(0, total);
        int acc = 0;
        foreach (var s in species)
        {
            acc += WeightFor(s.rarity);
            if (roll < acc) return s;
        }
        return species[species.Count - 1];
    }

    // Pick a random species that is NOT the excluded one
    public FishSpecies PickOther(FishSpecies exclude)
    {
        if (species == null || species.Count <= 1) return exclude;

        FishSpecies picked;
        int attempts = 0;
        do
        {
            picked = PickRandom();
            attempts++;
        } while (picked == exclude && attempts < 20);

        return picked;
    }

    public static int WeightFor(FishRarity rarity) => rarity switch
    {
        FishRarity.Common   => 6,
        FishRarity.Uncommon => 3,
        FishRarity.Rare     => 1,
        _                   => 1,
    };
}
