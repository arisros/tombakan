using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Acceptance tests for TASK-02: Fix daily level-up reward never applied.
///
/// Covers:
///   - DailyChallenge.TryClaimDailyBonus() exposes newLevel via out-param when a level-up occurs.
///   - TombakanOnboarding.Start() calls GameManager.I.ApplyLevelReward(newLevel) when newLevel > 0.
///   - After a daily level-up, stores receive the configured reward.
///   - The EndGame level-up path (ProgressionStore.AddXp) is not regressed.
/// </summary>
public class Week7_Task02_DailyLevelUpTests
{
    const string XpKey         = "tombakan_total_xp";
    const string LastPlayedKey = "tombakan_last_played_date";
    const string StreakKey     = "tombakan_daily_streak";
    const string StreakDateKey = "tombakan_streak_date";

    [SetUp]
    public void SetUp() => ClearDailyPrefs();

    [TearDown]
    public void TearDown() => ClearDailyPrefs();

    // -------------------------------------------------------------------------
    // Pure logic — DailyChallenge.IsNewDay
    // -------------------------------------------------------------------------

    [Test]
    public void IsNewDay_DifferentDates_ReturnsTrue()
    {
        Assert.IsTrue(DailyChallenge.IsNewDay("2026-01-01", "2026-01-02"));
    }

    [Test]
    public void IsNewDay_SameDates_ReturnsFalse()
    {
        Assert.IsFalse(DailyChallenge.IsNewDay("2026-01-01", "2026-01-01"));
    }

    [Test]
    public void IsNewDay_EmptyStored_ReturnsTrue()
    {
        Assert.IsTrue(DailyChallenge.IsNewDay("", "2026-09-07"),
            "An empty stored date means the player has never played — treat as new day.");
    }

    // -------------------------------------------------------------------------
    // Pure logic — DailyChallenge.TotalBonusXp
    // -------------------------------------------------------------------------

    [TestCase(0, 100)]
    [TestCase(1, 125)]
    [TestCase(7, 275)]
    public void TotalBonusXp_ReturnsBaseXpPlusStreakBonus(int streak, int expected)
    {
        Assert.AreEqual(expected, DailyChallenge.TotalBonusXp(streak),
            $"TotalBonusXp({streak}) must be {expected}.");
    }

    [Test]
    public void TotalBonusXp_StreakCappedAt7()
    {
        Assert.AreEqual(
            DailyChallenge.TotalBonusXp(7),
            DailyChallenge.TotalBonusXp(10),
            "Streak bonus is capped at 7 consecutive days.");
    }

    // -------------------------------------------------------------------------
    // ProgressionStore.AddXp return value (EndGame path non-regression)
    // -------------------------------------------------------------------------

    [Test]
    public void AddXp_WhenLevelUpOccurs_ReturnsNewLevel()
    {
        PlayerPrefs.DeleteKey(XpKey);
        PlayerPrefs.Save();

        // Level 2 threshold = XpForLevel(2) = 100 * (2-1)^1.5 = 100.
        // Adding 100 XP from 0 should trigger level 2.
        int newLevel = ProgressionStore.AddXp(100);

        Assert.AreEqual(2, newLevel,
            "AddXp must return the new level (2) when XP crosses the level-2 threshold.");
    }

    [Test]
    public void AddXp_WhenNoLevelUpOccurs_ReturnsZero()
    {
        PlayerPrefs.DeleteKey(XpKey);
        PlayerPrefs.Save();

        // 50 XP from 0 stays at level 1.
        int newLevel = ProgressionStore.AddXp(50);

        Assert.AreEqual(0, newLevel,
            "AddXp must return 0 when no level-up occurs.");
    }

    // AC: EndGame path not regressed — AddXp returns correct level across boundaries.
    [Test]
    public void AddXp_CrossingLevel3Boundary_Returns3()
    {
        // XpForLevel(3) = 100 * (3-1)^1.5 ≈ 283.
        int threshold = ProgressionRules.XpForLevel(3);
        PlayerPrefs.SetInt(XpKey, threshold - 10);
        PlayerPrefs.Save();

        int newLevel = ProgressionStore.AddXp(10);

        Assert.AreEqual(3, newLevel,
            "AddXp must return 3 when XP crosses the level-3 threshold (EndGame path regression check).");
    }

    // -------------------------------------------------------------------------
    // AC: TryClaimDailyBonus exposes newLevel via out-param
    // -------------------------------------------------------------------------

    [Test]
    public void TryClaimDailyBonus_WhenLevelUpOccurs_ReturnsPositiveNewLevel()
    {
        // New day: LastPlayedKey not set.
        // XP = 0; bonus at streak 1 = TotalBonusXp(1) = 125 XP → crosses level 2 (threshold 100).
        PlayerPrefs.SetInt(XpKey, 0);
        PlayerPrefs.Save();

        bool claimed = DailyChallenge.TryClaimDailyBonus(
            out int xpAwarded, out int streak, out int newLevel);

        Assert.IsTrue(claimed,   "Bonus must be claimed on a new day.");
        Assert.Greater(xpAwarded, 0, "XP awarded must be positive.");
        Assert.AreEqual(2, newLevel,
            "newLevel out-param must be 2 when the daily XP pushes the player from level 1 to level 2.");
    }

    [Test]
    public void TryClaimDailyBonus_WhenNoLevelUp_ReturnsZeroNewLevel()
    {
        // Start well into a high level so the bonus won't cause a level-up.
        int xpAtLevel10 = ProgressionRules.XpForLevel(10); // 2700
        PlayerPrefs.SetInt(XpKey, xpAtLevel10);
        PlayerPrefs.Save();

        DailyChallenge.TryClaimDailyBonus(out _, out _, out int newLevel);

        Assert.AreEqual(0, newLevel,
            "newLevel must be 0 when the daily bonus does not trigger a level-up.");
    }

    [Test]
    public void TryClaimDailyBonus_WhenAlreadyClaimedToday_ReturnsFalse()
    {
        string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString(LastPlayedKey, today);
        PlayerPrefs.Save();

        bool claimed = DailyChallenge.TryClaimDailyBonus(out _, out _, out _);

        Assert.IsFalse(claimed,
            "TryClaimDailyBonus must return false when the bonus was already claimed today.");
    }

    // -------------------------------------------------------------------------
    // AC: TombakanOnboarding.Start() calls ApplyLevelReward when newLevel > 0
    // -------------------------------------------------------------------------

    [Test]
    public void OnboardingStart_WhenDailyBonusCausesLevelUp_ActivatesLevelUpPanel()
    {
        // Arrange: daily bonus will occur (new day) and level-up will trigger (XP = 0).
        PlayerPrefs.SetInt(XpKey, 0);
        PlayerPrefs.Save();

        var gmGo = new GameObject("TestGameManager");
        var gm   = gmGo.AddComponent<GameManager>();
        GameManager.I = gm; // bypass Awake which doesn't run in EditMode

        var levelUpPanelGo = new GameObject("LevelUpPanel");
        levelUpPanelGo.SetActive(false);
        gm.levelUpPanel = levelUpPanelGo;

        var obGo = new GameObject("Onboarding");
        var ob   = obGo.AddComponent<TombakanOnboarding>();
        ob.skipOnboardingForReturning = false; // prevent goalManager branch from running

        try
        {
            // Act: invoke Start() directly since EditMode tests don't run lifecycle methods.
            var startMethod = typeof(TombakanOnboarding).GetMethod(
                "Start",
                BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod.Invoke(ob, null);

            // Assert: levelUpPanel activated proves ApplyLevelReward(2) was called.
            Assert.IsTrue(levelUpPanelGo.activeSelf,
                "levelUpPanel must be activated because TombakanOnboarding.Start() should call " +
                "GameManager.I.ApplyLevelReward(newLevel) when the daily bonus triggers a level-up.");
        }
        finally
        {
            GameManager.I = null;
            Object.DestroyImmediate(gmGo);
            Object.DestroyImmediate(levelUpPanelGo);
            Object.DestroyImmediate(obGo);
        }
    }

    [Test]
    public void OnboardingStart_WhenNoDailyLevelUp_DoesNotActivateLevelUpPanel()
    {
        // Arrange: force today as already claimed so no bonus fires.
        string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString(LastPlayedKey, today);
        PlayerPrefs.Save();

        var gmGo = new GameObject("TestGameManager");
        var gm   = gmGo.AddComponent<GameManager>();
        GameManager.I = gm;

        var levelUpPanelGo = new GameObject("LevelUpPanel");
        levelUpPanelGo.SetActive(false);
        gm.levelUpPanel = levelUpPanelGo;

        var obGo = new GameObject("Onboarding");
        var ob   = obGo.AddComponent<TombakanOnboarding>();
        ob.skipOnboardingForReturning = false;

        try
        {
            var startMethod = typeof(TombakanOnboarding).GetMethod(
                "Start",
                BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod.Invoke(ob, null);

            Assert.IsFalse(levelUpPanelGo.activeSelf,
                "levelUpPanel must NOT be activated when no daily bonus is awarded.");
        }
        finally
        {
            GameManager.I = null;
            Object.DestroyImmediate(gmGo);
            Object.DestroyImmediate(levelUpPanelGo);
            Object.DestroyImmediate(obGo);
        }
    }

    // -------------------------------------------------------------------------
    // AC: Level-up reward applied to CurrencyStore / SpearStore / FishdexStore.
    //     Verifies that ApplyLevelReward routes the reward fields correctly.
    // -------------------------------------------------------------------------

    [Test]
    public void ApplyLevelReward_WithCoinBonus_AddsCoinsToStore()
    {
        const string CoinsKey = "tombakan_coins";
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.Save();

        var gmGo = new GameObject("TestGameManager_Reward");
        var gm   = gmGo.AddComponent<GameManager>();
        GameManager.I = gm;

        try
        {
            var reward = new LevelReward
            {
                level              = 5,
                softCurrencyBonus  = 50,
                unlockedSpeciesId  = "",
                unlockedSpearSkinId = "",
            };

            // Invoke the private overload that takes a LevelReward directly.
            var method = typeof(GameManager).GetMethod(
                "ApplyLevelReward",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(LevelReward) },
                null);
            method.Invoke(gm, new object[] { reward });

            Assert.AreEqual(50, CurrencyStore.GetCoins(),
                "CurrencyStore must receive the softCurrencyBonus from the LevelReward.");
        }
        finally
        {
            GameManager.I = null;
            Object.DestroyImmediate(gmGo);
            PlayerPrefs.DeleteKey(CoinsKey);
            PlayerPrefs.Save();
        }
    }

    // -------------------------------------------------------------------------
    // helpers
    // -------------------------------------------------------------------------

    static void ClearDailyPrefs()
    {
        PlayerPrefs.DeleteKey(XpKey);
        PlayerPrefs.DeleteKey(LastPlayedKey);
        PlayerPrefs.DeleteKey(StreakKey);
        PlayerPrefs.DeleteKey(StreakDateKey);
        PlayerPrefs.Save();
    }
}
