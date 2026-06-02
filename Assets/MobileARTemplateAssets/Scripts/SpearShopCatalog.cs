using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tombakan/Spear Shop Catalog")]
public class SpearShopCatalog : ScriptableObject
{
    public List<SpearSkin> skins = new List<SpearSkin>();

    public SpearSkin GetById(string id) => skins.Find(s => s.id == id);

    public SpearSkin DefaultSkin => skins.Find(s => s.unlockedByDefault) ?? (skins.Count > 0 ? skins[0] : null);
}
