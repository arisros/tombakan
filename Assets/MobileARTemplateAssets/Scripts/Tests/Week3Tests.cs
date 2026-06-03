// QA — Week 3 regression tests (EditMode).
// Cover the new pure helpers added in Week 3: PacingRules, Accuracy.
using NUnit.Framework;

[TestFixture]
public class PacingRulesTests
{
    // TASK-02: delay shrinks with progress, clamped to a floor and the base
    [Test]
    public void HitDelay_StartsAtBaseWithNoHits()
    {
        Assert.AreEqual(2.2f, PacingRules.HitDelayForProgress(2.2f, 0), 1e-4f);
    }

    [Test]
    public void HitDelay_ShrinksByPointOnePerHit()
    {
        Assert.AreEqual(2.1f, PacingRules.HitDelayForProgress(2.2f, 1), 1e-4f);
        Assert.AreEqual(1.7f, PacingRules.HitDelayForProgress(2.2f, 5), 1e-4f);
    }

    [Test]
    public void HitDelay_NeverDropsBelowFloor()
    {
        // 2.2 - 0.1*50 would be negative; must clamp to the 1.0 s floor
        Assert.AreEqual(PacingRules.MinHitDelay, PacingRules.HitDelayForProgress(2.2f, 50), 1e-4f);
        Assert.GreaterOrEqual(PacingRules.HitDelayForProgress(2.2f, 1000), PacingRules.MinHitDelay);
    }

    [Test]
    public void HitDelay_NeverExceedsBase_AndHandlesNegativeHits()
    {
        Assert.AreEqual(2.2f, PacingRules.HitDelayForProgress(2.2f, -3), 1e-4f);
    }

    [Test]
    public void HitDelay_FloorClampsToBaseWhenBaseBelowMin()
    {
        // If a designer sets a tiny base delay, the floor must not exceed it
        Assert.AreEqual(0.5f, PacingRules.HitDelayForProgress(0.5f, 0), 1e-4f);
        Assert.AreEqual(0.5f, PacingRules.HitDelayForProgress(0.5f, 10), 1e-4f);
    }
}

[TestFixture]
public class AccuracyTests
{
    // TASK-03: rounded percent + readable format
    [Test]
    public void Percent_ZeroAttempts_IsZero()
    {
        Assert.AreEqual(0, Accuracy.Percent(0, 0));
    }

    [Test]
    public void Percent_AllCorrect_IsHundred()
    {
        Assert.AreEqual(100, Accuracy.Percent(8, 0));
    }

    [Test]
    public void Percent_RoundsToNearestInteger()
    {
        Assert.AreEqual(75, Accuracy.Percent(9, 3));   // 9/12 = 75
        Assert.AreEqual(67, Accuracy.Percent(2, 1));   // 66.67 -> 67
        Assert.AreEqual(33, Accuracy.Percent(1, 2));   // 33.33 -> 33
    }

    [Test]
    public void Format_ProducesReadout()
    {
        Assert.AreEqual("9/12 (75%)", Accuracy.Format(9, 3));
    }

    [Test]
    public void Format_ZeroThrows_ReturnsDash()
    {
        // Week 5 TASK-03: zero-throw state must not display "0/0 (0%)" to the player
        Assert.AreEqual("--", Accuracy.Format(0, 0));
    }
}
