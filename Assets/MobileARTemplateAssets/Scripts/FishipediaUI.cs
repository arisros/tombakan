using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fishipedia collection screen. Attach to the Fishipedia panel GameObject.
/// Populate the catalog reference in the Inspector. The entry prefab needs:
///   - Image (icon / silhouette)
///   - TMP_Text (species name)
///   - TMP_Text (fun fact) — optional, only shown when unlocked
///   - CanvasGroup or Image alpha (for locked state visual)
/// </summary>
public class FishipediaUI : MonoBehaviour
{
    [Header("References")]
    public FishCatalog catalog;
    public Transform entryContainer;   // ScrollView Content
    public GameObject entryPrefab;     // prefab for one catalog card
    public GameObject fishipediaPanel; // the full-screen panel to open/close
    public TMP_Text countLabel;        // "3 / 8 Ditemukan"

    [Header("Sprites")]
    public Sprite lockedSilhouette;    // shown instead of icon when not yet caught

    void OnEnable()
    {
        Refresh();
    }

    public void Open()
    {
        fishipediaPanel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        fishipediaPanel.SetActive(false);
    }

    void Refresh()
    {
        if (catalog == null || entryContainer == null || entryPrefab == null)
            return;

        // Clear existing cards
        foreach (Transform child in entryContainer)
            Destroy(child.gameObject);

        var unlocked = new HashSet<string>(FishdexStore.UnlockedIds());
        int unlockedCount = 0;

        foreach (var species in catalog.species)
        {
            bool isUnlocked = unlocked.Contains(species.id);
            if (isUnlocked) unlockedCount++;

            GameObject card = Instantiate(entryPrefab, entryContainer);
            ApplyCard(card, species, isUnlocked);
        }

        if (countLabel != null)
            countLabel.text = $"{unlockedCount} / {catalog.species.Count} Ditemukan";
    }

    void ApplyCard(GameObject card, FishSpecies species, bool isUnlocked)
    {
        // Icon
        var images = card.GetComponentsInChildren<Image>(true);
        if (images.Length > 0)
        {
            images[0].sprite = isUnlocked && species.icon != null
                ? species.icon
                : lockedSilhouette;
            images[0].color = isUnlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        // Text fields — assumes first TMP = name, second = fun fact
        var texts = card.GetComponentsInChildren<TMP_Text>(true);
        if (texts.Length > 0)
            texts[0].text = isUnlocked ? species.displayName : "???";

        if (texts.Length > 1)
        {
            texts[1].text    = isUnlocked ? species.funFact : "";
            texts[1].enabled = isUnlocked;
        }
    }
}
