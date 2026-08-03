// Tests covering ITERATION_week5_SCOPE.md acceptance criteria.
// Pure-logic tests live here; scene-dependent tests are in Week5PlayModeTests.cs.

using NUnit.Framework;

public class Week5AcceptanceTests
{
    // ── TASK-03 · Result-screen text polish ───────────────────────────────────
    // AC (a): Accuracy.Format(0, 0) returns a non-numeric placeholder string.
    // AC (b): resultXpText is not updated when xpEarned == 0 — verified via pure logic:
    //         the guard is "xpEarned > 0", so the text is set to "" (empty) when xp == 0.

    [Test]
    public void Accuracy_Format_BothZero_ReturnsNonNumericPlaceholder()
    {
        string result = Accuracy.Format(0, 0);
        Assert.AreEqual("--", result,
            "Accuracy.Format(0, 0) must return \"--\" (or another non-numeric placeholder), " +
            "not \"0/0 (0%)\" which looks like broken UI");
    }

    [Test]
    public void Accuracy_Format_BothZero_DoesNotContainSlash()
    {
        string result = Accuracy.Format(0, 0);
        StringAssert.DoesNotContain("/", result,
            "No-throw result must not contain the fraction separator '/'");
    }

    [Test]
    public void Accuracy_Format_BothZero_DoesNotContainPercent()
    {
        string result = Accuracy.Format(0, 0);
        StringAssert.DoesNotContain("%", result,
            "No-throw result must not contain a percentage sign");
    }

    // ── TASK-01 · Score guard (pure side) ─────────────────────────────────────
    // The guard `if (!gameRunning) return;` in PickNewTarget and OnFishHit cannot
    // be tested without a MonoBehaviour, but the underlying pure logic can be verified.

    [Test]
    public void ClampScore_PenaltyOnZeroScore_YieldsZeroDeduction()
    {
        // Mirrors the TASK-01a fix: OnFishHit calls ClampScore then computes actualDeduction.
        // When score == 0: ClampScore(0 - 25) == 0 → deduction == 0.
        int score   = 0;
        int penalty = 25;
        int clamped = GameManager.ClampScore(score - penalty);
        int actual  = score - clamped;

        Assert.AreEqual(0, actual,
            "When score is already at 0 the actual deduction must be 0 (floor enforced by ClampScore)");
    }
}
