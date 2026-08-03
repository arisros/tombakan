// Tests covering ITERATION_week3_SCOPE.md acceptance criteria.
// All tests here are pure-logic and do not require a running scene.

using NUnit.Framework;

public class Week3AcceptanceTests
{
    // ── TASK-02 · Adaptive inter-round delay ──────────────────────────────────
    // AC: PacingRules.HitDelayForProgress shrinks by 0.1 s per correct hit,
    //     clamped to a 1.0 s floor and never above baseDelay.

    [Test]
    public void PacingRules_ZeroHits_ReturnsBaseDelay()
    {
        float baseDelay = 2.2f;
        float result    = PacingRules.HitDelayForProgress(baseDelay, 0);
        Assert.AreEqual(baseDelay, result, 0.0001f,
            "Zero correct hits must yield exactly the base delay");
    }

    [Test]
    public void PacingRules_OneHit_ReducesByOneStep()
    {
        float baseDelay = 2.2f;
        float result    = PacingRules.HitDelayForProgress(baseDelay, 1);
        Assert.AreEqual(baseDelay - PacingRules.DelayReductionPerHit, result, 0.0001f);
    }

    [Test]
    public void PacingRules_ManyHits_ClampsAtFloor()
    {
        float result = PacingRules.HitDelayForProgress(2.2f, 100);
        Assert.AreEqual(PacingRules.MinHitDelay, result, 0.0001f,
            "Delay must never fall below the MinHitDelay floor (1.0 s)");
    }

    [Test]
    public void PacingRules_FloorIs1Seconds()
    {
        Assert.AreEqual(1.0f, PacingRules.MinHitDelay, 0.0001f);
    }

    [Test]
    public void PacingRules_DelayReductionIs0Point1PerHit()
    {
        Assert.AreEqual(0.1f, PacingRules.DelayReductionPerHit, 0.0001f);
    }

    [Test]
    public void PacingRules_NeverExceedsBaseDelay()
    {
        float baseDelay = 2.2f;
        for (int hits = 0; hits <= 20; hits++)
        {
            float result = PacingRules.HitDelayForProgress(baseDelay, hits);
            Assert.LessOrEqual(result, baseDelay,
                $"Delay at {hits} correct hits exceeded base delay {baseDelay}");
        }
    }

    [Test]
    public void PacingRules_NegativeHits_ReturnsBaseDelay()
    {
        float baseDelay = 2.2f;
        float result    = PacingRules.HitDelayForProgress(baseDelay, -1);
        Assert.AreEqual(baseDelay, result, 0.0001f,
            "Negative correctHitCount should be treated as 0 (no reduction)");
    }

    [TestCase(0,  2.2f)]
    [TestCase(1,  2.1f)]
    [TestCase(5,  1.7f)]
    [TestCase(12, 1.0f)] // hits floor before reaching 0
    [TestCase(50, 1.0f)]
    public void PacingRules_KnownValues_Correct(int hits, float expectedDelay)
    {
        float result = PacingRules.HitDelayForProgress(2.2f, hits);
        Assert.AreEqual(expectedDelay, result, 0.001f,
            $"HitDelayForProgress(2.2f, {hits}) should be ~{expectedDelay}");
    }

    // ── TASK-03 · Accuracy stat on the result screen ──────────────────────────
    // AC: Accuracy.Percent(correct, wrong) → rounded int %; 0 when no attempts.
    //     Accuracy.Format(correct, wrong) → "9/12 (75%)"; "--" when no throws.

    [Test]
    public void Accuracy_Percent_ZeroAttempts_ReturnsZero()
    {
        Assert.AreEqual(0, Accuracy.Percent(0, 0),
            "No attempts must yield 0% accuracy");
    }

    [Test]
    public void Accuracy_Percent_AllCorrect_Returns100()
    {
        Assert.AreEqual(100, Accuracy.Percent(5, 0));
    }

    [Test]
    public void Accuracy_Percent_AllWrong_ReturnsZero()
    {
        Assert.AreEqual(0, Accuracy.Percent(0, 5));
    }

    [Test]
    public void Accuracy_Percent_NineOf12_Returns75()
    {
        Assert.AreEqual(75, Accuracy.Percent(9, 3));
    }

    [Test]
    public void Accuracy_Percent_OneOfTwo_Returns50()
    {
        Assert.AreEqual(50, Accuracy.Percent(1, 1));
    }

    [Test]
    public void Accuracy_Percent_Rounding_TwoOfThree_Returns67()
    {
        // 2/3 = 66.66... rounds to 67
        Assert.AreEqual(67, Accuracy.Percent(2, 1));
    }

    [Test]
    public void Accuracy_Format_ZeroAttempts_ReturnsDash()
    {
        // Week 5 TASK-03 also mandates this: (0, 0) → "--"
        string result = Accuracy.Format(0, 0);
        Assert.AreEqual("--", result,
            "Accuracy.Format(0, 0) must return \"--\" not a numeric value");
    }

    [Test]
    public void Accuracy_Format_NineOf12_MatchesExpectedPattern()
    {
        string result = Accuracy.Format(9, 3);
        Assert.AreEqual("9/12 (75%)", result,
            "Format must match the spec example '9/12 (75%)'");
    }

    [Test]
    public void Accuracy_Format_AllCorrect_ShowsMaxPercent()
    {
        string result = Accuracy.Format(5, 0);
        StringAssert.Contains("100%", result);
    }

    [Test]
    public void Accuracy_Format_ShowsTotalThrowsNotJustCorrect()
    {
        // 3 correct + 2 wrong = 5 total
        string result = Accuracy.Format(3, 2);
        StringAssert.Contains("3/5", result);
    }
}
