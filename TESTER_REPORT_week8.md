# Tester Report — Week 8

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `GoalManager.CompleteGoal` dereferences `m_OnboardingGoals.Count` when the queue is null; `ForceCompleteGoal()` is called by `TombakanOnboarding.Start()` for every returning player before `StartCoaching()` ever initialises the queue — guaranteed `NullReferenceException` crash on every returning-player launch | `GoalManager.cs:202`, `TombakanOnboarding.cs:30` | CRITICAL: all returning players crash immediately on launch when `goalManager` is wired; game is unplayable on every session after the first |
| 2 | `AchievementChecker.CheckAll` discards the return value of `ProgressionStore.AddXp` at line 100; any level-up triggered by achievement XP is silent and `LevelRewardTable` rewards are not applied | `AchievementChecker.cs:100` | HIGH: player crosses a level boundary via achievement XP → level increments in data but `levelUpPanel` never shows and no reward (coins, species, spear skin) is granted |
| 3 | `DailyChallenge.TryClaimDailyBonus` discards the return value of `ProgressionStore.AddXp` at line 58; `TombakanOnboarding` detects the level-up via before/after comparison but never calls `ApplyLevelReward` — level rewards are always skipped on daily-bonus level-ups | `DailyChallenge.cs:58`, `TombakanOnboarding.cs:39-44` | HIGH: daily-bonus level-up is shown in text but the associated reward (soft currency, species unlock, spear skin) is silently skipped every time it occurs |
| 4 | `ColourBlindSettings.ShapeForColor` handles only four exact hex values (`FF0000`, `00FF00`, `0000FF`, `FFFF00`); `FishSpecies.baseColor` is a free-form Color field and the starter catalog defines custom colours outside that palette — shape overlay shows `"?"` for the majority of catalog species; `ColorHexLocalization.ToIndonesian` also falls back to the raw hex string for the same colours, so the target label shows e.g. `"E6993F"` instead of an Indonesian name | `ColourBlindSettings.cs:22-27`, `Dict.cs:35` | HIGH: colour-blind mode is fully broken for any game session that uses `FishCatalog`; accessibility-critical feature produces unreadable output on nearly every species |
| 5 | `GameManager.ShowSad` always texts `"-{penaltyPerWrongHit}!"` regardless of whether `ClampScore` absorbed the full penalty; when score is already 0 every wrong hit shows `"-25!"` but the displayed score never changes | `GameManager.cs:507` | MED: misleading feedback on every wrong hit at score = 0; players think the game is ignoring their mistakes |
| 6 | `PlaceWaterOnPlane.Update()` processes any `TouchPhase.Began` touch without checking `EventSystem.current.IsPointerOverGameObject(touch.fingerId)`; a tap on a UI button that overlaps with a detected AR plane simultaneously triggers the button action AND places water at the tap position | `PlaceWaterOnPlane.cs:18-38` | MED: first-time player tapping the Start button (or any HUD element) during the plane-detection phase can accidentally place water at an unintended location — water cannot be re-positioned (one-shot guard from Week 5) |
| 7 | `FishSwim.swimCenter` is set from the fish's instantiation position (not the waterPlane centre); fish spawned at the corner of the ±1.5 m spawn square have their swim origin up to 2.12 m from the waterPlane centre; with `horizontalRadius = spawnRadius = 1.5 m`, those fish can legitimately swim 3.6 m from the play area centre — well outside a typical home AR space at max difficulty (7 fish) | `FishSwim.cs:34`, `FishSpawner.cs:72` | MED: at high difficulty (10+ correct hits) several fish are unreachable; player cannot complete rounds; perceived as game freeze |
| 8 | `TombakanOnboarding.Start()` activates `greetingPanel` (line 34) then immediately calls `DailyChallenge.TryClaimDailyBonus` which activates `dailyBonusPanel` (line 50); both panels can be active simultaneously on first-ever launch, overlapping on small screens | `TombakanOnboarding.cs:33-44` | MED: first-time player sees two overlapping UI panels at launch; greeting is obscured by daily-bonus text; confusing onboarding |

---

## Session Simulations

### Session 1 — First-time Player (fresh install, no prior data)

**Profile:** `ScoreStore.GetBest() = 0`, `ProgressionStore.GetTotalXp() = 0`, no AR experience.

**Flow:**

1. App launches. `GameManager.Start()` → `AudioManager.I.PlayMainBGM()`. `RefreshProgressionHUD()` shows "Lv 1", XP bar empty.
2. `TombakanOnboarding.Start()`: `isReturningPlayer = false` → `greetingPanel.SetActive(true)`.
3. `DailyChallenge.TryClaimDailyBonus(out xp, out streak)`: `LastPlayedKey = ""`, `IsNewDay("", Today) = true`. `streak = 1`, `xpAwarded = 125` (100 + 25). `ProgressionStore.AddXp(125)` at `DailyChallenge.cs:58` — return value discarded. XP = 125, player is now Level 2. `levelAfter = 2 > levelBefore = 1` → `ShowDailyBonus(125, 1, 2)` → `dailyBonusPanel.SetActive(true)`.

   **BUG confirmed (W8-RANK-8):** `greetingPanel` and `dailyBonusPanel` both active simultaneously. On a 6-inch phone, the daily bonus panel covers the greeting dismiss button.

   **BUG confirmed (W8-RANK-3):** `LevelRewardTable.GetRewardForLevel(2)` is never called. Any Level-2 reward (coin bonus, species unlock) is skipped.

4. Player scans floor. `PlaceWaterOnPlane.Update()` — player taps near the main UI. **BUG confirmed (W8-RANK-6):** If a UI button overlaps with a detected AR plane, the tap both activates the button and places water at an unintended position. Water cannot be repositioned.

5. Player taps a valid floor surface away from UI. Water placed. `DisableARPlanes()` fires. Component disabled.

6. Player presses Start. `GameManager.StartGame()` — 3 fish spawn. Target label shows "Merah" (Color.red is in Dict.cs). Player sees coloured swatch + label. **UX-1 (still open):** No instruction on how to throw. Player taps background — nothing happens (PlaceWaterOnPlane disabled). Player discovers throw button by exploration.

7. Player throws correctly. `+100`. Combo 1. Correct label: "+100!". HapticFeedback fires.

8. Timer expires at 60s. `EndGame()`. Score = 100, correct = 1. `Accuracy.Format(1, 0) = "1/1 (100%)"`. Tier = TierLow. `xpEarned = ProgressionRules.XpForResult(1, 100, 0, 1) = 10 + 0 + 100 + 0 = 110`. `ProgressionStore.AddXp(110)` total = 235, still Level 2. `newLevel = 0`. No level-up panel. `resultXpText = "+110 XP"`. `StaggerResultCelebrations(false, 0)` — no badge, no level-up panel. Correct.

---

### Session 2 — Returning Player (3rd session, daily login) [CRASH SESSION]

**Profile:** `ScoreStore.GetBest() = 400`, `ProgressionStore.GetTotalXp() = 85`, Level 1. Daily bonus pending.

**Flow:**

1. App launches. `TombakanOnboarding.Start()`: `isReturningPlayer = true` → line 30: `goalManager.ForceCompleteGoal()`.

2. **BUG confirmed (W8-RANK-1 — CRASH):** `GoalManager.ForceCompleteGoal()` → `CompleteGoal()` at line 195. `m_CurrentGoal` is the default struct (zero-initialised): `CurrentGoal = OnboardingGoals.Empty`, `Completed = false`. Line 197: `m_CurrentGoal.CurrentGoal == TapSurface → false`. Line 200-201: `m_CurrentGoal.Completed = true; m_CurrentGoalIndex++ → 1`. Line 202: `if (m_OnboardingGoals.Count > 0)` — **`m_OnboardingGoals` is null** (never set; `StartCoaching()` has not been called for this returning player). `NullReferenceException` thrown at line 202. Unity logs the exception and the scene may continue with broken state or the app may freeze depending on Unity error handling settings.

3. Simulation stops here. The daily-bonus check at `TombakanOnboarding.cs:38-44` never runs. Daily bonus is not claimed. The main menu does not appear correctly. The player restarts or deletes the app.

**Reproduction:** Install app, play one game (score > 0), close, reopen. Crash on second launch when `goalManager` is wired.

---

### Session 3 — Casual Player (5th session, returning, daily login — goalManager NOT wired)

**Profile:** `goalManager` reference is null in `TombakanOnboarding`, so crash is avoided. `ScoreStore.GetBest() = 650`, Level 3, XP = 290.

**Flow:**

1. `TombakanOnboarding.Start()`: `isReturningPlayer = true`, `goalManager == null` → `ForceCompleteGoal()` not called. No crash.

2. `DailyChallenge.TryClaimDailyBonus()`: new day → `xpAwarded = 125` (streak 2: 100 + 2×25 = 150). `ProgressionStore.AddXp(150)` — XP = 440. Level stays at 3 (Level 4 needs `round(100 × 3^1.5) = 520` XP). `levelAfter = 3 = levelBefore = 3`. `newLevel = 0`. `ShowDailyBonus(150, 2, 0)` → panel text: "Bonus harian +150 XP!\nStreak 2 hari berturut-turut!". No level-up text. Correct.

3. `ProgressionHUD.OnEnable()` fired on scene init. HUD shows "Lv 3". After `DailyChallenge.TryClaimDailyBonus`, HUD still shows "Lv 3" with old XP ratio. **POLISH-NEW-2 (still open):** `ProgressionHUD.Refresh()` is not called after daily bonus. XP bar on main screen is stale until the HUD panel is re-enabled.

4. Player starts game. 4 fish (correct = 0 → `FishCountForDifficulty(0) = 3`, then after first hit `correct = 1 → 3`, then after 3 correct `correct = 3 → 4`). Difficulty ramp working.

5. Player hits 5 correct fish in a row. `comboStreak = 5`, `maxComboStreak = 5`. Timer extends by `0.5 × 3 = 1.5s` per hit (combo multiplier 3). Total time bonus: ~7.5s.

6. Timer expires. `EndGame()`. `correctHitCount = 5`, `wrongHitCount = 1`. `Accuracy.Format(5, 1) = "5/6 (83%)"`. `xpEarned = ProgressionRules.XpForResult(5, 83, 0, 5) = 50 + 10 + 83 + 0 = 143`. `ProgressionStore.AddXp(143)` → XP = 583 → Level 4 threshold = 520. Level-up! `newLevel = 4`. `StaggerResultCelebrations(false, 4)` — badge at +0.4s (no record), level-up panel at +0.8s.

7. `AchievementChecker.CheckAll()`: `Combo5: maxComboStreak >= 5 → true`. First time. `AchievementStore.Unlock("combo_5")`. `ProgressionStore.AddXp(xpReward)` — **BUG confirmed (W8-RANK-2):** return value discarded at `AchievementChecker.cs:100`. If `xpReward = 75` (hypothetical), XP = 658. Level 4 max = 520, Level 5 = `round(100 × 4^1.5) = 800`. Still Level 4. No level-up from achievement XP this time. But if xpReward were larger, silent level-up would occur.

8. `ShowAchievementsSequenced(["combo_5"], catalog)` — starts at +1.2s. Level-up panel shows at +0.8s. First achievement toast fires at +1.2s — **UX-NEW-3 (still open):** toast appears only 0.4s after level-up panel, interrupting the player reading their level-up message.

---

### Session 4 — Speed Runner (high-level player, maximising score)

**Profile:** Level 6, XP = 820, `ScoreStore.GetBest() = 2200`. Goal: 15+ correct hits in 60s.

**Flow:**

1. Game starts. Timer at 60s. Fish count begins at 3.

2. Player throws rapidly. Correct hit at t=2s. `LockThrow(delay)` called. `PacingRules.HitDelayForProgress(2.2, 1) = 2.1`. `Invoke(PickNewTarget, 2.9)`. Player waits.

3. At correct = 5: `FishPalette.CountForProgress(5) = 4`. Fourth colour (Kuning) enters active set. Fish spawned with 4 possible colours. Difficulty confirmed ramping.

4. At correct = 10: `fishSpawner.fishCount = FishCountForDifficulty(10) = 6`. Six fish spawned per round.

5. **BUG confirmed (W8-RANK-7):** At `fishCount = 6`, fish are spawned within ±1.5m. Fish spawned at a corner position, e.g., `(1.4, -0.2, 1.4)` relative to water, have `swimCenter` at that corner. They swim within 1.5m of that corner: effective max reach = 2.1 + 1.5 = 3.6m from waterPlane centre. In a 2.5×2.5m room, some fish are unreachable. Player cannot complete rounds cleanly.

6. Timer runs to 0. `gameRunning = false`. `EndGame()`. Score = 1800, correct = 14, wrong = 2. `Accuracy = "14/16 (87%)"`. `xpEarned = 140 + 10 + 87 + 0 = 237`. `ProgressionStore.AddXp(237)` — XP = 1057. Level 6 needs `round(100 × 5^1.5) = 1118`. Level stays 6. `newLevel = 0`. No level-up panel. Correct.

7. `isRecord = ScoreStore.TrySetBest(1800) = false` (best was 2200). Badge not shown. Correct.

---

### Session 5 — Frustrated Player (3+ wrong hits, score 0)

**Profile:** Level 2, casual player who consistently misidentifies target colour.

**Flow:**

1. `StartGame()`. Target colour = Hijau (Color.green). Fish spawn: 1 green, 2 decoys.

2. Hit 1 (wrong fish, blue): `OnFishHit(Color.blue, "")`. `correct = false`. `comboStreak = 0`. `wrongHitCount = 1`. `score = ClampScore(0 - 25) = 0`. `ShowSad()`.

   **BUG confirmed (W8-RANK-5):** `sadFeedbackText.text = "-25!"` even though the displayed score did not change (clamped from 0 to 0). Player sees penalty text for a penalty that had no visible effect.

3. Hit 2 (wrong fish): `wrongHitCount = 2`. Score = 0. `ShowSad()` again. `"-25!"`. Same misleading feedback.

4. Hit 3 (wrong fish): `wrongHitCount = 3`. `"-25!"` again.

5. Player hits correct fish: `comboStreak = 1`. `score = 100`. `ShowHappy(100, 1)` → "+100!". `timeLeft += 0.5`.

6. Timer expires. `Accuracy.Format(1, 3) = "1/4 (25%)"`. TierLow (correct = 1, threshold = 1-4). `xpEarned = ProgressionRules.XpForResult(1, 25, 0, 1) = 10 + 0 + 25 + 0 = 35`. `resultXpText = "+35 XP"`. Correct.

---

### Session 6 — Colour-blind Player (catalog active, accessibility mode on)

**Profile:** Level 4, `ColourBlindSettings.IsEnabled() = true`. `FishCatalog` assigned with species whose `baseColor` are custom values.

**Flow:**

1. `StartGame()`. `PickNewTarget()`. `FishPalette.CountForProgress(0) = 3`. `FishSpawner.SpawnFish(Color.red, 3)` called. `catalog.PickRandom()` returns a species with `baseColor = new Color(0.85f, 0.42f, 0.15f)` (warm orange, an artistic fish).

2. `resolvedTargetColor = species.baseColor`. `targetColor` updated. `targetColorImage.color = (0.85f, 0.42f, 0.15f)`.

3. `targetColorLabel.text = ColorHexLocalization.ToIndonesian(targetColor)`. `ColorUtility.ToHtmlStringRGB` → `"D96B26"`. Not in Dict.cs map (only 20 standard entries). **BUG confirmed (W8-RANK-4 / UX-NEW-4):** Target label shows raw hex `"D96B26"` instead of an Indonesian name. Player reads a hex code.

4. `FishShapeOverlay.Start()` fires on each spawned fish. `ColourBlindSettings.IsEnabled() = true`. `ColourBlindSettings.ShapeForColor(fishTarget.fishColor)` where `fishTarget.fishColor = (0.85f, 0.42f, 0.15f)`. Hex = `"D96B26"`. Switch default → `"?"`. **BUG confirmed (W8-RANK-4 / BUG-NEW-3):** Shape overlay shows `"?"` on every fish (both target and decoys, because decoy species also have non-palette colors from the catalog).

5. Player cannot identify target fish by either the shape overlay (all `"?"`) or the target label (hex string). Colour-blind mode is completely non-functional.

---

### Session 7 — Achievement Hunter (veteran player, first achievement unlock)

**Profile:** Level 5, XP = 805. Level 6 threshold = `round(100 × 5^1.5) = 1118`. Remaining = 313 XP. `combo_5` not yet unlocked.

**Flow:**

1. `StartGame()`. Player builds a 5-hit combo. `maxComboStreak = 5`.

2. Timer expires. `EndGame()`. `correctHitCount = 9`, `wrongHitCount = 1`. `xpEarned = ProgressionRules.XpForResult(9, 90, 0, 5) = 90 + 10 + 90 + 0 = 190`. `ProgressionStore.AddXp(190)` → XP = 995. Level threshold for 6 = 1118. No level-up. `newLevel = 0`.

3. `AchievementChecker.CheckAll()`: `Combo5: maxComboStreak >= 5 → true`. Not yet unlocked. `AchievementStore.Unlock("combo_5")`. `achievement.xpReward = 100` (hypothetical catalog value). `ProgressionStore.AddXp(100)` at line 100 — **BUG confirmed (W8-RANK-2):** return value discarded. XP = 1095. Still below Level 6 threshold (1118). No level-up yet. (Close call — 23 XP from triggering the silent-level-up scenario.)

4. `newlyUnlocked = ["combo_5"]`. `ShowAchievementsSequenced(["combo_5"], catalog)` — wait 1.2s, show toast for 2s.

5. **UX-NEW-3:** Level-up panel (if triggered) would overlap with toast. In this session, no level-up, so only toast shown. Toast correctly shows `achievement.titleIndonesian` for 2s. Week-6 fix confirmed working.

---

### Session 8 — Player who discovers the Fishipedia (Level 3, FishCatalog assigned)

**Profile:** Level 3, `fishdexUnlockedCount = 4` (4 species previously caught). `species_collector_5` not yet unlocked.

**Flow:**

1. Plays game. Catches a species for the first time. `FishdexStore.Unlock(speciesId) = true`. `newSpeciesThisGame.Add(speciesId)` → count = 1.

2. `EndGame()`. `xpEarned = ProgressionRules.XpForResult(5, 80, 1, 3) = 50 + 5 + 80 + 50 = 185`. XP earned includes 50 for new species. `ProgressionStore.AddXp(185)` → XP grows. Assume no level-up.

3. `AchievementChecker.CheckAll()`: `SpeciesCollector5: FishdexStore.UnlockedCount() >= 5 → true` (was 4, now 5 with new catch). First unlock. `AchievementStore.Unlock("species_collector_5")`. `ProgressionStore.AddXp(xpReward)` — discarded. If `xpReward = 150` and the game XP put player just below the next level, achievement XP crosses it silently. **BUG-NEW-1 path active.**

4. `CollectSummary()`: `collectedSpeciesIds.Count = 1 > 0` and `fishSpawner.catalog != null` → uses species-name path. Shows `"Ikan Badut"` (for example). Correct.

---

### Session 9 — Zen Mode Player

**Profile:** `currentMode = GameMode.Zen`. Player wants a relaxed session.

**Flow:**

1. `StartGame()`. `timeLeft = float.MaxValue`. `timerCountdownText.text = "--"`. `timerBarFill.fillAmount = 1f`.

2. `Update()`: `currentMode == Zen` → `timerBarFill.fillAmount = 1f; return`. No countdown, no warning pulse. Timer state never written.

3. Player hits correct fish. `timeLeft += TimeBonus.ForHit(comboStreak)` — adding to `float.MaxValue` is no-op (float saturation). Fine.

4. Player hits wrong fish. `ShowSad()`. **BUG confirmed (W8-RANK-5):** If score is already 0, `"-25!"` shown despite no actual deduction. Same as Session 5.

5. Player presses End Game button. `EndGameManual()` → `gameRunning = false` → `EndGame()`. Result screen shows correctly.

6. **Observation:** `PickNewTarget` may have been `Invoke`d before `EndGameManual()`. The guard at `GameManager.cs:398` (`if (!gameRunning) return;`) prevents any stale `PickNewTarget` from running post-EndGame. Week-5 fix confirmed working.

---

### Session 10 — Edge Case: Rapid Correct Hits (LockThrow race condition probe)

**Profile:** Level 7. Spear cooldown = 1.2s. Hit delay = 1.0s (minimum). Player aims to spam correct fish.

**Flow:**

1. Player throws at t=0s. Spear in flight. `CooldownRoutine` running (1.2s).

2. Spear hits correct fish at t=0.4s (0.8s into cooldown). `OnFishHit` → `LockThrow(1.0)`. `SpearThrower.LockThrow` → `StopAllCoroutines()` cancels `CooldownRoutine` (which had 0.8s remaining). `LockRoutine(1.0)` starts. After 1.0s, `canThrow = true`, `spearFake.SetActive(true)`.

3. **Week 7 Bug 6 still open but benign in this path:** `StopAllCoroutines` kills the running `CooldownRoutine`. The `LockRoutine` effectively replaces it. Player can throw again after 1.0s (lock delay) rather than the original 0.8s remaining cooldown. Behaviour is slightly more restrictive (extra 0.2s lock). No permanent lock occurs here.

4. Edge case probe: if `LockThrow` is called while `LockRoutine` is already running (possible only if two `OnFishHit` calls fire within `lockDelay` of each other, which pacing rules normally prevent). `StopAllCoroutines()` kills the first `LockRoutine` mid-flight with `canThrow = false`, `spearFake` off. New `LockRoutine` starts and correctly re-enables both after its delay. No permanent lock.

5. **Conclusion:** The week-7 Bug 6 race condition cannot produce a permanent throw lock in this game's pacing constraints. Severity downgraded to LOW for this report.

---

## Bugs Found

### New Bugs (not in Week 7 report)

- [ ] **BUG-W8-1 — CRITICAL** — `GoalManager.CompleteGoal()` dereferences `m_OnboardingGoals.Count` at line 202 when the queue is null; `ForceCompleteGoal()` is called by `TombakanOnboarding.Start()` at line 30 for all returning players before `StartCoaching()` ever creates the queue — guaranteed `NullReferenceException` on every returning-player app launch — `GoalManager.cs:202`, `TombakanOnboarding.cs:30` — reproduction: play any game, exit, relaunch; crash occurs in `TombakanOnboarding.Start()` if `goalManager` is wired

- [ ] **BUG-W8-2 — MED** — `PlaceWaterOnPlane.Update()` does not call `EventSystem.current.IsPointerOverGameObject(touch.fingerId)` before processing the touch for AR raycast; a tap on a UI button overlapping a detected AR plane fires both the button event and places water — `PlaceWaterOnPlane.cs:18-38` — reproduction: during plane detection, tap any UI button while the camera is pointed at a detected AR plane; water appears at the button tap position; one-shot guard (enabled = false) then prevents any correction

- [ ] **BUG-W8-3 — MED** — `FishSwim.swimCenter` is the fish's spawn position, not the waterPlane centre; fish spawned at the corner of the ±1.5 m spawn square have their swim centre up to 2.12 m from the waterPlane; with `horizontalRadius = spawnRadius = 1.5 m` these fish can be 3.6 m from the play area centre, outside most home AR play spaces at high difficulty (7 fish) — `FishSwim.cs:34`, `FishSpawner.cs:37-43,72` — reproduction: reach 15+ correct hits (7 fish); observe fish consistently swimming to the boundary of the AR view and beyond

### Confirmed Still Open (from Week 7)

- [ ] **BUG-NEW-1** (Week 7) — `AchievementChecker.cs:100` — `ProgressionStore.AddXp` return value discarded; silent level-up and missed level rewards on achievement unlock — STILL OPEN
- [ ] **BUG-NEW-2** (Week 7) — `DailyChallenge.cs:58` + `TombakanOnboarding.cs:39-44` — daily-bonus level-up never calls `ApplyLevelReward`; all level rewards silently skipped — STILL OPEN
- [ ] **BUG-NEW-3** (Week 7) — `ColourBlindSettings.cs:22-27` — `ShapeForColor` returns `"?"` for non-palette `FishSpecies.baseColor`; colour-blind mode broken with FishCatalog — STILL OPEN
- [ ] **BUG-NEW-4** (Week 7) — `GameManager.cs:507` — `ShowSad()` shows `"-25!"` even when clamp absorbs the penalty at score = 0 — STILL OPEN
- [ ] **BUG-NEW-5** (Week 7) — `GoalManager.cs:202`, `TombakanOnboarding.cs:30` — upgraded to CRITICAL in this report (BUG-W8-1 above); confirmed NullReferenceException path

### Confirmed Fixed (Week 6 and prior)

- **BUG-1 FIXED** — `OnFishHit` guard `if (!gameRunning) return;` prevents post-EndGame score mutation — verified again in Sessions 9 and 10.
- **HapticFeedback decoupled FIXED** — vibration fires independently of mute state — confirmed in current `HapticFeedback.cs` (no AudioPrefs reference).
- **Week 5 PickNewTarget guard FIXED** — stale Invoke cannot fire after EndGame.

---

## UX Gaps

- [ ] **UX-1** *(known, still open)* — No throw-mechanic tutorial for first-time players; player taps background expecting something to happen and gets no feedback — add a 10-second hint panel ("Tekan tombol untuk melempar!") in `GameManager.StartGame()` or via `TombakanOnboarding` on first game
- [ ] **UX-NEW-2** *(still open)* — `greetingPanel` and `dailyBonusPanel` active simultaneously on first-ever launch; `greetingPanel.SetActive(true)` at `TombakanOnboarding.cs:34` and `dailyBonusPanel.SetActive(true)` at line 50 both run in the same `Start()` call — show greeting first; trigger daily bonus panel only after greeting is dismissed (from `DismissGreeting()`) or add mutual-exclusion logic
- [ ] **UX-NEW-3** *(still open)* — Achievement toast fires at +1.2s after `EndGame`, only 0.4s after the level-up panel appears at +0.8s; player cannot read the level-up message before the toast overlaps it — delay toast start to at least +4.0s or require player to dismiss the level-up panel first
- [ ] **UX-NEW-4** *(still open)* — `ColorHexLocalization.ToIndonesian` falls back to raw hex string for any `FishSpecies.baseColor` not in the 20-entry map; target label shows e.g. `"D96B26"` — either extend Dict.cs to cover all catalog species colours, or suppress the colour label and use `targetSpeciesLabel` exclusively when a catalog is active

---

## Polish Opportunities

- [ ] **POLISH-1** *(still open)* — Level-up panel has no auto-dismiss; achievement toast appears 0.4s later and overlaps — add a 2s auto-dismiss or tap-to-continue on `levelUpPanel` so the toast starts on a clean screen
- [ ] **POLISH-2** *(still open)* — `HapticFeedback.PlayCorrect()` and `PlayWrong()` both call `Handheld.Vibrate()` identically; no tactile distinction — implement Android-specific duration (75ms pulse for correct, 30ms-gap-30ms double for wrong) via `AndroidJavaObject("android.os.Vibrator")`
- [ ] **POLISH-3** *(still open)* — `ApplyLevelReward` at `GameManager.cs:387` overwrites `levelUpText.text` with `reward.celebrationText` potentially before player finishes reading the initial "Level N! Selamat!" — use a dedicated `rewardText` UI element or apply `celebrationText` with a 1s delay
- [ ] **POLISH-NEW-1** *(still open)* — `ColorSummary.Format` appends `×N` for every count including 1, producing `"Merah ×1"` instead of `"Merah"` — fix at `ColorSummary.cs:41`: `sb.Append(name).Append(counts[name] > 1 ? $" ×{counts[name]}" : "")`
- [ ] **POLISH-NEW-2** *(still open)* — `ProgressionHUD.Refresh()` is triggered only on `OnEnable`; daily-bonus XP granted in `TombakanOnboarding.Start()` leaves the main-screen XP bar stale until the panel is toggled — call `ProgressionHUD.Refresh()` (or expose a static refresh event) from `TombakanOnboarding` immediately after `ShowDailyBonus`

---

## Validation Week 9

Tasks sourced from `ITERATION_week9_SCOPE.md`. Each changed file was traced against the acceptance criteria in that scope document.

| Task ID | Status | Evidence |
|---------|--------|----------|
| TASK-01 | PASS | `GoalManager.cs:188` adds `public bool IsCoachingActive => m_OnboardingGoals != null && !m_AllGoalsFinished;`. `GoalManager.cs:209` already guards `CompleteGoal()` with `if (m_OnboardingGoals != null && m_OnboardingGoals.Count > 0)`. `TombakanOnboarding.cs:38-39` wraps the `ForceCompleteGoal()` call in `if (goalManager.IsCoachingActive)` — on a returning-player cold launch `m_OnboardingGoals` is null, so `IsCoachingActive` is false, `ForceCompleteGoal()` is skipped entirely, and no `NullReferenceException` can occur. Acceptance criterion met: main menu appears correctly for all returning players. |
| TASK-02 | PASS | (a) `AchievementChecker.cs:56,103` — new `out int levelGained` overloads added; inner loop at line 143 captures `int lvUp = ProgressionStore.AddXp(achievement.xpReward)` and tracks the highest level-up with `if (lvUp > levelGained) levelGained = lvUp;` — return value is no longer discarded. `GameManager.cs:288-289` calls `CheckAll(this, achievementCatalog, out int achievementLevel)` and merges with `if (achievementLevel > newLevel) newLevel = achievementLevel` before `StaggerResultCelebrations` runs. (b) `DailyChallenge.cs:43` signature now includes `out int newLevel`; line 72 assigns `newLevel = ProgressionStore.AddXp(xpAwarded)` — return value captured. (c) `TombakanOnboarding.cs:49` calls the 3-out-param overload; lines 65-66 and 106-107 call `GameManager.I?.ApplyLevelReward(newLevel)` when `newLevel > 0` on both the immediate and deferred paths. `GameManager.cs:382` exposes `public void ApplyLevelReward(int level)` for external callers. Acceptance criteria met: no level reward is silently skipped in achievement or daily-bonus paths. |
| TASK-03 | PASS | (a) `TombakanOnboarding.cs:52-67` — when `greetingPanel` is active, `dailyBonusPanel.SetActive(true)` is never called; bonus data is stored in `_pendingBonus/_pendingXp/_pendingStreak/_pendingNewLevel` and `ShowDailyBonus` is invoked only from `DismissGreeting()` (lines 101-108) after greeting is hidden. The two panels cannot be simultaneously visible. (b) `GameManager.cs:490-493` — `int scoreBefore = score; score = ClampScore(score - penaltyPerWrongHit); int actualDeduction = scoreBefore - score;` then `ShowSad(actualDeduction)`. `GameManager.cs:522-525` — `ShowSad(int actualDeduction)` returns early when `actualDeduction <= 0`, suppressing all feedback when score is already at 0. Acceptance criteria met: panels never overlap; penalty text is suppressed (not shown as `-25!`) when the score floor absorbs the hit. |
