// ═══════════════════════════════════════════════════════════
// MANUAL SETUP REQUIRED before running PlayMode tests:
// In Unity Editor, create: Assets/Tests/PlayMode/TombakanTestScene.unity
// Scene contents:
//   - GameObject "GameManager" with GameManager.cs component
//     Wire all SerializeField UI references to lightweight stub objects
//   - GameObject "AudioManager" with AudioManager.cs + AudioSource
//     Wire mainBGMSource, gameplayBGMSource, sfxSource
//   - No ARSession, ARRaycastManager, or ARFoundation components needed
// Add scene to Build Settings before running PlayMode tests
// ═══════════════════════════════════════════════════════════

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class GameManagerPlayModeTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("TombakanTestScene", LoadSceneMode.Single);
        yield return null;
    }

    [UnityTest]
    public IEnumerator StartGame_ResetsScoreToZero()
    {
        GameManager.I.score = 999;
        GameManager.I.StartGame();
        yield return null;
        Assert.AreEqual(0, GameManager.I.score);
    }

    [UnityTest]
    public IEnumerator StartGame_ResetsCorrectHitCount()
    {
        GameManager.I.correctHitCount = 10;
        GameManager.I.StartGame();
        yield return null;
        Assert.AreEqual(0, GameManager.I.correctHitCount);
    }

    [UnityTest]
    public IEnumerator StartGame_SetsGameRunningTrue()
    {
        GameManager.I.StartGame();
        yield return null;
        Assert.IsTrue(GameManager.I.gameRunning);
    }

    [UnityTest]
    public IEnumerator OnFishHit_CorrectColor_AddsHundredPoints()
    {
        GameManager.I.StartGame();
        yield return null;
        Color target = GameManager.I.targetColor;
        int before = GameManager.I.score;
        GameManager.I.OnFishHit(target);
        yield return null;
        Assert.AreEqual(before + GameConstants.PointPerCorrectHit, GameManager.I.score);
    }

    [UnityTest]
    public IEnumerator OnFishHit_CorrectColor_IncrementsCorrectHitCount()
    {
        GameManager.I.StartGame();
        yield return null;
        Color target = GameManager.I.targetColor;
        int before = GameManager.I.correctHitCount;
        GameManager.I.OnFishHit(target);
        yield return null;
        Assert.AreEqual(before + 1, GameManager.I.correctHitCount);
    }

    [UnityTest]
    public IEnumerator OnFishHit_WrongColor_SubtractsTwentyFivePoints()
    {
        GameManager.I.StartGame();
        yield return null;
        Color wrong = GameManager.I.targetColor == Color.red ? Color.green : Color.red;
        int before = GameManager.I.score;
        GameManager.I.OnFishHit(wrong);
        yield return null;
        Assert.AreEqual(before - GameConstants.PenaltyPerWrongHit, GameManager.I.score);
    }

    [UnityTest]
    public IEnumerator Timer_WhenExpired_SetsGameNotRunning()
    {
        GameManager.I.StartGame();
        yield return null;
        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);
        Assert.IsFalse(GameManager.I.gameRunning);
    }
}
