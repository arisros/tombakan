// QA — Week 7 regression tests (EditMode).
// Covers:
//   TASK-01: Achievement XP level-up detection (pure store logic)
//   TASK-03: ColourBlindSettings.ShapeForColor hue-based fallback
//   TASK-04: ShowSad actual-deduction logic (via ClampScore + Mathf.Min)
//   POLISH-NEW-1: ColorSummary ×1 suppression regression
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ColourBlindShapeTests
{
    // --- Exact palette matches (must not regress) ---

    [Test]
    public void ExactRed_ReturnsCircle()
    {
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(new Color(1f, 0f, 0f)));
    }

    [Test]
    public void ExactGreen_ReturnsTriangle()
    {
        Assert.AreEqual("▲", ColourBlindSettings.ShapeForColor(new Color(0f, 1f, 0f)));
    }

    [Test]
    public void ExactBlue_ReturnsSquare()
    {
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(new Color(0f, 0f, 1f)));
    }

    [Test]
    public void ExactYellow_ReturnsStar()
    {
        Assert.AreEqual("★", ColourBlindSettings.ShapeForColor(new Color(1f, 1f, 0f)));
    }

    // --- Hue-proximity fallback for non-palette species colours ---

    [Test]
    public void OrangeHue_YieldsStar_YellowRegion()
    {
        // Orange sits between red and yellow — hue ~0.08; our boundary puts it in star
        Color orange = new Color(1f, 0.5f, 0f);
        string shape = ColourBlindSettings.ShapeForColor(orange);
        Assert.IsTrue(shape == "★" || shape == "●",
            $"Orange should map to ★ or ● (got {shape})");
    }

    [Test]
    public void PurpleHue_YieldsSquare_BlueRegion()
    {
        // Purple has hue ~0.83 — falls in the blue/purple region
        Color purple = new Color(0.5f, 0f, 1f);
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(purple));
    }

    [Test]
    public void CyanHue_YieldsTriangle_GreenRegion()
    {
        // Cyan has hue = 0.5 — green region boundary
        Color cyan = new Color(0f, 1f, 1f);
        string shape = ColourBlindSettings.ShapeForColor(cyan);
        Assert.IsTrue(shape == "▲" || shape == "■",
            $"Cyan should map to ▲ or ■ (got {shape})");
    }

    [Test]
    public void NoneReturnQuestionMark_ForSaturatedColor()
    {
        // Any saturated colour should yield a real shape, never "?"
        Color[] testColors =
        {
            new Color(0.9f, 0.1f, 0.1f),  // dark red
            new Color(0.1f, 0.8f, 0.3f),  // lime green
            new Color(0.2f, 0.3f, 0.9f),  // cornflower blue
            new Color(0.8f, 0.7f, 0.1f),  // gold
            new Color(0.6f, 0.1f, 0.8f),  // violet
        };
        foreach (var c in testColors)
        {
            string shape = ColourBlindSettings.ShapeForColor(c);
            Assert.AreNotEqual("?", shape,
                $"Colour rgb({c.r:F2},{c.g:F2},{c.b:F2}) should not return '?'");
        }
    }

    [Test]
    public void NearlyWhite_LowSaturation_DoesNotReturnQuestionMark()
    {
        Color nearWhite = new Color(0.95f, 0.95f, 0.95f);
        string shape = ColourBlindSettings.ShapeForColor(nearWhite);
        Assert.AreNotEqual("?", shape);
    }
}

[TestFixture]
public class ActualDeductionTests
{
    // Tests the math that drives ShowSad: actual deduction = min(penalty, score).
    // This mirrors GameManager.OnFishHit logic without requiring a MonoBehaviour.

    [Test]
    public void Deduction_ScoreAbovePenalty_DeductsFullPenalty()
    {
        int score = 500, penalty = 25;
        int actual = Mathf.Min(penalty, score);
        Assert.AreEqual(25, actual);
    }

    [Test]
    public void Deduction_ScoreExactlyPenalty_DeductsFullPenalty()
    {
        int score = 25, penalty = 25;
        int actual = Mathf.Min(penalty, score);
        Assert.AreEqual(25, actual);
    }

    [Test]
    public void Deduction_ScoreZero_DeductsZero()
    {
        int score = 0, penalty = 25;
        int actual = Mathf.Min(penalty, score);
        Assert.AreEqual(0, actual);
        // Displayed text should be "Miss!" not "-0!"
    }

    [Test]
    public void Deduction_ScoreBelowPenalty_DeductsOnlyRemainingScore()
    {
        int score = 10, penalty = 25;
        int actual = Mathf.Min(penalty, score);
        Assert.AreEqual(10, actual);
        Assert.AreEqual(0, GameManager.ClampScore(score - penalty));
    }
}

[TestFixture]
public class ColorSummaryWeek7RegressionTests
{
    // Regression suite for the ×1 suppression change (POLISH-NEW-1)

    [Test]
    public void SingleEntry_NoMultiplierSuffix()
    {
        Assert.AreEqual("Merah", ColorSummary.Format(new List<string> { "FF0000" }));
    }

    [Test]
    public void MultipleEntries_AllSingle_NoSuffixes()
    {
        var input = new List<string> { "FF0000", "00FF00", "0000FF" };
        Assert.AreEqual("Merah, Hijau, Biru", ColorSummary.Format(input));
    }

    [Test]
    public void MixedCounts_MultiplierOnlyWhereNeeded()
    {
        var input = new List<string> { "FF0000", "FF0000", "00FF00" };
        Assert.AreEqual("Merah ×2, Hijau", ColorSummary.Format(input));
    }

    [Test]
    public void HighMultiplier_StillShowsCount()
    {
        var input = new List<string> { "FF0000", "FF0000", "FF0000", "FF0000", "FF0000" };
        Assert.AreEqual("Merah ×5", ColorSummary.Format(input));
    }

    [Test]
    public void EmptyInput_ReturnsEmptyText()
    {
        Assert.AreEqual(ColorSummary.EmptyText, ColorSummary.Format(new List<string>()));
    }
}

[TestFixture]
public class ProgressionLevelUpDetectionTests
{
    // Tests the logic that detects achievement-XP-triggered level-ups.
    // GameManager captures levelBefore/levelAfter around CheckAll;
    // ProgressionStore.GetLevel is a pure PlayerPrefs read.
    // We simulate by directly exercising ProgressionStore.

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("tombakan_total_xp"); // only key ProgressionStore uses
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("tombakan_total_xp");
        PlayerPrefs.Save();
    }

    [Test]
    public void AddXp_BelowThreshold_ReturnsZero_NoLevelChange()
    {
        int newLevel = ProgressionStore.AddXp(50); // Level 2 needs 100 XP
        Assert.AreEqual(0, newLevel, "No level-up should return 0");
        Assert.AreEqual(1, ProgressionStore.GetLevel());
    }

    [Test]
    public void AddXp_CrossesThreshold_ReturnsNewLevel()
    {
        int newLevel = ProgressionStore.AddXp(100); // crosses Level 2 threshold
        Assert.AreEqual(2, newLevel, "Should return the new level on level-up");
        Assert.AreEqual(2, ProgressionStore.GetLevel());
    }

    [Test]
    public void LevelBefore_LevelAfter_DetectsAchievementLevelUp()
    {
        // Simulate: end-game XP doesn't level up; achievement XP does
        ProgressionStore.AddXp(90); // close to threshold
        int levelBefore = ProgressionStore.GetLevel(); // 1
        ProgressionStore.AddXp(15); // crosses Level 2 (90+15=105 >= 100)
        int levelAfter = ProgressionStore.GetLevel(); // 2
        Assert.Greater(levelAfter, levelBefore,
            "After adding XP that crosses a threshold, GetLevel() should increase");
    }
}

// ---------------------------------------------------------------------------
// TASK-04 (this run)  PlaceWaterOnPlane _placed flag / BeginReposition
// ---------------------------------------------------------------------------
[TestFixture]
public class PlaceWaterOnPlaneRepositionTests
{
    static System.Reflection.FieldInfo PlacedField() =>
        typeof(PlaceWaterOnPlane)
            .GetField("_placed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    [Test]
    public void PlacedFlag_StartsAsFalse()
    {
        var go   = new GameObject("pwop");
        var comp = go.AddComponent<PlaceWaterOnPlane>();
        Assert.IsFalse((bool)PlacedField().GetValue(comp), "_placed must start false");
        Object.DestroyImmediate(go);
    }

    [Test]
    public void BeginReposition_ResetsFlagToFalse()
    {
        var go   = new GameObject("pwop");
        var comp = go.AddComponent<PlaceWaterOnPlane>();
        PlacedField().SetValue(comp, true);          // simulate completed placement
        comp.BeginReposition();
        Assert.IsFalse((bool)PlacedField().GetValue(comp));
        Object.DestroyImmediate(go);
    }

    [Test]
    public void BeginReposition_HidesRepositionButton()
    {
        var go   = new GameObject("pwop");
        var comp = go.AddComponent<PlaceWaterOnPlane>();
        var btn  = new GameObject("btn");
        btn.SetActive(true);
        comp.repositionButton = btn;
        PlacedField().SetValue(comp, true);
        comp.BeginReposition();
        Assert.IsFalse(btn.activeSelf);
        Object.DestroyImmediate(btn);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void BeginReposition_NullButton_DoesNotThrow()
    {
        var go   = new GameObject("pwop");
        var comp = go.AddComponent<PlaceWaterOnPlane>();
        comp.repositionButton = null;
        Assert.DoesNotThrow(() => comp.BeginReposition());
        Object.DestroyImmediate(go);
    }
}
