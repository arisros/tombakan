// QA — Week 2 regression tests (EditMode).
// Cover the new pure helpers added in Week 2: FishPalette, ScoreStore,
// ColorSummary. Run via Test Runner > EditMode or game-ci in CI.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class FishPaletteTests
{
    // TASK-01: every palette colour must localise (guards the Color.yellow != FFFF00 trap)
    [Test]
    public void EveryPaletteColor_HasIndonesianMapping()
    {
        foreach (var c in FishPalette.Options)
        {
            string hex = ColorUtility.ToHtmlStringRGB(c);
            string name = ColorHexLocalization.ToIndonesian(hex);
            Assert.AreNotEqual(hex, name,
                $"Palette colour #{hex} has no Dict.cs mapping (fell back to raw hex).");
        }
    }

    [Test]
    public void Palette_IncludesYellowAsPureFFFF00()
    {
        string hex = ColorUtility.ToHtmlStringRGB(new Color(1f, 1f, 0f));
        Assert.AreEqual("FFFF00", hex);
        Assert.AreEqual("Kuning", ColorHexLocalization.ToIndonesian("FFFF00"));
    }

    [Test]
    public void Palette_HasFourDistinctColours()
    {
        Assert.AreEqual(4, FishPalette.Options.Length);
        CollectionAssert.AllItemsAreUnique(FishPalette.Options);
    }

    [Test]
    public void RandomOther_NeverReturnsExcludedColour()
    {
        foreach (var exclude in FishPalette.Options)
        {
            for (int i = 0; i < 50; i++)
                Assert.AreNotEqual(exclude, FishPalette.RandomOther(exclude));
        }
    }
}

[TestFixture]
public class ScoreStoreTests
{
    // TASK-03: pure record rule (no PlayerPrefs needed)
    [Test]
    public void IsNewRecord_OnlyWhenStrictlyGreater()
    {
        Assert.IsTrue(ScoreStore.IsNewRecord(101, 100));
        Assert.IsFalse(ScoreStore.IsNewRecord(100, 100));
        Assert.IsFalse(ScoreStore.IsNewRecord(99, 100));
        Assert.IsTrue(ScoreStore.IsNewRecord(1, 0));
    }

    [Test]
    public void TrySetBest_PersistsAndReportsRecord()
    {
        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);

        Assert.IsTrue(ScoreStore.TrySetBest(500), "First score should be a record");
        Assert.AreEqual(500, ScoreStore.GetBest());

        Assert.IsFalse(ScoreStore.TrySetBest(400), "Lower score is not a record");
        Assert.AreEqual(500, ScoreStore.GetBest());

        Assert.IsTrue(ScoreStore.TrySetBest(900), "Higher score is a record");
        Assert.AreEqual(900, ScoreStore.GetBest());

        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);
    }
}

[TestFixture]
public class ColorSummaryTests
{
    // TASK-04: aggregation, order, empty handling
    [Test]
    public void Format_EmptyOrNull_ReturnsPlaceholder()
    {
        Assert.AreEqual(ColorSummary.EmptyText, ColorSummary.Format(null));
        Assert.AreEqual(ColorSummary.EmptyText, ColorSummary.Format(new List<string>()));
    }

    [Test]
    public void Format_AggregatesCountsInFirstSeenOrder()
    {
        var input = new List<string> { "FF0000", "FF0000", "0000FF", "FF0000" };
        // Merah seen first (×3), then Biru (single — no ×1 suffix per Week 7 POLISH-NEW-1)
        Assert.AreEqual("Merah ×3, Biru", ColorSummary.Format(input));
    }

    [Test]
    public void Format_SingleColour()
    {
        var input = new List<string> { "00FF00" };
        // Single occurrence: no ×1 suffix (Week 7 POLISH-NEW-1)
        Assert.AreEqual("Hijau", ColorSummary.Format(input));
    }

    [Test]
    public void Format_AllFourColours()
    {
        var input = new List<string> { "FF0000", "00FF00", "0000FF", "FFFF00" };
        // All single occurrences: no ×1 suffix (Week 7 POLISH-NEW-1)
        Assert.AreEqual("Merah, Hijau, Biru, Kuning", ColorSummary.Format(input));
    }

    [Test]
    public void Format_MultipleOccurrences_ShowsMultiplier()
    {
        var input = new List<string> { "FF0000", "FF0000", "00FF00", "00FF00", "0000FF" };
        Assert.AreEqual("Merah ×2, Hijau ×2, Biru", ColorSummary.Format(input));
    }
}
