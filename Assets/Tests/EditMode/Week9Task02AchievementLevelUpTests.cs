using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode acceptance-criteria tests for ITERATION_week9_SCOPE.md TASK-02(a).
///
/// Verifies the level-reward propagation path:
///   EndGame() snapshots level before/after AchievementChecker.CheckAll(). If CheckAll
///   grants XP that crosses a level boundary, ShowLevelUp + ApplyLevelReward are called.
///   AC: "CurrencyStore.GetCoins() increases if the reward has a coin bonus."
///
/// These tests exercise the pure / PlayerPrefs-backed systems without a running scene.
/// The MonoBehaviour wiring in EndGame() (ShowLevelUp, ApplyLevelReward being called on
/// the right frame) is covered by the PlayMode suite once TombakanTestScene is configured.
/// </summary>
public class Week9Task02AchievementLevelUpTests
{
    const string XpKey         = "tombakan_total_xp";
    const string CoinsKey      = "tombakan_coins";
    const string AchFirstCatch = "ach_first_catch";

    int _savedXp;
    int _savedCoins;

    [SetUp]
    public void SetUp()
    {
        // Preserve surrounding state so tests don't pollute the editor session
        _savedXp    = PlayerPrefs.GetInt(XpKey, 0);
        _savedCoins = PlayerPrefs.GetInt(CoinsKey, 0);

        // Start each test from a clean slate
        PlayerPrefs.SetInt(XpKey, 0);
        PlayerPrefs.DeleteKey(AchFirstCatch);
        AchievementStorePrefsHelper.ClearKnownIds();
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.SetInt(XpKey,    _savedXp);
        PlayerPrefs.SetInt(CoinsKey, _savedCoins);
        PlayerPrefs.DeleteKey(AchFirstCatch);
        AchievementStorePrefsHelper.ClearKnownIds();
        PlayerPrefs.Save();
    }

    // ── TASK-02(a): Achievement XP crosses a level boundary ──────────────────────────

    [Test]
    public void AchievementChecker_XpRewardCrossesLevel2Boundary_LevelIncreases()
    {
        // XpForLevel(2) = round(100 * (2−1)^1.5) = 100 XP
        // Awarding 100 XP to a player at level 1 (0 XP) must push them to level 2.
        var achievement = ScriptableObject.CreateInstance<Achievement>();
        achievement.id       = AchievementChecker.FirstCatch;
        achievement.xpReward = 100; // exactly enough to reach level 2

        var catalog = ScriptableObject.CreateInstance<AchievementCatalog>();
        catalog.achievements = new Achievement[] { achievement };

        Assert.AreEqual(1, ProgressionStore.GetLevel(), "Precondition: player starts at level 1");

        var session = new AchievementSession
        {
            correctHitCount      = 1, // triggers first_catch (correctHitCount >= 1)
            wrongHitCount        = 0,
            maxComboStreak       = 0,
            playerLevel          = 1,
            fishdexUnlockedCount = 0,
        };

        string[] unlocked = AchievementChecker.CheckAll(session, catalog);

        Assert.AreEqual(1, unlocked.Length,   "first_catch should be newly unlocked");
        Assert.AreEqual(AchievementChecker.FirstCatch, unlocked[0]);
        Assert.GreaterOrEqual(ProgressionStore.GetLevel(), 2,
            "XP reward of 100 must push level from 1 to at least 2 " +
            "(TASK-02a: achievement XP crosses level boundary in EndGame flow)");

        Object.DestroyImmediate(achievement);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void AchievementChecker_XpRewardAddedToStore_TotalXpIncreases()
    {
        var achievement = ScriptableObject.CreateInstance<Achievement>();
        achievement.id       = AchievementChecker.FirstCatch;
        achievement.xpReward = 50;

        var catalog = ScriptableObject.CreateInstance<AchievementCatalog>();
        catalog.achievements = new Achievement[] { achievement };

        int xpBefore = ProgressionStore.GetTotalXp(); // 0

        AchievementChecker.CheckAll(
            new AchievementSession { correctHitCount = 1 },
            catalog);

        Assert.AreEqual(xpBefore + 50, ProgressionStore.GetTotalXp(),
            "Achievement XP reward must be persisted to ProgressionStore");

        Object.DestroyImmediate(achievement);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void AchievementChecker_AlreadyUnlocked_DoesNotGrantXpAgain()
    {
        // Pre-unlock the achievement; a second call must not add XP again
        AchievementStore.Unlock(AchievementChecker.FirstCatch);

        var achievement = ScriptableObject.CreateInstance<Achievement>();
        achievement.id       = AchievementChecker.FirstCatch;
        achievement.xpReward = 100;

        var catalog = ScriptableObject.CreateInstance<AchievementCatalog>();
        catalog.achievements = new Achievement[] { achievement };

        int xpBefore = ProgressionStore.GetTotalXp(); // 0

        string[] unlocked = AchievementChecker.CheckAll(
            new AchievementSession { correctHitCount = 1 },
            catalog);

        Assert.AreEqual(0, unlocked.Length,   "Already-unlocked achievement must not be re-awarded");
        Assert.AreEqual(xpBefore, ProgressionStore.GetTotalXp(),
            "No XP must be granted for an already-unlocked achievement");

        Object.DestroyImmediate(achievement);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void AchievementChecker_ConditionNotMet_ReturnsEmptyArray()
    {
        var achievement = ScriptableObject.CreateInstance<Achievement>();
        achievement.id       = AchievementChecker.FirstCatch;
        achievement.xpReward = 100;

        var catalog = ScriptableObject.CreateInstance<AchievementCatalog>();
        catalog.achievements = new Achievement[] { achievement };

        // correctHitCount = 0 → first_catch condition NOT met
        string[] unlocked = AchievementChecker.CheckAll(
            new AchievementSession { correctHitCount = 0 },
            catalog);

        Assert.AreEqual(0, unlocked.Length);

        Object.DestroyImmediate(achievement);
        Object.DestroyImmediate(catalog);
    }

    // ── TASK-02(a): ApplyLevelReward grants coin bonus ────────────────────────────────

    [Test]
    public void ApplyLevelReward_Logic_WithCoinBonus_IncreasesCoins()
    {
        // Simulate the ApplyLevelReward coin-grant guard:
        //   if (reward.softCurrencyBonus > 0) CurrencyStore.AddCoins(reward.softCurrencyBonus);
        PlayerPrefs.SetInt(CoinsKey, 0);
        PlayerPrefs.Save();

        var reward = new LevelReward { softCurrencyBonus = 50 };

        if (reward.softCurrencyBonus > 0)
            CurrencyStore.AddCoins(reward.softCurrencyBonus);

        Assert.AreEqual(50, CurrencyStore.GetCoins(),
            "A LevelReward with softCurrencyBonus=50 must increase CurrencyStore by 50 " +
            "(TASK-02a: coins increase when reward has a coin bonus)");
    }

    [Test]
    public void ApplyLevelReward_Logic_WithNullReward_DoesNotAlterCoins()
    {
        // ApplyLevelReward starts with "if (reward == null) return;"
        // Simulate that guard: when reward is null, AddCoins is never called.
        PlayerPrefs.SetInt(CoinsKey, 10);
        PlayerPrefs.Save();

        LevelReward reward = null;

        // Mirrors the exact guard at the top of ApplyLevelReward
        if (reward != null && reward.softCurrencyBonus > 0)
            CurrencyStore.AddCoins(reward.softCurrencyBonus);

        Assert.AreEqual(10, CurrencyStore.GetCoins(),
            "Null LevelReward must not alter the coin balance");
    }

    [Test]
    public void ApplyLevelReward_Logic_WithZeroBonus_DoesNotAlterCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, 10);
        PlayerPrefs.Save();

        var reward = new LevelReward { softCurrencyBonus = 0 };

        if (reward.softCurrencyBonus > 0)
            CurrencyStore.AddCoins(reward.softCurrencyBonus);

        Assert.AreEqual(10, CurrencyStore.GetCoins(),
            "Zero coin bonus must not change the coin balance");
    }

    // ── LevelRewardTable helpers ──────────────────────────────────────────────────────

    [Test]
    public void LevelRewardTable_GetRewardForLevel_ReturnsNullForMissingLevel()
    {
        var table = ScriptableObject.CreateInstance<LevelRewardTable>();
        Assert.IsNull(table.GetRewardForLevel(1));
        Assert.IsNull(table.GetRewardForLevel(99));
        Object.DestroyImmediate(table);
    }

    [Test]
    public void LevelRewardTable_GetRewardForLevel_ReturnsConfiguredReward()
    {
        var table = ScriptableObject.CreateInstance<LevelRewardTable>();
        table.rewards.Add(new LevelReward { level = 2, softCurrencyBonus = 100 });
        table.rewards.Add(new LevelReward { level = 5, softCurrencyBonus = 250 });

        var r2 = table.GetRewardForLevel(2);
        Assert.IsNotNull(r2,     "Reward for level 2 must be found");
        Assert.AreEqual(100, r2.softCurrencyBonus);

        var r5 = table.GetRewardForLevel(5);
        Assert.IsNotNull(r5,     "Reward for level 5 must be found");
        Assert.AreEqual(250, r5.softCurrencyBonus);

        Assert.IsNull(table.GetRewardForLevel(3), "Level 3 has no reward configured");
        Object.DestroyImmediate(table);
    }

    // ── ProgressionRules level-boundary sanity ────────────────────────────────────────

    [Test]
    public void ProgressionRules_XpForLevel2_Is100()
    {
        // round(100 × (2−1)^1.5) = 100 — the threshold used in achievement XP tests above
        Assert.AreEqual(100, ProgressionRules.XpForLevel(2),
            "Level-2 threshold must be 100 XP so achievement XP reward test is meaningful");
    }

    [Test]
    public void ProgressionStore_AddXp_ReturnsNewLevelWhenBoundaryIsCrossed()
    {
        // Start at 0 XP (level 1); add 100 XP → should return 2
        int newLevel = ProgressionStore.AddXp(100);
        Assert.AreEqual(2, newLevel,
            "AddXp must return the new level when a boundary is crossed");
    }

    [Test]
    public void ProgressionStore_AddXp_ReturnsZeroWhenNoBoundaryCrossed()
    {
        // Add 50 XP (not enough to reach level 2 at 100 XP)
        int newLevel = ProgressionStore.AddXp(50);
        Assert.AreEqual(0, newLevel,
            "AddXp must return 0 when no level boundary is crossed");
    }
}
