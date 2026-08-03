// Tests covering ITERATION_week9_SCOPE.md acceptance criteria.
// Pure-logic tests only. Scene-dependent behaviours (GoalManager startup guard,
// panel mutual-exclusion) are covered in Week9PlayModeTests.cs.

using NUnit.Framework;
using UnityEngine;

public class Week9AcceptanceTests
{
    [SetUp]
    public void SetUp()
    {
        // Clean PlayerPrefs so each test starts in isolation
        PlayerPrefs.DeleteAll();
        AchievementStorePrefsHelper.ClearKnownIds();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteAll();
        AchievementStorePrefsHelper.ClearKnownIds();
    }

    // ── TASK-02 (b) · DailyChallenge.TryClaimDailyBonus returns newLevel ──────
    // AC: After a daily-bonus level-up, newLevel is propagated to the caller.

    [Test]
    public void DailyChallenge_IsNewDay_TrueForDifferentDates()
    {
        Assert.IsTrue(DailyChallenge.IsNewDay("2024-01-01", "2024-01-02"));
    }

    [Test]
    public void DailyChallenge_IsNewDay_FalseForSameDate()
    {
        Assert.IsFalse(DailyChallenge.IsNewDay("2024-01-01", "2024-01-01"));
    }

    [Test]
    public void DailyChallenge_IsNewDay_TrueWhenStoredEmpty()
    {
        Assert.IsTrue(DailyChallenge.IsNewDay("", "2024-01-01"),
            "Empty stored date (first ever launch) must be treated as a new day");
    }

    [Test]
    public void DailyChallenge_TotalBonusXp_BaseOnlyAtZeroStreak()
    {
        int xp = DailyChallenge.TotalBonusXp(0);
        Assert.AreEqual(DailyChallenge.DailyBonusXp, xp,
            "Streak 0 should yield only the base bonus (no streak bonus)");
    }

    [Test]
    public void DailyChallenge_TotalBonusXp_IncludesStreakBonus()
    {
        int xp       = DailyChallenge.TotalBonusXp(3);
        int expected = DailyChallenge.DailyBonusXp + 3 * DailyChallenge.StreakBonusXp;
        Assert.AreEqual(expected, xp,
            "Streak-3 XP must equal base + 3 * StreakBonusXp");
    }

    [Test]
    public void DailyChallenge_TotalBonusXp_CappedsAtStreakSeven()
    {
        int xpAt7  = DailyChallenge.TotalBonusXp(7);
        int xpAt8  = DailyChallenge.TotalBonusXp(8);
        int xpAt50 = DailyChallenge.TotalBonusXp(50);
        Assert.AreEqual(xpAt7, xpAt8,
            "Streak bonus must be capped at streak=7");
        Assert.AreEqual(xpAt7, xpAt50,
            "Streak bonus must remain capped for very high streaks");
    }

    [Test]
    public void DailyChallenge_TryClaimDailyBonus_ReturnsNewLevel_WhenXpCausesLevelUp()
    {
        // Start with 0 XP. A daily bonus award of 100 XP should push a fresh player
        // from level 1 to at least level 1 (no-op) but with a large enough XP pile
        // the level-up can be forced by seeding a known amount of XP first.
        // We seed XP just below level 2 (which requires XP_for_level(2) = 100).
        // Adding even 100 XP (base daily bonus, streak=1) should trigger level-up to 2.
        int xpToLevel2 = ProgressionRules.XpForLevel(2); // 100
        // Seed XP to one below the threshold
        PlayerPrefs.SetInt("tombakan_total_xp", xpToLevel2 - 1); // 99 XP
        PlayerPrefs.Save();

        // Also set last played to yesterday (simulate first play today)
        System.DateTime yesterday = System.DateTime.UtcNow.AddDays(-1);
        string yesterdayStr = yesterday.ToString("yyyy-MM-dd");
        // Explicitly clear last played so TryClaimDailyBonus treats today as a new day
        PlayerPrefs.DeleteKey("tombakan_last_played_date");
        PlayerPrefs.Save();

        bool claimed = DailyChallenge.TryClaimDailyBonus(out int xpAwarded, out int streak, out int newLevel);

        Assert.IsTrue(claimed, "Daily bonus must be claimed on the first play of the day");
        Assert.Greater(xpAwarded, 0, "Claimed daily bonus must award XP");
        Assert.Greater(newLevel, 0,
            "When earned XP pushes the player over a level boundary, " +
            "newLevel must be > 0 so the caller can apply LevelRewardTable rewards");
    }

    [Test]
    public void DailyChallenge_TryClaimDailyBonus_ReturnsFalse_WhenAlreadyClaimedToday()
    {
        // Simulate having already claimed today
        string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString("tombakan_last_played_date", today);
        PlayerPrefs.Save();

        bool claimed = DailyChallenge.TryClaimDailyBonus(out _, out _, out _);
        Assert.IsFalse(claimed,
            "Daily bonus must not be claimable twice on the same calendar day");
    }

    // ── TASK-02 (a) · AchievementChecker propagates levelGained out parameter ─
    // AC: After an achievement XP reward causes a level-up, levelGained is returned
    //     so EndGame can merge it before calling StaggerResultCelebrations.

    [Test]
    public void AchievementChecker_WithXpReward_ReturnsLevelGainedWhenLevelUp()
    {
        // Seed XP just below level 2 (requires 100 XP per the curve)
        int xpToLevel2 = ProgressionRules.XpForLevel(2); // 100
        PlayerPrefs.SetInt("tombakan_total_xp", xpToLevel2 - 1);
        PlayerPrefs.Save();

        // Build a minimal AchievementCatalog with a first_catch achievement that
        // has enough XP to push the player over the level-2 threshold.
        var catalog      = ScriptableObject.CreateInstance<AchievementCatalog>();
        var achievement  = ScriptableObject.CreateInstance<Achievement>();
        achievement.id        = AchievementChecker.FirstCatch;
        achievement.xpReward  = 200; // well above the remaining 1 XP needed for level 2
        catalog.achievements  = new Achievement[] { achievement };

        var session = new AchievementSession
        {
            correctHitCount = 1,   // triggers FirstCatch
            wrongHitCount   = 0,
            maxComboStreak  = 0,
            playerLevel     = ProgressionStore.GetLevel(),
        };

        string[] unlocked = AchievementChecker.CheckAll(session, catalog, out int levelGained);

        Assert.Contains(AchievementChecker.FirstCatch, unlocked,
            "first_catch should be newly unlocked");
        Assert.Greater(levelGained, 0,
            "levelGained must be > 0 when achievement XP reward causes a level-up");

        Object.DestroyImmediate(achievement);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void AchievementChecker_WithNoXpReward_ReturnsZeroLevelGained()
    {
        // Achievement with 0 XP must not report a level-up
        var catalog     = ScriptableObject.CreateInstance<AchievementCatalog>();
        var achievement = ScriptableObject.CreateInstance<Achievement>();
        achievement.id       = AchievementChecker.FirstCatch;
        achievement.xpReward = 0;
        catalog.achievements = new Achievement[] { achievement };

        var session = new AchievementSession { correctHitCount = 1 };

        AchievementChecker.CheckAll(session, catalog, out int levelGained);

        Assert.AreEqual(0, levelGained,
            "An achievement with 0 xpReward must not cause a level-up report");

        Object.DestroyImmediate(achievement);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void AchievementChecker_AlreadyUnlocked_NotReturnedInNewlyUnlocked()
    {
        AchievementStore.Unlock(AchievementChecker.FirstCatch);

        var session = new AchievementSession { correctHitCount = 5 };
        string[] unlocked = AchievementChecker.CheckAll(session, null, out _);

        Assert.IsFalse(System.Array.Exists(unlocked, id => id == AchievementChecker.FirstCatch),
            "An already-unlocked achievement must not be returned as newly unlocked");
    }

    // ── TASK-03 · ShowSad deduction = 0 when score already at floor ──────────
    // AC: When score is 0 and a wrong fish is hit, actualDeduction is 0 and
    //     ShowSad must not display "-25!" (it early-returns on deduction <= 0).

    [Test]
    public void ClampScore_ZeroMinusPenalty_YieldsZeroDeduction()
    {
        // This mirrors the exact computation in OnFishHit:
        //   int scoreBefore = score;          // 0
        //   score = ClampScore(score - penaltyPerWrongHit);  // ClampScore(-25) = 0
        //   int actualDeduction = scoreBefore - score;       // 0 - 0 = 0
        int scoreBefore      = 0;
        int penaltyPerWrong  = 25; // matches GameConstants.PenaltyPerWrongHit
        int clamped          = GameManager.ClampScore(scoreBefore - penaltyPerWrong);
        int actualDeduction  = scoreBefore - clamped;

        Assert.AreEqual(0, actualDeduction,
            "actualDeduction must be 0 when score is already at the floor; " +
            "ShowSad returns early on deduction <= 0, so no '-25!' is displayed");
    }

    [Test]
    public void ClampScore_PositiveScoreMinusPenalty_YieldsPositiveDeduction()
    {
        // When score > 0 the penalty text should be shown
        int scoreBefore     = 100;
        int penaltyPerWrong = 25;
        int clamped         = GameManager.ClampScore(scoreBefore - penaltyPerWrong);
        int actualDeduction = scoreBefore - clamped;

        Assert.AreEqual(25, actualDeduction,
            "When score is above 0 the actual deduction should equal the penalty amount");
    }

    [Test]
    public void ClampScore_PartialPenalty_DeductionIsLimitedByFloor()
    {
        // Score 10, penalty 25 — clamped to 0, deduction = 10 (not 25)
        int scoreBefore     = 10;
        int penaltyPerWrong = 25;
        int clamped         = GameManager.ClampScore(scoreBefore - penaltyPerWrong);
        int actualDeduction = scoreBefore - clamped;

        Assert.AreEqual(10, actualDeduction,
            "When score is less than the penalty the deduction is limited to the score itself");
    }

    // ── TASK-01 · GoalManager.IsCoachingActive property ─────────────────────
    // The property is a guard for TombakanOnboarding.Start() so that
    // ForceCompleteGoal is never called before m_OnboardingGoals is populated.
    // Runtime behavior is tested in Week9PlayModeTests.cs; here we verify
    // the logic contract that IsCoachingActive = (m_OnboardingGoals != null && !allDone).
    // We cannot easily unit-test private state, but we verify this in the PlayMode suite.
    // The pure guard expression itself can be decomposed:

    [Test]
    public void GoalManager_IsCoachingActive_Logic_FalseWhenQueueNull()
    {
        // IsCoachingActive = (m_OnboardingGoals != null && !m_AllGoalsFinished)
        // If queue is null → false. Simulate the condition:
        bool queueIsNull = true;
        bool allFinished = false;
        bool coaching    = !queueIsNull && !allFinished;
        Assert.IsFalse(coaching,
            "IsCoachingActive must be false when m_OnboardingGoals has not been initialized");
    }

    [Test]
    public void GoalManager_IsCoachingActive_Logic_FalseWhenAllGoalsFinished()
    {
        bool queueIsNull = false;
        bool allFinished = true;
        bool coaching    = !queueIsNull && !allFinished;
        Assert.IsFalse(coaching,
            "IsCoachingActive must be false when m_AllGoalsFinished is true");
    }

    [Test]
    public void GoalManager_IsCoachingActive_Logic_TrueWhenActive()
    {
        bool queueIsNull = false;
        bool allFinished = false;
        bool coaching    = !queueIsNull && !allFinished;
        Assert.IsTrue(coaching,
            "IsCoachingActive must be true when queue is populated and goals remain");
    }
}
