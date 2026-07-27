# Tester Report — Week 8

**Date:** 2026-07-27
**Scope:** Full trace of all seven core scripts plus carry-forward audit of Week 7 open issues.
**Sessions simulated:** 10 (first-timer, casual × 3, frustrated × 2, speed-runner × 2, colour-blind, veteran)

---

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `GoalManager.ForceCompleteGoal()` calls `CompleteGoal()` which dereferences `m_OnboardingGoals` — null for all returning players who bypass `StartCoaching()` — guaranteed NullReferenceException on every returning-player launch | `GoalManager.cs:197`, `TombakanOnboarding.cs:30` | CRITICAL: every player after their first session crashes on app open |
| 2 | `AchievementChecker.cs:100` and `DailyChallenge.cs:58` both call `ProgressionStore.AddXp()` and discard the return value; level-ups triggered through either path never call `ApplyLevelReward()`, silently dropping coin bonuses, species unlocks, and spear skin unlocks | `AchievementChecker.cs:100`, `DailyChallenge.cs:58` | HIGH: any player near a level boundary who earns a daily bonus or unlocks a first-time achievement loses their reward permanently |
| 3 | `ColourBlindSettings.ShapeForColor` only handles four exact hex values (`FF0000`, `00FF00`, `0000FF`, `FFFF00`); any `FishSpecies.baseColor` outside this set returns `"?"` — with 8 species in the starter catalog, most species trigger the fallback | `ColourBlindSettings.cs:22-27` | HIGH: accessibility mode is broken for the entire catalog-active game; colour-blind players see `"?"` on the majority of fish |
| 4 | `GameManager.PickNewTarget()` sets `targetColorLabel.text` from the FishPalette colour before `SpawnFish()` resolves the actual species colour, then updates `targetColorImage.color` to `CurrentTargetSpecies.baseColor` but never updates `targetColorLabel.text`; label and image show different colours | `GameManager.cs:404-421` | MEDIUM: every catalog-mode round shows a mismatched color name vs. color swatch on the target HUD |
| 5 | `ShowSad()` always displays `"-{penaltyPerWrongHit}!"` (i.e., `"-25!"`) regardless of whether `ClampScore` absorbed the penalty; when score is already 0, the player sees a deduction notification for a deduction that had no effect | `GameManager.cs:473-477`, `GameManager.cs:507` | MEDIUM: misleading negative feedback on every wrong hit once a player reaches score 0 |

---

## Session Simulations

### Session 1 — First-timer (Rina, no AR experience, fresh install)

**Profile:** `ScoreStore.GetBest() == 0`, `ProgressionStore.GetTotalXp() == 0`, first calendar day.

**Flow trace:**

1. App launches. `GameManager.Start()` → `AudioManager.I.PlayMainBGM()`. HUD shows `Lv 1`, XP bar empty.
2. `TombakanOnboarding.Start()`: `isReturningPlayer = false` → `greetingPanel.SetActive(true)`. `DailyChallenge.TryClaimDailyBonus` → first-ever call, no `LastPlayedKey` → awards 100 XP (streak=1). `ShowDailyBonus(100, 1, 0)`.
3. **Simultaneous panels:** `greetingPanel` and `dailyBonusPanel` are both active at the same time. No ordering or mutual exclusion. On a small phone screen both panels overlap. *(UX-NEW-2, carried from Week 7 — still open.)*
4. Player dismisses greeting → `DismissGreeting()` → `goalManager.StartCoaching()`. Coaching steps shown (FindSurfaces → TapSurface → Hints → Scale).
5. Player scans floor. `PlaceWaterOnPlane.Update()` fires on touch → water positioned, `enabled = false` (one-shot guard).
6. Player presses Start. `GameManager.StartGame()`: `score = 0`, `timeLeft = 60`, `gameRunning = true`. `PickNewTarget()` called.
7. `FishPalette.ActiveOptions(3)` → `[Red, Green, Blue]`. Target randomly chosen. `targetColorLabel.text = "Merah"`. Fish spawn.
8. Player sees fish and colour swatch. **No instruction anywhere explains the throw mechanic.** *(UX-1, carried from Week 6 — still open.)*
9. Player taps fish directly — `PlaceWaterOnPlane` disabled, `SpearHit` is on the spear projectile not the screen, throw button (if wired) is not obvious. First session ends in confusion.

**New observation — session 1:** Daily bonus auto-claims before the greeting is dismissed. The player is awarded 100 XP and the level bar fills before they understand the game. If they never start a game session, the XP is already consumed. No issue with correctness, but it creates an odd "you already progressed before playing" feeling.

---

### Session 2 — Returning casual player (first session of the day, Level 3)

**Profile:** `ScoreStore.GetBest() = 800`, `ProgressionStore.GetTotalXp() = 315` (Level 3, 15 XP below Level 4 threshold of 330).

**Flow trace:**

1. `TombakanOnboarding.Start()`: `isReturningPlayer = true` (GetBest > 0) → **`goalManager.ForceCompleteGoal()` called.**
2. **BUG trace — GoalManager NRE (Rank 1):**
   - `ForceCompleteGoal()` → `CompleteGoal()`.
   - `CompleteGoal()` line 197 (based on read): `m_CurrentGoal.Completed = true; m_CurrentGoalIndex++ (= 1)`.
   - Line: `if (m_OnboardingGoals.Count > 0)` — `m_OnboardingGoals` is `null` (never initialised; `StartCoaching()` only runs for new players via `DismissGreeting()`).
   - **NullReferenceException thrown. App crashes or exception propagates. Returning player launch fails.**
   - Even if Unity silently swallows the exception: `m_AllGoalsFinished = true` is set inside the `else` branch, meaning the step list's `SetActive(false)` call on line 205 is skipped; coaching UI panels may remain permanently visible.
3. Daily bonus check (if crash doesn't prevent it): `DailyChallenge.TryClaimDailyBonus(out xp, out streak)` → xp=125 (streak=3). Inside `TryClaimDailyBonus`: `ProgressionStore.AddXp(125)` — total XP = 440, crosses Level 4 threshold (330 XP). **Level-up to Level 4. Return value discarded (line 58).** `TombakanOnboarding.Start:40`: `levelAfter = GetLevel() = 4`. `newLevel = 4`. `ShowDailyBonus(125, 3, 4)` — panel shows level-up text. **But `ApplyLevelReward(levelRewardTable.GetRewardForLevel(4))` is never called — any coin bonus, species unlock, or skin unlock for Level 4 is silently skipped.**

---

### Session 3 — Casual player (mid-game, mute toggled, colour-blind mode on)

**Profile:** Level 5, catalog active, `ColourBlindSettings.IsEnabled() = true`. FishCatalog has 8 species, most with non-palette colors.

**Flow trace:**

1. Game starts. `PickNewTarget()` → catalog active → `FishSpawner.SpawnFish(paletteColor, 3)` → `targetSpecies = catalog.PickRandom()` → returns e.g. `IkanBadut` with `baseColor = new Color(1f, 0.5f, 0f)` (orange).
2. **BUG trace — targetColorLabel mismatch (Rank 4):**
   - Before `SpawnFish()`: `targetColor = Color.red` (from FishPalette). `targetColorLabel.text = "Merah"`. `targetColorImage.color = Color.red`.
   - After `SpawnFish()`: `CurrentTargetSpecies = IkanBadut`. `targetColor = new Color(1, 0.5, 0)`. `targetColorImage.color = new Color(1, 0.5, 0)` (orange).
   - `targetColorLabel.text` is never updated. Player sees orange swatch + species name "Ikan Badut" + **color label still reading "Merah"**. The label contradicts the image every round in catalog mode.
3. Fish spawn. `FishShapeOverlay.Start()` reads `fishTarget.fishColor` (set by SpawnFish before Start fires). For `IkanBadut` (orange): `ColourBlindSettings.ShapeForColor(orange)` → hex = `"FF8000"` → no match → returns `"?"`.
4. **BUG trace — ColourBlind "?" (Rank 3):** Every fish shape overlay shows `"?"` instead of a meaningful symbol. The four palette symbols (●▲■★) are never reached because catalog species use free-form colors. Colour-blind player has no way to identify fish other than color, defeating the accessibility feature entirely.
5. Player toggles colour-blind mode mid-game via `ColourBlindToggleUI.OnClick()`. `FindObjectsOfType<FishShapeOverlay>()` broadcasts to active fish. Overlays re-evaluate — same result, still `"?"`.

---

### Session 4 — Frustrated player (hitting zero score repeatedly)

**Profile:** Level 2, consistently hitting wrong fish. Score reaches 0 after first wrong hit.

**Flow trace:**

1. Round 1: correct hit. `score = 100`.
2. Round 2: wrong hit. `score = ClampScore(100 - 25) = 75`. `ShowSad()` → `sadFeedbackText.text = "-25!"`. Correct.
3. Round 3: wrong hit. `score = ClampScore(75 - 25) = 50`. `"-25!"`. Correct.
4. Round 4: wrong hit. `score = ClampScore(50 - 25) = 25`. `"-25!"`. Correct.
5. Round 5: wrong hit. `score = ClampScore(25 - 25) = 0`. `"-25!"`. Correct.
6. Round 6: wrong hit. `score = ClampScore(0 - 25) = ClampScore(-25) = 0`. **`ShowSad()` → `"-25!"` displayed. Score stays 0.** Player sees penalty notification but score does not change. *(Rank 5 bug)*
7. Rounds 7-10: same — `"-25!"` every hit, score never moves. Player is confused by a repeated deduction that has no effect.

**Fix path:** In `ShowSad()`, check if the penalty was absorbed: `int actual = score - ClampScore(score - penaltyPerWrongHit); sadFeedbackText.text = actual > 0 ? $"-{actual}!" : "Meleset!";`

---

### Session 5 — Rapid-fire speed-runner (testing throw timing)

**Profile:** Level 6. Throws immediately when spear re-appears. Consecutive hits within 1.1s window.

**Flow trace:**

1. Hit at t=0. `OnFishHit()` → `LockThrow(1.0)` → `spearThrower.StopAllCoroutines()` + `LockRoutine(1.0)`.
2. `Invoke(PickNewTarget, 1.0 + 0.8 = 1.8)`. `ShowHappy(...)` → `Invoke(HideFeedback, 1.0)`.
3. At t=1.0: `LockRoutine` ends → `canThrow = true`. `spearFake.SetActive(true)`.
4. **HideFeedback Invoke fires at t=1.0 exactly** (from step 2).
5. At t=1.8: new fish spawn. Player throws at t=1.9 and hits again. `ShowHappy(...)` → `SetActive(true)` for happyFeedback. `Invoke(HideFeedback, 1.0)`.
6. **Second HideFeedback Invoke** added to the queue (from step 5). The one from step 2 already fired; this is a new one at t+1.0 from hit 2.
7. However, if the player hits at t=1.05 (just 50ms after lock expires and 0.75s before fish spawn), `ShowHappy` fires. Then at t=1.0, the earlier `HideFeedback` was supposed to fire at t=0+1.0=1.0 — it fires immediately and hides the new feedback that just appeared at t=1.05. The player sees feedback for ~0ms before it's hidden. *(New bug W8-2)*

**Critical observation:** `Invoke` with the same method name in Unity does NOT cancel previous calls — they accumulate. Using `CancelInvoke(nameof(HideFeedback))` at the top of `ShowHappy()` and `ShowSad()` would fix this. As-is, the feedback timing is unreliable when hits occur near the 1.0s pacing floor.

---

### Session 6 — Speed-runner (large catalog, PickOther degenerate case)

**Profile:** FishCatalog contains 2 species: `IkanKoi` (Common, weight 6) and `IkanArwana` (Rare, weight 1).

**Flow trace:**

1. `SpawnFish()`: `targetSpecies = IkanArwana` (Rare, picked). `correctIndex = 2` (e.g., fish 2 is target).
2. For fish 0, 1, 3, 4: `catalog.PickOther(IkanArwana)`. `PickOther` random rolls: total weight = 7. P(IkanArwana) = 1/7 ≈ 14%. P(IkanKoi) = 6/7 ≈ 86%.
3. After 20 attempts, if all 20 rolls happen to pick IkanArwana (probability ~(1/7)^20, negligible), the fallback at line 43 returns `picked = IkanArwana` — same as `exclude`. Probability is negligible in practice.
4. **However:** If `targetSpecies = IkanKoi` (Common, weight 6), and catalog has only 2 species, `PickOther(IkanKoi)` must pick `IkanArwana` (Rare). P(IkanArwana) = 1/7. Expected picks before first success = 7. After 20 attempts, P(failure) = (6/7)^20 ≈ 4.7%. **With a 4.7% probability per decoy fish and 4 decoy fish per round**, the chance of at least one decoy matching the target color is 1 - (1 - 0.047)^4 ≈ 18% per round. These duplicate decoys look identical to the target, making the round unsolvable or guesswork.
5. The fix: replace the retry loop with a deterministic pick from a filtered list (all species except `exclude`).

---

### Session 7 — Veteran player (achievement milestone hit)

**Profile:** Level 4, XP = 790 (10 XP below Level 5 threshold). Game earns 15 correct hits, maxComboStreak = 6.

**Flow trace:**

1. `EndGame()`: `xpEarned = ProgressionRules.XpForResult(15, 88, 1, 6)` = 150 + 2 + 88 + 50 = 290. `ProgressionStore.AddXp(290)` → total XP = 1080. Level 5 threshold = 800 XP → still Level 4? Let me recalculate: `XpForLevel(5) = round(100 × 4^1.5) = round(100 × 8) = 800`. 790 + 290 = 1080 > 800. `newLevel = 5`. Returned by `AddXp`.
2. `StaggerResultCelebrations(isRecord, 5)`: badge at +0.4s, `ShowLevelUp(5)` → `levelUpPanel` shown at +0.8s with "Level 5! Selamat!". `ApplyLevelReward(levelRewardTable.GetRewardForLevel(5))` called. **This path is CORRECT — level reward IS applied in EndGame.**
3. `ShowAchievementsSequenced` starts at +1.2s. `AchievementChecker.CheckAll` detected `Combo5` (streak=6 >= 5). Achievement XP: say 30 XP. `ProgressionStore.AddXp(30)` at `AchievementChecker.cs:100`. Total XP = 1110. Level 5 threshold = 800, Level 6 threshold = `round(100 × 5^1.5)` = 1118. 1110 < 1118 — no level-up here. In this run, no bug triggered.
4. **But scenario variant:** if achievement XP pushes XP from 1115 to 1145 → crosses Level 6. `AddXp(30)` returns 6. **Return value discarded at AchievementChecker.cs:100.** `levelUpPanel` does NOT show, `ApplyLevelReward(GetRewardForLevel(6))` NOT called. Level 6 reward permanently lost. *(Rank 2 bug, second path)*

---

### Session 8 — First-timer on a small table (AR placement friction)

**Profile:** Playing on a 60cm × 60cm table. Fresh install.

**Flow trace:**

1. Water placed at table centre. `PlaceWaterOnPlane.enabled = false`.
2. `FishSpawner.SpawnFish()`: `spawnRadius = 1.5f`. Fish 0 spawns at `waterPlane.position + (1.3, -0.2, 0.8)` — 1.5m offset. This position is 1.3m to the right of a 60cm table, physically in mid-air at desk height.
3. `FishSwim.Start()`: `swimCenter = transform.position` (the mid-air spawn). Fish swim within 1.5m of this mid-air point, never returning to the water surface.
4. Player cannot see or reach mid-air fish on the table. They see only 1-2 of 3-7 fish within the visible water area.
5. No error, no warning. Player sees fewer fish than expected and cannot catch the hidden ones.

**UX gap — spawnRadius vs typical AR surface (new W8 observation):** `spawnRadius` of 1.5m assumes a 3m × 3m floor area, but the default Unity template is often tested on floors. On a table, a 1.5m radius means fish may spawn off the edge. Reducing `spawnRadius` to 0.8m or tying it to the detected plane bounds would fix this.

---

### Session 9 — Returning player (Zen mode discovery attempt)

**Profile:** Level 7, looking for different game modes.

**Flow:** Player searches every UI element. `GameMode.Zen` enum exists in `GameMode.cs`. `EndGameManual()` exists in `GameManager.cs:189`. No game-mode selection screen exists. No "End Session" button exists. `currentMode = GameMode.Standard` by default with no setter UI. **Zen mode is inaccessible to all players.** *(Carried from Week 7, still open.)*

---

### Session 10 — Veteran with full achievement stack on same run

**Profile:** Level 9, earns `FirstCatch`, `Combo5`, `PerfectRound`, `Level10` in a single session.

**Flow trace:**

1. `EndGame()` → `AchievementChecker.CheckAll` → 4 achievements newly unlocked. `newlyUnlocked = ["first_catch", "combo_5", "perfect_round", "level_10"]`.
2. `ShowAchievementsSequenced(4 ids, catalog)` starts at +1.2s. Each toast: 2s show + 0s gap = 8s total.
3. Level-up panel activated at +0.8s and never auto-dismissed (no `Invoke(HidePanel, ...)`, no tap-to-dismiss).
4. At +1.2s: first achievement toast appears ON TOP of or BEHIND the level-up panel (layout-dependent). Player is reading "Level 10! Selamat!" when the first toast fires.
5. Result screen is visible but interactive elements (Play Again button) are potentially blocked by the achievement toast panel for 8.2 seconds. No skip or dismiss mechanism. *(UX-NEW-3, carried from Week 7.)*

---

## Bugs Found This Week

### New (Week 8)

- [ ] **BUG-W8-1** — `GameManager.PickNewTarget()` sets `targetColorLabel.text` to the FishPalette colour name before `SpawnFish()` resolves the species colour; after spawn, `targetColorImage.color` is updated to `CurrentTargetSpecies.baseColor` but `targetColorLabel.text` is not; in catalog mode the label and image describe different colours every round — `GameManager.cs:404-421` — reproduction: assign FishCatalog in Inspector with species whose `baseColor` differs from the four FishPalette entries; observe that the colour name label does not match the colour swatch during gameplay

- [ ] **BUG-W8-2** — `ShowHappy()` and `ShowSad()` both call `Invoke(nameof(HideFeedback), 1f)` without first calling `CancelInvoke(nameof(HideFeedback))`; when a second fish hit occurs within 1.0s of the first (possible at minimum pacing floor of 1.0s), the earlier `HideFeedback` Invoke fires and hides the newly active feedback immediately — `GameManager.cs:502-503, 508-509` — reproduction: achieve rapid consecutive hits at pacing floor; second-hit feedback disappears in under 100ms

- [ ] **BUG-W8-3** — `FishCatalog.PickOther(exclude)` retries up to 20 times and returns the last `picked` value even if it equals `exclude`; with a two-species catalog where the target is the Common species (weight 6) and the only alternative is Rare (weight 1), the expected number of draws before picking the Rare species is 7 — after 20 attempts the probability of still being stuck on Common is (6/7)^20 ≈ 4.7% per decoy; with 4 decoys per round this yields an ~18% per-round probability of at least one visually identical target+decoy pair making the round ambiguous — `FishCatalog.cs:36-44` — reproduction: create a catalog with one Common and one Rare species and play several rounds; occasionally all decoys will share the target's colour

### Carry-over (Week 7 — confirmed still open)

- [ ] **BUG-NEW-1** — `AchievementChecker.cs:100`: `ProgressionStore.AddXp(achievement.xpReward)` return value discarded; achievement-triggered level-ups are silent and `ApplyLevelReward` is never called — reproduction: be near a level threshold; trigger a first-time achievement whose `xpReward` crosses the boundary; confirm level increments in HUD with no panel and no reward
- [ ] **BUG-NEW-2** — `TombakanOnboarding.cs:39-44`: daily-bonus level-up detected and shown in text but `ApplyLevelReward` never called; coin bonuses, species unlocks, skin unlocks from level rewards are permanently skipped — `DailyChallenge.cs:58`
- [ ] **BUG-NEW-3** — `ColourBlindSettings.ShapeForColor` returns `"?"` for any species colour outside the four exact FishPalette hex values — `ColourBlindSettings.cs:22-27`
- [ ] **BUG-NEW-4** — `ShowSad()` always shows `"-25!"` even when `ClampScore` absorbs the penalty — `GameManager.cs:507`
- [ ] **BUG-NEW-5** — `GoalManager.ForceCompleteGoal()` → `CompleteGoal()` dereferences `m_OnboardingGoals` which is `null` for returning players; NullReferenceException on every returning-player launch — `GoalManager.cs:197`, `TombakanOnboarding.cs:30`

---

## UX Gaps

- [ ] **UX-1** *(open since Week 6)* — No throw-mechanic tutorial for first-time players; after water placement the HUD shows fish and target colour with no instruction on how to throw
- [ ] **UX-W8-1** *(new)* — `FishSpawner.spawnRadius = 1.5f` causes fish to spawn beyond the bounds of typical small AR surfaces (tables, desks); fish appear floating in mid-air; players on small surfaces see fewer catchable fish with no explanation — `FishSpawner.cs:9` — adaptive radius (e.g., clamp to detected plane extents, or reduce default to 0.8m) would prevent off-surface spawning
- [ ] **UX-NEW-2** *(carried)* — `greetingPanel` and `dailyBonusPanel` can be active simultaneously; no mutual exclusion; panels overlap on small screens — `TombakanOnboarding.cs:24-44`
- [ ] **UX-NEW-3** *(carried)* — Achievement toast queue fires 0.4s after level-up panel with no dismiss/skip; blocks result screen for up to 12s (6 achievements × 2s); no tap-to-skip — `GameManager.cs:354`
- [ ] **UX-NEW-4** *(carried)* — `targetColorLabel` shows raw hex fallback (e.g. `"E6993F"`) when a species colour is not in `Dict.cs`; compounded by BUG-W8-1 (label not updated post-spawn) — `Dict.cs:35`, `GameManager.cs:406`

---

## Items Confirmed Fixed in Previous Weeks

- **Week 5:** PickNewTarget guard (`if (!gameRunning) return;`) prevents fish spawn after EndGame — verified in code at `GameManager.cs:397`.
- **Week 5:** PlaceWaterOnPlane one-shot (`enabled = false`) prevents mid-game re-positioning — verified at `PlaceWaterOnPlane.cs:37`.
- **Week 6:** `OnFishHit` guard prevents post-EndGame score mutation — verified at `GameManager.cs:440`.
- **Week 6:** `HapticFeedback` has no reference to `AudioPrefs`; vibration is mute-independent.
- **Week 6:** `StaggerResultCelebrations` correctly staggers badge (+0.4s) and level-up panel (+0.8s) using `WaitForSecondsRealtime`.
- **Week 6:** `Accuracy.Format(0, 0)` returns `"--"` — verified at `Accuracy.cs:22`.
- **Week 6:** `resultXpText.text = ""` when xpEarned = 0 — verified at `GameManager.cs:280`.

---

## Polish Opportunities

- [ ] **POLISH-NEW-1** *(carried)* — `ColorSummary.Format` always appends `×N` including for N=1 ("Merah ×1" instead of "Merah") — `ColorSummary.cs:41`; fix: `counts[name] > 1 ? $" ×{counts[name]}" : ""`
- [ ] **POLISH-W8-1** *(new)* — `ProgressionHUD.Refresh()` is only called from `OnEnable()` and explicit callers; after `TombakanOnboarding` grants daily-bonus XP, the HUD shows stale level/XP values until the HUD panel is toggled; `TombakanOnboarding.Start()` should call `FindObjectOfType<ProgressionHUD>()?.Refresh()` after `ShowDailyBonus()` — `TombakanOnboarding.cs:39-44`
- [ ] **POLISH-2** *(carried)* — `HapticFeedback.PlayCorrect` and `PlayWrong` produce identical `Handheld.Vibrate()` calls with no pattern differentiation — `HapticFeedback.cs:14,22`
- [ ] **POLISH-3** *(carried)* — `ApplyLevelReward` overwrites `levelUpText.text` with `reward.celebrationText` immediately on the same coroutine tick as `ShowLevelUp`; original "Level N! Selamat!" is never readable — `GameManager.cs:387-388`

---

## Validation Week 9

**Date:** 2026-07-27
**Scope re-traced:** ITERATION_week9_SCOPE.md — 4 tasks (TASK-01 through TASK-04)

| Task ID | Status | Evidence |
|---------|--------|---------|
| TASK-01(a) — GoalManager null guard | PASS | `GoalManager.cs:197-203`: `CompleteGoal()` opens with `if (m_OnboardingGoals == null) { m_AllGoalsFinished = true; return; }`. Session trace — returning player: `TombakanOnboarding.Start():32` → `goalManager.ForceCompleteGoal()` → `CompleteGoal()` → null queue detected → exits cleanly; NRE eliminated. |
| TASK-01(b) — ShowSad absorbed penalty | PASS | `GameManager.cs:495-498`: `scoreBefore` recorded before clamped subtract; `actualDeduction = scoreBefore - score` passed to `ShowSad(int)`. Line 535: `sadFeedbackText.text = actualDeduction == 0 ? "Meleset!" : $"-{actualDeduction}!"`. Score-at-0 wrong hit now shows "Meleset!" not "-25!". |
| TASK-01(c) — CancelInvoke before feedback | PASS | `GameManager.cs:522` (`ShowHappy`) and `GameManager.cs:533` (`ShowSad`) both call `CancelInvoke(nameof(HideFeedback))` before activating the panel. Stale hide-call from a prior hit can no longer collapse the next feedback panel within 0–100 ms. |
| TASK-01(d) — targetColorLabel post-spawn sync | PASS | `GameManager.cs:428-434`: after `fishSpawner.SpawnFish()`, if `fishSpawner.CurrentTargetSpecies != null`, `targetColor`, `targetColorImage.color`, and `targetColorLabel.text` are all updated from `CurrentTargetSpecies.baseColor`. Catalog-mode label and swatch describe the same colour every round. |
| TASK-02(a) — Achievement level-up reward | PASS | `GameManager.cs:294-305`: `levelBeforeAch = ProgressionStore.GetLevel()` before `AchievementChecker.CheckAll()`; `levelAfterAch` re-read after; `ShowLevelUp(levelAfterAch)` and `ApplyLevelReward(...)` called when `levelAfterAch > levelBeforeAch`. `AchievementChecker.cs:100` still discards the `AddXp` return value, but the external `GetLevel()` snapshot correctly detects any level-up regardless. Coins/unlocks from achievement-triggered level rewards are now granted. |
| TASK-02(b) — Daily-bonus level reward via TombakanOnboarding | PASS | `GameManager.cs:389`: `ApplyLevelReward` is `public`. `TombakanOnboarding.cs:15`: `public GameManager gameManager` field added. Lines 40/43-50: level snapshotted before `TryClaimDailyBonus`; `GetLevel()` read after; `gameManager.ApplyLevelReward(gameManager.levelRewardTable?.GetRewardForLevel(newLevel))` called when `newLevel > 0`. `DailyChallenge.cs:58` still discards `AddXp` return, but the snapshot approach in `TombakanOnboarding` detects the level change correctly. |
| TASK-02(c) — ColourBlind HSV hue-range classifier | PASS | `ColourBlindSettings.cs:29-39`: `Color.RGBToHSV` extracts hue and saturation. Low-saturation guard (s < 0.15) returns "■". Four hue buckets follow: h < 0.083 or h > 0.917 → "●"; h < 0.250 → "★"; h < 0.458 → "▲"; otherwise "■". No code path returns "?". All 8 catalog species now receive a symbol from `FishShapeOverlay`. |
| TASK-03(a) — spawnRadius 1.5 → 0.8 in scene | PASS | `GamePlay.unity:3572`: `spawnRadius: 0.8` serialised on the `FishSpawner` component. `FishSpawner.cs:9` code-default is still `1.5f`; the scene override takes effect at runtime (standard Unity serialisation). Fish spawned within an 0.8 m radius stay on a typical 60 cm table surface. |
| TASK-03(b) — ThrowHintPanel in scene | PASS | `GamePlay.unity` fileID 7001001: `m_Name: ThrowHintPanel`, `m_IsActive: 0` (inactive by default), parent fileID 1199677538 (GamePlayUI verified at line 3909), anchors `{x:0.5,y:0}/{x:0.5,y:0}` (bottom-centre), background `Image` alpha 0.6 (semi-transparent), child `TMP_Text` reading "Tekan tombol untuk melempar tombak!". `GameManager.throwHintPanel` wired to this fileID at `GamePlay.unity:6101`. |
| TASK-04 — FishSpecies colour audit | PARTIAL | Core fix confirmed: all 8 species produce a valid symbol after TASK-02(c). Saturation and value constraints met for all updated assets (s >= 0.6, v >= 0.5; tuna_sirip_kuning unchanged at v = 0.5, borderline but passes). However, the acceptance criterion "no two species produce the same symbol" is unachievable with 8 species and a 4-symbol system. Each bucket holds exactly 2 species: ● (kakap_merah h=0.013, kerapu_bebek h=0.027); ★ (ikan_badut h=0.100, bandeng h=0.139); ▲ (lele h=0.359, ikan_nila h=0.333); ■ (kembung h=0.587, tuna_sirip_kuning h=0.610). A round can still display two fish with the same symbol. The criterion flaw should be carried forward: to prevent same-symbol fish in one round, `FishCatalog.PickOther` would need to exclude same-bucket species, a code change deferred from this scope. |
