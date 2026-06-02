// QA — Week 1 regression tests (EditMode).
// These call the REAL static logic in GameManager so they fail if the game
// rules drift. Run locally via Window > General > Test Runner > EditMode,
// or automatically in CI via game-ci/unity-test-runner.
using NUnit.Framework;

[TestFixture]
public class GameManagerTests
{
    // ── TASK-01: Score floor (ClampScore) ────────────────────────────────────

    [Test]
    public void ClampScore_FloorsNegativeToZero()
    {
        Assert.AreEqual(0, GameManager.ClampScore(-25));
        Assert.AreEqual(0, GameManager.ClampScore(-1000));
    }

    [Test]
    public void ClampScore_LeavesNonNegativeUntouched()
    {
        Assert.AreEqual(0, GameManager.ClampScore(0));
        Assert.AreEqual(100, GameManager.ClampScore(100));
    }

    [Test]
    public void ClampScore_RepeatedPenaltiesNeverGoNegative()
    {
        int score = 50;
        for (int i = 0; i < 10; i++)
            score = GameManager.ClampScore(score - 25);

        Assert.GreaterOrEqual(score, 0);
    }

    // ── TASK-02: Combo multiplier ─────────────────────────────────────────────

    [Test]
    public void ComboMultiplier_IsOneBeforeStreak()
    {
        Assert.AreEqual(1, GameManager.ComboMultiplier(0));
        Assert.AreEqual(1, GameManager.ComboMultiplier(1));
        Assert.AreEqual(1, GameManager.ComboMultiplier(2));
    }

    [Test]
    public void ComboMultiplier_IsTwoAtThreeStreak()
    {
        Assert.AreEqual(2, GameManager.ComboMultiplier(3));
        Assert.AreEqual(2, GameManager.ComboMultiplier(4));
    }

    [Test]
    public void ComboMultiplier_IsThreeAtFiveOrMore()
    {
        Assert.AreEqual(3, GameManager.ComboMultiplier(5));
        Assert.AreEqual(3, GameManager.ComboMultiplier(99));
    }

    // ── TASK-03: Fish count difficulty scaling ────────────────────────────────

    [Test]
    [TestCase(0, 3)]
    [TestCase(2, 3)]
    [TestCase(3, 4)]
    [TestCase(5, 4)]
    [TestCase(6, 5)]
    [TestCase(9, 5)]
    [TestCase(10, 6)]
    [TestCase(14, 6)]
    [TestCase(15, 7)]
    [TestCase(50, 7)]
    public void FishCountForDifficulty_ScalesWithCorrectHits(int correct, int expected)
    {
        Assert.AreEqual(expected, GameManager.FishCountForDifficulty(correct));
    }

    // ── TASK-05: Tier thresholds ──────────────────────────────────────────────

    [Test]
    [TestCase(0, 0)]   // Empty
    [TestCase(1, 1)]   // Low
    [TestCase(4, 1)]
    [TestCase(5, 2)]   // Mid
    [TestCase(9, 2)]
    [TestCase(10, 3)]  // High
    [TestCase(14, 3)]
    [TestCase(15, 4)]  // Legend
    [TestCase(99, 4)]
    public void TierIndex_CoversAllRanges(int correct, int expectedTier)
    {
        Assert.AreEqual(expectedTier, GameManager.TierIndex(correct));
    }
}
