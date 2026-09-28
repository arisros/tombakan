using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Carries the session snapshot required to evaluate achievement conditions.
/// Using a plain struct keeps AchievementChecker testable without a MonoBehaviour.
/// </summary>
public struct AchievementSession
{
    public int correctHitCount;
    public int wrongHitCount;
    public int maxComboStreak;
    public int playerLevel;         // ProgressionStore.GetLevel() at end-of-game
    public int fishdexUnlockedCount; // FishdexStore.UnlockedCount() at end-of-game
}

/// <summary>
/// Evaluates achievement conditions against the current game session and catalog.
/// Call <see cref="CheckAll(GameManager, AchievementCatalog)"/> at the end of a game
/// session (e.g. in EndGame) to discover and persist any newly unlocked achievements.
/// </summary>
public static class AchievementChecker
{
    // Known achievement ids — kept in sync with the catalog convention.
    public const string FirstCatch        = "first_catch";
    public const string Combo5            = "combo_5";
    public const string PerfectRound      = "perfect_round";
    public const string Level5            = "level_5";
    public const string Level10           = "level_10";
    public const string SpeciesCollector5 = "species_collector_5";

    // ── MonoBehaviour entry point ────────────────────────────────────────────

    /// <summary>
    /// Convenience overload: reads session data from <paramref name="gm"/> and
    /// live stores, then delegates to <see cref="CheckAll(AchievementSession, AchievementCatalog)"/>.
    /// </summary>
    public static string[] CheckAll(GameManager gm, AchievementCatalog catalog)
    {
        if (gm == null) return new string[0];

        var session = new AchievementSession
        {
            correctHitCount      = gm.correctHitCount,
            wrongHitCount        = gm.WrongHitCount,
            maxComboStreak       = gm.MaxComboStreak,
            playerLevel          = ProgressionStore.GetLevel(),
            fishdexUnlockedCount = FishdexStore.UnlockedCount(),
        };

        return CheckAll(session, catalog);
    }

    // ── Pure / testable entry point ───────────────────────────────────────────

    /// <summary>
    /// Checks all achievement conditions against the supplied session snapshot.
    /// Only unlocks achievements not already unlocked, and grants XP if a catalog
    /// is provided.
    /// </summary>
    /// <param name="session">End-of-session snapshot.</param>
    /// <param name="catalog">Optional; pass null to skip XP reward grant.</param>
    /// <returns>Array of achievement ids newly unlocked during this call.</returns>
    public static string[] CheckAll(AchievementSession session, AchievementCatalog catalog)
    {
        // Register all known ids so GetAllUnlocked can discover them.
        RegisterAllKnownIds();

        int  totalThrows = session.correctHitCount + session.wrongHitCount;
        bool isPerfect   = totalThrows > 0 && session.wrongHitCount == 0;

        var conditions = new Dictionary<string, bool>
        {
            { FirstCatch,        session.correctHitCount >= 1 },
            { Combo5,            session.maxComboStreak >= 5 },
            { PerfectRound,      isPerfect },
            { Level5,            session.playerLevel >= 5 },
            { Level10,           session.playerLevel >= 10 },
            { SpeciesCollector5, session.fishdexUnlockedCount >= 5 },
        };

        var newlyUnlocked = new List<string>();

        foreach (var kv in conditions)
        {
            string id  = kv.Key;
            bool   met = kv.Value;

            if (!met) continue;
            if (AchievementStore.IsUnlocked(id)) continue;

            AchievementStore.Unlock(id);
            newlyUnlocked.Add(id);

            // Grant XP reward if catalog is provided.
            if (catalog != null)
            {
                Achievement achievement = catalog.GetById(id);
                if (achievement != null && achievement.xpReward > 0)
                {
                    int newLevel = ProgressionStore.AddXp(achievement.xpReward);
                    if (newLevel > 0)
                        GameManager.I?.ApplyLevelReward(
                            GameManager.I?.levelRewardTable?.GetRewardForLevel(newLevel));
                }
            }
        }

        return newlyUnlocked.ToArray();
    }

    static void RegisterAllKnownIds()
    {
        AchievementStorePrefsHelper.RegisterKnownId(FirstCatch);
        AchievementStorePrefsHelper.RegisterKnownId(Combo5);
        AchievementStorePrefsHelper.RegisterKnownId(PerfectRound);
        AchievementStorePrefsHelper.RegisterKnownId(Level5);
        AchievementStorePrefsHelper.RegisterKnownId(Level10);
        AchievementStorePrefsHelper.RegisterKnownId(SpeciesCollector5);
    }
}
