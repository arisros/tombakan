// ═══════════════════════════════════════════════════════════════════════════════
// Week 9 PlayMode tests — TASK-03 scene configuration.
//
// PREREQUISITE: "GamePlay" must be added to the Build Settings scene list before
// running these tests. In Unity Editor: File → Build Settings → Add Open Scenes
// (or drag Assets/Scenes/GamePlay.unity into the list). AR plane detection is
// inactive in Play mode, so the scene loads without crashing in Editor.
// ═══════════════════════════════════════════════════════════════════════════════

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode acceptance-criteria tests for ITERATION_week9_SCOPE.md TASK-03.
///
/// Verifies scene-serialised fields in GamePlay.unity:
///   (a) FishSpawner.spawnRadius == 0.8  (changed from 1.5)
///   (c) GameManager.throwHintPanel is wired and inactive by default
/// </summary>
[Category("SceneConfig")]
public class Week9Task03SceneTests
{
    const string SceneName = "GamePlay";

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Assume skips the test gracefully rather than failing if the scene is not in
        // Build Settings (common on fresh checkouts before the scene list is configured).
        Assume.That(
            Application.CanStreamedLevelBeLoaded(SceneName),
            $"Scene '{SceneName}' must be added to Build Settings to run these tests.");

        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
        yield return null; // one frame for scene initialisation
    }

    // ── TASK-03(a): FishSpawner.spawnRadius serialised as 0.8 ────────────────────────

    [UnityTest]
    public IEnumerator FishSpawner_SpawnRadiusIs0_8()
    {
        var spawner = Object.FindObjectOfType<FishSpawner>();
        Assert.IsNotNull(spawner, "FishSpawner component not found in the GamePlay scene");

        Assert.AreEqual(0.8f, spawner.spawnRadius, 0.001f,
            "FishSpawner.spawnRadius must be 0.8 (TASK-03a: fish must stay " +
            "within the visible water surface on a 60 cm table)");

        yield return null;
    }

    // ── TASK-03(b): Implied by spawnRadius — confirm it is strictly less than 1.5 ────

    [UnityTest]
    public IEnumerator FishSpawner_SpawnRadiusIsLessThanOriginal1_5()
    {
        var spawner = Object.FindObjectOfType<FishSpawner>();
        Assert.IsNotNull(spawner);

        Assert.Less(spawner.spawnRadius, 1.5f,
            "spawnRadius must be reduced from the original 1.5 — " +
            "fish were escaping the visible water surface (TASK-03a)");

        yield return null;
    }

    // ── TASK-03(c): ThrowHintPanel wired in GameManager and inactive by default ───────

    [UnityTest]
    public IEnumerator GameManager_ThrowHintPanel_IsWired()
    {
        var gm = Object.FindObjectOfType<GameManager>();
        Assert.IsNotNull(gm, "GameManager not found in the GamePlay scene");

        Assert.IsNotNull(gm.throwHintPanel,
            "GameManager.throwHintPanel must be assigned in the Inspector " +
            "(TASK-03c: wire ThrowHintPanel GameObject to the field)");

        yield return null;
    }

    [UnityTest]
    public IEnumerator ThrowHintPanel_IsInactiveByDefault()
    {
        var gm = Object.FindObjectOfType<GameManager>();
        Assert.IsNotNull(gm);
        Assume.That(gm.throwHintPanel, Is.Not.Null, "throwHintPanel must be wired first");

        Assert.IsFalse(gm.throwHintPanel.activeSelf,
            "ThrowHintPanel must be inactive at scene start — " +
            "it is activated by StartGame() only when needed (TASK-03c)");

        yield return null;
    }

    [UnityTest]
    public IEnumerator ThrowHintPanel_NameMatchesConvention()
    {
        var gm = Object.FindObjectOfType<GameManager>();
        Assert.IsNotNull(gm);
        Assume.That(gm.throwHintPanel, Is.Not.Null, "throwHintPanel must be wired first");

        Assert.AreEqual("ThrowHintPanel", gm.throwHintPanel.name,
            "The panel GameObject must be named 'ThrowHintPanel' per TASK-03 spec");

        yield return null;
    }
}
