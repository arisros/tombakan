using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Acceptance tests for TASK-01: Throw-mechanic tutorial hint for first-time players.
///
/// Covers:
///   - ShowThrowHint() activates the panel and registers a one-shot listener on SpearThrower.OnThrowFired.
///   - HideThrowHint() deactivates the panel and unregisters the listener.
///   - StartCoaching() calls ShowThrowHint() only when ProgressionStore.GetTotalXp() == 0.
///   - Hint panel is inactive by default.
///   - Auto-dismiss fires with no player action after the throw event.
///   - No hint shown for returning players (xp > 0).
/// </summary>
public class Week7_Task01_ThrowHintTests
{
    TombakanOnboarding onboarding;
    GameObject hintPanel;
    GameObject pointerGO;

    [SetUp]
    public void SetUp()
    {
        var host = new GameObject("OnboardingHost");
        onboarding = host.AddComponent<TombakanOnboarding>();

        hintPanel = new GameObject("ThrowHintPanel");
        hintPanel.SetActive(false);
        onboarding.throwHintPanel = hintPanel;

        var pointerHost = new GameObject("ThrowHintPointer");
        onboarding.throwHintPointer = pointerHost.AddComponent<RectTransform>();
        pointerGO = pointerHost;
        pointerGO.SetActive(false);
    }

    [TearDown]
    public void TearDown()
    {
        // Remove any residual subscription on the static event before destroying the component.
        if (onboarding != null)
            SpearThrower.OnThrowFired -= onboarding.HideThrowHint;

        if (onboarding != null) Object.DestroyImmediate(onboarding.gameObject);
        if (hintPanel  != null) Object.DestroyImmediate(hintPanel);
        if (pointerGO  != null) Object.DestroyImmediate(pointerGO);

        PlayerPrefs.DeleteKey("tombakan_total_xp");
        PlayerPrefs.Save();
    }

    // AC: Hint panel is inactive by default; auto-dismissed on first throw event.
    [Test]
    public void ThrowHintPanel_IsInactiveByDefault()
    {
        Assert.IsFalse(hintPanel.activeSelf,
            "Throw hint panel must start inactive before any method is called.");
    }

    // AC: ShowThrowHint() activates the throw hint panel.
    [Test]
    public void ShowThrowHint_ActivatesHintPanel()
    {
        onboarding.ShowThrowHint();

        Assert.IsTrue(hintPanel.activeSelf,
            "throwHintPanel must be active after ShowThrowHint() is called.");
    }

    // AC: ShowThrowHint() activates the visual pointer at the throw button.
    [Test]
    public void ShowThrowHint_ActivatesPointer()
    {
        onboarding.ShowThrowHint();

        Assert.IsTrue(pointerGO.activeSelf,
            "throwHintPointer must be active after ShowThrowHint() is called.");
    }

    // AC: ShowThrowHint() is safe when throwHintPanel is not wired.
    [Test]
    public void ShowThrowHint_NullPanel_DoesNotThrow()
    {
        onboarding.throwHintPanel = null;

        Assert.DoesNotThrow(() => onboarding.ShowThrowHint(),
            "ShowThrowHint() must be null-safe when throwHintPanel is not assigned.");
    }

    // AC: HideThrowHint() deactivates the panel.
    [Test]
    public void HideThrowHint_DeactivatesHintPanel()
    {
        onboarding.ShowThrowHint();
        onboarding.HideThrowHint();

        Assert.IsFalse(hintPanel.activeSelf,
            "throwHintPanel must be inactive after HideThrowHint() is called.");
    }

    // AC: HideThrowHint() deactivates the pointer.
    [Test]
    public void HideThrowHint_DeactivatesPointer()
    {
        onboarding.ShowThrowHint();
        onboarding.HideThrowHint();

        Assert.IsFalse(pointerGO.activeSelf,
            "throwHintPointer must be inactive after HideThrowHint() is called.");
    }

    // AC: Hint panel auto-dismissed on first throw event with no player action required.
    [Test]
    public void OnThrowFired_AutoDismissesPanel_WithoutPlayerAction()
    {
        onboarding.ShowThrowHint();
        Assert.IsTrue(hintPanel.activeSelf, "Precondition: panel must be active.");

        RaiseOnThrowFired();

        Assert.IsFalse(hintPanel.activeSelf,
            "Hint panel must auto-dismiss when SpearThrower.OnThrowFired fires — no player action required.");
    }

    // AC: ShowThrowHint() registers a ONE-SHOT listener — fires only once.
    [Test]
    public void OnThrowFired_IsOneShot_DoesNotDismissOnSecondFire()
    {
        onboarding.ShowThrowHint();
        RaiseOnThrowFired(); // first fire: dismisses and unsubscribes

        hintPanel.SetActive(true); // manually re-activate to probe second fire

        RaiseOnThrowFired(); // should NOT affect the panel

        Assert.IsTrue(hintPanel.activeSelf,
            "Panel must remain active on a second throw event — the listener is one-shot and already removed.");
    }

    // AC: StartCoaching() (called after placement) invokes ShowThrowHint() when totalXp == 0.
    [Test]
    public void StartCoaching_WhenXpIsZero_ShowsHintPanel()
    {
        PlayerPrefs.DeleteKey("tombakan_total_xp");
        PlayerPrefs.Save();

        onboarding.StartCoaching();

        Assert.IsTrue(hintPanel.activeSelf,
            "StartCoaching() must call ShowThrowHint() for first-time players (totalXp == 0).");
    }

    // AC: No changes to flow for players with totalXp > 0.
    [Test]
    public void StartCoaching_WhenXpIsPositive_DoesNotShowHintPanel()
    {
        PlayerPrefs.SetInt("tombakan_total_xp", 200);
        PlayerPrefs.Save();

        onboarding.StartCoaching();

        Assert.IsFalse(hintPanel.activeSelf,
            "StartCoaching() must NOT call ShowThrowHint() for returning players (totalXp > 0).");
    }

    // --- helpers ---

    /// <summary>
    /// Raises SpearThrower.OnThrowFired via the backing delegate field.
    /// SpearThrower is a MonoBehaviour; the event is a plain static C# event.
    /// </summary>
    static void RaiseOnThrowFired()
    {
        var field = typeof(SpearThrower).GetField(
            "OnThrowFired",
            BindingFlags.Static | BindingFlags.NonPublic);
        var del = field?.GetValue(null) as System.Action;
        del?.Invoke();
    }
}
