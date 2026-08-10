using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode tests for BUG-1: DailyChallenge.TryClaimDailyBonus must expose the
/// ProgressionStore.AddXp return value via the <c>out int newLevel</c> parameter so
/// callers (TombakanOnboarding) can invoke GameManager.ApplyLevelReward when a
/// level boundary is crossed during a daily bonus claim.
/// </summary>
public class DailyChallengeProgressionTests
{
    // PlayerPrefs keys mirrored from DailyChallenge (private consts).
    const string XpKey       = "tombakan_total_xp";
    const string LastPlayKey = "tombakan_last_played_date";
    const string StreakKey   = "tombakan_daily_streak";
    const string StreakDate  = "tombakan_streak_date";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(XpKey);
        PlayerPrefs.DeleteKey(LastPlayKey);
        PlayerPrefs.DeleteKey(StreakKey);
        PlayerPrefs.DeleteKey(StreakDate);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(XpKey);
        PlayerPrefs.DeleteKey(LastPlayKey);
        PlayerPrefs.DeleteKey(StreakKey);
        PlayerPrefs.DeleteKey(StreakDate);
    }

    // ── BUG-1 core ───────────────────────────────────────────────────────────

    [Test]
    public void TryClaimDailyBonus_WhenXpCrossesLevelBoundary_ReturnsCorrectNewLevel()
    {
        // Level 2 threshold = XpForLevel(2) = 100 XP.
        // Start 10 XP short so the streak-1 daily bonus (125 XP) crosses it.
        int xpForLevel2 = ProgressionRules.XpForLevel(2); // 100
        PlayerPrefs.SetInt(XpKey, xpForLevel2 - 10);       // 90 XP → level 1

        // No last-played date on record → IsNewDay returns true → bonus fires.
        bool claimed = DailyChallenge.TryClaimDailyBonus(
            out int xpAwarded, out int streak, out int newLevel);

        Assert.IsTrue(claimed,       "Daily bonus should be claimable on a new day");
        Assert.Greater(xpAwarded, 0, "XP should be awarded");
        Assert.AreEqual(2, newLevel, "newLevel must equal 2 when the level-2 boundary is crossed");
    }

    [Test]
    public void TryClaimDailyBonus_WhenXpDoesNotCrossLevelBoundary_ReturnsZeroNewLevel()
    {
        // Level 10 requires 2700 XP; level 11 requires 3162 XP.
        // Starting at 2800 XP: 2800 + 125 (streak-1 bonus) = 2925 < 3162 → no level-up.
        PlayerPrefs.SetInt(XpKey, 2800);

        bool claimed = DailyChallenge.TryClaimDailyBonus(
            out int xpAwarded, out int streak, out int newLevel);

        Assert.IsTrue(claimed,       "Daily bonus should fire on new day");
        Assert.Greater(xpAwarded, 0, "XP should still be awarded");
        Assert.AreEqual(0, newLevel, "newLevel must be 0 when no level boundary is crossed");
    }

    [Test]
    public void TryClaimDailyBonus_WhenAlreadyClaimedToday_ReturnsFalseAndZeroNewLevel()
    {
        string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString(LastPlayKey, today);
        PlayerPrefs.SetInt(XpKey, 0);

        bool claimed = DailyChallenge.TryClaimDailyBonus(
            out int xpAwarded, out int streak, out int newLevel);

        Assert.IsFalse(claimed,      "Should not claim bonus if already claimed today");
        Assert.AreEqual(0, xpAwarded,"xpAwarded must be 0 when not a new day");
        Assert.AreEqual(0, newLevel, "newLevel must be 0 when bonus is not claimed (BUG-1 guard)");
    }

    // ── Week-6 streak-text regression guard ─────────────────────────────────

    [Test]
    public void TryClaimDailyBonus_StreakIncrementsOnConsecutiveDay()
    {
        // Simulate a player who played yesterday with a streak of 3.
        string yesterday = System.DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        PlayerPrefs.SetString(StreakDate, yesterday);
        PlayerPrefs.SetInt(StreakKey, 3);
        PlayerPrefs.SetInt(XpKey, 0);

        DailyChallenge.TryClaimDailyBonus(out int xpAwarded, out int streak, out int _);

        Assert.AreEqual(4, streak,
            "Streak should increment from 3 to 4 on a consecutive calendar day");
        Assert.Greater(xpAwarded, DailyChallenge.DailyBonusXp,
            "Streak bonus XP must exceed the base daily bonus (Week-6 toast text source)");
    }

    [Test]
    public void TryClaimDailyBonus_StreakResetsOnNonConsecutiveDay()
    {
        // Gap of 2 days breaks the streak.
        string twoDaysAgo = System.DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd");
        PlayerPrefs.SetString(StreakDate, twoDaysAgo);
        PlayerPrefs.SetInt(StreakKey, 5);
        PlayerPrefs.SetInt(XpKey, 0);

        DailyChallenge.TryClaimDailyBonus(out int xpAwarded, out int streak, out int _);

        Assert.AreEqual(1, streak, "Streak must reset to 1 when consecutive-day chain is broken");
    }
}
