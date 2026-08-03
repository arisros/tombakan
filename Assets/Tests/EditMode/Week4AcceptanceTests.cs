// Tests covering ITERATION_week4_SCOPE.md acceptance criteria.
// All tests here are pure-logic and do not require a running scene.

using NUnit.Framework;
using UnityEngine;

public class Week4AcceptanceTests
{
    // ── TASK-01 · Audio robustness + persisted mute API ───────────────────────
    // AC: AudioPrefs wraps PlayerPrefs; IsMuted/SetMuted persist across reads.

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(AudioPrefs.MuteKey);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(AudioPrefs.MuteKey);
        PlayerPrefs.Save();
    }

    [Test]
    public void AudioPrefs_Default_IsNotMuted()
    {
        // Fresh state — no stored preference — must default to unmuted
        Assert.IsFalse(AudioPrefs.IsMuted(),
            "AudioPrefs.IsMuted() must return false when no preference has been saved");
    }

    [Test]
    public void AudioPrefs_SetMutedTrue_IsMuted()
    {
        AudioPrefs.SetMuted(true);
        Assert.IsTrue(AudioPrefs.IsMuted());
    }

    [Test]
    public void AudioPrefs_SetMutedFalse_IsNotMuted()
    {
        AudioPrefs.SetMuted(true);
        AudioPrefs.SetMuted(false);
        Assert.IsFalse(AudioPrefs.IsMuted());
    }

    [Test]
    public void AudioPrefs_MutedStatePersistedInPlayerPrefs()
    {
        AudioPrefs.SetMuted(true);
        // Read the prefs key directly to verify the value is actually stored
        int stored = PlayerPrefs.GetInt(AudioPrefs.MuteKey, 0);
        Assert.AreEqual(1, stored,
            "Muted state must be stored as 1 in PlayerPrefs under the expected key");
    }

    [Test]
    public void AudioPrefs_UnmutedStatePersistedInPlayerPrefs()
    {
        AudioPrefs.SetMuted(false);
        int stored = PlayerPrefs.GetInt(AudioPrefs.MuteKey, 1); // default 1 to detect absence
        Assert.AreEqual(0, stored,
            "Unmuted state must be stored as 0 in PlayerPrefs");
    }

    // ── TASK-02 · Progressive colour difficulty ───────────────────────────────
    // AC: CountForProgress starts at 3 and adds 1 per 5 correct hits, clamped to palette size.
    //     ActiveOptions and RandomOther(exclude, activeCount) operate on the active subset.

    [TestCase(0,  3)]
    [TestCase(4,  3)]
    [TestCase(5,  4)]   // +1 at every 5 hits
    [TestCase(9,  4)]
    [TestCase(10, 4)]   // palette has only 4 colours; clamped at 4
    [TestCase(50, 4)]
    public void FishPalette_CountForProgress_ReturnsCorrectCount(int hits, int expected)
    {
        Assert.AreEqual(expected, FishPalette.CountForProgress(hits),
            $"CountForProgress({hits}) should be {expected}");
    }

    [Test]
    public void FishPalette_CountForProgress_MinIs3()
    {
        Assert.AreEqual(FishPalette.MinActiveColors, FishPalette.CountForProgress(0));
    }

    [Test]
    public void FishPalette_CountForProgress_MaxIsPaletteSize()
    {
        int max = FishPalette.CountForProgress(1000);
        Assert.AreEqual(FishPalette.Options.Length, max,
            "CountForProgress must be clamped to the total palette size");
    }

    [Test]
    public void FishPalette_CountForProgress_NegativeHits_ReturnsMin()
    {
        Assert.AreEqual(FishPalette.MinActiveColors, FishPalette.CountForProgress(-5));
    }

    [Test]
    public void FishPalette_ActiveOptions_ReturnsRequestedCount()
    {
        int count = 3;
        Color[] active = FishPalette.ActiveOptions(count);
        Assert.AreEqual(count, active.Length);
    }

    [Test]
    public void FishPalette_ActiveOptions_ReturnsFirstNColorsFromPalette()
    {
        int count = 3;
        Color[] active = FishPalette.ActiveOptions(count);
        for (int i = 0; i < count; i++)
            Assert.AreEqual(FishPalette.Options[i], active[i],
                $"ActiveOptions({count})[{i}] should match Options[{i}]");
    }

    [Test]
    public void FishPalette_ActiveOptions_ClampsExcessiveCount()
    {
        Color[] active = FishPalette.ActiveOptions(100);
        Assert.AreEqual(FishPalette.Options.Length, active.Length,
            "Requesting more colours than the palette has must be clamped");
    }

    [Test]
    public void FishPalette_RandomOtherWithCount_NeverReturnsExclude()
    {
        // With 3 active colours, excluding Red should never produce Red
        for (int i = 0; i < 200; i++)
        {
            Color result = FishPalette.RandomOther(Color.red, 3);
            Assert.AreNotEqual(Color.red, result,
                $"RandomOther(Color.red, 3) returned Color.red on iteration {i}");
        }
    }

    [Test]
    public void FishPalette_RandomOtherWithCount_StaysWithinActiveSubset()
    {
        int activeCount = 3;
        Color[] active  = FishPalette.ActiveOptions(activeCount);
        for (int i = 0; i < 100; i++)
        {
            Color result = FishPalette.RandomOther(Color.red, activeCount);
            bool inSubset = System.Array.Exists(active, c => c == result);
            Assert.IsTrue(inSubset,
                $"RandomOther must return a colour from the {activeCount}-colour active subset");
        }
    }

    // ── TASK-03 · Combo-scaled time bonus ─────────────────────────────────────
    // AC: TimeBonus.ForHit(streak) = BaseSeconds × combo multiplier.
    //     Base bonus = 0.5 s × multiplier.

    [Test]
    public void TimeBonus_BaseSeconds_IsHalfSecond()
    {
        Assert.AreEqual(0.5f, TimeBonus.BaseSeconds, 0.0001f);
    }

    [Test]
    public void TimeBonus_ForHit_NoCombo_ReturnsBase()
    {
        // streak=1: multiplier=1 → bonus=0.5
        float bonus = TimeBonus.ForHit(1);
        Assert.AreEqual(TimeBonus.BaseSeconds * 1, bonus, 0.0001f);
    }

    [Test]
    public void TimeBonus_ForHit_ThreeCombo_DoubleBonus()
    {
        // streak=3: multiplier=2 → bonus=1.0
        float bonus = TimeBonus.ForHit(3);
        Assert.AreEqual(TimeBonus.BaseSeconds * 2, bonus, 0.0001f);
    }

    [Test]
    public void TimeBonus_ForHit_FiveCombo_TripleBonus()
    {
        // streak=5: multiplier=3 → bonus=1.5
        float bonus = TimeBonus.ForHit(5);
        Assert.AreEqual(TimeBonus.BaseSeconds * 3, bonus, 0.0001f);
    }

    [TestCase(0, 0.5f)]
    [TestCase(1, 0.5f)]
    [TestCase(2, 0.5f)]
    [TestCase(3, 1.0f)]
    [TestCase(4, 1.0f)]
    [TestCase(5, 1.5f)]
    [TestCase(6, 1.5f)]
    public void TimeBonus_ForHit_MatchesBaseTimesMultiplier(int streak, float expected)
    {
        Assert.AreEqual(expected, TimeBonus.ForHit(streak), 0.0001f);
    }
}
