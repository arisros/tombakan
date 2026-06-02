// ═══════════════════════════════════════════════════════════════════
// E2E test suite — full game loop from StartGame to result screen.
// Prerequisite: run Tombakan → Create Test Scene in the Unity Editor
// to generate Assets/Tests/PlayMode/TombakanTestScene.unity, then
// commit that scene so CI can find it.
// ═══════════════════════════════════════════════════════════════════

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class GameLoopE2ETests
{
    // ── Setup / Teardown ─────────────────────────────────────────────

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("TombakanTestScene", LoadSceneMode.Single);
        yield return null; // wait one frame for Awake/Start
    }

    // ── State machine ────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator StartGame_SetsRunningState()
    {
        GameManager.I.StartGame();
        yield return null;

        Assert.IsTrue(GameManager.I.gameRunning);
        Assert.AreEqual(0, GameManager.I.score);
        Assert.AreEqual(0, GameManager.I.correctHitCount);
        Assert.IsTrue(
            Mathf.Approximately(GameManager.I.timeLeft, GameConstants.GameDuration),
            $"timeLeft should be {GameConstants.GameDuration}, got {GameManager.I.timeLeft}"
        );
    }

    [UnityTest]
    public IEnumerator TimerExpiry_StopsGame_ShowsResultContainer()
    {
        GameManager.I.StartGame();
        yield return null;

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        Assert.IsFalse(GameManager.I.gameRunning, "gameRunning should be false after timer expires");
        Assert.IsTrue(GameManager.I.resultContainer.activeSelf, "resultContainer must be visible");
    }

    // ── Scoring ──────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator CorrectHit_AddsPoints_IncrementsCount()
    {
        GameManager.I.StartGame();
        yield return null;

        Color target = GameManager.I.targetColor;
        GameManager.I.OnFishHit(target);
        yield return null;

        Assert.AreEqual(GameConstants.PointPerCorrectHit, GameManager.I.score);
        Assert.AreEqual(1, GameManager.I.correctHitCount);
    }

    [UnityTest]
    public IEnumerator WrongHit_SubtractsPoints_CountUnchanged()
    {
        GameManager.I.StartGame();
        yield return null;

        Color wrong = GameManager.I.targetColor == Color.red ? Color.green : Color.red;
        GameManager.I.OnFishHit(wrong);
        yield return null;

        Assert.AreEqual(-GameConstants.PenaltyPerWrongHit, GameManager.I.score);
        Assert.AreEqual(0, GameManager.I.correctHitCount);
    }

    [UnityTest]
    public IEnumerator ThreeCorrectThenOneWrong_ScoreAccumulatesCorrectly()
    {
        GameManager.I.StartGame();
        yield return null;

        for (int i = 0; i < 3; i++)
        {
            GameManager.I.OnFishHit(GameManager.I.targetColor);
            yield return null;
        }

        Color wrong = GameManager.I.targetColor == Color.red ? Color.green : Color.red;
        GameManager.I.OnFishHit(wrong);
        yield return null;

        int expected = 3 * GameConstants.PointPerCorrectHit - GameConstants.PenaltyPerWrongHit;
        Assert.AreEqual(expected, GameManager.I.score);
        Assert.AreEqual(3, GameManager.I.correctHitCount);
    }

    // ── Result screen ────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator ResultScreen_NoCorrectHits_ShowsEmptyMessage_TierZero()
    {
        GameManager.I.StartGame();
        yield return null;

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        Assert.AreEqual(
            "Tidak ada ikan dikumpulkan",
            GameManager.I.resultCorrectFishText.text,
            "Result text should show fallback message when no fish caught"
        );
        Assert.IsTrue(GameManager.I.TierEmpty.gameObject.activeSelf, "TierEmpty must be active");
        Assert.IsFalse(GameManager.I.TierLow.gameObject.activeSelf);
        Assert.IsFalse(GameManager.I.TierMid.gameObject.activeSelf);
        Assert.IsFalse(GameManager.I.TierHigh.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator ResultScreen_TwoCorrectHits_ShowsTierLow()
    {
        GameManager.I.StartGame();
        yield return null;

        for (int i = 0; i < 2; i++)
        {
            GameManager.I.OnFishHit(GameManager.I.targetColor);
            yield return null;
        }

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        Assert.IsTrue(GameManager.I.TierLow.gameObject.activeSelf, "2 correct → TierLow");
    }

    [UnityTest]
    public IEnumerator ResultScreen_FourCorrectHits_ShowsTierMid()
    {
        GameManager.I.StartGame();
        yield return null;

        for (int i = 0; i < 4; i++)
        {
            GameManager.I.OnFishHit(GameManager.I.targetColor);
            yield return null;
        }

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        Assert.IsTrue(GameManager.I.TierMid.gameObject.activeSelf, "4 correct → TierMid");
    }

    [UnityTest]
    public IEnumerator ResultScreen_FiveCorrectHits_ShowsTierHigh()
    {
        GameManager.I.StartGame();
        yield return null;

        for (int i = 0; i < 5; i++)
        {
            GameManager.I.OnFishHit(GameManager.I.targetColor);
            yield return null;
        }

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        Assert.IsTrue(GameManager.I.TierHigh.gameObject.activeSelf, "5 correct → TierHigh");
    }

    [UnityTest]
    public IEnumerator ResultScreen_CorrectHit_ShowsIndonesianColorName()
    {
        GameManager.I.StartGame();
        yield return null;

        GameManager.I.OnFishHit(Color.red);
        yield return null;

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        // resultCorrectFishText should contain "Merah", not a hex string
        string text = GameManager.I.resultCorrectFishText.text;
        Assert.IsTrue(text.Contains("Merah"), $"Expected 'Merah' in result text, got: '{text}'");
    }

    [UnityTest]
    public IEnumerator ResultScreen_ThreeColors_ListsAllIndonesianNames()
    {
        GameManager.I.StartGame();
        yield return null;

        // Hit red, green, blue targets in sequence
        foreach (Color c in new[] { Color.red, Color.green, Color.blue })
        {
            GameManager.I.targetColor = c;
            GameManager.I.OnFishHit(c);
            yield return null;
        }

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        string text = GameManager.I.resultCorrectFishText.text;
        Assert.IsTrue(text.Contains("Merah"),  $"Missing 'Merah' in: {text}");
        Assert.IsTrue(text.Contains("Hijau"),  $"Missing 'Hijau' in: {text}");
        Assert.IsTrue(text.Contains("Biru"),   $"Missing 'Biru' in: {text}");
    }

    // ── Full loop with restart ────────────────────────────────────────

    [UnityTest]
    public IEnumerator RestartAfterResult_ResetsAllState()
    {
        GameManager.I.StartGame();
        yield return null;

        GameManager.I.OnFishHit(GameManager.I.targetColor);
        yield return null;

        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);

        // First game ended
        Assert.IsFalse(GameManager.I.gameRunning);
        Assert.AreEqual(GameConstants.PointPerCorrectHit, GameManager.I.score);

        // Restart
        GameManager.I.StartGame();
        yield return null;

        Assert.IsTrue(GameManager.I.gameRunning);
        Assert.AreEqual(0, GameManager.I.score);
        Assert.AreEqual(0, GameManager.I.correctHitCount);
        Assert.IsFalse(GameManager.I.resultContainer.activeSelf);
    }

    // ── Physics hit detection ─────────────────────────────────────────

    // Tests that SpearHit.Update() detects a fish via Physics.OverlapSphere
    // and routes through FishHitBox → GameManager.OnFishHit correctly.
    // Fish placed far from the water spawn area to avoid accidental collisions.
    [UnityTest]
    public IEnumerator PhysicsHit_SpearOverlapsFish_TriggersCorrectScoring()
    {
        GameManager.I.StartGame();
        yield return null;

        const float FAR = 50f; // outside any spawn radius
        Color targetColor = GameManager.I.targetColor;
        int scoreBefore = GameManager.I.score;

        // Stub fish — SphereCollider radius 0.3 m, target color
        var fishGO = new GameObject("E2E_TestFish");
        fishGO.transform.position = new Vector3(FAR, FAR, FAR);
        fishGO.layer = 0; // Default layer
        var col = fishGO.AddComponent<SphereCollider>();
        col.radius = 0.3f;
        var fishTarget = fishGO.AddComponent<FishTarget>();
        fishTarget.fishColor = targetColor;
        fishGO.AddComponent<FishHitBox>();

        // Stub spear — placed exactly at fish position, hits all layers
        var spearGO = new GameObject("E2E_TestSpear");
        spearGO.transform.position = fishGO.transform.position;
        var spearHit = spearGO.AddComponent<SpearHit>();
        spearHit.hitRadius = 0.5f;
        spearHit.fishLayer = ~0; // all layers

        // One physics tick is enough for OverlapSphere to detect
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.AreEqual(
            scoreBefore + GameConstants.PointPerCorrectHit,
            GameManager.I.score,
            "SpearHit should have detected the stub fish and added correct-hit points"
        );

        Object.Destroy(spearGO);
    }

    [UnityTest]
    public IEnumerator PhysicsHit_WrongColorFish_SubtractsPoints()
    {
        GameManager.I.StartGame();
        yield return null;

        const float FAR = 60f;
        Color wrongColor = GameManager.I.targetColor == Color.red ? Color.green : Color.red;
        int scoreBefore = GameManager.I.score;

        var fishGO = new GameObject("E2E_WrongFish");
        fishGO.transform.position = new Vector3(FAR, FAR, FAR);
        fishGO.layer = 0;
        fishGO.AddComponent<SphereCollider>().radius = 0.3f;
        var fishTarget = fishGO.AddComponent<FishTarget>();
        fishTarget.fishColor = wrongColor;
        fishGO.AddComponent<FishHitBox>();

        var spearGO = new GameObject("E2E_WrongSpear");
        spearGO.transform.position = fishGO.transform.position;
        var spearHit = spearGO.AddComponent<SpearHit>();
        spearHit.hitRadius = 0.5f;
        spearHit.fishLayer = ~0;

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.AreEqual(
            scoreBefore - GameConstants.PenaltyPerWrongHit,
            GameManager.I.score,
            "Wrong-color fish should deduct penalty points"
        );

        Object.Destroy(spearGO);
    }
}
