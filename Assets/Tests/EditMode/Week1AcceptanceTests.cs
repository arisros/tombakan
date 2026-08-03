// Tests covering ITERATION_week1_SCOPE.md acceptance criteria.
// All tests here are pure-logic and do not require a running scene.

using NUnit.Framework;

public class Week1AcceptanceTests
{
    // ── TASK-01 · Score floor ──────────────────────────────────────────────────
    // AC: `score` never drops below 0

    [Test]
    public void ClampScore_NegativeInput_ReturnsZero()
    {
        Assert.AreEqual(0, GameManager.ClampScore(-1));
    }

    [Test]
    public void ClampScore_LargeNegative_ReturnsZero()
    {
        Assert.AreEqual(0, GameManager.ClampScore(-1000));
    }

    [Test]
    public void ClampScore_Zero_ReturnsZero()
    {
        Assert.AreEqual(0, GameManager.ClampScore(0));
    }

    [Test]
    public void ClampScore_Positive_PassesThrough()
    {
        Assert.AreEqual(75, GameManager.ClampScore(75));
    }

    [Test]
    public void ClampScore_AtFloor_DeductionIsZero()
    {
        // When score is 0 and a 25-point penalty is applied the actual deduction must be 0.
        // This mirrors the on-wrong-hit computation: actualDeduction = scoreBefore - ClampScore(scoreBefore - penalty)
        int scoreBefore  = 0;
        int scoreAfter   = GameManager.ClampScore(scoreBefore - 25);
        int actualDeduction = scoreBefore - scoreAfter;

        Assert.AreEqual(0, actualDeduction,
            "Applying a penalty to a score of 0 must yield a deduction of 0, not -25");
    }

    // ── TASK-02 · Combo streak multiplier ─────────────────────────────────────
    // AC: 3-in-a-row = 2× points; 5-in-a-row = 3× points

    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(2, 1)]
    [TestCase(3, 2)]
    [TestCase(4, 2)]
    [TestCase(5, 3)]
    [TestCase(6, 3)]
    [TestCase(10, 3)]
    public void ComboMultiplier_ReturnsCorrectFactor(int streak, int expected)
    {
        Assert.AreEqual(expected, GameManager.ComboMultiplier(streak));
    }

    [Test]
    public void ComboMultiplier_ThreeInARow_IsTwoX()
    {
        Assert.AreEqual(2, GameManager.ComboMultiplier(3),
            "Exactly 3 in a row must yield a 2x multiplier");
    }

    [Test]
    public void ComboMultiplier_FiveInARow_IsThreeX()
    {
        Assert.AreEqual(3, GameManager.ComboMultiplier(5),
            "Exactly 5 in a row must yield a 3x multiplier");
    }

    [Test]
    public void ComboMultiplier_TwoInARow_IsOneX()
    {
        Assert.AreEqual(1, GameManager.ComboMultiplier(2),
            "A streak of 2 must not yet award a multiplier bonus");
    }

    // ── TASK-03 · Dynamic fish count ──────────────────────────────────────────
    // AC: 0–2=3 fish, 3–5=4 fish, 6–9=5 fish, 10–14=6 fish, 15+=7 fish

    [TestCase(0,  3)]
    [TestCase(1,  3)]
    [TestCase(2,  3)]
    [TestCase(3,  4)]
    [TestCase(5,  4)]
    [TestCase(6,  5)]
    [TestCase(9,  5)]
    [TestCase(10, 6)]
    [TestCase(14, 6)]
    [TestCase(15, 7)]
    [TestCase(20, 7)]
    [TestCase(100,7)]
    public void FishCountForDifficulty_ScalesWithCorrectHits(int correctHits, int expected)
    {
        Assert.AreEqual(expected, GameManager.FishCountForDifficulty(correctHits));
    }

    [Test]
    public void FishCountForDifficulty_AtThreeSixTenFifteenBoundaries()
    {
        Assert.AreEqual(4, GameManager.FishCountForDifficulty(3));
        Assert.AreEqual(5, GameManager.FishCountForDifficulty(6));
        Assert.AreEqual(6, GameManager.FishCountForDifficulty(10));
        Assert.AreEqual(7, GameManager.FishCountForDifficulty(15));
    }

    // ── TASK-05 · Revised tier thresholds + TierLegend ────────────────────────
    // AC: Empty=0 hits, Low=1-4, Mid=5-9, High=10-14, Legend≥15

    [TestCase(0,  0)]  // Empty
    [TestCase(-1, 0)]  // Empty (guard against negative)
    [TestCase(1,  1)]  // Low
    [TestCase(4,  1)]  // Low upper boundary
    [TestCase(5,  2)]  // Mid lower boundary
    [TestCase(9,  2)]  // Mid upper boundary
    [TestCase(10, 3)]  // High lower boundary
    [TestCase(14, 3)]  // High upper boundary
    [TestCase(15, 4)]  // Legend lower boundary
    [TestCase(100,4)]  // Legend (very high)
    public void TierIndex_ReturnsCorrectTier(int correctHits, int expected)
    {
        Assert.AreEqual(expected, GameManager.TierIndex(correctHits),
            $"TierIndex({correctHits}) should be {expected}");
    }

    [Test]
    public void TierIndex_FourHits_IsLowNotMid()
    {
        // Four correct hits sits in Low (1), not Mid (2)
        Assert.AreEqual(1, GameManager.TierIndex(4));
    }

    [Test]
    public void TierIndex_FifteenHits_IsLegend()
    {
        // 15+ correct hits must reach Legend (4)
        Assert.AreEqual(4, GameManager.TierIndex(15));
    }
}
