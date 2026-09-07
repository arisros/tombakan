// Week 7 EditMode tests — T1 (BUG-5) and T2 (UX-1)
// Pure-logic and component-isolation tests: no scene loading, no coroutines.
//
// Run via: Window → General → Test Runner → EditMode
//
// AC coverage:
//   BUG-5 (T1): GoalManager.CompleteGoal null guard prevents NRE when m_OnboardingGoals is null.
//   UX-1  (T2): TombakanOnboarding exposes public throwHintPanel field + DismissThrowHint method;
//               DismissGreeting activates the panel; DismissThrowHint deactivates it.

using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class Week7BugFixTests
{
    // ── T1 BUG-5 ──────────────────────────────────────────────────────────────
    // ForceCompleteGoal → CompleteGoal must not NRE on m_OnboardingGoals.Count
    // when the queue has never been initialised (returning-player fast-path calls
    // ForceCompleteGoal before StartCoaching populates the queue).

    [Test]
    public void GoalManager_ForceCompleteGoal_WithNullQueue_DoesNotThrow()
    {
        // In EditMode, Awake/Start are NOT called by AddComponent.
        // GoalManager.m_OnboardingGoals is therefore null — exactly the crash
        // scenario that BUG-5 introduces.
        var go = new GameObject("GoalManagerBug5");
        var gm = go.AddComponent<GoalManager>();

        Assert.DoesNotThrow(
            () => gm.ForceCompleteGoal(),
            "GoalManager.CompleteGoal must guard against null m_OnboardingGoals " +
            "and return early rather than throwing NullReferenceException (BUG-5).");

        Object.DestroyImmediate(go);
    }

    // ── T2 UX-1: throwHintPanel field ─────────────────────────────────────────
    // AC: "TombakanOnboarding has a new public GameObject throwHintPanel serialized field."

    [Test]
    public void TombakanOnboarding_ThrowHintPanel_IsPublicGameObjectField()
    {
        FieldInfo fi = typeof(TombakanOnboarding).GetField(
            "throwHintPanel",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(fi,
            "throwHintPanel must exist as a public instance field on TombakanOnboarding (UX-1).");
        Assert.AreEqual(typeof(GameObject), fi.FieldType,
            "throwHintPanel must be typed as GameObject (UX-1).");
    }

    // ── T2 UX-1: DismissThrowHint method ─────────────────────────────────────
    // AC: "a public DismissThrowHint() method hides [the panel]."

    [Test]
    public void TombakanOnboarding_DismissThrowHint_IsPublicMethod()
    {
        MethodInfo mi = typeof(TombakanOnboarding).GetMethod(
            "DismissThrowHint",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(mi,
            "DismissThrowHint() must be a public method on TombakanOnboarding (UX-1).");
        Assert.AreEqual(typeof(void), mi.ReturnType,
            "DismissThrowHint() must return void.");
    }

    // ── T2 UX-1: DismissGreeting activates throwHintPanel ────────────────────
    // AC: "the panel is shown after DismissGreeting() completes."

    [Test]
    public void TombakanOnboarding_DismissGreeting_ActivatesThrowHintPanel()
    {
        // Arrange — Awake/Start intentionally not called; fields wired manually.
        var rootGO = new GameObject("TombakanOnboarding_UX1_Show");
        var panel  = new GameObject("ThrowHintPanel");
        panel.SetActive(false);

        var onboarding = rootGO.AddComponent<TombakanOnboarding>();
        onboarding.throwHintPanel = panel;
        // greetingPanel is null  → SetActive(false) call is skipped (null-safe).
        // goalManager   is null  → StartCoaching()  call is skipped (null-safe).

        // Act
        onboarding.DismissGreeting();

        // Assert
        Assert.IsTrue(panel.activeSelf,
            "DismissGreeting() must call throwHintPanel.SetActive(true) for first-time players (UX-1).");

        Object.DestroyImmediate(panel);
        Object.DestroyImmediate(rootGO);
    }

    // ── T2 UX-1: DismissThrowHint deactivates throwHintPanel ─────────────────
    // AC: "a public DismissThrowHint() method hides it (to be called by the throw
    //      button or on first throw detection)."

    [Test]
    public void TombakanOnboarding_DismissThrowHint_DeactivatesThrowHintPanel()
    {
        // Arrange
        var rootGO = new GameObject("TombakanOnboarding_UX1_Hide");
        var panel  = new GameObject("ThrowHintPanel");
        panel.SetActive(true);

        var onboarding = rootGO.AddComponent<TombakanOnboarding>();
        onboarding.throwHintPanel = panel;

        // Act
        onboarding.DismissThrowHint();

        // Assert
        Assert.IsFalse(panel.activeSelf,
            "DismissThrowHint() must call throwHintPanel.SetActive(false) (UX-1).");

        Object.DestroyImmediate(panel);
        Object.DestroyImmediate(rootGO);
    }
}
