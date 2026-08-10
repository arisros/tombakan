using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode tests for BUG-2: AchievementChecker.CheckAll must capture the
/// ProgressionStore.AddXp return value and call GameManager.I.ApplyLevelReward(newLevel)
/// when an achievement XP reward pushes the player across a level boundary.
/// </summary>
public class AchievementCheckerProgressionTests
{
    const string XpKey    = "tombakan_total_xp";
    const string CoinsKey = "tombakan_coins";

    GameObject      m_GameManagerGo;
    GameManager     m_GameManager;
    AchievementCatalog m_Catalog;
    Achievement     m_FirstCatchAch;
    LevelRewardTable m_Table;

    [SetUp]
    public void SetUp()
    {
        // Clean PlayerPrefs state.
        PlayerPrefs.DeleteKey(XpKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.FirstCatch);
        AchievementStorePrefsHelper.ClearKnownIds();

        // Build a minimal GameManager with a fake LevelRewardTable.
        // In EditMode, AddComponent does not invoke Awake automatically, so we
        // set GameManager.I manually.
        m_GameManagerGo = new GameObject("TestGameManager");
        m_GameManager   = m_GameManagerGo.AddComponent<GameManager>();

        m_Table = ScriptableObject.CreateInstance<LevelRewardTable>();
        m_Table.rewards = new List<LevelReward>
        {
            // Level 2 reward: 500 coins, no species/skin unlock (keeps test simple).
            new LevelReward { level = 2, softCurrencyBonus = 500 },
        };
        m_GameManager.levelRewardTable = m_Table;
        GameManager.I = m_GameManager;

        // Build a minimal AchievementCatalog with a single FirstCatch achievement.
        m_FirstCatchAch = ScriptableObject.CreateInstance<Achievement>();
        m_FirstCatchAch.id        = AchievementChecker.FirstCatch;
        m_FirstCatchAch.xpReward  = 150; // enough to push level 1 → 2 (needs 100 XP)

        m_Catalog = ScriptableObject.CreateInstance<AchievementCatalog>();
        m_Catalog.achievements = new Achievement[] { m_FirstCatchAch };
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(XpKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.FirstCatch);
        AchievementStorePrefsHelper.ClearKnownIds();

        GameManager.I = null;
        Object.DestroyImmediate(m_GameManagerGo);
        Object.DestroyImmediate(m_Catalog);
        Object.DestroyImmediate(m_FirstCatchAch);
        Object.DestroyImmediate(m_Table);
    }

    // ── BUG-2 core ───────────────────────────────────────────────────────────

    [Test]
    public void CheckAll_WhenAchievementXpCrossesLevelBoundary_AppliesLevelRewardCoins()
    {
        // Start just below the level-2 threshold so the 150 XP reward crosses it.
        int xpForLevel2 = ProgressionRules.XpForLevel(2); // 100
        PlayerPrefs.SetInt(XpKey, xpForLevel2 - 10);       // 90 XP → level 1

        var session = new AchievementSession
        {
            correctHitCount      = 1, // satisfies FirstCatch condition
            wrongHitCount        = 0,
            maxComboStreak       = 0,
            playerLevel          = ProgressionStore.GetLevel(),
            fishdexUnlockedCount = 0,
        };

        string[] unlocked = AchievementChecker.CheckAll(session, m_Catalog);

        Assert.AreEqual(1, unlocked.Length,
            "FirstCatch should be newly unlocked in this session");
        Assert.AreEqual(AchievementChecker.FirstCatch, unlocked[0]);

        // The 500-coin reward from the level-2 LevelReward is the observable proof that
        // GameManager.ApplyLevelReward(2) was called by AchievementChecker (BUG-2 fix).
        Assert.AreEqual(500, CurrencyStore.GetCoins(),
            "ApplyLevelReward must be called with the correct new level, applying the table reward");
    }

    [Test]
    public void CheckAll_WhenAchievementXpDoesNotCrossLevelBoundary_DoesNotApplyLevelReward()
    {
        // 2800 XP (level 10); level 11 needs 3162. Adding 150 XP → 2950 < 3162 → no level-up.
        PlayerPrefs.SetInt(XpKey, 2800);

        var session = new AchievementSession
        {
            correctHitCount      = 1,
            wrongHitCount        = 0,
            maxComboStreak       = 0,
            playerLevel          = ProgressionStore.GetLevel(),
            fishdexUnlockedCount = 0,
        };

        AchievementChecker.CheckAll(session, m_Catalog);

        Assert.AreEqual(0, CurrencyStore.GetCoins(),
            "No level-up reward should be applied when XP stays within the current level");
    }

    [Test]
    public void CheckAll_WhenGameManagerIsNull_DoesNotThrow()
    {
        // Safety: if GameManager.I is null (e.g. during tests or non-game scenes),
        // AchievementChecker must not throw a NullReferenceException.
        GameManager.I = null;

        PlayerPrefs.SetInt(XpKey, ProgressionRules.XpForLevel(2) - 10);

        var session = new AchievementSession { correctHitCount = 1 };

        Assert.DoesNotThrow(
            () => AchievementChecker.CheckAll(session, m_Catalog),
            "CheckAll must be null-safe when GameManager.I is absent");
    }

    [Test]
    public void CheckAll_AlreadyUnlockedAchievement_DoesNotGrantXpAgain()
    {
        // Pre-unlock the achievement so it is not newly discovered this session.
        AchievementStore.Unlock(AchievementChecker.FirstCatch);
        PlayerPrefs.SetInt(XpKey, 0);

        var session = new AchievementSession { correctHitCount = 1 };
        AchievementChecker.CheckAll(session, m_Catalog);

        Assert.AreEqual(0, CurrencyStore.GetCoins(),
            "No XP or rewards should be granted for an already-unlocked achievement");
        Assert.AreEqual(0, ProgressionStore.GetTotalXp(),
            "Total XP must not change when achievement was already unlocked");
    }
}
