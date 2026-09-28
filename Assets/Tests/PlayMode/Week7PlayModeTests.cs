// Week 7 PlayMode tests — T1 (BUG-1, BUG-4), T2 (BUG-2, BUG-3, UX-5)
//
// Prerequisites:
//   TombakanTestScene.unity must exist in Assets/Tests/PlayMode/.
//   Generate / regenerate via: Tombakan → Create Test Scene
//
// AC coverage:
//   BUG-1 (T1): CancelInvoke guard prevents accumulated PickNewTarget invocations
//               on rapid consecutive OnFishHit calls.
//   BUG-4 (T1): LockRoutine nulls leash.spearTip before its first yield point,
//               so the rope is hidden immediately when a hit is registered.
//   BUG-2 (T2): targetColorLabel.text is re-set to ToIndonesian(targetColor) after
//               the species-catalog override block in PickNewTarget (standard and
//               catalog modes).
//   BUG-3 (T2): ShowDailyBonus calls GameManager.I.ApplyLevelReward(newLevel) when
//               a daily XP grant triggers a level-up.
//   UX-5  (T2): After a correct hit, targetColorLabel shows "—" and
//               targetColorImage becomes Color.grey to prevent stale-label
//               accidental wrong hits.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class Week7PlayModeTests
{
    // ── Scene setup ───────────────────────────────────────────────────────────

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("TombakanTestScene", LoadSceneMode.Single);
        yield return null;  // wait for scene Awake/Start

        // Wipe any PlayerPrefs written by previous tests so store state is clean.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // TombakanTestScene intentionally leaves some optional GameManager UI
        // references null (they are not needed for most scoring tests).
        // Week 7 label / UX-5 tests require targetColorLabel to be non-null.
        EnsureTargetColorLabel();
    }

    // TombakanTestScene has no targetColorLabel stub; create one at runtime so
    // UX-5 and BUG-2 tests can read the text value.  TextMeshPro (world-space)
    // works without a Canvas parent, which is sufficient for read/write testing.
    static void EnsureTargetColorLabel()
    {
        if (GameManager.I == null || GameManager.I.targetColorLabel != null) return;

        var go = new GameObject("TargetColorLabel_Week7Stub");
        GameManager.I.targetColorLabel = go.AddComponent<TextMeshPro>();
    }

    // ── T2 UX-5: Stale label cleared after correct hit ────────────────────────
    // AC: "after a correct hit's ShowHappy() call, targetColorLabel.text is set
    //      to '—' and targetColorImage.color is set to Color.grey."

    [UnityTest]
    public IEnumerator OnFishHit_CorrectHit_SetsTargetColorLabelToDash()
    {
        GameManager.I.StartGame();
        yield return null;

        Color target = GameManager.I.targetColorImage.color;
        GameManager.I.OnFishHit(target, "");
        yield return null;

        Assert.AreEqual("—", GameManager.I.targetColorLabel.text,
            "After a correct hit, targetColorLabel must show '—' to blank the " +
            "stale target and prevent accidental wrong-hit penalties (UX-5).");
    }

    [UnityTest]
    public IEnumerator OnFishHit_CorrectHit_SetsTargetColorImageToGrey()
    {
        GameManager.I.StartGame();
        yield return null;

        Color target = GameManager.I.targetColorImage.color;
        GameManager.I.OnFishHit(target, "");
        yield return null;

        Assert.AreEqual(Color.grey, GameManager.I.targetColorImage.color,
            "After a correct hit, targetColorImage must become Color.grey to " +
            "visually blank the target indicator (UX-5).");
    }

    // ── T2 BUG-2 (no catalog): label matches image color after PickNewTarget ──
    // AC: "targetColorLabel.text is re-set to ToIndonesian(targetColor) after the
    //      species catalog override block."

    [UnityTest]
    public IEnumerator PickNewTarget_NoCatalog_LabelMatchesImageColor()
    {
        // No catalog is assigned in TombakanTestScene — standard colour-only mode.
        GameManager.I.StartGame();
        yield return null;

        Color resolvedColor = GameManager.I.targetColorImage.color;
        string expected     = ColorHexLocalization.ToIndonesian(resolvedColor);

        Assert.AreEqual(expected, GameManager.I.targetColorLabel.text,
            "targetColorLabel must always reflect ToIndonesian(targetColorImage.color) " +
            "after PickNewTarget completes — label and swatch must never diverge (BUG-2).");
    }

    // ── T2 BUG-2 (catalog): label matches species base color ─────────────────
    // When a FishCatalog is active, PickNewTarget overrides targetColor with the
    // species base color.  The label must be re-set AFTER that override.

    [UnityTest]
    public IEnumerator PickNewTarget_WithCatalog_LabelMatchesSpeciesBaseColor()
    {
        // Build a one-entry catalog so PickRandom always returns this species.
        var species = ScriptableObject.CreateInstance<FishSpecies>();
        species.id          = "test_red_species";
        species.displayName = "Ikan Uji Merah";
        species.baseColor   = Color.red;
        species.rarity      = FishRarity.Common;

        var catalog = ScriptableObject.CreateInstance<FishCatalog>();
        catalog.species = new List<FishSpecies> { species };

        GameManager.I.fishSpawner.catalog = catalog;
        GameManager.I.StartGame();
        yield return null;

        string expectedLabel = ColorHexLocalization.ToIndonesian(Color.red); // "Merah"

        Assert.AreEqual(Color.red, GameManager.I.targetColorImage.color,
            "targetColorImage must reflect the species base color (red) when catalog is active (BUG-2).");
        Assert.AreEqual(expectedLabel, GameManager.I.targetColorLabel.text,
            "targetColorLabel must read 'Merah' (ToIndonesian of species.baseColor=red) " +
            "after the catalog override in PickNewTarget — not the pre-resolution colour (BUG-2).");

        // Cleanup
        GameManager.I.fishSpawner.catalog = null;
        Object.Destroy(catalog);
        Object.Destroy(species);
    }

    // ── T1 BUG-4: LockRoutine nulls leash.spearTip before first yield ─────────
    // AC: "in SpearThrower.LockRoutine, 'if (leash) leash.spearTip = null;' is
    //      the first statement after 'canThrow = false'."
    //
    // StartCoroutine runs the coroutine synchronously to its first yield point.
    // Therefore leash.spearTip must be null immediately after LockThrow returns —
    // no frame yield is required before the assertion.

    [UnityTest]
    public IEnumerator SpearThrower_LockThrow_ImmediatelyNullsLeashSpearTip()
    {
        // Create a minimal SpearThrower in the current scene.
        var spearGO  = new GameObject("SpearThrower_BUG4Test");
        var thrower  = spearGO.AddComponent<SpearThrower>();

        // SpearLeash requires a LineRenderer (declared via [RequireComponent]).
        var leashGO  = new GameObject("SpearLeash_BUG4Test");
        leashGO.AddComponent<LineRenderer>();
        var leash    = leashGO.AddComponent<SpearLeash>();

        // Simulate an in-flight spear by assigning a non-null spearTip.
        var tipGO       = new GameObject("SpearTip_BUG4Test");
        leash.spearTip  = tipGO.transform;

        // SpearThrower.leash is private (set in Awake via FindObjectOfType).
        // Inject it via reflection so the test controls the exact leash reference.
        typeof(SpearThrower)
            .GetField("leash", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(thrower, leash);

        // Act — LockRoutine runs to its first yield synchronously.
        thrower.LockThrow(delay: 10f);

        // Assert — no frame yield needed; null-out must occur before WaitForSeconds.
        Assert.IsNull(leash.spearTip,
            "LockRoutine must null leash.spearTip before yielding on WaitForSeconds, " +
            "so the rope LineRenderer is hidden the instant the hit registers (BUG-4).");

        Object.Destroy(tipGO);
        Object.Destroy(leashGO);
        Object.Destroy(spearGO);
        yield return null;  // PlayMode [UnityTest] must have at least one yield.
    }

    // ── T1 BUG-1: CancelInvoke prevents double-spawn on rapid hits ────────────
    // AC: "CancelInvoke(nameof(PickNewTarget)) is called immediately before every
    //      Invoke(nameof(PickNewTarget), ...) in GameManager.OnFishHit."
    //
    // Without the guard, three rapid calls queue three concurrent invocations;
    // each fires independently and clears + re-spawns fish, leaving the game in
    // an inconsistent state.  With the guard, each call cancels any pending invoke
    // and issues exactly one new one — only the last survives.

    [UnityTest]
    public IEnumerator OnFishHit_RapidHits_OnlyOnePickNewTargetFires()
    {
        GameManager.I.StartGame();
        yield return null;

        Color target = GameManager.I.targetColorImage.color;
        // Ensure the "wrong" colour differs from target.
        Color wrong  = (target == Color.red) ? Color.green : Color.red;

        // Three rapid hits in the same logical frame; each OnFishHit schedules a
        // PickNewTarget invoke while the next cancels the previous.
        GameManager.I.OnFishHit(target, "");   // correct  — correct count = 1
        GameManager.I.OnFishHit(wrong,  "");   // wrong    — correct count = 1
        GameManager.I.OnFishHit(target, "");   // correct  — correct count = 2

        // Wait long enough for all three hypothetical invocations to have fired
        // (2× the maximum single-invoke delay gives comfortable headroom).
        float maxSingleDelay = GameManager.I.hitDelay + 0.8f;
        yield return new WaitForSeconds(maxSingleDelay * 2f + 0.5f);

        // The result screen must not have appeared — the game is still valid.
        Assert.IsFalse(GameManager.I.resultContainer.activeSelf,
            "Result screen must not show after rapid hits; game must remain running (BUG-1).");

        // Exactly fishCount fish must exist — one active spawn batch, not zero or multiple.
        var fish = Object.FindObjectsByType<FishTarget>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Assert.AreEqual(GameManager.I.fishSpawner.fishCount, fish.Length,
            "After rapid hits, exactly one fish batch must exist in the scene. " +
            "Accumulated invocations would leave zero fish between clear/re-spawn " +
            "cycles and may result in an incorrect final count (BUG-1).");

        // correctHitCount must exactly equal the two correct hits called.
        Assert.AreEqual(2, GameManager.I.correctHitCount,
            "correctHitCount must be 2 — reflecting the two correct OnFishHit calls " +
            "made, no more (BUG-1 game-state sanity).");
    }

    // ── T2 BUG-3: Daily bonus level-up triggers ApplyLevelReward ─────────────
    // AC: "In TombakanOnboarding.ShowDailyBonus, when newLevel > 0,
    //      GameManager.I.ApplyLevelReward(newLevel) is called."
    //
    // Setup: XP = 90 (level 1).  Daily bonus adds 125 XP (streak=1: 100 base + 25),
    // pushing total to 215 → level 2 (threshold: 100 XP).  A level-2 reward with
    // softCurrencyBonus=50 is configured on the LevelRewardTable.  After Start(),
    // CurrencyStore must reflect the bonus coins from ApplyLevelReward.

    [UnityTest]
    public IEnumerator TombakanOnboarding_DailyBonusLevelUp_CoinsAreAwardedViaApplyLevelReward()
    {
        const int startXp            = 90;   // level 1; needs 100 XP to reach level 2
        const int rewardLevel        = 2;
        const int softCurrencyBonus  = 50;

        // ── PlayerPrefs: simulate a returning player whose last session was yesterday ──
        string yesterday = System.DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

        PlayerPrefs.SetInt   ("tombakan_total_xp",          startXp);
        PlayerPrefs.SetString("tombakan_last_played_date",  yesterday);
        PlayerPrefs.SetString("tombakan_streak_date",       yesterday); // consecutive → streak+1
        PlayerPrefs.SetInt   ("tombakan_streak",            0);
        PlayerPrefs.SetInt   ("tombakan_coins",             0);
        PlayerPrefs.Save();

        // ── Wire a LevelRewardTable entry for level 2 onto the existing GameManager ──
        var table = ScriptableObject.CreateInstance<LevelRewardTable>();
        table.rewards = new List<LevelReward>
        {
            new LevelReward { level = rewardLevel, softCurrencyBonus = softCurrencyBonus }
        };
        GameManager.I.levelRewardTable = table;

        // ── Create TombakanOnboarding — Start() fires on the next frame ───────────
        var panelGO      = new GameObject("DailyBonusPanel_BUG3Test");
        var onboardingGO = new GameObject("TombakanOnboarding_BUG3Test");
        var onboarding   = onboardingGO.AddComponent<TombakanOnboarding>();

        // dailyBonusPanel must be non-null so ShowDailyBonus does not return early.
        onboarding.dailyBonusPanel          = panelGO;
        // Prevent goalManager null-call: isReturningPlayer=true, but skip flag=false
        // means neither ForceCompleteGoal nor greetingPanel branch executes.
        onboarding.skipOnboardingForReturning = false;

        yield return null;  // Start() executes here

        // ── Assert: softCurrencyBonus was credited via ApplyLevelReward ──────────
        Assert.GreaterOrEqual(
            CurrencyStore.GetCoins(), softCurrencyBonus,
            $"Level-{rewardLevel} reward (softCurrencyBonus={softCurrencyBonus}) must be " +
            "applied when ShowDailyBonus receives newLevel > 0 — the daily XP grant that " +
            "crosses the level threshold must trigger ApplyLevelReward (BUG-3).");

        // Cleanup
        GameManager.I.levelRewardTable = null;
        Object.Destroy(panelGO);
        Object.Destroy(onboardingGO);
        Object.Destroy(table);
    }
}
