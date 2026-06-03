# Tester Report — Week 7

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `DailyChallenge.TryClaimDailyBonus` still discards `ProgressionStore.AddXp` return value at line 58 — level-up from daily bonus is invisible and level rewards are never applied | `DailyChallenge.cs:58` | HIGH: player levels up before game starts, rewards silently skipped every time it happens |
| 2 | `AchievementChecker.CheckAll` runs after `ProgressionStore.AddXp` in `EndGame` — achievement XP grant at line 100 fires `AddXp` again but the new-level return value is discarded; a fresh level-up from achievement XP is silently lost | `AchievementChecker.cs:100` | HIGH: reward cascade on achievement unlock silently levels up player with no panel, no reward |
| 3 | `ColourBlindSettings.ShapeForColor` returns `"?"` for any colour beyond the base four (Merah/Hijau/Biru/Kuning); when `FishCatalog` assigns a `baseColor` outside those four exact hex values the shape overlay shows `"?"` with no Indonesian mapping | `ColourBlindSettings.cs:22-27` | HIGH: colour-blind players see `"?"` on every non-palette-base fish when catalog is active |
| 4 | `GoalManager.ForceCompleteGoal` calls `CompleteGoal()` which immediately dereferences `m_StepList[m_CurrentGoalIndex]`; if `TombakanOnboarding.Start` calls this before `GoalManager.StartCoaching()` initialises the queue and step list, index is 0, the queue is empty, and the `else` branch sets `m_AllGoalsFinished = true` and returns before `m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false)` — skipping over an uninitialised step at index -1 is one off-by-one away from a crash | `GoalManager.cs:196-213`, `TombakanOnboarding.cs:30` | MED-HIGH: returning player launch could produce a NullReferenceException or silently skip onboarding dismissal leaving old step panels on screen |
| 5 | Achievement toast sequencing overlaps with level-up panel: `ShowAchievementsSequenced` starts at +1.2 s (`GameManager.cs:354`), level-up panel shows at +0.8 s (`GameManager.cs:344-348`); the first toast fires 0.4 s after the level-up panel opens while it is still being read | `GameManager.cs:344,354` | MED: level-up panel and achievement toast compete for attention; achievement text may obscure or clash with level-up text |
| 6 | `SpearThrower.LockThrow` calls `StopAllCoroutines()` which stops both `LockRoutine` and `CooldownRoutine`; if a spear is in-flight when a new fish round begins, `CooldownRoutine` is cancelled — `canThrow` stays `false` and `spearFake.SetActive(true)` is never called, permanently locking the throw button | `SpearThrower.cs:100-101` | MED: player throws, hits fish, new round starts mid-flight; throw button becomes visually present (spearFake visible from `LockRoutine`) but `canThrow` is `false` — silent lock |
| 7 | `SpearThrower.LockRoutine` and `CooldownRoutine` both call `spearFake.SetActive(true)` on completion; if `LockThrow` is called before `CooldownRoutine` finishes, `spearFake` re-appears before the cooldown is over, giving a visual hint that the player can throw before they actually can | `SpearThrower.cs:94,110` | LOW-MED: misleading visual feedback every time `LockThrow` is called during a cooldown window |
| 8 | No throw-mechanic tutorial for first-time players — persists from Week 6 UX-1 | `TombakanOnboarding.cs` | LOW-MED: first-time player learns throw mechanic through trial and error only |

---

## Session Simulations

### Session 1 — First-time Player (no AR experience)

**Profile:** Fresh install, `ScoreStore.GetBest() == 0`, `ProgressionStore.GetTotalXp() == 0`.

**Flow:**
1. App launches. `GameManager.Start()` → `AudioManager.I.PlayMainBGM()`. `ProgressionHUD` shows `Lv 1`, XP bar empty.
2. `TombakanOnboarding.Start()` — `isReturningPlayer = false` — shows `greetingPanel` if wired. Calls `DailyChallenge.TryClaimDailyBonus` — first day, no prior `LastPlayedKey` → returns `true`, awards 100 XP (`streak = 1`). `ShowDailyBonus(100, 1, newLevel: 0)` — shows `dailyBonusPanel` for 3 s. Panel hides via `HideDailyBonus`.
3. Player dismisses greeting → `DismissGreeting()` → `goalManager.StartCoaching()`. Coaching begins with `FindSurfaces`.
4. Player scans floor (real device). `PlaceWaterOnPlane.Update()` detects touch on AR plane → positions water, calls `DisableARPlanes()`, sets `enabled = false`.
5. Player presses Start. `GameManager.StartGame()` → `gameRunning = true`, timer at 60 s, fish spawn.
6. Player sees target colour swatch + Indonesian label (e.g. "Merah"). No hint exists for how to throw.
7. Player taps background — `PlaceWaterOnPlane` is already disabled, no response. Taps fish directly — `SpearHit` is on the spear, not on a tap target. No throw occurs.

**Friction — UX-1 (known, still present):** No throw tutorial. Silent failure on random taps. First-timer has no clear affordance for the throw button. `TombakanOnboarding` has no game-mechanic step beyond AR coaching.

**New observation:** The `greetingPanel` has a `DismissGreeting` button wired to call `goalManager.StartCoaching()`. However `TombakanOnboarding.Start()` checks the daily bonus before the player has a chance to dismiss the greeting — so the daily-bonus panel and the greeting panel could both be active simultaneously. There is no ordering or mutual exclusion between them. On a small phone screen both panels may overlap.

---

### Session 2 — Casual Player (3rd session, daily login)

**Profile:** `ScoreStore.GetBest() = 400`, `ProgressionStore.GetTotalXp() = 85` (Level 1, 15 XP from Level 2 threshold at 100 XP).

**Flow:**
1. `TombakanOnboarding.Start()` — `isReturningPlayer = true` → `goalManager.ForceCompleteGoal()` called.
2. **BUG-4 trace (GoalManager state):** `ForceCompleteGoal()` calls `CompleteGoal()`. If `GoalManager.StartCoaching()` was never called this session (it runs on `DismissGreeting` for new players, not for returning players), `m_OnboardingGoals` may be null or empty. Inside `CompleteGoal()`:
   - `m_CurrentGoal.Completed = true`
   - `m_CurrentGoalIndex++` → now 1
   - `m_OnboardingGoals.Count` could be 0 (or null-exception if queue never initialised) → falls into `else`: sets `m_AllGoalsFinished = true`, returns.
   - But **before** that, line 204: `m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false)` — this accesses `m_StepList[0]`. If `m_StepList` is empty or not wired in the scene, this throws `IndexOutOfRangeException` or `NullReferenceException`. The scene dependency is untested.
3. Daily bonus: `levelBefore = ProgressionStore.GetLevel() = 1`. `DailyChallenge.TryClaimDailyBonus(out xp, out streak)` → XP = 100, streak = 3. `ProgressionStore.AddXp(100)` inside `TryClaimDailyBonus` — total XP goes from 85 to 185 → crosses Level 2 threshold (100 XP needed). **Level-up occurs inside `DailyChallenge.cs:58`** but the return value is discarded. `levelAfter = ProgressionStore.GetLevel() = 2`. `newLevel = 2 > 1` → `ShowDailyBonus(100, 3, 2)`. Daily bonus panel shows `"Bonus harian +100 XP!\nStreak 3 hari berturut-turut!\nLevel 2! Selamat!"`.
4. **Week 6 BUG-2 status (PARTIAL FIX — CONFIRMED):** The `TombakanOnboarding.cs:38-44` fix correctly detects the level-up and surfaces it in the daily panel. However, `LevelRewardTable.GetRewardForLevel(2)` is **never called** in this code path. Any reward configured for Level 2 (soft currency bonus, species unlock, spear skin unlock) is silently skipped. The reward is only applied via `GameManager.EndGame` → `StaggerResultCelebrations` → `ApplyLevelReward`. Level-up from daily bonus has no path to reward application.

---

### Session 3 — Speed Runner (maximising score)

**Profile:** Level 6, XP = 820, streak 3. Aims for 15+ correct hits in 60 s.

**Flow:**
1. `StartGame()` — all counters reset.
2. Round 1: hits correct fish at t=3 s. `OnFishHit(Color.green, "koi")` — `gameRunning = true` guard passes. `comboStreak = 1`, `score = 100`, `timeLeft += 0.5 s` (TimeBonus). `LockThrow(delay)` called, `PickNewTarget` invoked after delay.
3. At correct = 5: `FishPalette.CountForProgress(5)` → `3 + 5/5 = 4` — fourth colour (Kuning) joins the active set. Difficulty ramp confirmed working.
4. At correct = 10: `fishSpawner.fishCount = FishCountForDifficulty(10) = 6`.
5. At correct = 15: `fishSpawner.fishCount = 7`.

**Race condition trace — SpearThrower.LockThrow:**
At t=12s, player throws (spear flying). Fish hit at t=12.4s. `OnFishHit` fires → `LockThrow(delay)` called at `GameManager.cs:485`. `SpearThrower.LockThrow` → `StopAllCoroutines()`. This stops any running `CooldownRoutine`. `LockRoutine` starts with `canThrow = false`, `spearFake.SetActive(false)`. After `delay` seconds, `spearFake.SetActive(true)`, `canThrow = true`. Visually correct.

But now consider: player throws again at t=15s, hits at t=15.3s (0.3 s into the throw). `CooldownRoutine` is running (cooldown = 1.2 s, only 0.3 s elapsed). `LockThrow(delay)` calls `StopAllCoroutines()` — kills `CooldownRoutine`. `LockRoutine` starts. At end of lock, `spearFake.SetActive(true)`, `canThrow = true`. **Correct behaviour in happy path.**

However the problem is asymmetric: if `LockRoutine` is already running when a second `LockThrow` is called, the new `StopAllCoroutines` stops the old `LockRoutine` mid-flight leaving `canThrow = false` and `spearFake` potentially invisible. The new `LockRoutine` then starts fresh and resolves correctly. In practice this path only triggers if two fish hits occur faster than `hitDelay` (which pacing rules prevent with the `LockThrow` call). **Low probability but latent.**

**EndGame:**
- `xpEarned = ProgressionRules.XpForResult(16, 100, 2, 5)` = 160 + 2 + 100 + 100 = 362 XP.
- `newLevel = ProgressionStore.AddXp(362)` — returns new level if crossed.
- `StaggerResultCelebrations(isRecord, newLevel)` — badge at +0.4 s, level-up panel at +0.8 s. **Week 6 POLISH-1 fix confirmed working.**
- `ShowAchievementsSequenced(newlyUnlocked, catalog)` — starts at +1.2 s after `EndGame`.
- **Overlap issue:** `levelUpPanel` activates at +0.8 s. First achievement toast fires at +1.2 s. If achievement toast panel overlaps levelUpPanel in the scene layout, both are visible from +1.2 s to +2.8 s (toast shows for 2 s). Player reading level-up text at +0.8 s is interrupted by toast at +1.2 s. Gap is only 0.4 s — not enough for the level-up message to register.

---

### Session 4 — Achievement Hunter (chasing first milestone)

**Profile:** Level 3, 12 games played, combo streaks of 4 previously. This game: combo of 5 achieved.

**Flow:**
1. Player builds 5 consecutive correct hits. `comboStreak = 5`, `maxComboStreak = 5`.
2. `EndGame()` fires.
3. `AchievementChecker.CheckAll(this, achievementCatalog)` → evaluates `Combo5: maxComboStreak >= 5` → true. `AchievementStore.IsUnlocked("combo_5")` → false (first time). `AchievementStore.Unlock("combo_5")` persists. `ProgressionStore.AddXp(achievement.xpReward)` — **BUG-5:** return value discarded (line 100). If this XP crosses a level boundary, the level-up is silent and `ApplyLevelReward` is never called.
4. `newlyUnlocked = ["combo_5"]`.
5. `StartCoroutine(ShowAchievementsSequenced(["combo_5"], catalog))` — waits 1.2 s, then shows `achievementToastPanel` with `achievement.titleIndonesian` for 2 s. **Week 6 UX-2 fix confirmed working — toast is now shown.**
6. Toast closes at +3.2 s after EndGame.

**New issue confirmed (BUG-NEW-1):** `AchievementChecker.cs:100` — `ProgressionStore.AddXp` return value discarded. The level-up from achievement XP reward goes unacknowledged. The `levelUpPanel` will not appear for this level-up path.

---

### Session 5 — Frustrated Player (3 misses in a row)

**Profile:** Level 2, first session of the day. Player hits wrong fish 3 times consecutively.

**Flow:**
1. Wrong hit 1: `OnFishHit(blue_fish, "salmon")` where `targetColor = Color.red`. `correct = false`. `comboStreak = 0`, `wrongHitCount = 1`. `score = ClampScore(0 - 25) = 0` (clamped at 0 — week 1 fix). `ShowSad()` → sadFeedback shows `"-25!"`.
2. Wrong hit 2: same. `wrongHitCount = 2`. Score stays 0 (clamped). `ShowSad()` again.
3. Wrong hit 3: `wrongHitCount = 3`. Score 0.

**Observed pattern:** Feedback text always shows `"-25!"` even when the deduction was absorbed by the clamp and the displayed score did not change. Player sees `"-25!"` feedback but score stays at 0, creating confusion. The sadFeedback text uses `penaltyPerWrongHit` (25) but the actual deduction was 0 after clamping.

**UX Gap (UX-NEW-1):** `GameManager.cs:473-477` — `ShowSad()` always shows `"-{penaltyPerWrongHit}!"` (line 507) regardless of whether the clamp absorbed the penalty. When score is already 0, the player sees a penalty notification for a penalty that did not apply to the displayed score.

**Continuation:**
4. Timer runs out. `EndGame()`. `resultAccuracyText.text = Accuracy.Format(0, 3) = "0/3 (0%)"`. Tier = TierEmpty (correct = 0). XP = 0. `resultXpText.text = ""` (suppressed — week 5 fix confirmed).

---

### Session 6 — Veteran Player (10+ games, high XP, Level 8)

**Profile:** Level 8, XP = 2400, best score = 3600.

**Flow:** Full 60 s game. Hits 18 fish correct, 1 wrong. `maxComboStreak = 10`. `score = 18 × 100 + various combo bonuses`. `xpEarned = ProgressionRules.XpForResult(18, 94, 0, 10)` = 180 + 2 (combo bonus: multiplier 3, (3-1)×5=10 XP... wait — `XpForResult` uses `Mathf.Max(0, multiplier - 1) * XpPerComboBonus`: `(3-1) * 5 = 10`) + 94 (accuracy %) + 0 (no new species) = 284 XP.

**Achievement check:** `AchievementChecker.CheckAll` — `Level10: playerLevel >= 10 → false`. `PerfectRound: wrongHitCount == 0 → false (1 wrong)`. `Combo5: maxComboStreak >= 5 → true`. If already unlocked, no action. `SpeciesCollector5: FishdexStore.UnlockedCount() >= 5` — if catalog assigned and player has caught 5+ species, unlocks and grants XP. Return value still discarded.

**Result screen:** Badge +0.4 s, level-up panel +0.8 s (if level-up occurred), achievement toast +1.2 s. All three stagger correctly. Veteran player can read each in sequence. **Week 6 POLISH-1 fix working correctly for this session.**

**XP bar after game:** `RefreshProgressionHUD()` called in `EndGame` at line 282 — updates `levelBadgeText` and `xpBarFill` correctly before any coroutine fires. **No bug here.**

---

### Session 7 — Colour-blind Player

**Profile:** `ColourBlindSettings.IsEnabled() = true`, FishCatalog assigned in scene.

**Flow:**
1. `FishSpawner.SpawnFish` — `targetSpecies = catalog.PickRandom()`. `resolvedTargetColor = targetSpecies.baseColor`. Fish spawned with `fishColor = targetSpecies.baseColor`.
2. `FishShapeOverlay.Start()` — `ColourBlindSettings.IsEnabled() = true` → `label.enabled = true`. `label.text = ColourBlindSettings.ShapeForColor(fishTarget.fishColor)`.
3. **BUG-3 trace:** `ShapeForColor` at `ColourBlindSettings.cs:18-29` maps only four exact hex values: `FF0000`, `00FF00`, `0000FF`, `FFFF00`. `FishSpecies.baseColor` is a free-form `Color` field. If a species has `baseColor = new Color(0.9f, 0.6f, 0.2f)` (an orange fish), `ColorUtility.ToHtmlStringRGB` produces `E6993F`. No match in the switch → returns `"?"`. The overlay shows `"?"` to the player. With 8 defined species in the starter catalog (BACKLOG), the majority of species likely have colours outside the four-colour palette, making this bug affect most catalog play.
4. The `targetColorLabel` also shows `ColorHexLocalization.ToIndonesian(targetColor)` — if the species `baseColor` is not in `Dict.cs`'s 20-entry map, the hex string itself is shown instead of an Indonesian name (fallback at `Dict.cs:35`). Both the label and the shape overlay degrade for out-of-palette species colours.

**Confirmed bug — colour-blind mode + FishCatalog = broken experience.**

---

### Session 8 — Player Who Mutes Audio Mid-Game

**Profile:** Mutes audio at t=30s during gameplay.

**Flow:**
1. `MuteButtonUI.OnClick()` → `AudioManager.I.ToggleMute()` → `SetMuted(true)` → `ApplyMute()` mutes both `bgmSource` and `sfxSource`. BGM fades instantly (`.mute = true`). `AudioPrefs.SetMuted(true)` persists.
2. At t=35s, player hits correct fish. `OnFishHit` → `AudioManager.I.PlayCorrect()` → `sfxSource.PlayOneShot(sfxCorrect)` — audio source is muted, SFX is silent. Expected.
3. `HapticFeedback.PlayCorrect()` — **Week 6 fix confirmed:** no longer checks `AudioPrefs.IsMuted()`. `HapticFeedback.cs:12-17` shows just `#if UNITY_ANDROID || UNITY_IOS Handheld.Vibrate() #endif`. Vibration fires regardless of mute state. **BUG-3 from Week 6 is fixed.**
4. `ScreenShake.I.ShakeOnCorrect()` — runs independently of mute. Camera shakes. Good.

**Week 6 fix verification — HapticFeedback decoupled from audio mute: CONFIRMED FIXED.** `HapticFeedback.cs` has no reference to `AudioPrefs` or `AudioManager`.

**Remaining polish (POLISH-2, still open):** Both `PlayCorrect` and `PlayWrong` call `Handheld.Vibrate()` identically — no duration or pattern difference. Correct and wrong hits feel identical on device.

---

### Session 9 — Edge Case: Timer Expires While Spear Is Mid-Flight (BUG-1 Fix Verification)

**Profile:** Player throws at t=59.5 s. Timer reaches 0 at t=60.0 s.

**Code trace:**
1. t=60.0s: `GameManager.Update()` → `timeLeft <= 0f` → `gameRunning = false` → `EndGame()` called. Result screen shows score = 1000. `fishSpawner.ClearAll()` destroys all fish.
2. t=60.4s: In-flight spear reaches where fish was — but fish is already destroyed. `SpearHit.CheckFishHit()` finds no colliders. No hit registered. Spear destroyed at t=62.0s (spearLifeTime = 2.5s from throw at t=59.5s).
3. Alternate: fish not yet cleared when spear lands (timing depends on frame). If spear hits before `ClearAll`:
   - `FishHitBox.OnHit(fishColor, speciesId, spear)` → `GameManager.I.OnFishHit(...)`.
   - **`GameManager.cs:440`: `if (!gameRunning) return;`** — `gameRunning` is already `false`. Returns immediately. Score not modified. Result screen unchanged.

**Week 6 BUG-1 fix: CONFIRMED FIXED.** The guard at `GameManager.cs:440` prevents any post-EndGame score mutation. The result screen score is correct regardless of in-flight spear timing.

---

### Session 10 — Daily Bonus XP Triggers Level-Up (BUG-2 Partial Fix Verification)

**Profile:** Player has 95 XP (5 XP from Level 2 threshold at 100). Streak = 1 (first daily bonus).

**Code trace:**
1. `TombakanOnboarding.Start()` at line 38: `levelBefore = ProgressionStore.GetLevel()` = 1.
2. `DailyChallenge.TryClaimDailyBonus(out xp, out streak)` called. Inside `DailyChallenge.cs:51`: `xpAwarded = TotalBonusXp(1) = 100 + 1 × 25 = 125`. Line 58: `ProgressionStore.AddXp(125)` — total XP = 220, Level 2 threshold = 100, Level 3 threshold = 283. New level = 2. **Return value discarded.**
3. Back in `TombakanOnboarding.cs:40`: `levelAfter = ProgressionStore.GetLevel()` = 2. `newLevel = 2`. `ShowDailyBonus(125, 1, 2)` — panel text = `"Bonus harian +125 XP!\nSelamat datang kembali!\nLevel 2! Selamat!"`. Panel shown for 3 s.

**Week 6 partial fix verified:** Level-up IS surfaced in the daily bonus text — `TombakanOnboarding.cs:57-59` appends the level-up line. **The UI acknowledgement is now present.**

**Remaining gap — Level rewards not applied (BACKLOG Week 7 candidate):** `LevelRewardTable.GetRewardForLevel(2)` is never called in `TombakanOnboarding`. If Level 2 grants a coin bonus or species unlock, it is silently skipped. The only path that calls `ApplyLevelReward` is `GameManager.StaggerResultCelebrations` — which runs only during `EndGame`, not during onboarding. The reward gap is confirmed at `DailyChallenge.cs:58` (AddXp without capturing return for rewards) and also at the `TombakanOnboarding.cs:39-44` path (no call to `ApplyLevelReward`).

---

### Session 11 — Edge Case: Achievement XP Causes Silent Level-Up

**Profile:** Level 4, XP = 395. Level 5 threshold = `round(100 × 4^1.5) = round(100 × 8) = 800`. Remaining to Level 5 = 405 XP. Player earns `first_catch` achievement (xpReward = 50 XP, hypothetical catalog value).

**Code trace:**
1. `EndGame()` at line 274: `xpEarned = ProgressionRules.XpForResult(...)`, say 200 XP. `ProgressionStore.AddXp(200)` → XP = 595. Level still 4. `newLevel = 0`.
2. Line 290: `AchievementChecker.CheckAll(this, catalog)` — `first_catch` condition met (if not previously unlocked). `AchievementStore.Unlock("first_catch")`. `ProgressionStore.AddXp(50)` at `AchievementChecker.cs:100` — XP = 645. Still Level 4. No level-up.
3. Same scenario but xpEarned = 380 XP: after EndGame XP = 775, `newLevel = 0`. Achievement XP adds 50 → XP = 825 → crosses Level 5 threshold. `AddXp(50)` returns 5 (new level) but return is discarded at line 100. No `levelUpPanel`, no reward from `LevelRewardTable.GetRewardForLevel(5)`.

**This is a real failure path with tangible reward loss** — confirmed as BUG-NEW-1.

---

### Session 12 — Zen Mode Player

**Profile:** Player switches to Zen mode (`currentMode = GameMode.Zen`).

**Flow:**
1. `StartGame()` — `timeLeft = float.MaxValue` (line 223). `timerCountdownText.text = "--"`. `timerBarFill.fillAmount = 1f`.
2. `Update()` — `currentMode == GameMode.Zen` → `timerBarFill.fillAmount = 1f; return`. Timer never decrements.
3. `EndGameManual()` — the only way to end. Sets `gameRunning = false`, calls `EndGame()`.
4. `EndGame()` — `resultAccuracyText`, XP, coins, achievements all fire as normal.

**Issue found — Zen mode + `LockThrow`:** `GameManager.OnFishHit` line 484: `float delay = PacingRules.HitDelayForProgress(hitDelay, correctHitCount)`. With `correctHitCount = 12`, delay = `2.2 - 0.1×12 = 1.0` (floored at `MinHitDelay = 1.0`). `spearThrower.LockThrow(1.0)`. `Invoke(nameof(PickNewTarget), 1.0 + 0.8 = 1.8)`.

In Zen mode there is no auto-end, but the Invoke for `PickNewTarget` still checks `!gameRunning` guard. If player triggers `EndGameManual()` during the 1.8 s delay, the Invoke fires after EndGame but hits the guard. **No bug — guard prevents it.**

**Observation:** `timerWarningActive` check in `Update()` returns early before it can ever be set in Zen mode, so the warning state is never cleaned up if the player starts a non-Zen game after Zen. However `StartGame()` at line 228-230 explicitly stops and clears `timerPulseRoutine` and resets `timerWarningActive = false`. **No bug** — state is reset on start.

---

## Bugs Found

- [ ] **BUG-NEW-1** — `AchievementChecker.CheckAll` discards the return value of `ProgressionStore.AddXp(achievement.xpReward)` at line 100; any level-up triggered by achievement XP is silent and `LevelRewardTable` rewards are not applied — `AchievementChecker.cs:100` — reproduction: be near a level boundary, trigger a first-time achievement with a non-zero `xpReward`; confirm level increments in HUD with no panel and no reward applied
- [ ] **BUG-NEW-2** — `TombakanOnboarding.cs` level-up path (`levelAfter > levelBefore`) never calls `ApplyLevelReward`; level rewards (coins, species unlock, spear skin) are skipped on daily-bonus level-ups — `TombakanOnboarding.cs:39-44`, `DailyChallenge.cs:58` — reproduction: have XP near a level threshold; login on a new day; level increments and shows in the daily panel but reward is never granted (check `CurrencyStore.GetCoins()` before and after — should increase if Level N has a coin reward)
- [ ] **BUG-NEW-3** — `ColourBlindSettings.ShapeForColor` only handles four exact hex values (`FF0000`, `00FF00`, `0000FF`, `FFFF00`); any `FishSpecies.baseColor` outside this set returns `"?"` on shape overlays — `ColourBlindSettings.cs:22-27` — reproduction: enable colour-blind mode with a `FishCatalog` that has a species with a non-palette colour; observe `"?"` on fish shapes
- [ ] **BUG-NEW-4** — `ShowSad()` always displays `"-25!"` even when `ClampScore` absorbs the full penalty and the visible score does not change; player sees a penalty notification for a penalty that had no effect — `GameManager.cs:473-477,507` — reproduction: let score reach 0, then hit a wrong fish; `"-25!"` appears but score stays 0
- [ ] **BUG-NEW-5** — `GoalManager.ForceCompleteGoal` is called from `TombakanOnboarding.Start()` before `StartCoaching()` initialises the goal queue; `CompleteGoal()` can access `m_StepList` at an uninitialised or out-of-range index for returning players — `GoalManager.cs:196-213`, `TombakanOnboarding.cs:30` — reproduction: returning player (non-zero score/XP) launches app; `ForceCompleteGoal()` fires immediately in `Start()`; if `m_StepList` is populated in the Inspector the step at index 0 is forcibly hidden; if the list is unpopulated, `NullReferenceException` or `IndexOutOfRangeException` may crash the scene

---

## Known Items Confirmed Fixed (Week 6)

- **BUG-1 FIXED** — `OnFishHit` now has `if (!gameRunning) return;` guard at `GameManager.cs:440`; mid-flight spear no longer mutates score or stats after EndGame.
- **HapticFeedback decoupled FIXED** — `HapticFeedback.cs` contains no reference to `AudioPrefs` or `AudioManager`; vibration fires independently of mute state.
- **Achievement toast FIXED** — `ShowAchievementsSequenced` coroutine in `GameManager.cs:352-371` correctly sequences toast display; null-safe guard at line 358 (`yield break` if panel is null).
- **Result celebrations staggered FIXED** — `StaggerResultCelebrations` coroutine: badge at +0.4 s, level-up panel at +0.8 s; both have null guards.
- **BUG-2 PARTIAL FIX** — `TombakanOnboarding.cs:38-44` now detects and surfaces daily-bonus level-up in the daily panel text. The level reward application gap remains (BUG-NEW-2 above).

---

## UX Gaps

- [ ] **UX-1** *(known, still open)* — No throw-mechanic tutorial for first-time players; after water placement no hint explains how to throw — first game, after water is placed — add a timed hint panel: "Tekan tombol untuk melempar tombak!" visible for 8 s on game start; wire via `TombakanOnboarding` or `GameManager.StartGame()`
- [ ] **UX-NEW-2** — `greetingPanel` and `dailyBonusPanel` can both be active simultaneously on a returning player's first daily session (rare edge: new player who accrued XP without a best score — or if panels are both present and onboarding flags are set); no mutual exclusion exists between them — first daily launch — show greeting first, only trigger daily bonus panel after greeting is dismissed, or check for both conditions in `TombakanOnboarding.Start()`
- [ ] **UX-NEW-3** — Achievement toast appears 0.4 s after level-up panel, interrupting the level-up reading moment; combined with `celebrationText` that may overwrite `levelUpText` in `ApplyLevelReward`, the level-up panel content is unstable during its display window — result screen — delay achievement toast to at least +3.5 s (after level-up panel has been visible for 2+ seconds), or require player tap-to-dismiss level-up panel before toasts begin
- [ ] **UX-NEW-4** — `ColorHexLocalization.ToIndonesian` falls back to the raw hex string (e.g. `"E6993F"`) when a species colour is not in the `Dict.cs` map; players see a hex code as the target label when playing with a catalog that has custom colours — any game with FishCatalog and non-palette species — either map all species `baseColor` values in `Dict.cs`, or use `FishSpecies.displayName` as the target label instead of the colour name when a catalog is active (the `targetSpeciesLabel` already shows the species name, so the colour label could be suppressed when a catalog is active)

---

## Polish Opportunities

- [ ] **POLISH-1** *(partially addressed, still improvable)* — Achievement toast fires 0.4 s after level-up panel; on small screens both may overlap; the level-up panel has no auto-dismiss or tap-to-continue, so it stays visible behind the toast — result screen — add a tap-to-dismiss to the level-up panel, or auto-dismiss it after 2 s before the first achievement toast
- [ ] **POLISH-2** *(known, still open)* — `HapticFeedback.PlayCorrect` and `PlayWrong` both call `Handheld.Vibrate()` with identical behaviour; no tactile distinction between correct and wrong hits — `HapticFeedback.cs:14,22` — implement Android-specific duration (short pulse for correct, double-pulse for wrong) via `AndroidJavaObject("android.os.Vibrator")`; use `UnityEngine.InputSystem.Haptics` on iOS
- [ ] **POLISH-3** *(still open)* — `ApplyLevelReward` overwrites `levelUpText.text` with `reward.celebrationText` at `GameManager.cs:387-388`; if called in the same coroutine frame as `ShowLevelUp`, the original `"Level N! Selamat!"` text is overwritten before the player can read it — level-up moment — only apply `celebrationText` if a delay has elapsed, or keep both texts in separate UI elements
- [ ] **POLISH-NEW-1** — `ColorSummary.Format` always appends `×N` even when N=1 (e.g. `"Merah ×1"` instead of `"Merah"`) for single catches; correct fish result text looks cluttered when fish are varied — `ColorSummary.cs:44-46` — show `×N` only when `N > 1`: `sb.Append(name).Append(counts[n] > 1 ? $" ×{counts[n]}" : "")`
- [ ] **POLISH-NEW-2** — `ProgressionHUD.Refresh()` is triggered by `OnEnable` only; if the HUD panel is always active, XP bar does not update mid-game when combo bonuses add time (no XP is awarded mid-game, so the bar only changes at EndGame — this is by design, but the bar also does not update when daily bonus XP is granted in `TombakanOnboarding.Start()`); level and XP shown on the main screen may be stale after the daily bonus — after `ShowDailyBonus` fires — call `ProgressionHUD.I.Refresh()` (or expose a static refresh hook) from `TombakanOnboarding` after claiming daily bonus
