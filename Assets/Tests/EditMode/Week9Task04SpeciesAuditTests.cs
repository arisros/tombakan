using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode acceptance-criteria tests for ITERATION_week9_SCOPE.md TASK-04.
///
/// Audits the 8 FishSpecies base colours sourced from Assets/TombakanData/Fish/*.asset.
/// Verifies three conditions from the AC:
///   1. Every species maps to a valid ShapeForColor symbol — never "?" (no symbol gap).
///   2. All 4 hue buckets (● ★ ▲ ■) are represented, giving maximum discrimination.
///   3. All base colours are saturated (s ≥ 0.6) and bright (v ≥ 0.5) for AR visibility.
///
/// Note on "no two species produce the same symbol": with 8 species and only 4 symbols
/// the mathematical minimum is 2 species per bucket. The AC is satisfied when (a) all
/// symbols are reachable (maximum spread) and (b) no bucket absorbs more than half of all
/// species (preventing a single-bucket degenerate state).
///
/// If the artist updates a base colour, update the corresponding tuple below.
/// </summary>
public class Week9Task04SpeciesAuditTests
{
    static readonly HashSet<string> ValidSymbols = new HashSet<string> { "●", "★", "▲", "■" };

    /// <summary>
    /// (speciesId, r, g, b) tuples copied from .asset YAML at the time of this test pass.
    /// Values are the baseColor field as stored (linear float, not gamma).
    /// </summary>
    static readonly (string id, float r, float g, float b)[] AllSpecies =
    {
        // Red bucket (h < 0.083): ●
        ("kakap_merah",       0.88f, 0.19f, 0.13f),
        ("kerapu_bebek",      0.80f, 0.23f, 0.12f),

        // Orange-yellow bucket (0.083 ≤ h < 0.25): ★
        ("ikan_badut",        1.00f, 0.60f, 0.00f),
        ("bandeng",           0.95f, 0.79f, 0.00f),

        // Green bucket (0.25 ≤ h < 0.458): ▲
        ("lele",              0.11f, 0.70f, 0.20f),
        ("ikan_nila",         0.20f, 0.65f, 0.20f),

        // Blue-cyan-purple bucket (h ≥ 0.458): ■
        ("kembung",           0.23f, 0.43f, 0.65f),
        ("tuna_sirip_kuning", 0.12f, 0.25f, 0.50f),
    };

    // ── Helper: build a TestCaseData source from the species table ────────────────────

    static IEnumerable<TestCaseData> SpeciesSource()
    {
        foreach (var (id, r, g, b) in AllSpecies)
            yield return new TestCaseData(id, r, g, b).SetName(id);
    }

    // ── TASK-04: Every species returns a valid shape symbol ───────────────────────────

    [TestCaseSource(nameof(SpeciesSource))]
    public void ShapeForColor_ReturnsValidSymbol(string id, float r, float g, float b)
    {
        string sym = ColourBlindSettings.ShapeForColor(new Color(r, g, b));
        Assert.IsTrue(ValidSymbols.Contains(sym),
            $"Species '{id}' produced invalid symbol '{sym}' — must be one of ● ★ ▲ ■");
    }

    [TestCaseSource(nameof(SpeciesSource))]
    public void ShapeForColor_NeverReturnsQuestionMark(string id, float r, float g, float b)
    {
        string sym = ColourBlindSettings.ShapeForColor(new Color(r, g, b));
        Assert.AreNotEqual("?", sym,
            $"Species '{id}' returned '?' — hue-range classifier must handle all species colours");
    }

    // ── TASK-04: Full hue-bucket coverage across the 8 species ───────────────────────

    [Test]
    public void AllSpecies_AllFourSymbolsRepresented()
    {
        var symbols = new HashSet<string>();
        foreach (var (id, r, g, b) in AllSpecies)
            symbols.Add(ColourBlindSettings.ShapeForColor(new Color(r, g, b)));

        foreach (string expected in ValidSymbols)
            Assert.IsTrue(symbols.Contains(expected),
                $"Symbol '{expected}' not produced by any of the 8 species — " +
                "at least one hue bucket is empty, reducing colour-blind discrimination");
    }

    [Test]
    public void AllSpecies_AtLeastThreeDistinctSymbols()
    {
        var symbols = new HashSet<string>();
        foreach (var (id, r, g, b) in AllSpecies)
            symbols.Add(ColourBlindSettings.ShapeForColor(new Color(r, g, b)));

        Assert.GreaterOrEqual(symbols.Count, 3,
            "The 8 species must span at least 3 distinct colour-blind shape buckets");
    }

    [Test]
    public void AllSpecies_NoBucketContainsMoreThanHalfOfAllSpecies()
    {
        var counts = new Dictionary<string, int>();
        foreach (var sym in ValidSymbols) counts[sym] = 0;

        foreach (var (_, r, g, b) in AllSpecies)
        {
            string sym = ColourBlindSettings.ShapeForColor(new Color(r, g, b));
            if (counts.ContainsKey(sym)) counts[sym]++;
        }

        int halfCount = AllSpecies.Length / 2; // 4 for 8 species
        foreach (var kv in counts)
            Assert.Less(kv.Value, halfCount,
                $"Symbol '{kv.Key}' is used by {kv.Value} species — " +
                "more than half the catalog in one bucket removes discrimination");
    }

    // ── TASK-04: AR visibility constraints on base colours ───────────────────────────

    [TestCaseSource(nameof(SpeciesSource))]
    public void BaseColor_SaturationAtLeast0_6(string id, float r, float g, float b)
    {
        Color.RGBToHSV(new Color(r, g, b), out _, out float s, out _);
        Assert.GreaterOrEqual(s, 0.6f,
            $"Species '{id}': saturation {s:F3} < 0.6 — fish will look washed-out in AR");
    }

    [TestCaseSource(nameof(SpeciesSource))]
    public void BaseColor_ValueAtLeast0_5(string id, float r, float g, float b)
    {
        Color.RGBToHSV(new Color(r, g, b), out _, out _, out float v);
        Assert.GreaterOrEqual(v, 0.5f,
            $"Species '{id}': value {v:F3} < 0.5 — fish will be too dark in AR lighting");
    }

    // ── Expected per-species bucket assertions ────────────────────────────────────────

    [Test]
    public void KakapMerah_MapsToRedBucket()
    {
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(new Color(0.88f, 0.19f, 0.13f)));
    }

    [Test]
    public void KerapuBebek_MapsToRedBucket()
    {
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(new Color(0.80f, 0.23f, 0.12f)));
    }

    [Test]
    public void IkanBadut_MapsToOrangeYellowBucket()
    {
        Assert.AreEqual("★", ColourBlindSettings.ShapeForColor(new Color(1.00f, 0.60f, 0.00f)));
    }

    [Test]
    public void Bandeng_MapsToOrangeYellowBucket()
    {
        Assert.AreEqual("★", ColourBlindSettings.ShapeForColor(new Color(0.95f, 0.79f, 0.00f)));
    }

    [Test]
    public void Lele_MapsToGreenBucket()
    {
        Assert.AreEqual("▲", ColourBlindSettings.ShapeForColor(new Color(0.11f, 0.70f, 0.20f)));
    }

    [Test]
    public void IkanNila_MapsToGreenBucket()
    {
        Assert.AreEqual("▲", ColourBlindSettings.ShapeForColor(new Color(0.20f, 0.65f, 0.20f)));
    }

    [Test]
    public void Kembung_MapsToBlueSquareBucket()
    {
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(new Color(0.23f, 0.43f, 0.65f)));
    }

    [Test]
    public void TunaSiripKuning_MapsToBlueSquareBucket()
    {
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(new Color(0.12f, 0.25f, 0.50f)));
    }
}
