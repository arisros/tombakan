// ═══════════════════════════════════════════════════════════════════════════════
// Week 9 PlayMode tests — TASK-01(a) null-queue guard in GoalManager.
//
// These tests do NOT require a custom test scene. They create lightweight
// GameObjects at runtime and destroy them after each test.
// ═══════════════════════════════════════════════════════════════════════════════

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode acceptance-criteria tests for ITERATION_week9_SCOPE.md TASK-01.
///
/// TASK-01(a) AC: "A returning player (ScoreStore.GetBest() > 0) launches without
/// NullReferenceException."
///
/// Root cause: TombakanOnboarding.Start() calls goalManager.ForceCompleteGoal()
/// before StartCoaching() has initialised m_OnboardingGoals, leaving the queue null.
/// The fix null-guards the queue in CompleteGoal() and returns early.
/// </summary>
public class Week9Task01PlayModeTests
{
    // ── TASK-01(a): GoalManager.ForceCompleteGoal is safe with a null queue ──────────

    [UnityTest]
    public IEnumerator GoalManager_ForceCompleteGoal_WithNullQueue_DoesNotThrow()
    {
        // Arrange: instantiate GoalManager without calling StartCoaching() so that
        // m_OnboardingGoals is never initialised — exactly the returning-player path.
        var go = new GameObject("TestGoalManager_NullQueue");
        var gm = go.AddComponent<GoalManager>();

        yield return null; // let Awake/Start run (GoalManager has none, but be safe)

        // Act + Assert
        Assert.DoesNotThrow(
            () => gm.ForceCompleteGoal(),
            "GoalManager.ForceCompleteGoal() must null-guard m_OnboardingGoals. " +
            "Returning players hit this path on every launch (TASK-01a).");

        yield return null;
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator GoalManager_ForceCompleteGoal_CalledTwice_DoesNotThrow()
    {
        // Regression: if TombakanOnboarding fires twice (e.g. scene reload in tests)
        // repeated null-queue calls must remain safe.
        var go = new GameObject("TestGoalManager_DoubleCall");
        var gm = go.AddComponent<GoalManager>();

        yield return null;

        Assert.DoesNotThrow(
            () => { gm.ForceCompleteGoal(); gm.ForceCompleteGoal(); },
            "Repeated ForceCompleteGoal() calls on an uninitialised queue must not throw.");

        yield return null;
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator GoalManager_ForceCompleteGoal_DoesNotThrow_RegardlessOfBestScore()
    {
        // Simulate the full returning-player trigger: ScoreStore.GetBest() > 0 AND
        // ForceCompleteGoal is called before the queue is ready.
        string key = ScoreStore.BestScoreKey;
        int saved  = PlayerPrefs.GetInt(key, 0);
        PlayerPrefs.SetInt(key, 100); // mark player as returning
        PlayerPrefs.Save();

        var go = new GameObject("TestGoalManager_ReturningPlayer");
        var gm = go.AddComponent<GoalManager>();
        yield return null;

        Assert.DoesNotThrow(
            () => gm.ForceCompleteGoal(),
            "Returning-player path (GetBest() > 0) must not throw NullReferenceException.");

        yield return null;

        // Restore
        PlayerPrefs.SetInt(key, saved);
        PlayerPrefs.Save();
        Object.Destroy(go);
    }
}
