// PlayMode tests for scene-dependent acceptance criteria from all iteration scopes.
//
// ═══════════════════════════════════════════════════════════════
// SCENE REQUIREMENT: Assets/Tests/PlayMode/TombakanTestScene.unity
//   (same scene used by GameManagerPlayModeTests.cs)
//   Requires: GameObject "GameManager" with GameManager.cs and lightweight UI stubs.
// ═══════════════════════════════════════════════════════════════

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class IterationPlayModeTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("TombakanTestScene", LoadSceneMode.Single);
        yield return null; // wait for Awake/Start
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 1 – TASK-04 · Numeric timer countdown text
    // AC: timerCountdownText is null-safe; no NullReferenceException when not wired.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator TimerCountdownText_NullSafe_NoExceptionDuringStartGame()
    {
        GameManager.I.timerCountdownText = null;
        Assert.DoesNotThrow(() => GameManager.I.StartGame(),
            "StartGame must not throw when timerCountdownText is null");
        yield return null;
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 5 – TASK-01 · PickNewTarget is a no-op when game is not running
    // AC: GameManager.PickNewTarget returns early when !gameRunning.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator PickNewTarget_BeforeStartGame_DoesNotIncrementCorrectHitCount()
    {
        // gameRunning is false before StartGame is called
        int hitsBefore = GameManager.I.correctHitCount;
        GameManager.I.PickNewTarget();
        yield return null;
        Assert.AreEqual(hitsBefore, GameManager.I.correctHitCount,
            "PickNewTarget must be a no-op when gameRunning is false (before StartGame)");
    }

    [UnityTest]
    public IEnumerator PickNewTarget_AfterEndGameManual_DoesNotIncrementCorrectHitCount()
    {
        GameManager.I.StartGame();
        yield return null;
        GameManager.I.EndGameManual(); // sets gameRunning = false
        yield return null;

        int hitsBefore = GameManager.I.correctHitCount;
        GameManager.I.PickNewTarget();
        yield return null;
        Assert.AreEqual(hitsBefore, GameManager.I.correctHitCount,
            "PickNewTarget must be a no-op after EndGame sets gameRunning to false");
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 5 / Week 6 – TASK-01 · OnFishHit returns when !gameRunning
    // AC: OnFishHit returns immediately when !gameRunning; score unchanged.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator OnFishHit_BeforeStartGame_ScoreUnchanged()
    {
        int scoreBefore = GameManager.I.score;
        // Colour.white is never a target (not in FishPalette), so wrong-hit path
        GameManager.I.OnFishHit(Color.white, "");
        yield return null;
        Assert.AreEqual(scoreBefore, GameManager.I.score,
            "OnFishHit must be a no-op when the game has not been started");
    }

    [UnityTest]
    public IEnumerator OnFishHit_AfterEndGameManual_ScoreUnchanged()
    {
        GameManager.I.StartGame();
        yield return null;
        GameManager.I.EndGameManual();
        yield return null;

        int scoreBefore = GameManager.I.score;
        GameManager.I.OnFishHit(Color.white, "");
        yield return null;
        Assert.AreEqual(scoreBefore, GameManager.I.score,
            "OnFishHit must be a no-op after EndGame");
    }

    [UnityTest]
    public IEnumerator OnFishHit_AfterEndGameManual_CorrectHitCountUnchanged()
    {
        GameManager.I.StartGame();
        yield return null;
        int correctBefore = GameManager.I.correctHitCount;
        GameManager.I.EndGameManual();
        yield return null;

        // Try every palette colour — none should register because gameRunning is false
        GameManager.I.OnFishHit(Color.red,   "");
        GameManager.I.OnFishHit(Color.green, "");
        GameManager.I.OnFishHit(Color.blue,  "");
        yield return null;

        Assert.AreEqual(correctBefore, GameManager.I.correctHitCount,
            "OnFishHit must not increment correctHitCount after EndGame");
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 1 – TASK-01 · Score never drops below 0 (integration)
    // AC: Hitting wrong fish when score is 0 must not produce a negative score.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator OnFishHit_WrongColor_WhenScoreIsZero_ScoreRemainsZero()
    {
        GameManager.I.StartGame();
        yield return null;
        GameManager.I.score = 0; // public field — reset to 0

        // Color.white is definitely not in the palette so it is always a wrong hit
        GameManager.I.OnFishHit(Color.white, "");
        yield return null;

        Assert.GreaterOrEqual(GameManager.I.score, 0,
            "Score must never drop below 0, even after multiple wrong hits at zero");
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 9 – TASK-03 · sadFeedback suppressed when score already at floor
    // AC: When score=0 and wrong fish hit, actualDeduction=0 → sadFeedback not shown.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator OnFishHit_WrongColor_AtZeroScore_SadFeedbackNotShown()
    {
        GameManager.I.StartGame();
        yield return null;
        GameManager.I.score = 0;

        // Ensure sadFeedback starts inactive
        GameManager.I.sadFeedback.SetActive(false);

        GameManager.I.OnFishHit(Color.white, ""); // always wrong (not in palette)
        yield return null;

        Assert.IsFalse(GameManager.I.sadFeedback.activeSelf,
            "sadFeedback must NOT be activated when deduction is 0 " +
            "(score already at floor → ShowSad early-returns on deduction <= 0)");
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 2 – TASK-03 · bestScoreText null-safe
    // AC: No exception when bestScoreText is not wired.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator EndGame_BestScoreText_NullSafe()
    {
        GameManager.I.bestScoreText       = null;
        GameManager.I.resultBestScoreText = null;
        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);
        GameManager.I.StartGame();
        yield return null;
        Assert.DoesNotThrow(() => GameManager.I.EndGameManual(),
            "EndGame must not throw when bestScoreText/resultBestScoreText are null");
        yield return null;
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 2 – TASK-03 · newRecordBadge null-safe
    // AC: No exception when newRecordBadge is not wired.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator EndGame_NewRecordBadge_NullSafe()
    {
        GameManager.I.newRecordBadge = null;
        PlayerPrefs.DeleteKey(ScoreStore.BestScoreKey);
        GameManager.I.StartGame();
        yield return null;
        Assert.DoesNotThrow(() => GameManager.I.EndGameManual(),
            "EndGame must not throw when newRecordBadge is null");
        yield return null;
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 2 – TASK-02 · FishSpawner.ClearAll null-safe in EndGame
    // AC: EndGame must not throw when fishSpawner is null.
    //     fishSpawner is nulled AFTER StartGame so PickNewTarget can still run.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator EndGame_FishSpawner_NullSafe()
    {
        // Start the game first so PickNewTarget can use the valid spawner reference
        GameManager.I.StartGame();
        yield return null;

        // Null the spawner after the game is running — EndGame must handle this gracefully
        var savedSpawner = GameManager.I.fishSpawner;
        GameManager.I.fishSpawner = null;

        Assert.DoesNotThrow(() => GameManager.I.EndGameManual(),
            "EndGame must not throw a NullReferenceException when fishSpawner is null");
        yield return null;
        GameManager.I.fishSpawner = savedSpawner;
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 3 – TASK-01 · targetColorLabel null-safe
    // AC: No NullReferenceException when targetColorLabel is not wired.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator StartGame_TargetColorLabel_NullSafe()
    {
        GameManager.I.targetColorLabel = null;
        Assert.DoesNotThrow(() => GameManager.I.StartGame(),
            "StartGame (which calls PickNewTarget) must not throw when targetColorLabel is null");
        yield return null;
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 1 – TASK-05 · TierLegend null-safe fallback
    // AC: If TierLegend is not wired, EndGame falls back to TierHigh gracefully.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator EndGame_TierLegend_NullSafe_FallsBackToTierHigh()
    {
        GameManager.I.TierLegend = null;
        // StartGame resets correctHitCount to 0, so set it afterwards
        GameManager.I.StartGame();
        yield return null;
        GameManager.I.correctHitCount = 20; // ensures Legend-tier path (15+)
        Assert.DoesNotThrow(() => GameManager.I.EndGameManual(),
            "EndGame must fall back to TierHigh without exception when TierLegend is null");
        yield return null;
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 6 – TASK-02 · achievementToastPanel null-safe
    // AC: EndGame must not throw when achievementToastPanel / Text are null.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator EndGame_AchievementToastPanelAndText_NullSafe()
    {
        GameManager.I.achievementToastPanel = null;
        GameManager.I.achievementToastText  = null;
        GameManager.I.StartGame();
        yield return null;
        Assert.DoesNotThrow(() => GameManager.I.EndGameManual(),
            "EndGame must not throw a NullReferenceException when achievementToastPanel/Text are null");
        yield return null;
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 5 – TASK-02 · PlaceWaterOnPlane disables itself after first placement
    // AC: Verified via component API (requires PlaceWaterOnPlane in scene).
    //     This test is a documentation test — the behaviour is AR-device-only.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator PlaceWaterOnPlane_ComponentExists_IsAMonoBehaviour()
    {
        // Structural test: confirm PlaceWaterOnPlane is a MonoBehaviour (not static class)
        // so that `enabled = false` (the one-shot guard) is a valid operation.
        var go   = new GameObject("WaterPlane");
        var comp = go.AddComponent<PlaceWaterOnPlane>();
        yield return null;
        Assert.IsNotNull(comp, "PlaceWaterOnPlane must be a MonoBehaviour");
        Assert.IsTrue(comp.enabled, "PlaceWaterOnPlane starts enabled");
        comp.enabled = false;
        Assert.IsFalse(comp.enabled,
            "PlaceWaterOnPlane.enabled can be set to false (one-shot placement guard)");
        Object.Destroy(go);
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 9 – TASK-01 · GoalManager.IsCoachingActive false before StartCoaching
    // AC: ForceCompleteGoal must never be called before m_OnboardingGoals is populated.
    //     TombakanOnboarding guards this via IsCoachingActive.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator GoalManager_IsCoachingActive_FalseBeforeStartCoaching()
    {
        var go = new GameObject("GoalManagerTest");
        var gm = go.AddComponent<GoalManager>();
        yield return null; // let Start (if any) run — GoalManager has no Start

        Assert.IsFalse(gm.IsCoachingActive,
            "GoalManager.IsCoachingActive must be false before StartCoaching() is called; " +
            "this guards TombakanOnboarding from calling ForceCompleteGoal on a null queue");

        Object.Destroy(go);
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 9 – TASK-03 · greetingPanel and dailyBonusPanel never simultaneously active
    // AC: During new-player first launch, dailyBonusPanel must be deferred
    //     while greetingPanel is visible; they must never both be active.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator TombakanOnboarding_Start_GreetingAndDailyBonus_NeverSimultaneouslyActive()
    {
        // Arrange: fresh player state — no score, no XP, no last-played date
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        var go         = new GameObject("Onboarding");
        var onboarding = go.AddComponent<TombakanOnboarding>();
        onboarding.skipOnboardingForReturning = false;

        var greetingGO   = new GameObject("GreetingPanel");
        var dailyBonusGO = new GameObject("DailyBonusPanel");
        greetingGO.SetActive(false);
        dailyBonusGO.SetActive(false);
        onboarding.greetingPanel   = greetingGO;
        onboarding.dailyBonusPanel = dailyBonusGO;

        // Act: yield one frame — Unity calls Start() on the new MonoBehaviour.
        // Expected flow:
        //   1. isReturningPlayer = false → greetingPanel.SetActive(true)
        //   2. TryClaimDailyBonus → succeeds (no last-played) → greetingVisible=true
        //   3. _pendingBonus = true, dailyBonusPanel stays inactive
        yield return null;

        bool bothActive = greetingGO.activeSelf && dailyBonusGO.activeSelf;
        Assert.IsFalse(bothActive,
            "greetingPanel and dailyBonusPanel must never be active simultaneously; " +
            "the daily bonus must be deferred while the greeting is shown");
        Assert.IsFalse(dailyBonusGO.activeSelf,
            "dailyBonusPanel must remain inactive while greetingPanel is visible");

        Object.Destroy(go);
        Object.Destroy(greetingGO);
        Object.Destroy(dailyBonusGO);
    }

    [UnityTest]
    public IEnumerator TombakanOnboarding_AfterDismissGreeting_PanelsMutuallyExclusive()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        var go         = new GameObject("Onboarding2");
        var onboarding = go.AddComponent<TombakanOnboarding>();
        onboarding.skipOnboardingForReturning = false;

        var greetingGO   = new GameObject("GreetingPanel2");
        var dailyBonusGO = new GameObject("DailyBonusPanel2");
        greetingGO.SetActive(false);
        dailyBonusGO.SetActive(false);
        onboarding.greetingPanel   = greetingGO;
        onboarding.dailyBonusPanel = dailyBonusGO;

        yield return null; // Start() runs; greetingPanel shown, dailyBonus deferred

        onboarding.DismissGreeting(); // should hide greeting, show deferred daily bonus
        yield return null;

        // Key invariant: they must not be simultaneously active
        bool bothActive = greetingGO.activeSelf && dailyBonusGO.activeSelf;
        Assert.IsFalse(bothActive,
            "greetingPanel and dailyBonusPanel must not both be active after DismissGreeting");
        Assert.IsFalse(greetingGO.activeSelf,
            "greetingPanel must be hidden after DismissGreeting()");

        Object.Destroy(go);
        Object.Destroy(greetingGO);
        Object.Destroy(dailyBonusGO);
    }

    // ══════════════════════════════════════════════════════════════════
    // Week 9 – TASK-02 (c) · ApplyLevelReward public overload — null-safe
    // AC: GameManager.ApplyLevelReward(int level) with level=0 must be a no-op.
    // ══════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator ApplyLevelReward_ZeroLevel_IsNoOp()
    {
        // With level=0, the public overload must early-return without touching levelUpPanel
        GameManager.I.levelUpPanel = null; // null-safe
        Assert.DoesNotThrow(() => GameManager.I.ApplyLevelReward(0),
            "ApplyLevelReward(0) must be a no-op — it must not throw or activate the level-up panel");
        yield return null;
    }

    [UnityTest]
    public IEnumerator ApplyLevelReward_ValidLevel_DoesNotThrow_WhenLevelUpPanelNull()
    {
        GameManager.I.levelUpPanel = null;
        GameManager.I.levelUpText  = null;
        Assert.DoesNotThrow(() => GameManager.I.ApplyLevelReward(2),
            "ApplyLevelReward(2) must not throw when levelUpPanel and levelUpText are null");
        yield return null;
    }
}
