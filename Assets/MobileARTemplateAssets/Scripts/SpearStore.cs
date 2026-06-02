using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent spear skin ownership and equip state.
/// Pure buy/equip logic separated from storage for testability.
/// </summary>
public static class SpearStore
{
    const string OwnedKey  = "tombakan_owned_skins";
    const string EquipKey  = "tombakan_equipped_skin";

    // --- Pure logic (testable) ---

    public static bool IsOwned(string id, HashSet<string> owned) =>
        !string.IsNullOrEmpty(id) && owned.Contains(id);

    /// <summary>
    /// Adds <paramref name="id"/> to <paramref name="owned"/> if not already present.
    /// Returns true on new purchase, false if already owned (idempotent — no double-charge).
    /// </summary>
    public static bool Buy(string id, HashSet<string> owned)
    {
        if (string.IsNullOrEmpty(id) || owned.Contains(id)) return false;
        owned.Add(id);
        return true;
    }

    public static bool CanEquip(string id, HashSet<string> owned) =>
        owned.Contains(id);

    // --- Storage ---

    static HashSet<string> LoadOwned()
    {
        string raw = PlayerPrefs.GetString(OwnedKey, "");
        var set = new HashSet<string>();
        if (string.IsNullOrEmpty(raw)) return set;
        foreach (var id in raw.Split(','))
            if (!string.IsNullOrEmpty(id))
                set.Add(id);
        return set;
    }

    static void SaveOwned(HashSet<string> owned)
    {
        PlayerPrefs.SetString(OwnedKey, string.Join(",", owned));
        PlayerPrefs.Save();
    }

    public static bool IsOwned(string id) => IsOwned(id, LoadOwned());

    /// <summary>
    /// Attempts to purchase a skin. Deducts currency before writing ownership.
    /// Returns false if already owned or insufficient funds.
    /// </summary>
    public static bool Buy(string id, int price, SpearCurrency currency = SpearCurrency.Coins)
    {
        var owned = LoadOwned();
        if (owned.Contains(id)) return false; // already owned

        if (price > 0 && currency == SpearCurrency.Coins && !CurrencyStore.TrySpend(price))
            return false; // not enough coins

        owned.Add(id);
        SaveOwned(owned);
        return true;
    }

    public static void Equip(string id)
    {
        PlayerPrefs.SetString(EquipKey, id);
        PlayerPrefs.Save();
    }

    public static string EquippedId() => PlayerPrefs.GetString(EquipKey, "");

    /// <summary>Ensures the default skin is owned and equipped on first run.</summary>
    public static void EnsureDefault(string defaultId)
    {
        if (string.IsNullOrEmpty(defaultId)) return;
        var owned = LoadOwned();
        if (!owned.Contains(defaultId))
        {
            owned.Add(defaultId);
            SaveOwned(owned);
        }
        if (string.IsNullOrEmpty(EquippedId()))
            Equip(defaultId);
    }
}
