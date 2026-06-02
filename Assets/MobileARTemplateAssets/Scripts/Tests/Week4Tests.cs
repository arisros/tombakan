// QA — Week 4 regression tests (EditMode).
// Cover the new pure logic added in Week 4: AudioPrefs, TimeBonus, and the
// progressive-palette helpers on FishPalette.
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class AudioPrefsTests
{
    // TASK-01: persisted mute round-trips
    [Test]
    public void Mute_DefaultsToFalse()
    {
        PlayerPrefs.DeleteKey(AudioPrefs.MuteKey);
        Assert.IsFalse(AudioPrefs.IsMuted());
    }

    [Test]
    public void Mute_PersistsBothStates()
    {
        AudioPrefs.SetMuted(true);
        Assert.IsTrue(AudioPrefs.IsMuted());

        AudioPrefs.SetMuted(false);
        Assert.IsFalse(AudioPrefs.IsMuted());

        PlayerPrefs.DeleteKey(AudioPrefs.MuteKey);
    }
}

[TestFixture]
public class TimeBonusTests
{
    // TASK-03: bonus scales with the combo multiplier (base 0.5 s)
    [Test]
    public void ForHit_BaseBeforeStreak()
    {
        Assert.AreEqual(0.5f, TimeBonus.ForHit(1), 1e-4f); // x1
        Assert.AreEqual(0.5f, TimeBonus.ForHit(2), 1e-4f); // x1
    }

    [Test]
    public void ForHit_DoublesAtThreeStreak()
    {
        Assert.AreEqual(1.0f, TimeBonus.ForHit(3), 1e-4f); // x2
        Assert.AreEqual(1.0f, TimeBonus.ForHit(4), 1e-4f);
    }

    [Test]
    public void ForHit_TriplesAtFiveStreak()
    {
        Assert.AreEqual(1.5f, TimeBonus.ForHit(5), 1e-4f);  // x3
        Assert.AreEqual(1.5f, TimeBonus.ForHit(99), 1e-4f);
    }
}

[TestFixture]
public class FishPaletteProgressionTests
{
    // TASK-02: colour count ramps with progress, clamped to the palette size
    [Test]
    public void CountForProgress_StartsAtThree()
    {
        Assert.AreEqual(3, FishPalette.CountForProgress(0));
        Assert.AreEqual(3, FishPalette.CountForProgress(4));
    }

    [Test]
    public void CountForProgress_GrowsThenClampsToPaletteSize()
    {
        Assert.AreEqual(4, FishPalette.CountForProgress(5));
        Assert.AreEqual(4, FishPalette.CountForProgress(10));   // would be 5, clamped to 4
        Assert.AreEqual(FishPalette.Options.Length, FishPalette.CountForProgress(1000));
    }

    [Test]
    public void CountForProgress_HandlesNegative()
    {
        Assert.AreEqual(3, FishPalette.CountForProgress(-5));
    }

    [Test]
    public void ActiveOptions_ReturnsClampedPrefix()
    {
        Assert.AreEqual(3, FishPalette.ActiveOptions(3).Length);
        Assert.AreEqual(FishPalette.Options.Length, FishPalette.ActiveOptions(99).Length);
        Assert.AreEqual(1, FishPalette.ActiveOptions(0).Length); // clamped to >=1

        // prefix order preserved
        var three = FishPalette.ActiveOptions(3);
        Assert.AreEqual(FishPalette.Options[0], three[0]);
        Assert.AreEqual(FishPalette.Options[2], three[2]);
    }

    [Test]
    public void RandomOther_WithActiveCount_StaysInSubsetAndExcludes()
    {
        // Exclude red within a 3-colour subset -> only green or blue, never red, never yellow
        Color red = FishPalette.Options[0];
        Color yellow = FishPalette.Options[3];
        for (int i = 0; i < 100; i++)
        {
            Color c = FishPalette.RandomOther(red, 3);
            Assert.AreNotEqual(red, c);
            Assert.AreNotEqual(yellow, c, "Yellow is outside the 3-colour active subset");
        }
    }
}
