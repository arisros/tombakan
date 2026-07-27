using NUnit.Framework;

/// <summary>
/// EditMode acceptance-criteria tests for ITERATION_week9_SCOPE.md TASK-01.
///
/// Covers the PURE-LOGIC portions of each sub-criterion:
///   (b) Wrong fish hit at score=0 shows "Meleset!" — tested via ClampScore + text-decision formula.
///   (c) CancelInvoke prevents 0-ms flash — timing requires a running scene; confirmed manually.
///   (d) targetColorLabel.text matches targetColorImage.color after SpawnFish — integration test
///       in catalog mode requires a wired scene; the underlying ColorHexLocalization path is
///       already covered by ColorLocalizationTests.cs.
///   (a) GoalManager null-queue guard — PlayMode coverage in Week9Task01PlayModeTests.cs.
///
/// All tests here are [Test] (no MonoBehaviour, no scene loading required).
/// </summary>
public class Week9Task01ScoreLogicTests
{
    // ── TASK-01(b): ClampScore absorbs penalty when score is 0 ──────────────────────────

    [Test]
    public void ClampScore_AtZero_ReturnsZero()
    {
        Assert.AreEqual(0, GameManager.ClampScore(0));
    }

    [Test]
    public void ClampScore_WhenPenaltyExceedsScore_ClampsToZeroNotNegative()
    {
        // score 0 − penalty 25 must clamp to 0, not −25
        Assert.AreEqual(0, GameManager.ClampScore(0 - 25),
            "ClampScore must absorb the full 25-point penalty when score is already 0");
    }

    [Test]
    public void ClampScore_WhenScoreExceedsPenalty_DeductsNormally()
    {
        Assert.AreEqual(25, GameManager.ClampScore(50 - 25));
    }

    [Test]
    public void ClampScore_WhenScoreEqualsPenalty_ReturnsZero()
    {
        Assert.AreEqual(0, GameManager.ClampScore(25 - 25));
    }

    [Test]
    public void ClampScore_PartialAbsorption_ReturnsRemainingScore()
    {
        // score 10 − penalty 25: only 10 points can be deducted, result = 0
        Assert.AreEqual(0, GameManager.ClampScore(10 - 25));
    }

    /// <summary>
    /// Reproduces the exact OnFishHit → ShowSad path:
    ///   int scoreBefore = score;
    ///   score = ClampScore(score − penaltyPerWrongHit);   // ClampScore(0 − 25) = 0
    ///   int actualDeduction = scoreBefore − score;        // 0 − 0 = 0
    ///   ShowSad(actualDeduction);                         // "Meleset!"
    /// </summary>
    [Test]
    public void ShowSadTextDecision_WhenDeductionIsZero_ReturnsMeleset()
    {
        const int penaltyPerWrongHit = 25;
        int scoreBefore = 0;
        int scoreAfter  = GameManager.ClampScore(scoreBefore - penaltyPerWrongHit);
        int actualDeduction = scoreBefore - scoreAfter;

        string text = actualDeduction == 0 ? "Meleset!" : $"-{actualDeduction}!";

        Assert.AreEqual("Meleset!", text,
            "When score is 0 the penalty is fully absorbed, so ShowSad must display 'Meleset!'");
    }

    [Test]
    public void ShowSadTextDecision_WhenDeductionIs25_ReturnsDash25()
    {
        const int penaltyPerWrongHit = 25;
        int scoreBefore = 100;
        int scoreAfter  = GameManager.ClampScore(scoreBefore - penaltyPerWrongHit);
        int actualDeduction = scoreBefore - scoreAfter;

        string text = actualDeduction == 0 ? "Meleset!" : $"-{actualDeduction}!";

        Assert.AreEqual("-25!", text);
    }

    [Test]
    public void ShowSadTextDecision_WhenPartiallyAbsorbed_ShowsActualAmount()
    {
        // score = 10, penalty = 25 → deduction = 10
        const int penaltyPerWrongHit = 25;
        int scoreBefore = 10;
        int scoreAfter  = GameManager.ClampScore(scoreBefore - penaltyPerWrongHit);
        int actualDeduction = scoreBefore - scoreAfter;

        string text = actualDeduction == 0 ? "Meleset!" : $"-{actualDeduction}!";

        Assert.AreEqual("-10!", text,
            "Partially absorbed penalty should show the actual amount, not the full 25");
    }

    // ── Combo multiplier (unchanged contract) ─────────────────────────────────────────

    [TestCase(1, 1)]
    [TestCase(2, 1)]
    [TestCase(3, 2)]
    [TestCase(4, 2)]
    [TestCase(5, 3)]
    [TestCase(10, 3)]
    public void ComboMultiplier_ReturnsExpected(int streak, int expected)
    {
        Assert.AreEqual(expected, GameManager.ComboMultiplier(streak));
    }

    // ── FishCountForDifficulty thresholds ─────────────────────────────────────────────

    [TestCase(0,  3)]
    [TestCase(2,  3)]
    [TestCase(3,  4)]
    [TestCase(5,  4)]
    [TestCase(6,  5)]
    [TestCase(9,  5)]
    [TestCase(10, 6)]
    [TestCase(14, 6)]
    [TestCase(15, 7)]
    [TestCase(30, 7)]
    public void FishCountForDifficulty_ReturnsExpected(int correct, int expected)
    {
        Assert.AreEqual(expected, GameManager.FishCountForDifficulty(correct));
    }

    // ── TierIndex boundary values ─────────────────────────────────────────────────────

    [TestCase(0,   0)]
    [TestCase(1,   1)]
    [TestCase(4,   1)]
    [TestCase(5,   2)]
    [TestCase(9,   2)]
    [TestCase(10,  3)]
    [TestCase(14,  3)]
    [TestCase(15,  4)]
    [TestCase(100, 4)]
    public void TierIndex_ReturnsExpected(int correctHits, int expected)
    {
        Assert.AreEqual(expected, GameManager.TierIndex(correctHits));
    }
}
