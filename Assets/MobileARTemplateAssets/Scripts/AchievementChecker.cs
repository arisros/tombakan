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
    /// Backward-compatible overload — discards the level-up return value.
    /// Prefer <see cref="CheckAll(GameManager, AchievementCatalog, out int)"/> in EndGame
    /// so any XP-triggered level-up is merged before calling StaggerResultCelebrations.
    /// </summary>
    public static string[] CheckAll(GameManager gm, AchievementCatalog catalog)
    {
        int unused;
        return CheckAll(gm, catalog, out unused);
    }

    /// <summary>
    /// Reads session data from <paramref name="gm"/> and live stores, evaluates all
    /// achievement conditions, and reports any XP-triggered level-up via
    /// <paramref name="levelGained"/> so the caller can merge it before showing
    /// level-up UI (TASK-02a).
    /// </summary>
    /// <param name="levelGained">
    /// The new level if any achievement XP reward caused a level-up; 0 otherwise.
    /// </param>
    public static string[] CheckAll(GameManager gm, AchievementCatalog catalog, out int levelGained)
    {
        levelGained = 0;
        if (gm == null) return new string[0];

        var session = new AchievementSession
        {
            correctHitCount      = gm.correctHitCount,
            wrongHitCount        = gm.WrongHitCount,
            maxComboStreak       = gm.MaxComboStreak,
            playerLevel          = ProgressionStore.GetLevel(),
            fishdexUnlockedCount = FishdexStore.UnlockedCount(),
        };

        return CheckAll(session, catalog, out levelGained);
    }

    // ── Pure / testable entry point ───────────────────────────────────────────

    /// <summary>
    /// Checks all achievement conditions against the supplied session snapshot.
    /// Only unlocks achievements not already unlocked, and grants XP if a catalog
    /// is provided.
    /// Backward-compatible overload — discards the level-up return value.
    /// </summary>
    /// <param name="session">End-of-session snapshot.</param>
    /// <param name="catalog">Optional; pass null to skip XP reward grant.</param>
    /// <returns>Array of achievement ids newly unlocked during this call.</returns>
    public static string[] CheckAll(AchievementSession session, AchievementCatalog catalog)
    {
        int unused;
        return CheckAll(session, catalog, out unused);
    }

    /// <summary>
    /// Checks all achievement conditions against the supplied session snapshot.
    /// Only unlocks achievements not already unlocked, and grants XP if a catalog
    /// is provided. Reports any XP-triggered level-up via <paramref name="levelGained"/>
    /// so EndGame can merge it into newLevel before calling StaggerResultCelebrations
    /// and ApplyLevelReward (TASK-02a).
    /// </summary>
    /// <param name="session">End-of-session snapshot.</param>
    /// <param name="catalog">Optional; pass null to skip XP reward grant.</param>
    /// <param name="levelGained">
    /// The highest new level reached via achievement XP rewards; 0 if no level-up.
    /// </param>
    /// <returns>Array of achievement ids newly unlocked during this call.</returns>
    public static string[] CheckAll(AchievementSession session, AchievementCatalog catalog, out int levelGained)
    {
        levelGained = 0;

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

            // Grant XP reward if catalog is provided; capture any level-up
            // so it can be propagated to the caller (TASK-02a).
            if (catalog != null)
            {
                Achievement achievement = catalog.GetById(id);
                if (achievement != null && achievement.xpReward > 0)
                {
                    int lvUp = ProgressionStore.AddXp(achievement.xpReward);
                    if (lvUp > levelGained) levelGained = lvUp;
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
