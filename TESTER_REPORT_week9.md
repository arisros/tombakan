# Tester Report — Week 9

## Validation of Week 9 Scope (ITERATION_week9_SCOPE.md)

All three tasks are confirmed PASS on the current codebase.

| Task ID | Status | Evidence |
|---------|--------|----------|
| TASK-01 | PASS | `GoalManager.cs` exposes `public bool IsCoachingActive => m_OnboardingGoals != null && !m_AllGoalsFinished`. `CompleteGoal()` at line 209 now reads `if (m_OnboardingGoals != null && m_OnboardingGoals.Count > 0)` — null-safe. `TombakanOnboarding.cs:38-39` wraps the `ForceCompleteGoal()` call in `if (goalManager.IsCoachingActive)` — on a cold returning-player launch, `m_OnboardingGoals` is null, `IsCoachingActive` returns false, `ForceCompleteGoal()` is never invoked, no `NullReferenceException` occurs. Main menu renders correctly on second launch. |
| TASK-02 | PASS | (a) `AchievementChecker.cs` adds `CheckAll(GameManager, AchievementCatalog, out int levelGained)` and `CheckAll(AchievementSession, AchievementCatalog, out int levelGained)` overloads. The inner loop captures `ProgressionStore.AddXp(achievement.xpReward)` return value, tracking the highest new level with `if (lvUp > levelGained) levelGained = lvUp;`. `GameManager.cs:288-289` evaluates achievements before starting `StaggerResultCelebrations`, merges via `if (achievementLevel > newLevel) newLevel = achievementLevel`. (b) `DailyChallenge.TryClaimDailyBonus` signature extended to `out int newLevel`; line 72 assigns `newLevel = ProgressionStore.AddXp(xpAwarded)` — return no longer discarded. (c) `TombakanOnboarding.cs:49` calls the 3-out-param overload; `DismissGreeting()` lines 101-108 apply `GameManager.I?.ApplyLevelReward(newLevel)` when `newLevel > 0` on both immediate and deferred paths. `GameManager.cs:373-385` exposes `public void ApplyLevelReward(int level)` for external callers. |
| TASK-03 | PASS | (a) `TombakanOnboarding.cs:52-67` — when `greetingPanel` is visible, `ShowDailyBonus` is not called; bonus data is stored in `_pendingBonus/_pendingXp/_pendingStreak/_pendingNewLevel` fields and `ShowDailyBonus` is called from `DismissGreeting()` (lines 101-108) after `greetingPanel` is hidden. The two panels cannot be simultaneously active. (b) `GameManager.cs:487-493` — `int scoreBefore = score; score = ClampScore(score - penaltyPerWrongHit); int actualDeduction = scoreBefore - score;` then `ShowSad(actualDeduction)`. `GameManager.cs:519-526` — `ShowSad(int actualDeduction)` returns early (`if (actualDeduction <= 0) return;`) when score is already at 0, suppressing the `"-25!"` text entirely. |

---

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `ColourBlindSettings.ShapeForColor` maps only four exact hex values (`FF0000`, `00FF00`, `0000FF`, `FFFF00`); any `FishSpecies.baseColor` outside this set returns `"?"` on shape overlays; `ColorHexLocalization.ToIndonesian` also falls back to raw hex for the same out-of-palette colours | `ColourBlindSettings.cs:22-27`, `Dict.cs:35` | HIGH: colour-blind mode is non-functional for any session using `FishCatalog` with non-palette species; accessibility-critical feature produces unreadable `"?"` shapes and hex-code target labels |
| 2 | `PlaceWaterOnPlane.Update()` does not guard against UI-overlapping touches via `EventSystem.current.IsPointerOverGameObject(touch.fingerId)`; tapping a UI button while the camera sees a detected AR plane fires both the button action and an AR placement | `PlaceWaterOnPlane.cs:18-38` | MED: first-time player tapping Start (or any HUD button) during plane detection accidentally places water at an unintended position with no way to correct it (one-shot guard) |
| 3 | `FishSwim.swimCenter` is initialised from the fish's spawn position; fish spawned at corners of the ±1.5 m spawn area have a swim centre up to 2.12 m from the water plane; combined with `horizontalRadius = 1.5 m`, corner fish can roam 3.6 m from the play-area centre — outside most home AR spaces at high difficulty | `FishSwim.cs:34`, `FishSpawner.cs:72` | MED: at 15+ correct hits (7 fish), corner-spawned fish are routinely unreachable; perceived as game freeze or invisible fish |
| 4 | Achievement toast fires at +1.2 s after `EndGame`, only 0.4 s after the level-up panel appears at +0.8 s; player cannot read the level-up message before the toast overlaps it | `GameManager.cs:344,354` | MED: level-up panel and achievement toast compete for attention on small screens; level-up text obscured by toast content |
| 5 | `ColorHexLocalization.ToIndonesian` falls back to the raw hex string for any `FishSpecies.baseColor` not in the 20-entry `Dict.cs` map; target label shows e.g. `"D96B26"` instead of an Indonesian name when a FishCatalog species is active | `Dict.cs:35`, `GameManager.cs` (targetColorLabel assignment) | LOW-MED: non-Indonesian-speaking players unaffected; Indonesian players see a hex code as a game instruction |
| 6 | No throw-mechanic tutorial for first-time players — persists from Week 6 UX-1 | `TombakanOnboarding.cs` | LOW-MED: first-time player learns throw mechanic through trial and error only; onboarding step ends after water placement with no gameplay hint |

---

## Session Simulations

### Session 1 — Returning Player (TASK-01 fix verification)

**Profile:** `ScoreStore.GetBest() = 400`, `ProgressionStore.GetTotalXp() = 85`, Level 1. `goalManager` wired in scene.

**Flow:**

1. App launches. `TombakanOnboarding.Start()`: `isReturningPlayer = true`, `skipOnboardingForReturning = true`, `goalManager != null`.
2. `goalManager.IsCoachingActive` evaluated: `m_OnboardingGoals == null` (never initialised for returning player) → `IsCoachingActive = false`. `ForceCompleteGoal()` is NOT called. No `NullReferenceException`. **TASK-01 fix confirmed.**
3. Daily bonus: `DailyChallenge.TryClaimDailyBonus(out xp, out streak, out newLevel)` — new day, streak = 2, `xpAwarded = 150`. `newLevel = ProgressionStore.AddXp(150)` — XP = 235. Level 2 threshold = 100 XP. Player now Level 2. `newLevel = 2`.
4. `greetingPanel == null` for returning player → not visible. `ShowDailyBonus(150, 2, 2)` runs immediately. Panel shows `"Bonus harian +150 XP!\nStreak 2 hari berturut-turut!\nLevel 2! Selamat!"`.
5. `GameManager.I?.ApplyLevelReward(2)` — looks up `LevelRewardTable.GetRewardForLevel(2)`. If a reward is configured (e.g. 50 coins), `CurrencyStore.AddCoins(50)` executes. `ShowLevelUp(2)` fires the level-up panel. **TASK-02 reward path confirmed — level reward no longer silently skipped.**

**Result:** Main menu renders. No crash. Daily bonus shown with correct level-up text. Level reward applied.

---

### Session 2 — First-Time Player (TASK-03 panel ordering fix verification)

**Profile:** Fresh install. `ScoreStore.GetBest() = 0`, `ProgressionStore.GetTotalXp() = 0`.

**Flow:**

1. `TombakanOnboarding.Start()`: `isReturningPlayer = false` → `greetingPanel.SetActive(true)`. Greeting panel visible.
2. `DailyChallenge.TryClaimDailyBonus(out xp, out streak, out newLevel)` — first day, `xpAwarded = 125`, `newLevel = ProgressionStore.AddXp(125)`. XP = 125, Level 2. `newLevel = 2`.
3. `greetingVisible = true` → `_pendingBonus = true; _pendingXp = 125; _pendingStreak = 1; _pendingNewLevel = 2`. `dailyBonusPanel.SetActive(true)` is NOT called. **Only greeting panel is visible. TASK-03 panel ordering fix confirmed.**
4. Player taps "Dismiss" button. `DismissGreeting()` runs: `greetingPanel.SetActive(false)`. `goalManager.StartCoaching()` — coaching queue initialised. `_pendingBonus = true` → `ShowDailyBonus(125, 1, 2)` called. `dailyBonusPanel.SetActive(true)`. `_pendingNewLevel = 2 > 0` → `GameManager.I?.ApplyLevelReward(2)`. Level reward applied.
5. Now `IsCoachingActive = true`. Player scans floor, places water. `PlaceWaterOnPlane` responds.

**Result:** Panels shown sequentially, never simultaneously. Level reward applied on first-ever daily bonus that triggers a level-up.

---

### Session 3 — Score-Floor Wrong Hit (TASK-03 ShowSad fix verification)

**Profile:** Level 1. Player hits wrong fish with score at 0.

**Flow:**

1. `StartGame()`. Target = Hijau (Color.green). Player hits wrong fish (blue).
2. `OnFishHit(Color.blue, "")`. `correct = false`. `comboStreak = 0`. `wrongHitCount = 1`.
3. `int scoreBefore = 0`. `score = ClampScore(0 - 25) = 0`. `actualDeduction = 0 - 0 = 0`.
4. `ShowSad(0)` — `if (actualDeduction <= 0) return;` — early return. `sadFeedbackText` never updated. No `"-25!"` text visible. **TASK-03 ShowSad fix confirmed.**
5. Score display remains at 0. No misleading penalty feedback.

**Wrong-hit when score > 0 (regression check):**
6. Player hits correct fish next: `score = 100`. Then hits wrong fish.
7. `scoreBefore = 100`. `score = ClampScore(100 - 25) = 75`. `actualDeduction = 25`. `ShowSad(25)` → `sadFeedbackText.text = "-25!"`. Correct feedback shown.

---

### Session 4 — Achievement XP Level-Up (TASK-02a verification)

**Profile:** Level 4, XP = 750. Level 5 threshold = 800. `combo_5` not yet unlocked.

**Flow:**

1. `StartGame()`. Player builds 5-combo. `maxComboStreak = 5`.
2. `EndGame()`. `xpEarned = ProgressionRules.XpForResult(8, 88, 0, 5) = 80 + 10 + 88 + 0 = 178`. `ProgressionStore.AddXp(178)` → XP = 928. Level 5 threshold crossed. `newLevel = 5`.
3. `AchievementChecker.CheckAll(this, achievementCatalog, out int achievementLevel)` evaluates.
   - `Combo5: maxComboStreak >= 5 → true`. Not yet unlocked. `AchievementStore.Unlock("combo_5")`. `ProgressionStore.AddXp(xpReward)` — return value captured as `lvUp`. If `xpReward = 50`, XP = 978. Level 5 max. Still Level 5. `lvUp = 0`. `achievementLevel = 0`.
   - Hypothetical: if player were at XP = 1100, Level 5 = 800, Level 6 = `round(100 × 5^1.5) = 1118`. `AddXp(50)` → XP = 1150. Level 6. `lvUp = 6`. `achievementLevel = 6`. `if (6 > newLevel) newLevel = 6`.
4. `StaggerResultCelebrations(isRecord, newLevel)` runs with correct merged `newLevel`. `levelUpPanel` shown at +0.8 s. `ApplyLevelReward` called in coroutine. **TASK-02a confirmed — no silent level-up from achievement XP.**

---

### Session 5 — Colour-Blind Player with FishCatalog (STILL BROKEN — BUG-NEW-3)

**Profile:** Level 4, `ColourBlindSettings.IsEnabled() = true`, `FishCatalog` assigned with species using `baseColor` outside the four-value palette.

**Flow:**

1. `FishSpawner.SpawnFish()` → `catalog.PickRandom()` returns species with `baseColor = new Color(0.85f, 0.42f, 0.15f)` (warm orange).
2. `FishShapeOverlay.Start()` → `ColourBlindSettings.ShapeForColor(fishTarget.fishColor)`. Hex = `"D96B26"`. Switch at `ColourBlindSettings.cs:22-27` only handles `FF0000`, `00FF00`, `0000FF`, `FFFF00`. Default case → `"?"`.
3. Shape overlay shows `"?"`. Target label shows `"D96B26"` (raw hex from `Dict.cs:35` fallback).
4. Player cannot identify target fish. **BUG-NEW-3 NOT FIXED. Confirmed still open. This was deferred to Week 10 in ITERATION_week9_SCOPE.md.**

---

### Session 6 — UI-Overlap Touch During AR Plane Detection (BUG-W8-2 still open)

**Profile:** First-time player, `PlaceWaterOnPlane.enabled = true`. Camera sees detected AR plane.

**Flow:**

1. Player taps the "Start" button (or any HUD button) positioned over a detected AR plane.
2. `PlaceWaterOnPlane.Update()` fires `foreach (Touch touch in Input.touches)`. `touch.phase == TouchPhase.Began`.
3. `EventSystem.current.IsPointerOverGameObject(touch.fingerId)` — NOT called. AR raycast fires unconditionally. `ArRaycast` hit → water placed at tap position. Simultaneously, button `OnClick()` fires.
4. Water placed at Start-button location. `enabled = false` (one-shot guard). Cannot reposition.
5. **BUG-W8-2 NOT FIXED. Confirmed still open. Deferred to Week 10.**

---

### Session 7 — High-Difficulty Fish Boundary (BUG-W8-3 still open)

**Profile:** Level 7, correct hits = 18. `FishCountForDifficulty(18) = 7`. Fish spawned across ±1.5 m spawn area.

**Flow:**

1. `FishSpawner.SpawnFish()` instantiates fish at random positions within ±1.5 m offset from waterPlane.
2. Fish at spawn position `(1.4, -0.2, 1.4)` relative to water. `FishSwim.swimCenter = transform.position` (spawn pos, not water centre).
3. `FishSwim.horizontalRadius = spawnRadius = 1.5 m`. Fish roams within 1.5 m of its spawn corner → effective reach from water centre = `sqrt(1.4² + 1.4²) + 1.5 = 1.98 + 1.5 = 3.48 m`.
4. In a 3 m × 3 m room (typical AR play space), this fish is permanently outside the player's reachable area.
5. **BUG-W8-3 NOT FIXED. Confirmed still open. Deferred to Week 10.**

---

## Bugs Found

### Confirmed Still Open (from Week 8 / Week 7)

- [ ] **BUG-NEW-3 (HIGH)** — `ColourBlindSettings.ShapeForColor` returns `"?"` for any `FishSpecies.baseColor` outside the four-entry switch; `Dict.cs` falls back to raw hex for same colours; colour-blind mode non-functional with FishCatalog — `ColourBlindSettings.cs:22-27`, `Dict.cs:35` — week 10 candidate: add approximate-colour matching or a per-species `accessibilityShape` field in `FishSpecies`
- [ ] **BUG-W8-2 (MED)** — `PlaceWaterOnPlane.Update()` does not check `EventSystem.current.IsPointerOverGameObject(touch.fingerId)` before AR raycast; UI-button taps accidentally place water — `PlaceWaterOnPlane.cs:18-38` — week 10 candidate: two-line guard addition
- [ ] **BUG-W8-3 (MED)** — `FishSwim.swimCenter` uses fish spawn position; at high difficulty, corner-spawned fish roam outside typical AR play spaces — `FishSwim.cs:34`, `FishSpawner.cs:72` — week 10 candidate: pass `waterPlane.transform.position` to fish at spawn and use that as `swimCenter`

### Confirmed Fixed (Week 9)

- **BUG-W8-1 FIXED** — `GoalManager.CompleteGoal()` null-dereference on `m_OnboardingGoals` for returning players; now guarded by `IsCoachingActive` property and null-check at line 209 — all returning-player launches succeed without NullReferenceException.
- **BUG-NEW-1 FIXED** — `AchievementChecker.CheckAll` discarded `ProgressionStore.AddXp` return value; now captured via `out int levelGained` overloads and merged into `newLevel` before `StaggerResultCelebrations` — level-up from achievement XP now shows `levelUpPanel` and applies `LevelRewardTable` reward.
- **BUG-NEW-2 FIXED** — `DailyChallenge.TryClaimDailyBonus` discarded `ProgressionStore.AddXp` return value; `TombakanOnboarding` never called `ApplyLevelReward` on daily-bonus level-up; both gaps closed — daily-bonus level reward (coins, species unlock, spear skin) now applied on every level-up path.
- **BUG-NEW-4 FIXED** — `ShowSad()` always showed `"-25!"` even when score floor absorbed the penalty; `ShowSad(actualDeduction)` now returns early when `actualDeduction <= 0`, suppressing misleading feedback entirely.

---

## UX Gaps

- [ ] **UX-1** *(known, still open)* — No throw-mechanic tutorial for first-time players; after coaching ends player has no hint that there is a throw button — week 10 candidate: 10 s hint panel ("Tekan tombol untuk melempar!") in `GameManager.StartGame()` for first game only
- [ ] **UX-NEW-3** *(still open)* — Achievement toast fires at +1.2 s, only 0.4 s after level-up panel at +0.8 s; both visible simultaneously — `GameManager.cs:344,354` — week 10 candidate: delay toast start to +4.0 s or add tap-to-dismiss on level-up panel
- [ ] **UX-NEW-4** *(still open)* — `ColorHexLocalization.ToIndonesian` falls back to raw hex for non-palette species `baseColor`; target label shows hex code — week 10 candidate: use `FishSpecies.displayName` as target label when a catalog is active

## Polish Opportunities

- [ ] **POLISH-1** *(still open)* — Level-up panel has no auto-dismiss; achievement toast can appear on top of it — add 2 s auto-dismiss or tap-to-continue
- [ ] **POLISH-2** *(still open)* — `HapticFeedback.PlayCorrect` and `PlayWrong` both call `Handheld.Vibrate()` identically — implement distinct patterns (Android: `AndroidJavaObject("android.os.Vibrator")`)
- [ ] **POLISH-3** *(still open)* — `ApplyLevelReward` may overwrite `levelUpText.text` with `reward.celebrationText` before player finishes reading "Level N! Selamat!" — use a dedicated `rewardText` element or apply with a 1 s delay
- [ ] **POLISH-NEW-1** *(still open)* — `ColorSummary.Format` shows `×N` even when N=1 — suppress when N=1: `sb.Append(counts[name] > 1 ? $" ×{counts[name]}" : "")`
- [ ] **POLISH-NEW-2** *(still open)* — `ProgressionHUD.Refresh()` not called after daily bonus XP; XP bar stale on main screen — call `ProgressionHUD.I?.Refresh()` from `TombakanOnboarding` after `ShowDailyBonus`

---

## Week 10 Candidates

| Priority | Item | Source |
|----------|------|--------|
| HIGH | BUG-NEW-3 — colour-blind mode fix for FishCatalog (`ShapeForColor` approximate matching or per-species `accessibilityShape` field) | BUG-NEW-3 |
| MED | BUG-W8-2 — `PlaceWaterOnPlane` UI-touch guard (`EventSystem.IsPointerOverGameObject`) | BUG-W8-2 |
| MED | BUG-W8-3 — `FishSwim.swimCenter` anchored to waterPlane centre | BUG-W8-3 |
| MED | UX-NEW-3 — achievement toast delay to +4.0 s or tap-to-dismiss on level-up panel | UX-NEW-3 |
| LOW-MED | UX-1 — throw-mechanic tutorial hint panel (first game only) | UX-1 |
| LOW | POLISH-NEW-2 — `ProgressionHUD.Refresh()` after daily bonus | POLISH-NEW-2 |
| LOW | POLISH-NEW-1 — suppress `×1` in `ColorSummary.Format` | POLISH-NEW-1 |
