# Tester Report — Week 7

## Executive Summary

The core game loop is stable and all six Week 6 deliverables were verified as fixed. However, four Week 7 candidates remain fully unimplemented: the throw-mechanic tutorial hint, platform-specific haptic differentiation, the "re-position water" button, and the DailyChallenge level-up reward grant. Three additional bugs were discovered during session traces: achievement XP silently discards level-up return values (so `LevelReward` is never applied on achievement level-ups), `FishShapeOverlay` only reads the colour-blind setting at `Start()` so a runtime toggle never refreshes already-spawned fish, and `ColourBlindSettings.ShapeForColor` returns `"?"` for any species colour outside the four exact palette hex values.

---

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `DailyChallenge.TryClaimDailyBonus` discards `ProgressionStore.AddXp` return value — level rewards (coins, unlocks) never applied on daily-bonus level-ups | `DailyChallenge.cs:58` + `TombakanOnboarding.cs:39-44` | Silent failure; rewards lost every time daily bonus crosses a level boundary |
| 2 | No throw-mechanic tutorial hint — first-timers see fish swimming with zero indication of how to throw | `TombakanOnboarding.cs` (missing step) | High drop-off risk; silent failure on random taps |
| 3 | No "re-position water" button — `PlaceWaterOnPlane` self-disables permanently; mis-placed water requires app restart | `PlaceWaterOnPlane.cs:37` | Unrecoverable UX dead-end for new and casual players |
| 4 | `AchievementChecker` discards `ProgressionStore.AddXp` return value — level-up from achievement XP is silent, no panel, no reward | `AchievementChecker.cs:100` | Silent failure; rewards lost when achievement XP pushes player over a level boundary |
| 5 | `HapticFeedback.PlayCorrect` and `PlayWrong` both call identical `Handheld.Vibrate()` — no tactile difference between success and failure | `HapticFeedback.cs:13,21` | Haptic feedback is ambiguous; undermines the "juice" goal of Phase 3 |
| 6 | `FishShapeOverlay` only refreshes at `Start()` — toggling colour-blind mode mid-game has no effect until the next `SpawnFish` cycle | `FishShapeOverlay.cs:19` | Accessibility feature silently broken for mid-game toggles |
| 7 | `ColourBlindSettings.ShapeForColor` returns `"?"` for any `FishSpecies.baseColor` outside the four palette hex values | `ColourBlindSettings.cs:25` | Colour-blind mode shows useless `"?"` on most catalog fish |

---

## Bugs Found

- [ ] **BUG-W7-1** — `DailyChallenge.TryClaimDailyBonus` calls `ProgressionStore.AddXp(xpAwarded)` at line 58 but discards the return value; `TombakanOnboarding.cs:40-44` detects the level-up and shows it in the panel text but never calls `ApplyLevelReward` — reproduction: set XP within 125 pts of a level threshold, relaunch on a new day; confirm level increments and panel shows level-up text, then check `CurrencyStore.GetCoins()` and `SpearStore` — no reward is applied

- [ ] **BUG-W7-2** — `AchievementChecker.cs:100` (inferred line; `AchievementChecker.cs` not directly read but referenced from `GameManager.cs:290`) calls `ProgressionStore.AddXp(achievement.xpReward)` and discards the return value; if the XP award crosses a level boundary, no level-up panel fires and `ApplyLevelReward` is never called — reproduction: be within an achievement's xpReward of the next level, trigger the achievement condition, observe no level-up panel and no reward applied

- [ ] **BUG-W7-3** — `ColourBlindSettings.ShapeForColor` (`ColourBlindSettings.cs:18-29`) maps only four exact hex values (`FF0000`, `00FF00`, `0000FF`, `FFFF00`); any `FishSpecies.baseColor` with a different hex returns `"?"` — reproduction: enable colour-blind mode with a FishCatalog that has a species whose `baseColor` is not one of the four exact palette colours; observe `"?"` on the shape overlay

- [ ] **BUG-W7-4** — `FishShapeOverlay.Refresh()` is called only from `Start()` (line 19) and `OnSettingChanged()` (line 33); `OnSettingChanged()` has no subscriber — no code calls it from `ColourBlindToggleUI` or `ColourBlindSettings` — reproduction: start a game, enable colour-blind mode mid-round via the settings toggle; fish spawned in the current round show no shape overlay; shapes only appear after the next `SpawnFish` call

- [ ] **BUG-W7-5** — `GameManager.ShowSad()` always displays `"-{penaltyPerWrongHit}!"` (`GameManager.cs:507`) regardless of whether `ClampScore` absorbed the penalty; when the displayed score is already 0, the player sees `"-25!"` for a deduction that had no visible effect — reproduction: let score reach 0, hit a wrong fish; `"-25!"` feedback appears while score stays at 0

- [ ] **BUG-W7-6** — `GoalManager.ForceCompleteGoal()` is called from `TombakanOnboarding.Start()` (line 30) on every returning-player launch; if the GoalManager's step list and goal queue are not yet initialised (returning player never triggers `StartCoaching()`), `CompleteGoal()` at `GoalManager.cs:196-213` can access `m_StepList` at an uninitialised index; if `m_StepList` is populated in the scene, `m_StepList[0].stepObject.SetActive(false)` hides a UI element that should never have been visible — reproduction: returning player launches app; observe the first step card being incorrectly hidden or a `NullReferenceException` / `IndexOutOfRangeException` in the console

---

## UX Gaps

- [ ] **No throw-mechanic tutorial** — first-game player moment, after water is placed and fish spawn — `TombakanOnboarding.cs` coaches AR placement via `GoalManager` but has no step for the throw mechanic; add a timed hint panel ("Sentuh tombol untuk melempar tombak!") that appears 3 s after `GameManager.StartGame()` if `correctHitCount == 0 && wrongHitCount == 0`, and auto-dismisses after the first throw attempt

- [ ] **No "re-position water" button** — player places water in wrong position (corner of plane, wall shadow, etc.) — `PlaceWaterOnPlane.cs:37` sets `enabled = false` with no re-enable path; add `public void ResetPlacement()` that sets `waterPlane.SetActive(false)` and re-enables the component, wire to a HUD button that is hidden once `GameManager.gameRunning` is true

- [ ] **Daily bonus panel overlaps greeting panel** — first-time player gets both panels simultaneously — `TombakanOnboarding.Start()` activates `greetingPanel` at line 34 and then immediately checks for a daily bonus at line 39; on a fresh install both can be active at once; show `greetingPanel` first and only run the daily bonus check after `DismissGreeting()` is called

- [ ] **Daily bonus auto-dismisses in 3 s with no manual dismiss** — `TombakanOnboarding.cs:62` uses `Invoke(nameof(HideDailyBonus), 3f)`; streak + level-up line makes the text long enough to require ~4 s to read; add a tap-to-dismiss `Button` component to `dailyBonusPanel` or extend the delay to 5 s for level-up variants

- [ ] **Throw cooldown gives no visual indicator** — `SpearThrower.ThrowSpear()` silently returns at line 48 when `canThrow` is false; the fake spear is hidden but no reload progress bar or greyed button exists; expose a normalised cooldown ratio and drive a small HUD icon or button fill

- [ ] **Wrong-hit feedback omits the correct answer** — `GameManager.ShowSad()` shows only `"-25!"` (`GameManager.cs:507`); a frustrated player who hit the wrong colour gets a penalty with no reminder of the target; append the current target name: `$"-{penaltyPerWrongHit}! Cari {targetColorLabel.text}"`

- [ ] **Colour-blind target indicator is colour-only** — the target colour swatch (`targetColorImage`) shows only a colour patch; a colour-blind player must mentally map the swatch to a shape; add a shape symbol to the target UI using the same `ShapeForColor` lookup when colour-blind mode is active

---

## Polish Opportunities

- [ ] **Platform-specific haptic differentiation** (POLISH-2, still open) — implement Android-specific duration via `AndroidJavaObject("android.os.Vibrator")` (short 40 ms for correct, 200 ms double-pulse for wrong); use `UnityEngine.InputSystem.Haptics` `.light`/`.heavy` on iOS; gate behind `#if UNITY_ANDROID` / `#if UNITY_IOS`; requires device test matrix before merge

- [ ] **Achievement toast fires 0.4 s after level-up panel** — `GameManager.cs:344,354`; level-up panel opens at +0.8 s, first toast at +1.2 s; player cannot finish reading the level-up message before the toast appears; delay the achievements coroutine start until +3.5 s, or add a tap-to-dismiss on the level-up panel before toasts begin

- [ ] **`ApplyLevelReward` overwrites level-up text** — `GameManager.cs:387-388` sets `levelUpText.text = reward.celebrationText` immediately after `ShowLevelUp` sets it to `"Level N! Selamat!"`; if both fire in the same coroutine frame the original message is lost before the player reads it; keep celebration text in a separate label or apply it with a short delay

- [ ] **`ColorSummary.Format` shows `×1` for single catches** — result screen shows e.g. `"Merah ×1"` when there was only one red fish caught; show `×N` only when `N > 1`

- [ ] **ProgressionHUD stale after daily bonus** — `ProgressionHUD.Refresh()` fires on `OnEnable`; after `TombakanOnboarding` grants daily XP, the level badge and XP bar on the main screen are not updated until the next scene enable cycle; call a refresh after `ShowDailyBonus` completes

---

## Session Traces

### Session 1 — First-time Player (no AR experience), run A

**Step 1 — App launch**
`GameManager.Start()` (GameManager.cs:118) plays main BGM. `TombakanOnboarding.Start()` (TombakanOnboarding.cs:24): `ScoreStore.GetBest() == 0` and `ProgressionStore.GetTotalXp() == 0` → `isReturningPlayer = false` → `greetingPanel.SetActive(true)` (line 34).

**Step 2 — Daily bonus fires immediately**
Still inside `TombakanOnboarding.Start()`, line 39: `DailyChallenge.TryClaimDailyBonus` runs. Day 1, streak = 1, xpAwarded = 125. `ProgressionStore.AddXp(125)` at `DailyChallenge.cs:58` — return value discarded. `TombakanOnboarding.cs:40`: `levelAfter = GetLevel()`. If level-up occurred, `ShowDailyBonus` fires `dailyBonusPanel.SetActive(true)`. **Both `greetingPanel` and `dailyBonusPanel` may be visible simultaneously on small screens.**

**Step 3 — Player dismisses greeting**
`DismissGreeting()` (line 72): hides `greetingPanel`, calls `goalManager.StartCoaching()`. AR coaching starts at `FindSurfaces`.

**Step 4 — AR scan and water placement**
Player waves phone, plane detected. Player taps plane. `PlaceWaterOnPlane.Update()` (line 14): raycast hits, `waterPlane.SetActive(true)`, `DisableARPlanes()`, `enabled = false`. Water placed.

**Step 5 — Start game**
`GameManager.StartGame()` (line 207). Fish spawn. Target label shows "Merah". **No hint exists for how to throw.** Player taps the water surface — `PlaceWaterOnPlane` is disabled, no response. Player taps a fish directly — `SpearHit` is attached to the spear projectile, not a tap target, so nothing happens. Player is confused. Eventually discovers the throw button or abandons.

**Friction logged:** UX-1 (throw tutorial absent), silent tap failure.

---

### Session 2 — First-time Player (no AR experience), run B — bad water placement

Steps 1–4 same as Session 1.

**Step 4B — Accidental tap on floor edge**
Water appears at the far edge of the detected plane, half-clipped into a wall. Fish spawn 2 m from the camera. Player tries to re-tap a better surface — `PlaceWaterOnPlane.Update()` never runs again (`enabled = false`, line 37). Every tap is handled as a throw attempt by `SpearThrower`. **No re-position path exists.** Player must force-quit and restart. Critical UX dead-end.

**Friction logged:** missing "re-position water" button (TASK-02 from Week 5 still unimplemented).

---

### Session 3 — Casual Returning Player (2nd session)

**Step 1 — App launch**
`TombakanOnboarding.Start()` (line 25): `ScoreStore.GetBest() > 0` → `isReturningPlayer = true`. `goalManager.ForceCompleteGoal()` called (line 30).

**Potential crash trace:** `ForceCompleteGoal()` → `CompleteGoal()` (GoalManager.cs:195). `m_CurrentGoalIndex++` → 1. `m_OnboardingGoals.Count` is 0 (queue not initialised for a returning player who never called `StartCoaching()` this session) → `else` branch: `m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false)` = `m_StepList[0].stepObject.SetActive(false)`. If `m_StepList` is populated in the Inspector and step 0's `stepObject` is the AR coaching card, it is force-hidden. If `m_StepList[0]` is null or not wired, this throws `NullReferenceException`. **BUG-W7-6.**

**Step 2 — Daily bonus**
Streak 2, xpAwarded = 150. Panel shown for 3 s. If player has 90 XP and threshold is 100, the 150 XP crosses Level 2. Level text added to panel. `LevelRewardTable.GetRewardForLevel(2)` is never called. Reward silently lost. **BUG-W7-1.**

**Step 3 — Game plays normally**
Fish swim, player throws, hits correct fish. Score increments. Audio plays. Screen shakes. Haptic fires. Loop works.

---

### Session 4 — Casual Returning Player (3rd session, full game)

Game plays cleanly. Player scores 5 correct, 2 wrong. `EndGame()`:
- `resultAccuracyText.text = Accuracy.Format(5, 2) = "5/7 (71%)"`.
- `xpEarned = ProgressionRules.XpForResult(5, 71, 0, 3)` = ~56 XP.
- `newLevel = ProgressionStore.AddXp(56)` — returns 0 (no level-up).
- `StaggerResultCelebrations(isRecord=false, newLevel=0)` — neither badge nor level-up panel fires. Clean result screen.
- Achievement check: `AchievementChecker.CheckAll` — if `first_catch` achievement exists and xpReward = 50, `AddXp(50)` fires inside checker. Return value discarded. If this 50 XP happens to cross a threshold (e.g. player was at 95 XP): **BUG-W7-2** — silent level-up.

---

### Session 5 — Frustrated Player (missed 3+ throws in a row)

**Step 1 — Three complete misses**
Player throws 3 times, spear travels forward, no fish in range. `SpearHit.CheckFishHit()` (SpearHit.cs:18): `Physics.OverlapSphere` returns empty array. No hit. Spear destroyed after 2.5 s. `CooldownRoutine` restores `canThrow` and `spearFake`. No feedback whatsoever — **silent failure for a complete miss**. Player does not know if the game registered the throw.

**Step 2 — Wrong hit**
Player hits a blue fish while target is red. `OnFishHit(Color.blue, "id")` (GameManager.cs:438). `correct = false`. `score = ClampScore(0 - 25) = 0`. `ShowSad()` → `"-25!"`. Score display stays at 0. **BUG-W7-5:** feedback shows `-25` when deduction was absorbed by clamp.

**Step 3 — Timer expires**
`EndGame()`. `resultAccuracyText.text = Accuracy.Format(0, 3) = "0/3 (0%)"`. TierEmpty shown. `resultXpText.text = ""` (correctly suppressed — Week 5 fix). No further confusion.

**Compounding frustration:** identical haptic on wrong hit as correct hit (`Handheld.Vibrate()` both, POLISH-2 open). Player cannot tell from feel which was right.

---

### Session 6 — Frustrated Player (continued, haptic confusion)

Player hits correct fish at last second. `HapticFeedback.PlayCorrect()` fires — `Handheld.Vibrate()`. Then immediately hits another fish (wrong colour) — `HapticFeedback.PlayWrong()` — identical `Handheld.Vibrate()`. Player felt two identical vibrations and cannot determine which was which. Score goes +100 then -25 but the haptic gave no signal. POLISH-2 remains unaddressed.

---

### Session 7 — Speed-runner (maximising score)

**Step 1 — First 5 correct hits**
`comboStreak` builds: 1, 2, 3 (multiplier → 2×), 4, 5 (multiplier → 3×). `maxComboStreak = 5`. Score per hit at streak ≥ 5: 300 pts. `timeLeft` extended by `TimeBonus.ForHit(5)` on each correct hit.

**Step 2 — LockThrow race condition trace**
Hit at t=12.0 s. `OnFishHit` (line 485): `LockThrow(delay)` called on `SpearThrower`. `SpearThrower.LockThrow` (line 100): `StopAllCoroutines()` — kills any running `CooldownRoutine`. New `LockRoutine(delay)` starts. At the end of `LockRoutine` (SpearThrower.cs:110): `canThrow = true`, `spearFake.SetActive(true)`. Safe in the single-hit case.

Edge case: player hits fish at t=12.0 s and again at t=12.2 s (two spears in flight simultaneously — only one `spearProjectilePrefab` is checked for `canThrow` but cooldown = 1.2 s, so second throw cannot happen this fast while locked). `LockThrow` prevents the race condition correctly because `LockThrow` is the entry point and `canThrow = false` blocks `ThrowSpear` before a second instantiate.

**Step 3 — End game, level-up + achievement**
Player reaches level boundary. `GameManager.EndGame`: `AddXp(earned)` returns new level. `StaggerResultCelebrations` fires. If achievement also earns XP crossing the next boundary: **BUG-W7-2** fires — achievement AddXp return value discarded.

**Achievement toast + level-up panel overlap:** level-up panel at +0.8 s, achievement toast at +1.2 s. Player sees both simultaneously for 0.4 s. Polish gap.

---

### Session 8 — Speed-runner (second game, same session)

Player taps "Play Again". `GameManager.StartGame()` resets all counters. `PickNewTarget()` → `SpawnFish()` → `ClearFish()` destroys previous `spawnedFish` array entries with `if (fish)` null guard (FishSpawner.cs:99) — safe even if some were already destroyed by `DestroyAfterDelay`. Clean start confirmed.

`DailyChallenge.TryClaimDailyBonus` in `TombakanOnboarding.Start()` is not called again (it ran at launch). No stale daily bonus. No issues found in back-to-back session.

---

### Session 9 — Colour-blind Player (mode on before game)

**Step 1 — Mode pre-enabled**
`ColourBlindSettings.IsEnabled()` returns `true`. Player starts game.

**Step 2 — Fish spawn, overlays set**
`FishSpawner.SpawnFish()` instantiates fish. Each fish: `FishShapeOverlay.Start()` (line 16) → `Refresh()` (line 19) → `label.enabled = true`, `label.text = ShapeForColor(fishTarget.fishColor)`.

**Step 3 — Shape lookup for catalog species**
`fishTarget.fishColor = targetSpecies.baseColor`, e.g. `new Color(0.8f, 0.5f, 0.1f)` → hex `CC8019`. `ShapeForColor("CC8019")` — no case matches the switch (ColourBlindSettings.cs:21-27) → returns `"?"`. Fish shows `"?"`. For the standard four-colour palette (exact hex values), shapes are correct. For any catalog species with a non-palette color: **BUG-W7-3 confirmed.**

**Step 4 — Target indicator**
`targetColorImage.color = targetSpecies.baseColor`. `targetColorLabel.text = ColorHexLocalization.ToIndonesian(targetColor)` (GameManager.cs:406). If `CC8019` is not in `Dict.cs`'s 20-entry map, label shows `"CC8019"` instead of an Indonesian name. Target indicator is meaningless. **UX gap confirmed.**

---

### Session 10 — Colour-blind Player (toggles mode mid-game)

**Step 1 — Game starts with colour-blind OFF**
Fish spawn. `FishShapeOverlay.Start()` → `Refresh()`: `ColourBlindSettings.IsEnabled() == false` → `label.enabled = false`. No shapes shown.

**Step 2 — Player opens settings, toggles ON**
`ColourBlindSettings.Toggle()` (ColourBlindSettings.cs:13): `SetEnabled(true)`. Persists to `PlayerPrefs`. **Nothing else happens.** `ColourBlindToggleUI` is not shown to call `FindObjectsOfType<FishShapeOverlay>()` and invoke `OnSettingChanged()`, and even if it did there is no wiring. All currently-spawned fish have `label.enabled = false` with no refresh path.

**Step 3 — Next correct hit, new fish round**
`SpawnFish()` runs. New fish instantiated. `FishShapeOverlay.Start()` fires with `IsEnabled() == true`. Shapes appear on the new fish. The delay between toggling and seeing shapes is one full round (could be 10–30 s depending on pacing). **BUG-W7-4 confirmed: runtime toggle has zero effect on live fish.**

**Step 4 — Shape `"?"` on catalog fish**
Same as Session 9, Step 3. Colour-blind mode + FishCatalog produces `"?"` overlays. Mode is functionally broken for catalog play.

---

## Validation Week 7

| Task | Status | Evidence |
|------|--------|----------|
| T1   | PARTIAL | `AchievementChecker.cs` lines 101–104 correctly capture the `ProgressionStore.AddXp` return value and call `GameManager.I?.ApplyLevelReward(GameManager.I?.levelRewardTable?.GetRewardForLevel(newLevel))` when `newLevel > 0`. `GameManager.cs` line 376 declares `ApplyLevelReward` as `public`, satisfying the visibility requirement. Logic is correct. **Missing:** the required `AchievementCheckerRewardTests` EditMode test class does not exist anywhere under `Assets/Tests/`. The acceptance criterion explicitly requires test coverage; without it this task is not fully done. |
| T2   | PARTIAL | `GameManager.cs` lines 477–480 compute `scoreBefore` before `ClampScore`, set `deductionAbsorbed = (score == scoreBefore)`, and pass the boolean to `ShowSad`. `ShowSad` at line 509–513 displays `"Miss!"` when absorbed and `"-{penaltyPerWrongHit}!"` otherwise — the conditional display is correct. **Missing:** the required `GameManagerFeedbackTests` EditMode test for the absorbed case does not exist under `Assets/Tests/`. The acceptance criterion explicitly requires this test; without it the task is PARTIAL. |
| T3   | PARTIAL | `TombakanOnboarding.cs` lines 52–58 capture `levelBefore = ProgressionStore.GetLevel()` before `TryClaimDailyBonus`, read `levelAfter` after it returns, compute `newLevel`, and pass it to `ShowDailyBonus`. `ShowDailyBonus` (lines 63–65) calls `GameManager.I?.ApplyLevelReward(GameManager.I?.levelRewardTable?.GetRewardForLevel(newLevel))` when `newLevel > 0`. The indirect level-up detection approach is sound. **Note:** `DailyChallenge.cs` line 58 still discards the `AddXp` return value internally, but the fix correctly reads `GetLevel()` before and after the call, which achieves the same detection. **Missing:** the required EditMode test that stubs `ProgressionStore` near a level boundary does not exist under `Assets/Tests/`. Without it the criterion is unmet. |
| T4   | PARTIAL | `TombakanOnboarding.cs` has `hintPanel` and `hintText` serialized fields (lines 24–25). `NotifyGameStarted()` (line 101) starts the `ShowThrowHint` coroutine. `ShowThrowHint` (lines 120–132) waits `2f` seconds, checks `correctHitCount == 0 && WrongHitCount == 0`, sets `hintText.text = "Sentuh tombol untuk melempar tombak!"`, activates `hintPanel`, and starts a 5 s auto-dismiss coroutine. `SpearThrower.cs` line 51 calls `TombakanOnboarding.I?.NotifyFirstThrow()` on `ThrowSpear()`, which hides the hint and cancels auto-dismiss (lines 110–118). `GameManager.StartGame()` line 252 calls `TombakanOnboarding.I?.NotifyGameStarted()`. All runtime logic is in place. **Naming discrepancy (cosmetic):** the scope specified the method on `SpearThrower` call as `DismissHint()` but the implementation uses `NotifyFirstThrow()` — functionally equivalent. **Gap:** the scope says the hint appears "only when `correctHitCount == 0 && wrongHitCount == 0`" — the coroutine correctly checks this, but `wrongHitCount` is a private field accessed via the `WrongHitCount` public property; this is correct. **Missing:** no inspector wiring is verifiable from code alone — `hintPanel` and `hintText` must be wired in the Unity scene (GamePlay.unity), which cannot be confirmed without the Editor. If those fields are `null` at runtime, the feature silently does nothing. No test coverage required by the criterion for T4, so the test gap does not apply here. |

---

## New Issues Found During Validation

- **VAL-1 — Missing `AchievementCheckerRewardTests` test class (T1 gap):** The acceptance criterion for T1 explicitly requires a new EditMode test in `Assets/Tests/EditMode/AchievementCheckerRewardTests.cs` that verifies `CurrencyStore.GetCoins()` reflects the soft-currency bonus after a level-crossing achievement unlock. No such file exists. The runtime fix is correct, but the test contract is unfulfilled.

- **VAL-2 — Missing `GameManagerFeedbackTests` test class (T2 gap):** The acceptance criterion for T2 requires a new EditMode test covering the absorbed-deduction path (`score == 0`, wrong hit → `"Miss!"`). No such file exists. Same situation as VAL-1.

- **VAL-3 — Missing daily-bonus level-up EditMode test (T3 gap):** The acceptance criterion for T3 requires an EditMode test that stubs `ProgressionStore` near a level boundary and verifies that `CurrencyStore.GetCoins()` increases after a day-boundary claim. No such file exists.

- **VAL-4 — `ApplyLevelReward` overwrites level-up text without delay (pre-existing, now observable via T1/T3 paths):** `GameManager.cs` line 389–390 sets `levelUpText.text = reward.celebrationText` immediately if `reward.celebrationText` is non-null. For the T1 path (achievement level-up) there is no `ShowLevelUp` call before `ApplyLevelReward`, so the celebration text will overwrite whatever was already in `levelUpText`. For the T3 path (daily bonus), `ApplyLevelReward` is called from `TombakanOnboarding.ShowDailyBonus` before `GameManager` has shown a level-up panel at all — the text write targets a UI element that may not be visible, which is harmless but inconsistent with the EndGame path. This was noted as a polish gap in the original report but is now reachable from two new code paths.

- **VAL-5 — `ShowThrowHint` uses `WrongHitCount` (public property) but scope mentioned `wrongHitCount` (private field):** The implementation uses the correct public accessor `GameManager.I.WrongHitCount`; this is fine and not a bug. Noted for clarity only.

- **VAL-6 — `hintPanel` / `hintText` scene wiring unverifiable:** `TombakanOnboarding` declares `hintPanel` and `hintText` as `[SerializeField]` fields. If they are not wired in `Assets/Scenes/GamePlay.unity`, the entire T4 feature silently does nothing at runtime. The code is correct but a scene audit is required to confirm the feature is actually active.
