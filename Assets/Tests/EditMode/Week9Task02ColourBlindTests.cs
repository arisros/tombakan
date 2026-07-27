using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode acceptance-criteria tests for ITERATION_week9_SCOPE.md TASK-02(c).
///
/// Verifies that ColourBlindSettings.ShapeForColor uses HSV hue-range classification
/// and never returns "?" for any colour — satisfying the AC:
///   "enabling colour-blind mode with any of the 8 catalog species active shows one of
///    ●▲■★ (never '?') on every fish overlay"
///
/// Bucket thresholds (from ColourBlindSettings.ShapeForColor docs):
///   h &lt; 0.083 or h &gt; 0.917  →  ● (red)
///   h &lt; 0.250               →  ★ (orange / yellow)
///   h &lt; 0.458               →  ▲ (green)
///   otherwise               →  ■ (blue / cyan / purple)
///   s &lt; 0.15 (greyscale)   →  ■ (neutral override)
/// </summary>
public class Week9Task02ColourBlindTests
{
    static readonly HashSet<string> ValidSymbols = new HashSet<string> { "●", "★", "▲", "■" };

    // ── Hue-bucket classification ─────────────────────────────────────────────────────

    [Test]
    public void ShapeForColor_PureRed_ReturnsCircle()
    {
        // Color.red: h ≈ 0 < 0.083
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(Color.red));
    }

    [Test]
    public void ShapeForColor_PureGreen_ReturnsTriangle()
    {
        // Color.green: h = 0.333, within green bucket (0.25 ≤ h < 0.458)
        Assert.AreEqual("▲", ColourBlindSettings.ShapeForColor(Color.green));
    }

    [Test]
    public void ShapeForColor_PureBlue_ReturnsSquare()
    {
        // Color.blue: h ≈ 0.667, within blue-cyan-purple bucket
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(Color.blue));
    }

    [Test]
    public void ShapeForColor_PureYellow_ReturnsStar()
    {
        // Color.yellow: h ≈ 0.167, within orange-yellow bucket (0.083 ≤ h < 0.25)
        Assert.AreEqual("★", ColourBlindSettings.ShapeForColor(Color.yellow));
    }

    [Test]
    public void ShapeForColor_Cyan_ReturnsSquare()
    {
        // Color.cyan: h = 0.5, within blue-cyan-purple bucket
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(Color.cyan));
    }

    [Test]
    public void ShapeForColor_Magenta_ReturnsSquare()
    {
        // Color.magenta: h ≈ 0.833, between 0.458 and 0.917 → blue-cyan-purple bucket
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(Color.magenta));
    }

    // ── Hue boundary values ───────────────────────────────────────────────────────────

    [Test]
    public void ShapeForColor_HueJustInsideRedBucketLow_ReturnsCircle()
    {
        // h = 0.04 (far inside red bucket)
        Color c = Color.HSVToRGB(0.04f, 1f, 1f);
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(c));
    }

    [Test]
    public void ShapeForColor_HueJustInsideRedBucketHigh_ReturnsCircle()
    {
        // h = 0.95 (> 0.917) wraps back into red bucket
        Color c = Color.HSVToRGB(0.95f, 1f, 1f);
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(c));
    }

    [Test]
    public void ShapeForColor_HueJustInsideOrangeBucket_ReturnsStar()
    {
        // h = 0.085 (just past the 0.083 red boundary)
        Color c = Color.HSVToRGB(0.085f, 1f, 1f);
        Assert.AreEqual("★", ColourBlindSettings.ShapeForColor(c));
    }

    [Test]
    public void ShapeForColor_HueJustInsideGreenBucket_ReturnsTriangle()
    {
        // h = 0.26 (just past the 0.25 orange boundary)
        Color c = Color.HSVToRGB(0.26f, 1f, 1f);
        Assert.AreEqual("▲", ColourBlindSettings.ShapeForColor(c));
    }

    [Test]
    public void ShapeForColor_HueJustInsideBlueBucket_ReturnsSquare()
    {
        // h = 0.46 (just past the 0.458 green boundary)
        Color c = Color.HSVToRGB(0.46f, 1f, 1f);
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(c));
    }

    // ── Greyscale (low-saturation) override ─────────────────────────────────────────

    [Test]
    public void ShapeForColor_PureWhite_ReturnsSquare()
    {
        // s = 0 < 0.15 → neutral override
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(Color.white));
    }

    [Test]
    public void ShapeForColor_PureBlack_ReturnsSquare()
    {
        // s = 0 < 0.15 → neutral override
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(Color.black));
    }

    [Test]
    public void ShapeForColor_NearGreyscale_ReturnsSquare()
    {
        // s = 0.10 < 0.15 → neutral override regardless of hue
        Color c = Color.HSVToRGB(0f, 0.10f, 0.8f); // very desaturated red
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(c),
            "Near-greyscale colours must use the neutral '■' override regardless of hue");
    }

    [Test]
    public void ShapeForColor_SaturationAtThreshold_NotOverridden()
    {
        // s = 0.15 is NOT < 0.15 → hue bucket applies
        Color c = Color.HSVToRGB(0f, 0.15f, 0.8f); // red hue, at saturation threshold
        // h ≈ 0 < 0.083 → "●"
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(c),
            "s=0.15 is at the boundary and must use hue classification, not greyscale override");
    }

    // ── Never "?" ────────────────────────────────────────────────────────────────────

    [Test]
    public void ShapeForColor_HueSweep24Steps_NeverReturnsQuestionMark()
    {
        for (int i = 0; i < 24; i++)
        {
            float h = i / 24f;
            Color c = Color.HSVToRGB(h, 1f, 1f);
            Assert.AreNotEqual("?", ColourBlindSettings.ShapeForColor(c),
                $"ShapeForColor returned '?' for hue {h:F3} — hue-range fix must cover all hues");
        }
    }

    [Test]
    public void ShapeForColor_HueSweep24Steps_AlwaysReturnsValidSymbol()
    {
        for (int i = 0; i < 24; i++)
        {
            float h = i / 24f;
            Color c = Color.HSVToRGB(h, 1f, 1f);
            string sym = ColourBlindSettings.ShapeForColor(c);
            Assert.IsTrue(ValidSymbols.Contains(sym),
                $"ShapeForColor returned '{sym}' for hue {h:F3} — only ●★▲■ are valid");
        }
    }

    // ── All four symbols reachable ────────────────────────────────────────────────────

    [Test]
    public void ShapeForColor_AllFourSymbolsAreReachable()
    {
        var seen = new HashSet<string>
        {
            ColourBlindSettings.ShapeForColor(Color.red),    // ●
            ColourBlindSettings.ShapeForColor(Color.yellow), // ★
            ColourBlindSettings.ShapeForColor(Color.green),  // ▲
            ColourBlindSettings.ShapeForColor(Color.blue),   // ■
        };
        Assert.AreEqual(4, seen.Count,
            "All four shape symbols (● ★ ▲ ■) must be reachable from at least one colour");
    }
}
