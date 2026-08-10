using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode tests for TASK-02:
///   BUG-8 — GoalManager.ForceCompleteGoal must not throw when called before
///            StartCoaching (returning players skip onboarding, so m_OnboardingGoals
///            is never initialised).
///   BUG-3 — GameConstants.SpawnDelay documents the single authoritative value used
///            by both LockThrow and Invoke(PickNewTarget) in GameManager.OnFishHit.
/// </summary>
public class GoalManagerTests
{
    // ── BUG-8 ────────────────────────────────────────────────────────────────

    [Test]
    public void ForceCompleteGoal_WithoutStartCoaching_DoesNotThrowNullOrIndexException()
    {
        // Simulate a returning player: TombakanOnboarding.Start calls ForceCompleteGoal
        // directly without calling StartCoaching first, leaving m_OnboardingGoals null.
        var go = new GameObject("TestGoalManager");
        var gm = go.AddComponent<GoalManager>();

        // No StartCoaching call — m_OnboardingGoals remains null.
        Assert.DoesNotThrow(
            () => gm.ForceCompleteGoal(),
            "ForceCompleteGoal must not throw NullReferenceException or " +
            "IndexOutOfRangeException when called before StartCoaching");

        Object.DestroyImmediate(go);
    }

    [Test]
    public void ForceCompleteGoal_CalledMultipleTimes_NeverThrows()
    {
        // Extra guard: rapid successive calls (e.g. if TombakanOnboarding is triggered
        // multiple times in testing) must all be safe.
        var go = new GameObject("TestGoalManager_Multi");
        var gm = go.AddComponent<GoalManager>();

        Assert.DoesNotThrow(() =>
        {
            gm.ForceCompleteGoal();
            gm.ForceCompleteGoal();
            gm.ForceCompleteGoal();
        }, "Repeated ForceCompleteGoal calls before StartCoaching must all be safe");

        Object.DestroyImmediate(go);
    }

    // ── BUG-3 ────────────────────────────────────────────────────────────────

    [Test]
    public void SpawnDelay_ConstantMatchesExpectedValue()
    {
        // GameConstants.SpawnDelay is the single source of truth used by both
        // spearThrower.LockThrow(delay + SpawnDelay) and
        // Invoke(PickNewTarget, delay + SpawnDelay) in GameManager.OnFishHit.
        // Any mismatch between these two would re-introduce BUG-3.
        Assert.AreEqual(0.8f, GameConstants.SpawnDelay, 0.001f,
            "SpawnDelay must equal 0.8 s — the deferral used by Invoke(PickNewTarget)");
    }
}
