// Week 7 EditMode tests — T3: GamePlay.unity scene structure validation.
//
// Verifies that the ThrowHintPanel GameObject required by UX-1 is correctly
// authored in the production scene and wired to TombakanOnboarding.
//
// The scene is opened additively in edit mode so no AR session is started.
// NOTE: if a developer has GamePlay.unity open in the editor when running these
// tests, closing the additively-opened copy in TearDown is safe — it does not
// affect the main editor scene.
//
// Run via: Window → General → Test Runner → EditMode
//
// AC coverage (T3):
//   • Canvas contains a child named "ThrowHintPanel".
//   • Panel is inactive by default (m_IsActive: 0).
//   • Panel has a background Image with semi-transparent alpha.
//   • Panel has a TMP_Text child reading "Sentuh tombol untuk melempar tombak".
//   • TombakanOnboarding.throwHintPanel is wired to the ThrowHintPanel object.

using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[TestFixture]
public class Week7SceneValidationTests
{
    const string SCENE_PATH = "Assets/Scenes/GamePlay.unity";

    UnityEngine.SceneManagement.Scene _scene;
    TombakanOnboarding _onboarding;
    GameObject _throwHintPanel;

    [SetUp]
    public void OpenGamePlayScene()
    {
        _scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Additive);
        // FindObjectOfType(true) includes inactive objects (Unity 2020.3.4+).
        _onboarding     = Object.FindObjectOfType<TombakanOnboarding>(includeInactive: true);
        _throwHintPanel = _onboarding != null ? _onboarding.throwHintPanel : null;
    }

    [TearDown]
    public void CloseGamePlayScene()
    {
        EditorSceneManager.CloseScene(_scene, removeScene: true);
        _onboarding     = null;
        _throwHintPanel = null;
    }

    // ── Wiring ────────────────────────────────────────────────────────────────

    [Test]
    public void TombakanOnboarding_ThrowHintPanelField_IsAssigned()
    {
        Assert.IsNotNull(_onboarding,
            "TombakanOnboarding component not found in GamePlay.unity.");
        Assert.IsNotNull(_throwHintPanel,
            "TombakanOnboarding.throwHintPanel is not assigned in GamePlay.unity (T3 wiring AC).");
    }

    // ── Naming ────────────────────────────────────────────────────────────────

    [Test]
    public void ThrowHintPanel_IsNamedCorrectly()
    {
        Assume.That(_throwHintPanel, Is.Not.Null, "throwHintPanel not found — run wiring test first.");

        Assert.AreEqual("ThrowHintPanel", _throwHintPanel.name,
            "The wired throwHintPanel GameObject must be named 'ThrowHintPanel' (T3).");
    }

    // ── Default state ─────────────────────────────────────────────────────────

    [Test]
    public void ThrowHintPanel_IsInactiveByDefault()
    {
        Assume.That(_throwHintPanel, Is.Not.Null, "throwHintPanel not found.");

        Assert.IsFalse(_throwHintPanel.activeSelf,
            "ThrowHintPanel.m_IsActive must be 0 (false) in the saved scene " +
            "so it is hidden until DismissGreeting() activates it (T3).");
    }

    // ── Background Image ──────────────────────────────────────────────────────

    [Test]
    public void ThrowHintPanel_HasImageComponent()
    {
        Assume.That(_throwHintPanel, Is.Not.Null, "throwHintPanel not found.");

        var img = _throwHintPanel.GetComponent<Image>();

        Assert.IsNotNull(img,
            "ThrowHintPanel must have an Image component as its background (T3).");
    }

    [Test]
    public void ThrowHintPanel_BackgroundImage_IsSemiTransparent()
    {
        Assume.That(_throwHintPanel, Is.Not.Null, "throwHintPanel not found.");
        var img = _throwHintPanel.GetComponent<Image>();
        Assume.That(img, Is.Not.Null, "Image component not found.");

        Assert.Less(img.color.a, 1f,
            "ThrowHintPanel background Image alpha must be < 1 (semi-transparent dark, " +
            "matching DailyBonusPanel style — T3).");
        Assert.Greater(img.color.a, 0f,
            "ThrowHintPanel background Image alpha must be > 0 (not fully invisible).");
    }

    // ── Text child ────────────────────────────────────────────────────────────

    [Test]
    public void ThrowHintPanel_HasTmpTextChild()
    {
        Assume.That(_throwHintPanel, Is.Not.Null, "throwHintPanel not found.");

        var tmp = _throwHintPanel.GetComponentInChildren<TMP_Text>(includeInactive: true);

        Assert.IsNotNull(tmp,
            "ThrowHintPanel must have a TMP_Text child component (T3).");
    }

    [Test]
    public void ThrowHintPanel_TmpTextChild_HasCorrectIndonesianText()
    {
        Assume.That(_throwHintPanel, Is.Not.Null, "throwHintPanel not found.");
        var tmp = _throwHintPanel.GetComponentInChildren<TMP_Text>(includeInactive: true);
        Assume.That(tmp, Is.Not.Null, "TMP_Text child not found.");

        Assert.AreEqual(
            "Sentuh tombol untuk melempar tombak",
            tmp.text,
            "ThrowHintPanel TMP_Text must read 'Sentuh tombol untuk melempar tombak' (T3).");
    }
}
