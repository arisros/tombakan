// Tests covering ITERATION_week2_SCOPE.md acceptance criteria.
// All tests here are pure-logic and do not require a running scene.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Week2AcceptanceTests
{
    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);
        PlayerPrefs.Save();
    }

    // ── TASK-01 · Centralise + expand the fish-colour palette ─────────────────
    // AC: FishPalette.Options = {Red, Green, Blue, Yellow}; RandomOther(exclude) works;
    //     all palette colours have a Dict.cs Indonesian mapping.

    [Test]
    public void FishPalette_Options_HasExactlyFourColors()
    {
        Assert.AreEqual(4, FishPalette.Options.Length);
    }

    [Test]
    public void FishPalette_Options_ContainsRed()
    {
        bool found = System.Array.Exists(FishPalette.Options, c => c == Color.red);
        Assert.IsTrue(found, "FishPalette.Options must contain Color.red (Merah)");
    }

    [Test]
    public void FishPalette_Options_ContainsGreen()
    {
        bool found = System.Array.Exists(FishPalette.Options, c => c == Color.green);
        Assert.IsTrue(found, "FishPalette.Options must contain Color.green (Hijau)");
    }

    [Test]
    public void FishPalette_Options_ContainsBlue()
    {
        bool found = System.Array.Exists(FishPalette.Options, c => c == Color.blue);
        Assert.IsTrue(found, "FishPalette.Options must contain Color.blue (Biru)");
    }

    [Test]
    public void FishPalette_Options_ContainsPureYellow_FFFF00()
    {
        // Must be Color(1,1,0) mapped to FFFF00, NOT Color.yellow which is FFEB04.
        Color pureYellow = new Color(1f, 1f, 0f);
        bool found = System.Array.Exists(FishPalette.Options, c => c == pureYellow);
        Assert.IsTrue(found,
            "FishPalette must include pure yellow Color(1,1,0) (FFFF00); " +
            "Color.yellow (FFEB04) is not the same value");
    }

    [Test]
    public void FishPalette_AllOptions_HaveIndonesianMapping()
    {
        foreach (Color color in FishPalette.Options)
        {
            string hex  = ColorUtility.ToHtmlStringRGB(color);
            string name = ColorHexLocalization.ToIndonesian(hex);
            Assert.AreNotEqual(hex, name,
                $"Palette color {hex} has no Indonesian mapping in Dict.cs — " +
                $"ColorHexLocalization falls back to the raw hex when the key is missing");
        }
    }

    [Test]
    public void FishPalette_RandomOther_NeverReturnsExcludedColor_Red()
    {
        for (int i = 0; i < 200; i++)
        {
            Color result = FishPalette.RandomOther(Color.red);
            Assert.AreNotEqual(Color.red, result,
                $"RandomOther(Color.red) returned Color.red on iteration {i}");
        }
    }

    [Test]
    public void FishPalette_RandomOther_NeverReturnsExcludedColor_Green()
    {
        for (int i = 0; i < 200; i++)
        {
            Color result = FishPalette.RandomOther(Color.green);
            Assert.AreNotEqual(Color.green, result,
                $"RandomOther(Color.green) returned Color.green on iteration {i}");
        }
    }

    [Test]
    public void FishPalette_RandomOther_ReturnsColorFromPalette()
    {
        for (int i = 0; i < 50; i++)
        {
            Color result = FishPalette.RandomOther(Color.red);
            bool inPalette = System.Array.Exists(FishPalette.Options, c => c == result);
            Assert.IsTrue(inPalette,
                $"RandomOther returned a color not in FishPalette.Options: {result}");
        }
    }

    // ── TASK-03 · High-score persistence + "new record" moment ────────────────
    // AC: TrySetBest returns true only when score strictly beats current best.

    [Test]
    public void ScoreStore_IsNewRecord_TrueWhenStrictlyHigher()
    {
        Assert.IsTrue(ScoreStore.IsNewRecord(101, 100));
    }

    [Test]
    public void ScoreStore_IsNewRecord_FalseWhenEqual()
    {
        Assert.IsFalse(ScoreStore.IsNewRecord(100, 100));
    }

    [Test]
    public void ScoreStore_IsNewRecord_FalseWhenLower()
    {
        Assert.IsFalse(ScoreStore.IsNewRecord(99, 100));
    }

    [Test]
    public void ScoreStore_IsNewRecord_TrueAgainstZeroBest()
    {
        // Any positive score beats a zero best (first game ever)
        Assert.IsTrue(ScoreStore.IsNewRecord(1, 0));
    }

    [Test]
    public void ScoreStore_TrySetBest_ReturnsTrueWhenNewRecord()
    {
        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);
        bool result = ScoreStore.TrySetBest(100);
        Assert.IsTrue(result, "TrySetBest must return true when score beats the stored best");
    }

    [Test]
    public void ScoreStore_TrySetBest_ReturnsFalseWhenNotNewRecord()
    {
        PlayerPrefs.SetInt(ScoreStore.BestScoreKey, 200);
        bool result = ScoreStore.TrySetBest(100);
        Assert.IsFalse(result, "TrySetBest must return false when score does not beat current best");
    }

    [Test]
    public void ScoreStore_TrySetBest_ReturnsFalseOnEqual()
    {
        PlayerPrefs.SetInt(ScoreStore.BestScoreKey, 100);
        bool result = ScoreStore.TrySetBest(100);
        Assert.IsFalse(result, "TrySetBest must return false when score exactly equals the best (not strictly greater)");
    }

    [Test]
    public void ScoreStore_TrySetBest_PersistsNewBest()
    {
        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);
        ScoreStore.TrySetBest(300);
        Assert.AreEqual(300, ScoreStore.GetBest());
    }

    [Test]
    public void ScoreStore_GetBest_ReturnsZeroWhenNeverSet()
    {
        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);
        Assert.AreEqual(0, ScoreStore.GetBest());
    }

    // ── TASK-04 · Aggregated result colour summary ────────────────────────────
    // AC: ColorSummary.Format aggregates correctly; empty = "Tidak ada ikan dikumpulkan".

    [Test]
    public void ColorSummary_EmptyList_ReturnsEmptyText()
    {
        string result = ColorSummary.Format(new List<string>());
        Assert.AreEqual(ColorSummary.EmptyText, result);
    }

    [Test]
    public void ColorSummary_NullInput_ReturnsEmptyText()
    {
        string result = ColorSummary.Format(null);
        Assert.AreEqual(ColorSummary.EmptyText, result);
    }

    [Test]
    public void ColorSummary_EmptyText_IsIndonesian()
    {
        // "Tidak ada ikan dikumpulkan" per the spec
        Assert.AreEqual("Tidak ada ikan dikumpulkan", ColorSummary.EmptyText);
    }

    [Test]
    public void ColorSummary_SingleRed_ContainsMerahAndCount()
    {
        string result = ColorSummary.Format(new List<string> { "FF0000" });
        StringAssert.Contains("Merah", result);
        StringAssert.Contains("×1", result);
    }

    [Test]
    public void ColorSummary_ThreeRed_ShowsCountThree()
    {
        string result = ColorSummary.Format(new List<string> { "FF0000", "FF0000", "FF0000" });
        StringAssert.Contains("Merah ×3", result);
    }

    [Test]
    public void ColorSummary_MixedColours_AggregatesBothColors()
    {
        var colors = new List<string> { "FF0000", "0000FF", "FF0000" };
        string result = ColorSummary.Format(colors);
        StringAssert.Contains("Merah ×2", result);
        StringAssert.Contains("Biru ×1", result);
    }

    [Test]
    public void ColorSummary_PreservesFirstSeenOrder()
    {
        // Red first, then Green — Red should appear before Green in the output
        var colors = new List<string> { "FF0000", "00FF00" };
        string result = ColorSummary.Format(colors);
        int redIdx   = result.IndexOf("Merah");
        int greenIdx = result.IndexOf("Hijau");
        Assert.Less(redIdx, greenIdx,
            "ColorSummary must preserve first-seen order: Merah appeared first");
    }

    [Test]
    public void ColorSummary_NonConsecutiveDuplicates_AreGrouped()
    {
        // Red, Blue, Red — should produce "Merah ×2, Biru ×1" (or first-seen order)
        var colors = new List<string> { "FF0000", "0000FF", "FF0000" };
        string result = ColorSummary.Format(colors);
        // Should not produce two separate Merah entries
        int count = 0;
        int idx = 0;
        while ((idx = result.IndexOf("Merah", idx)) != -1) { count++; idx++; }
        Assert.AreEqual(1, count, "Non-consecutive duplicates should be grouped into one entry");
    }
}
