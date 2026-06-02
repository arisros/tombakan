using UnityEngine;

public enum SpearCurrency { Coins, Premium }

[CreateAssetMenu(menuName = "Tombakan/Spear Skin")]
public class SpearSkin : ScriptableObject
{
    public string id;                               // stable key, e.g. "spear_bamboo"
    public string displayName;                      // Indonesian, e.g. "Tombak Bambu"
    public GameObject prefab;                       // null = use default spear prefab
    public Material material;                       // null = use default spear material
    public int price;                               // in soft currency (0 = free)
    public SpearCurrency currency = SpearCurrency.Coins;
    public bool unlockedByDefault;
    public Sprite previewIcon;
}
