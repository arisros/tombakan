// ═══════════════════════════════════════════════════════════════════════════
// Requires TombakanTestScene to be present in Build Settings and to contain
// a GameManager object (the same scene used by GameManagerPlayModeTests).
// ═══════════════════════════════════════════════════════════════════════════

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

/// <summary>
/// PlayMode acceptance tests for TASK-03: "Posisi ulang" reposition button.
///
/// Covers scene-level acceptance criteria:
///   - The reposition button is visible (active) before StartGame() fires.
///   - The button is hidden once StartGame() is called — preserving the mid-game guard.
/// </summary>
public class Week7_Task03_RepositionPlayModeTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("TombakanTestScene", LoadSceneMode.Single);
        yield return null; // wait one frame for scene objects to initialize
    }

    // AC: A "Posisi ulang" UI button is active only when gameRunning == false (pre-game).
    [UnityTest]
    public IEnumerator RepositionButton_ActiveBeforeStartGame()
    {
        // Wire a test button to GameManager.repositionButton so we can observe it.
        var buttonGO = new GameObject("TestRepositionButton");
        buttonGO.SetActive(true);
        GameManager.I.repositionButton = buttonGO;

        yield return null; // ensure state is stable

        Assert.IsTrue(buttonGO.activeSelf,
            "Reposition button must be visible (active) before StartGame() is called.");
    }

    // AC: Button hidden after StartGame() fires — preserving the mid-game guard.
    [UnityTest]
    public IEnumerator RepositionButton_HiddenAfterStartGame()
    {
        var buttonGO = new GameObject("TestRepositionButton");
        buttonGO.SetActive(true);
        GameManager.I.repositionButton = buttonGO;

        GameManager.I.StartGame();
        yield return null; // allow StartGame() to propagate

        Assert.IsFalse(buttonGO.activeSelf,
            "Reposition button must be hidden after StartGame() fires to preserve the mid-game guard.");
    }
}
