using UnityEngine;

[CreateAssetMenu(menuName = "Tombakan/Fish Species")]
public class FishSpecies : ScriptableObject
{
    public string id;
    public string displayName;   // Indonesian, e.g. "Ikan Badut"
    public string englishName;   // optional, for parents/older kids
    public string latinName;     // optional
    public GameObject modelPrefab; // null = tuna placeholder tinted by baseColor
    public Color baseColor = Color.white;
    public FishRarity rarity = FishRarity.Common;
    public string habitatId;     // e.g. "terumbu_karang"
    [TextArea] public string funFact; // Indonesian, one short verified line
    public Sprite icon;          // target card + Fishipedia card
}

public enum FishRarity { Common, Uncommon, Rare }
