using System;
using UnityEngine;

/// <summary>
/// Daily challenge: awards a bonus XP on the first game played each calendar day.
/// Pure date-check logic separated from storage for testability.
/// </summary>
public static class DailyChallenge
{
    const string LastPlayedKey = "tombakan_last_played_date";

    public const int DailyBonusXp   = 100;
    public const int StreakBonusXp  = 25;  // extra per consecutive-day streak (up to 7)
    const string StreakKey = "tombakan_daily_streak";
    const string StreakDateKey = "tombakan_streak_date";

    // --- Pure logic (testable) ---

    public static bool IsNewDay(string storedDate, string todayDate) =>
        storedDate != todayDate;

    public static int TotalBonusXp(int streak)
    {
        int capped = Mathf.Clamp(streak, 0, 7);
        return DailyBonusXp + capped * StreakBonusXp;
    }

    // --- Storage ---

    static string Today => DateTime.UtcNow.ToString("yyyy-MM-dd");

    /// <summary>
    /// Attempts to claim the daily login bonus for today.
    /// </summary>
    /// <param name="xpAwarded">XP granted if the claim succeeds, otherwise 0.</param>
    /// <param name="streak">Current consecutive-day streak count.</param>
    /// <param name="newLevel">
    /// The new level if the XP caused a level-up; 0 if no level-up occurred.
    /// Pass this to <see cref="GameManager.ApplyLevelReward(int)"/> to materialise
    /// any configured LevelRewardTable reward (coins, species unlock, spear skin).
    /// </param>
    /// <returns>True when a bonus was claimed (first play of the calendar day).</returns>
    public static bool TryClaimDailyBonus(out int xpAwarded, out int streak, out int newLevel)
    {
        string lastPlayed = PlayerPrefs.GetString(LastPlayedKey, "");
        string today      = Today;

        if (!IsNewDay(lastPlayed, today))
        {
            xpAwarded = 0;
            streak    = PlayerPrefs.GetInt(StreakKey, 0);
            newLevel  = 0;
            return false;
        }

        // Check if streak continues (played yesterday) or resets
        string lastStreakDate = PlayerPrefs.GetString(StreakDateKey, "");
        int currentStreak     = PlayerPrefs.GetInt(StreakKey, 0);

        bool consecutive = IsConsecutiveDay(lastStreakDate, today);
        streak = consecutive ? currentStreak + 1 : 1;

        xpAwarded = TotalBonusXp(streak);

        PlayerPrefs.SetString(LastPlayedKey, today);
        PlayerPrefs.SetString(StreakDateKey, today);
        PlayerPrefs.SetInt(StreakKey, streak);
        PlayerPrefs.Save();

        // Capture the return value so callers can surface a level-up celebration
        // and apply the LevelRewardTable reward (TASK-02b).
        newLevel = ProgressionStore.AddXp(xpAwarded);
        return true;
    }

    public static int GetStreak() => PlayerPrefs.GetInt(StreakKey, 0);

    static bool IsConsecutiveDay(string prevDate, string today)
    {
        if (string.IsNullOrEmpty(prevDate)) return false;
        if (!DateTime.TryParse(prevDate, out DateTime prev))  return false;
        if (!DateTime.TryParse(today,    out DateTime todayDt)) return false;
        return (todayDt - prev).Days == 1;
    }
}
