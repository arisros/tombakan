using UnityEngine;

/// <summary>
/// Pure stateless XP/level logic — no MonoBehaviour, no PlayerPrefs.
/// All methods are unit-testable without a running scene.
/// Curve: XP needed for level n = 100 * (n-1)^1.5, rounded to int.
/// Level 1 always requires 0 XP (every player starts at level 1).
/// </summary>
public static class ProgressionRules
{
    public const int MaxLevel = 50;

    // --- Level curve ---

    /// <summary>Cumulative XP required to reach <paramref name="level"/>.</summary>
    public static int XpForLevel(int level)
    {
        if (level <= 1) return 0;
        level = Mathf.Clamp(level, 1, MaxLevel);
        return Mathf.RoundToInt(100f * Mathf.Pow(level - 1, 1.5f));
    }

    /// <summary>Player's level for a given total XP accumulation.</summary>
    public static int LevelForXp(int totalXp)
    {
        totalXp = Mathf.Max(0, totalXp);
        for (int lv = MaxLevel; lv >= 2; lv--)
            if (totalXp >= XpForLevel(lv))
                return lv;
        return 1;
    }

    /// <summary>XP accumulated inside the current level (progress towards next level).</summary>
    public static int XpIntoLevel(int totalXp)
    {
        int lv = LevelForXp(totalXp);
        return Mathf.Max(0, totalXp - XpForLevel(lv));
    }

    /// <summary>XP still needed to reach the next level. 0 at max level.</summary>
    public static int XpToNextLevel(int totalXp)
    {
        int lv = LevelForXp(totalXp);
        if (lv >= MaxLevel) return 0;
        return XpForLevel(lv + 1) - totalXp;
    }

    // --- XP earn constants ---

    public const int XpPerCorrectHit  = 10;
    public const int XpPerComboBonus  = 5;   // per (multiplier - 1)
    public const int XpPerAccuracyPct = 1;   // per accuracy percent
    public const int XpNewSpecies     = 50;  // big reward for first catch

    /// <summary>
    /// XP awarded at end of a game run. All inputs are pure values so this is testable.
    /// </summary>
    /// <param name="correctHits">Number of correctly speared fish.</param>
    /// <param name="accuracy">Integer accuracy percent 0–100.</param>
    /// <param name="newSpeciesCount">Species caught for the first time this run.</param>
    /// <param name="maxComboStreak">Highest combo streak reached during the run.</param>
    public static int XpForResult(int correctHits, int accuracy, int newSpeciesCount, int maxComboStreak)
    {
        int multiplier = ComboMultiplierForStreak(maxComboStreak);
        int xp = 0;
        xp += Mathf.Max(0, correctHits) * XpPerCorrectHit;
        xp += Mathf.Max(0, multiplier - 1) * XpPerComboBonus;
        xp += Mathf.Clamp(accuracy, 0, 100) * XpPerAccuracyPct;
        xp += Mathf.Max(0, newSpeciesCount) * XpNewSpecies;
        return Mathf.Max(0, xp);
    }

    // Mirrors GameManager.ComboMultiplier so the rule is testable independently
    public static int ComboMultiplierForStreak(int streak)
    {
        if (streak >= 5) return 3;
        if (streak >= 3) return 2;
        return 1;
    }
}
