using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spear cosmetics shop screen.
/// Attach to the Shop panel. Wire catalog + UI references in Inspector.
/// Each shop tile prefab needs:
///   - Image (preview icon)
///   - TMP_Text (skin name)
///   - TMP_Text (price label)
///   - Button (buy / equip)
///   - TMP_Text on the button (state text: "Pasang" / "Beli" / "Terpasang")
/// </summary>
public class SpearShopUI : MonoBehaviour
{
    [Header("References")]
    public SpearShopCatalog catalog;
    public Transform tileContainer;  // ScrollView Content
    public GameObject tilePrefab;    // prefab per skin
    public GameObject shopPanel;
    public TMP_Text coinLabel;       // "Koin: 120"

    void OnEnable()
    {
        Refresh();
    }

    public void Open()
    {
        shopPanel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        shopPanel.SetActive(false);
    }

    void Refresh()
    {
        if (catalog == null || tileContainer == null || tilePrefab == null)
            return;

        foreach (Transform child in tileContainer)
            Destroy(child.gameObject);

        string equippedId = SpearStore.EquippedId();

        foreach (var skin in catalog.skins)
        {
            bool isOwned    = SpearStore.IsOwned(skin.id);
            bool isEquipped = skin.id == equippedId;

            GameObject tile = Instantiate(tilePrefab, tileContainer);
            ApplyTile(tile, skin, isOwned, isEquipped);
        }

        if (coinLabel != null)
            coinLabel.text = $"Koin: {CurrencyStore.GetCoins()}";
    }

    void ApplyTile(GameObject tile, SpearSkin skin, bool isOwned, bool isEquipped)
    {
        // Preview icon
        var img = tile.GetComponentInChildren<Image>();
        if (img != null && skin.previewIcon != null)
            img.sprite = skin.previewIcon;

        // Text fields
        var texts = tile.GetComponentsInChildren<TMP_Text>(true);
        if (texts.Length > 0) texts[0].text = skin.displayName;
        if (texts.Length > 1)
            texts[1].text = isOwned ? (isEquipped ? "Terpasang" : "Dimiliki") : $"{skin.price} Koin";

        // Button
        var btn = tile.GetComponentInChildren<Button>();
        if (btn == null) return;

        var btnText = btn.GetComponentInChildren<TMP_Text>();

        if (isEquipped)
        {
            if (btnText != null) btnText.text = "Terpasang";
            btn.interactable = false;
        }
        else if (isOwned)
        {
            if (btnText != null) btnText.text = "Pasang";
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SpearStore.Equip(skin.id);
                Refresh();
            });
        }
        else
        {
            if (btnText != null) btnText.text = $"Beli {skin.price}";
            btn.interactable = CurrencyStore.GetCoins() >= skin.price;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (SpearStore.Buy(skin.id, skin.price, skin.currency))
                {
                    SpearStore.Equip(skin.id);
                    Refresh();
                }
            });
        }
    }
}
