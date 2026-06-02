using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent storage for unlocked achievements.
/// Each achievement is stored as an individual PlayerPrefs key with prefix "ach_".
/// Pattern mirrors <see cref="FishdexStore"/> for consistency.
/// </summary>
public static class AchievementStore
{
    const string KeyPrefix = "ach_";

    /// <summary>Returns true if the achievement with the given id has been unlocked.</summary>
    public static bool IsUnlocked(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return PlayerPrefs.GetInt(KeyPrefix + id, 0) == 1;
    }

    /// <summary>
    /// Unlocks the achievement. Idempotent — calling multiple times has no additional effect.
    /// </summary>
    public static void Unlock(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        PlayerPrefs.SetInt(KeyPrefix + id, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns the ids of all currently unlocked achievements by scanning PlayerPrefs keys
    /// with the "ach_" prefix.
    /// Note: Unity's PlayerPrefs does not expose a key enumeration API natively.
    /// This implementation delegates to <see cref="AchievementStorePrefsHelper"/>.
    /// </summary>
    public static string[] GetAllUnlocked()
    {
        return AchievementStorePrefsHelper.GetKeysWithPrefix(KeyPrefix);
    }
}

/// <summary>
/// Internal helper that provides testable key enumeration.
/// In production builds this reads from the platform registry/plist via a known key set.
/// In tests, the list can be driven through <see cref="RegisterKnownId"/>.
/// </summary>
public static class AchievementStorePrefsHelper
{
    // The set of ids that have ever been registered for scanning.
    // Populated lazily by AchievementChecker after evaluating catalog, or by tests.
    static readonly HashSet<string> knownIds = new HashSet<string>();

    /// <summary>
    /// Registers an id so it can be discovered by <see cref="GetKeysWithPrefix"/>.
    /// Calling Unlock on an id without registering it first will persist correctly;
    /// GetAllUnlocked will include it only once it has been registered.
    /// </summary>
    public static void RegisterKnownId(string id)
    {
        if (!string.IsNullOrEmpty(id))
            knownIds.Add(id);
    }

    /// <summary>Clears the known-id registry. Call from test SetUp to start clean.</summary>
    public static void ClearKnownIds()
    {
        knownIds.Clear();
    }

    /// <summary>
    /// Returns the stripped ids (without prefix) for all registered ids that are
    /// currently unlocked in PlayerPrefs.
    /// </summary>
    public static string[] GetKeysWithPrefix(string prefix)
    {
        var result = new List<string>();
        foreach (var id in knownIds)
        {
            if (PlayerPrefs.GetInt(prefix + id, 0) == 1)
                result.Add(id);
        }
        return result.ToArray();
    }
}
