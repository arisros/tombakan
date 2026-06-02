// EditMode tests for the Achievements system.
// All tests exercise AchievementStore and AchievementChecker pure/static logic —
// no scene, no MonoBehaviour required.
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class AchievementStoreTests
{
    // Known ids used across tests — register them so GetAllUnlocked can find them.
    const string IdA = "test_ach_a";
    const string IdB = "test_ach_b";

    [SetUp]
    public void SetUp()
    {
        // Clear PlayerPrefs keys used by these tests.
        PlayerPrefs.DeleteKey("ach_" + IdA);
        PlayerPrefs.DeleteKey("ach_" + IdB);
        // Also clear known-id registry so each test starts clean.
        AchievementStorePrefsHelper.ClearKnownIds();
        AchievementStorePrefsHelper.RegisterKnownId(IdA);
        AchievementStorePrefsHelper.RegisterKnownId(IdB);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("ach_" + IdA);
        PlayerPrefs.DeleteKey("ach_" + IdB);
        AchievementStorePrefsHelper.ClearKnownIds();
    }

    // ── IsUnlocked ───────────────────────────────────────────────────────────

    [Test]
    public void IsUnlocked_ReturnsFalseBeforeUnlock()
    {
        Assert.IsFalse(AchievementStore.IsUnlocked(IdA));
    }

    [Test]
    public void IsUnlocked_ReturnsTrueAfterUnlock()
    {
        AchievementStore.Unlock(IdA);
        Assert.IsTrue(AchievementStore.IsUnlocked(IdA));
    }

    // ── Unlock idempotency ────────────────────────────────────────────────────

    [Test]
    public void Unlock_IsIdempotent_SecondCallDoesNotThrow()
    {
        AchievementStore.Unlock(IdA);
        Assert.DoesNotThrow(() => AchievementStore.Unlock(IdA));
        Assert.IsTrue(AchievementStore.IsUnlocked(IdA));
    }

    [Test]
    public void Unlock_CalledTwice_StillReturnsTrue()
    {
        AchievementStore.Unlock(IdA);
        AchievementStore.Unlock(IdA);
        Assert.IsTrue(AchievementStore.IsUnlocked(IdA));
    }

    // ── GetAllUnlocked ────────────────────────────────────────────────────────

    [Test]
    public void GetAllUnlocked_EmptyWhenNoneUnlocked()
    {
        string[] unlocked = AchievementStore.GetAllUnlocked();
        // Neither IdA nor IdB should appear.
        Assert.AreEqual(0, unlocked.Length);
    }

    [Test]
    public void GetAllUnlocked_ReturnsOnlyUnlockedIds()
    {
        AchievementStore.Unlock(IdA);
        string[] unlocked = AchievementStore.GetAllUnlocked();
        Assert.AreEqual(1, unlocked.Length);
        Assert.AreEqual(IdA, unlocked[0]);
    }

    [Test]
    public void GetAllUnlocked_ReturnsBothWhenBothUnlocked()
    {
        AchievementStore.Unlock(IdA);
        AchievementStore.Unlock(IdB);
        string[] unlocked = AchievementStore.GetAllUnlocked();
        Assert.AreEqual(2, unlocked.Length);
    }

    // ── Null/empty safety ─────────────────────────────────────────────────────

    [Test]
    public void IsUnlocked_NullId_ReturnsFalse()
    {
        Assert.IsFalse(AchievementStore.IsUnlocked(null));
    }

    [Test]
    public void Unlock_NullId_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => AchievementStore.Unlock(null));
    }

    [Test]
    public void Unlock_EmptyId_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => AchievementStore.Unlock(""));
    }
}

[TestFixture]
public class AchievementCheckerTests
{
    // Ids we check in these tests.
    const string FirstCatch  = AchievementChecker.FirstCatch;
    const string Combo5      = AchievementChecker.Combo5;
    const string PerfectRound = AchievementChecker.PerfectRound;

    [SetUp]
    public void SetUp()
    {
        // Clear all achievement PlayerPrefs keys for the ids we test.
        PlayerPrefs.DeleteKey("ach_" + FirstCatch);
        PlayerPrefs.DeleteKey("ach_" + Combo5);
        PlayerPrefs.DeleteKey("ach_" + PerfectRound);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.Level5);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.Level10);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.SpeciesCollector5);
        // Clear known-id registry — CheckAll will re-register on next call.
        AchievementStorePrefsHelper.ClearKnownIds();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("ach_" + FirstCatch);
        PlayerPrefs.DeleteKey("ach_" + Combo5);
        PlayerPrefs.DeleteKey("ach_" + PerfectRound);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.Level5);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.Level10);
        PlayerPrefs.DeleteKey("ach_" + AchievementChecker.SpeciesCollector5);
        AchievementStorePrefsHelper.ClearKnownIds();
    }

    // ── first_catch ───────────────────────────────────────────────────────────

    [Test]
    public void CheckAll_ReturnsFirstCatch_WhenCorrectHitCountIsOne()
    {
        var session = new AchievementSession { correctHitCount = 1 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.Contains(FirstCatch, unlocked);
    }

    [Test]
    public void CheckAll_DoesNotReturnFirstCatch_WhenNoHits()
    {
        var session = new AchievementSession { correctHitCount = 0 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.IsFalse(System.Array.IndexOf(unlocked, FirstCatch) >= 0,
            "first_catch must not unlock when correctHitCount == 0");
    }

    // ── combo_5 ───────────────────────────────────────────────────────────────

    [Test]
    public void CheckAll_ReturnsCombo5_WhenMaxComboStreakIsFive()
    {
        var session = new AchievementSession { maxComboStreak = 5 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.Contains(Combo5, unlocked);
    }

    [Test]
    public void CheckAll_DoesNotReturnCombo5_WhenStreakIsFour()
    {
        var session = new AchievementSession { maxComboStreak = 4 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.IsFalse(System.Array.IndexOf(unlocked, Combo5) >= 0,
            "combo_5 must not unlock when maxComboStreak < 5");
    }

    // ── perfect_round ─────────────────────────────────────────────────────────

    [Test]
    public void CheckAll_ReturnsPerfectRound_WhenAllHitsCorrect()
    {
        var session = new AchievementSession { correctHitCount = 3, wrongHitCount = 0 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.Contains(PerfectRound, unlocked);
    }

    [Test]
    public void CheckAll_DoesNotReturnPerfectRound_WhenZeroThrows()
    {
        var session = new AchievementSession { correctHitCount = 0, wrongHitCount = 0 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.IsFalse(System.Array.IndexOf(unlocked, PerfectRound) >= 0,
            "perfect_round must not unlock with zero throws");
    }

    [Test]
    public void CheckAll_DoesNotReturnPerfectRound_WhenAnyWrongHit()
    {
        var session = new AchievementSession { correctHitCount = 3, wrongHitCount = 1 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.IsFalse(System.Array.IndexOf(unlocked, PerfectRound) >= 0,
            "perfect_round must not unlock when wrongHitCount > 0");
    }

    // ── idempotency ───────────────────────────────────────────────────────────

    [Test]
    public void CheckAll_DoesNotReUnlock_AlreadyUnlockedAchievement()
    {
        // Pre-unlock first_catch manually.
        AchievementStore.Unlock(FirstCatch);

        var session = new AchievementSession { correctHitCount = 5 };
        string[] unlocked = AchievementChecker.CheckAll(session, null);

        Assert.IsFalse(System.Array.IndexOf(unlocked, FirstCatch) >= 0,
            "first_catch must not appear in newly-unlocked list when already unlocked");
    }

    [Test]
    public void CheckAll_CalledTwice_SecondCallReturnsEmpty_ForSameConditions()
    {
        var session = new AchievementSession { correctHitCount = 1 };

        string[] first  = AchievementChecker.CheckAll(session, null);
        string[] second = AchievementChecker.CheckAll(session, null);

        Assert.Contains(FirstCatch, first);
        Assert.AreEqual(0, second.Length,
            "Second call with same session must not produce new unlocks");
    }

    // ── empty session ─────────────────────────────────────────────────────────

    [Test]
    public void CheckAll_EmptySession_ReturnsEmpty()
    {
        var session = new AchievementSession();
        string[] unlocked = AchievementChecker.CheckAll(session, null);
        Assert.AreEqual(0, unlocked.Length);
    }
}
