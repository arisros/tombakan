using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Persistent Fishipedia (Fishdex) — tracks which species the player has caught.
/// Pure static helpers are separated from storage so they are unit-testable.
/// </summary>
public static class FishdexStore
{
    const string Key = "tombakan_fishdex";

    // --- Pure logic (testable without PlayerPrefs) ---

    public static bool IsUnlocked(string id, HashSet<string> set) => set.Contains(id);

    /// <summary>Returns true if the species was newly added (false if already present).</summary>
    public static bool Unlock(string id, HashSet<string> set)
    {
        if (string.IsNullOrEmpty(id) || set.Contains(id)) return false;
        set.Add(id);
        return true;
    }

    public static int Count(HashSet<string> set) => set.Count;

    // --- Storage ---

    public static HashSet<string> Load()
    {
        string raw = PlayerPrefs.GetString(Key, "");
        var set = new HashSet<string>();
        if (string.IsNullOrEmpty(raw)) return set;
        foreach (var id in raw.Split(','))
            if (!string.IsNullOrEmpty(id))
                set.Add(id);
        return set;
    }

    public static void Save(HashSet<string> set)
    {
        PlayerPrefs.SetString(Key, string.Join(",", set));
        PlayerPrefs.Save();
    }

    // --- Convenience (load + save automatically) ---

    public static bool IsUnlocked(string id) => Load().Contains(id);

    public static int UnlockedCount() => Load().Count;

    public static List<string> UnlockedIds() => Load().ToList();

    /// <summary>Persists the unlock. Returns true if it was a new discovery.</summary>
    public static bool Unlock(string id)
    {
        var set = Load();
        if (!Unlock(id, set)) return false;
        Save(set);
        return true;
    }
}
