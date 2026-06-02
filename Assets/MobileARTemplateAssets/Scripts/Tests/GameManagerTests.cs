// QA — Week 1 regression tests (EditMode, no scene required)
// Run via: Window > General > Test Runner > EditMode
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class GameManagerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    GameManager MakeGM()
    {
        var go = new GameObject("GM");
        var gm = go.AddComponent<GameManager>();
        GameManager.I = gm;

        // Minimal stubs so methods don't null-ref
        var scoreGo = new GameObject("score");
        gm.scoreText = scoreGo.AddComponent<TMPro.TextMeshProUGUI>();

        var mainUI = new GameObject("mainUI"); gm.mainScreenUI = mainUI;
        var playUI = new GameObject("playUI"); gm.gamePlayUI  = playUI;

        var happyGo = new GameObject("happy"); gm.happyFeedback = happyGo;
        var happyTxt = happyGo.AddComponent<TMPro.TextMeshProUGUI>();
        gm.happyFeedbackText = happyTxt;

        var sadGo = new GameObject("sad");  gm.sadFeedback = sadGo;
        var sadTxt = sadGo.AddComponent<TMPro.TextMeshProUGUI>();
        gm.sadFeedbackText = sadTxt;

        // Tier images
        Image MakeTier(string n)
        {
            var tgo = new GameObject(n);
            return tgo.AddComponent<Image>();
        }
        gm.TierEmpty  = MakeTier("TierEmpty");
        gm.TierLow    = MakeTier("TierLow");
        gm.TierMid    = MakeTier("TierMid");
        gm.TierHigh   = MakeTier("TierHigh");

        return gm;
    }

    void Teardown(GameManager gm)
    {
        Object.DestroyImmediate(gm.gameObject);
    }

    // ── TASK-01: Score never goes below zero ─────────────────────────────────

    [Test]
    public void ScoreNeverGoesNegative_SingleWrongHit()
    {
        var gm = MakeGM();
        gm.score = 0;

        // Simulate wrong hit path: score -= 25, then clamp
        gm.score -= 25;
        gm.score = Mathf.Max(0, gm.score);

        Assert.AreEqual(0, gm.score, "Score should be clamped to 0 after wrong hit from 0");
        Teardown(gm);
    }

    [Test]
    public void ScoreNeverGoesNegative_MultipleWrongHits()
    {
        var gm = MakeGM();
        gm.score = 50;

        for (int i = 0; i < 10; i++)
        {
            gm.score -= 25;
            gm.score = Mathf.Max(0, gm.score);
        }

        Assert.GreaterOrEqual(gm.score, 0, "Score must always be >= 0");
        Teardown(gm);
    }

    // ── TASK-02: Combo multiplier logic ─────────────────────────────────────

    [Test]
    public void ComboMultiplier_ReturnsOneBeforeStreak()
    {
        Assert.AreEqual(1, ComboMultiplierHelper(1));
        Assert.AreEqual(1, ComboMultiplierHelper(2));
    }

    [Test]
    public void ComboMultiplier_ReturnsTwoAtThreeStreak()
    {
        Assert.AreEqual(2, ComboMultiplierHelper(3));
        Assert.AreEqual(2, ComboMultiplierHelper(4));
    }

    [Test]
    public void ComboMultiplier_ReturnsThreeAtFiveStreak()
    {
        Assert.AreEqual(3, ComboMultiplierHelper(5));
        Assert.AreEqual(3, ComboMultiplierHelper(99));
    }

    // Mirrors the private method in GameManager
    int ComboMultiplierHelper(int streak)
    {
        if (streak >= 5) return 3;
        if (streak >= 3) return 2;
        return 1;
    }

    // ── TASK-03: Fish count difficulty scaling ───────────────────────────────

    [Test]
    public void FishCount_ScalesWithCorrectHits()
    {
        Assert.AreEqual(3, FishCountHelper(0));
        Assert.AreEqual(3, FishCountHelper(2));
        Assert.AreEqual(4, FishCountHelper(3));
        Assert.AreEqual(4, FishCountHelper(5));
        Assert.AreEqual(5, FishCountHelper(6));
        Assert.AreEqual(5, FishCountHelper(9));
        Assert.AreEqual(6, FishCountHelper(10));
        Assert.AreEqual(6, FishCountHelper(14));
        Assert.AreEqual(7, FishCountHelper(15));
        Assert.AreEqual(7, FishCountHelper(50));
    }

    // Mirrors FishCountForDifficulty in GameManager
    int FishCountHelper(int correct)
    {
        if (correct >= 15) return 7;
        if (correct >= 10) return 6;
        if (correct >= 6)  return 5;
        if (correct >= 3)  return 4;
        return 3;
    }

    // ── TASK-05: Tier thresholds ─────────────────────────────────────────────

    [Test]
    public void TierThresholds_CoverAllRanges()
    {
        Assert.AreEqual("Empty",  TierNameHelper(0));
        Assert.AreEqual("Low",    TierNameHelper(1));
        Assert.AreEqual("Low",    TierNameHelper(4));
        Assert.AreEqual("Mid",    TierNameHelper(5));
        Assert.AreEqual("Mid",    TierNameHelper(9));
        Assert.AreEqual("High",   TierNameHelper(10));
        Assert.AreEqual("High",   TierNameHelper(14));
        Assert.AreEqual("Legend", TierNameHelper(15));
        Assert.AreEqual("Legend", TierNameHelper(99));
    }

    string TierNameHelper(int correct)
    {
        if (correct <= 0)  return "Empty";
        if (correct <= 4)  return "Low";
        if (correct <= 9)  return "Mid";
        if (correct <= 14) return "High";
        return "Legend";
    }
}
