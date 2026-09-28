# Tester Report — Week 5

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `PickNewTarget` fires after `EndGame` — fish spawn over result screen | `GameManager.cs:437` | HIGH: result screen obscured; player thinks game glitched |
| 2 | `FishSpawner.SpawnFish` unconditionally adds a second `FishSwim` — erratic fish every round | `FishSpawner.cs:71` | HIGH: fish jitter and fight each other every spawn |
| 3 | `PlaceWaterOnPlane` has no game-active guard — stray tap mid-game teleports water plane | `PlaceWaterOnPlane.cs:14` | HIGH: entire fish shoal displaced; game unplayable |
| 4 | `targetSpeciesLabel` reads `CurrentTargetSpecies` before `SpawnFish` updates it — species label one round behind | `GameManager.cs:360` | MED: wrong species name shown in HUD every round when catalog active |
| 5 | `CollectSummary` uses `newSpeciesThisGame` (first-ever discoveries) — returning players always see "Tidak ada ikan dikumpulkan" | `GameManager.cs:306` | MED: confusing empty result for any veteran player |

---

## Session Simulations

### Session 1 — First-timer (ARKit device, never played before)

**Flow:** Launches → GoalManager coaching → scans floor → taps plane → waterPlane appears → game starts → sees coloured target swatch + Indonesian label → throws spear (long press? swipe? — no on-screen hint about the throw mechanic) → hits fish → `+100!` feedback → 60 s counts down.

**Friction found:**
- Player doesn't know *how* to throw. `SpearThrower.ThrowSpear()` is wired to a UI button or input action, but there's no in-game prompt. Player may tap and swipe with no response until they accidentally trigger the button.
- If player lands first hit near T=57s (`timeLeft ≈ 57`), `OnFishHit` invokes `PickNewTarget` at `delay + 0.8f ≈ 3.0s`. Timer reaches 0 at T=60. With `hitDelay=2.2s`, `PacingRules.HitDelayForProgress(2.2f, 1) = 2.1s`, so invoke fires at `~2.9s`. **Fine here — timer not expired yet.**

### Session 2 — Casual player (3rd session, 8 correct hits so far in career)

**Flow:** Returning player → `TombakanOnboarding.Start()` detects `ScoreStore.GetBest() > 0` → `goalManager.ForceCompleteGoal()` fires → plays normally.

**Critical path check — `ForceCompleteGoal()`:**
`GoalManager.CompleteGoal()` (line 196) accesses `m_OnboardingGoals.Count` — this Queue is only initialized inside `StartCoaching()`. If the scene's GoalManager has never had `StartCoaching()` called (which happens only from `DismissGreeting()`, never called for returning players), `m_OnboardingGoals` is **null → NullReferenceException** every returning player launch. *(This is scene-wiring dependent; flagged as P1 if GoalManager is in the scene.)*

**Issue found:** `CollectSummary` in EndGame — player has unlocked all 4 palette fish species in previous sessions. `newSpeciesThisGame` stays empty all game → result screen shows **"Tidak ada ikan dikumpulkan"** despite catching 8 fish. Very confusing.

### Session 3 — Frustrated player (missed last 3 throws, timer at 12 s)

**Flow:** 3 misses → `comboStreak=0`, score depleted by `3×25=75`. Timer at 12 s, `warningActive=true`, pulse active. Player lands a hit at T=11s.

**Critical bug trace:**
- `OnFishHit` line 435: `delay = PacingRules.HitDelayForProgress(2.2f, correctHitCount)`. With `correctHitCount=2`, `delay ≈ 2.0s`. `Invoke(nameof(PickNewTarget), 2.0 + 0.8f)` = **2.8 s** delay.
- Timer reaches 0 at T=11s → `gameRunning=false; EndGame()` runs at T=11s. `fishSpawner.ClearAll()` runs. Result screen appears.
- **At T=8.2s (11-2.8s)**, `PickNewTarget` fires. `gameRunning` is false, but there's no guard check. `FishPalette.CountForProgress`, `fishSpawner.fishCount = ...`, and **`fishSpawner.SpawnFish(...)`** all execute — **new fish are spawned over the result screen.** The result screen is now behind moving fish. Player is confused and tries to hit them, triggering `GameManager.OnFishHit` even though the game is "over", which updates the score UI that's now hidden.
- **BUG-1 confirmed: reproducible any time a fish is hit in the last ~3 seconds.**

### Session 4 — Speed runner (15+ correct hits, experienced)

**Flow:** High-speed play, `correctHitCount=15`, `FishCountForDifficulty(15)=7`, palette = 4 colours. Fish spawn 7 at a time with `activeColorCount=4`.

**Duplicate FishSwim trace:**
- `FishSpawner.SpawnFish` line 71: `fish.AddComponent<FishSwim>().horizontalRadius = spawnRadius` — adds a *second* `FishSwim` if prefab already has one.
- Both instances run `Update()` each frame, both calling `transform.position +=` each frame. Fish position increments **twice per frame** → fish move at double speed, direction fights cause visible jitter.
- After the first second, the two instances have different `noiseOffset`, `targetDepth`, and `direction` values — one tries to turn left, the other right → fish vibrate in place or teleport.
- **BUG-2 confirmed: present in every spawn, worst with 7 fish at high difficulty.**

### Session 5 — Player with FishCatalog assigned (species mode)

**Flow:** `fishSpawner.catalog` is a populated `FishCatalog`. Round starts.

**Species label stale trace (PickNewTarget):**
```
Line 352: targetColor = active[...] (e.g. Color.red)
Line 355: targetColorImage.color = Color.red
Line 357: targetColorLabel.text = "Merah"
Line 360: currentTarget = fishSpawner.CurrentTargetSpecies  // ← PREVIOUS round's species!
Line 362: targetSpeciesLabel.text = previousSpecies.displayName  // WRONG
Line 367: fishSpawner.SpawnFish(...)  // CurrentTargetSpecies updated HERE
Line 370-374: targetColor updated from new species
```
- Player sees the previous round's species name for the entire current round. On round 1, `CurrentTargetSpecies` is null → `targetSpeciesLabel.text = ""` (blank). On round 2, shows round 1's species. **Always one round behind.**
- **BUG-3 confirmed: every round when catalog is active.**

### Session 6 — Player accidentally taps floor during gameplay

**Flow:** Player is in gameplay, hunting a blue fish. Accidentally taps a AR-detected floor plane nearby (any `PlaneWithinPolygon`).

**PlaceWaterOnPlane trace:**
- `PlaceWaterOnPlane.Update()` runs every frame. `Input.touchCount > 0` → `touch.phase == Began` → `raycastManager.Raycast(...)` hits a detected plane → `waterPlane.transform.position = hitPose.position` — **water plane teleports.**
- `FishSpawner` stores `waterPlane` reference. New fish spawn relative to the new position. Existing fish continue swimming around their **original** `swimCenter` (set at spawn time in `FishSwim.Start()`) — now the fish are floating in mid-air away from the water.
- `DisableARPlanes()` disables plane detection — but this already happened on first placement, so the update was from a plane that was already disabled. Actually — once `DisableARPlanes()` fires, `planeManager.enabled = false`. So `raycastManager.Raycast` against `PlaneWithinPolygon` would still work because existing trackable planes remain in memory.
- **BUG-4 confirmed: a tap anywhere on a previously-detected plane mid-game teleports the water.**

### Session 7 — Player with 0 fish caught (first game, missed everything)

**Flow:** 60 seconds, 0 correct hits, some wrong hits.

**Result screen:**
- `resultAccuracyText.text = Accuracy.Format(0, 3)` → `"0/3 (0%)"` — OK.
- But if `wrongHitCount == 0` and `correctHitCount == 0`: `Accuracy.Format(0, 0)` → `"0/0 (0%)"` — looks like a code bug to the player.
- `CollectSummary()` with no caught fish → `ColorSummary.Format(empty)` = `"Tidak ada ikan dikumpulkan"` — this path is correct and clear.
- `resultXpText.text = "+0 XP"` — technically correct but deflating for a new player.

### Session 8 — Zen mode player

**Flow:** `currentMode = GameMode.Zen`. `StartGame()` sets `timeLeft = float.MaxValue`. Timer bar stays at 1.0. Player plays until they want to stop and calls `EndGameManual()`.

**Issue found:**
- `GameManager.cs:365`: `// TODO (Zen mode): pass a speed multiplier of 0.5f` — dead todo comment in production code.
- Zen mode has no UI button wired by default to call `EndGameManual()`. Player cannot end the game. *(Scene-wiring gap — noted as UX gap, not code bug.)*

### Session 9 — Player notices achievement system

**Flow:** Player hits 5 correct in a row → `maxComboStreak = 5`. `EndGame` → `AchievementChecker.CheckAll(this, achievementCatalog)` → `Combo5` condition met → `AchievementStore.Unlock("combo_5")` → `Debug.Log("[Tombakan] Achievement unlocked: combo_5")`.

**UX Gap:** No in-game achievement popup or banner. Achievement is silently unlocked. Player has no idea. The achievement system is fully functional in code but invisible to the player.

### Session 10 — Speed runner chasing high score (endgame focus)

**Flow:** Player ends with 1500 points, previous best was 1400. `ScoreStore.TrySetBest(1500)` → true → `newRecordBadge.SetActive(true)`. Level-up panel fires for level 4 → "Level 4! Selamat!" appears.

**Double-feedback congestion:** Both `newRecordBadge` and `levelUpPanel` are active simultaneously on the result screen. No ordering or sequencing — they appear at the same instant and may overlap depending on layout. First-time record AND level-up is the best-case scenario — but it creates visual noise.

---

## Bugs Found

- [x] **BUG-1** — `PickNewTarget` has no `gameRunning` guard; fires after `EndGame` if a fish is hit in the last ~3 seconds — `GameManager.cs:349,437` — hit fish with <3 s remaining, watch result screen; fish spawn over it
- [x] **BUG-2** — `FishSpawner.SpawnFish` calls `fish.AddComponent<FishSwim>()` unconditionally; if prefab already has `FishSwim`, two instances fight each frame — `FishSpawner.cs:71` — always reproducible; fish jitter or vibrate
- [x] **BUG-3** — `targetSpeciesLabel.text` is set from `fishSpawner.CurrentTargetSpecies` **before** `SpawnFish` updates it — species label one round behind — `GameManager.cs:360,367` — start a game with a FishCatalog assigned; species label shows previous round's species
- [x] **BUG-4** — `PlaceWaterOnPlane.Update()` runs during gameplay; any tap on a detected AR plane teleports the water surface mid-game — `PlaceWaterOnPlane.cs:14` — during gameplay, tap near a detected surface; water moves, fish displaced
- [x] **BUG-5** — `CollectSummary` gates on `newSpeciesThisGame` (first-ever discoveries only); returning players who've caught all species always see "Tidak ada ikan dikumpulkan" — `GameManager.cs:306` — play 2+ sessions; result summary is empty despite catching fish

## UX Gaps

- [ ] **UX-1** — No throw-mechanic tutorial hint — first-time player — add a brief on-screen prompt ("Sentuh tombol untuk melempar") visible for the first 10 seconds of the first game
- [ ] **UX-2** — Achievement unlocks are silent (only `Debug.Log`) — post-hit moment — add a brief achievement toast/banner on the HUD
- [ ] **UX-3** — `Accuracy.Format(0,0)` returns `"0/0 (0%)"` — result screen when player never throws — show `"--"` or `"Belum ada lemparan"` when no throws recorded
- [ ] **UX-4** — `resultXpText` shows `"+0 XP"` on a scoreless game — result screen — suppress if xpEarned == 0

## Polish Opportunities

- [ ] **POLISH-1** — Level-up panel and new-record badge appear simultaneously with no ordering — result screen — stagger reveals by 0.3–0.5 s so each moment lands
- [ ] **POLISH-2** — Dead TODO comment in production code — `GameManager.cs:365` — remove or resolve the Zen mode speed-multiplier note
- [ ] **POLISH-3** — ScreenShake moves the AR camera's local position — on low-end devices the world visibly lurches — consider limiting shake magnitude or using a UI flash instead for wrong hits

---

## Validation — Week 5

| Task | Status | Evidence |
|------|--------|----------|
| TASK-01a — `PickNewTarget` post-game guard | ✅ FIXED | `GameManager.cs:359` — `if (!gameRunning) return;` is the first statement; pending `Invoke` from `OnFishHit` is a no-op once `EndGame` sets `gameRunning = false` |
| TASK-01b — Duplicate `FishSwim` | ✅ FIXED | `FishSpawner.cs:71` — `fish.GetComponent<FishSwim>() ?? fish.AddComponent<FishSwim>()` — existing component reused; no second instance created |
| TASK-01c — Stale species label | ✅ FIXED | `GameManager.cs:379–382` — `targetSpeciesLabel.text` assigned after `SpawnFish(...)` call; `CurrentTargetSpecies` reflects the current round |
| TASK-02a — `PlaceWaterOnPlane` mid-game re-placement | ✅ FIXED | `PlaceWaterOnPlane.cs:37` — `enabled = false` after first successful hit; `Update()` no longer runs |
| TASK-02b — `CollectSummary` empty for returning players | ✅ FIXED | `GameManager.cs:305–325` — condition uses `collectedSpeciesIds.Count > 0` (every catch this session) instead of `newSpeciesThisGame.Count > 0` (first-ever discoveries only) |
| TASK-03a — `Accuracy.Format(0,0)` returns `"--"` | ✅ FIXED | `Accuracy.cs:26` — `if (total <= 0) return "--";`; covered by `AccuracyWeek5Tests.Format_ZeroCorrect_ZeroWrong_ReturnsDash` |
| TASK-03b — `resultXpText` suppressed when xpEarned==0 | ✅ FIXED | `GameManager.cs:277` — `xpEarned > 0 ? $"+{xpEarned} XP" : ""` |
| TASK-04 — Dead TODO removed | ✅ FIXED | `grep -n "TODO" GameManager.cs` → empty; no TODO comments remain |

**Regression checks:**
- `Accuracy.Format(9,3)` → `"9/12 (75%)"` ✅ unchanged
- `FishPalette`, `TimeBonus`, `PacingRules` — no changes; Week 4 tests unaffected
- `CollectSummary` colour-fallback path (`ColorSummary.Format`) ✅ unchanged
- Week 3 `AccuracyTests.Format_ProducesReadout` updated; new `Format_ZeroThrows_ReturnsDash` test added to align with new contract
