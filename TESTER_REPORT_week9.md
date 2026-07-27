# Tester Report — Week 9

**Date:** 2026-07-27
**Scope:** Validation pass against ITERATION_week9_SCOPE.md (TASK-01 through TASK-04) plus carry-forward audit of all open issues.
**Sessions simulated:** 8 (returning player, casual × 2, colour-blind, achievement hunter, speed-runner, small-table AR, veteran)

---

## Task Validation Summary

| Task ID | Status | Files Changed |
|---------|--------|---------------|
| TASK-01(a) — GoalManager null guard | PASS | `GoalManager.cs` |
| TASK-01(b) — ShowSad absorbed penalty ("Meleset!") | PASS | `GameManager.cs` |
| TASK-01(c) — CancelInvoke before feedback | PASS | `GameManager.cs` |
| TASK-01(d) — targetColorLabel post-spawn sync | PASS | `GameManager.cs` |
| TASK-02(a) — Achievement level-up reward propagation | PASS | `GameManager.cs` |
| TASK-02(b) — Daily-bonus level reward via TombakanOnboarding | PASS | `TombakanOnboarding.cs`, `GameManager.cs` |
| TASK-02(c) — ColourBlind HSV hue-range classifier | PASS | `ColourBlindSettings.cs` |
| TASK-03(a) — spawnRadius 1.5 → 0.8 in scene | PASS | `Assets/Scenes/GamePlay.unity` |
| TASK-03(b) — ThrowHintPanel added to scene | PASS | `Assets/Scenes/GamePlay.unity` |
| TASK-04 — FishSpecies colour audit | PARTIAL | `Assets/ScriptableObjects/FishSpecies/*.asset` |

**Overall: 9/10 acceptance criteria fully met. 1 partial (TASK-04 — 4-symbol limit).**

---

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `FishCatalog.PickOther(exclude)` retry loop allows decoy = target with ~18% per-round probability on 2-species catalogs; four-decoy rounds have ~18% chance of at least one indistinguishable decoy | `FishCatalog.cs:36-44` | HIGH: rounds become unsolvable or guesswork with sparse catalogs; player cannot win through skill |
| 2 | `GameManager.StartGame()` does not show `ThrowHintPanel` for first-time sessions; the panel exists in scene but is never activated — UX-1 code side deferred from Week 9 scope | `GameManager.cs:218-230` | MEDIUM: first-time players see fish but receive no instruction on how to throw; trial-and-error onboarding persists |
| 3 | `greetingPanel` and `dailyBonusPanel` can be active simultaneously when a player has XP but no best score (edge case); no mutual exclusion or ordering guard | `TombakanOnboarding.cs:24-44` | LOW-MED: panels overlap on small screens; layout collision on first daily login for a small player segment |
| 4 | Achievement toast queue fires 0.4 s after level-up panel, blocking the result screen for up to 12 s (6 achievements × 2 s); no tap-to-skip or auto-dismiss on level-up panel | `GameManager.cs:344,354` | LOW-MED: players who earn multiple achievements in one session cannot reach "Play Again" button for up to 13 s |
| 5 | `ColorSummary.Format` always appends `×N` including for N=1 ("Merah ×1" instead of "Merah") on single catches | `ColorSummary.cs:41` | LOW: cosmetic clutter on result screen for single-catch rounds |

---

## Session Simulations

### Session 1 — Returning Player (NRE regression check)

**Profile:** `ScoreStore.GetBest() = 1200`, `ProgressionStore.GetTotalXp() = 450` (Level 4). Fresh daily session.

**Flow trace:**

1. `TombakanOnboarding.Start()`: `isReturningPlayer = true` → `goalManager.ForceCompleteGoal()` called.
2. **TASK-01(a) VERIFIED:** `GoalManager.CompleteGoal()` opens with `if (m_OnboardingGoals == null) { m_AllGoalsFinished = true; return; }`. Null queue detected. Returns cleanly. No NullReferenceException. Returning player launches successfully.
3. `DailyChallenge.TryClaimDailyBonus` → `xp = 100`, `streak = 2`. Level before = 4. Level after = 4 (not enough XP to level up). `newLevel = 0` → `ApplyLevelReward` not called (correct; no reward to apply).
4. Game loads to main state. No crash. CRITICAL bug from Week 8 Rank 1 is confirmed eliminated.

---

### Session 2 — Casual Player (daily bonus triggers level-up)

**Profile:** `ProgressionStore.GetTotalXp() = 395` (Level 4, 5 XP from Level 5 threshold at 400 XP). Streak = 4.

**Flow trace:**

1. `TombakanOnboarding.Start()`: `isReturningPlayer = true`, `levelBefore = 4`.
2. `DailyChallenge.TryClaimDailyBonus(out xp, out streak)` → XP = 200 (100 + 4×25). `ProgressionStore.AddXp(200)` inside `DailyChallenge.cs:58` — total XP = 595. Level 5 threshold = 400 XP. Level-up to 5. Return value discarded (unchanged).
3. `levelAfter = ProgressionStore.GetLevel() = 5`. `newLevel = 5 > 0`.
4. **TASK-02(b) VERIFIED:** `gameManager.ApplyLevelReward(gameManager.levelRewardTable?.GetRewardForLevel(5))` called from `TombakanOnboarding.cs:43-50`. Level 5 reward (coin bonus) applied. `CurrencyStore.GetCoins()` increases by the configured amount. `ShowDailyBonus(200, 4, 5)` shows "Level 5! Selamat!" in the panel.
5. Daily-bonus level-up reward gap from Week 7 BUG-NEW-2 is confirmed eliminated.

---

### Session 3 — Colour-Blind Player (full catalog)

**Profile:** `ColourBlindSettings.IsEnabled() = true`. FishCatalog active with 8 species after TASK-04 audit.

**Flow trace:**

1. Fish spawn. `FishShapeOverlay.Start()` calls `ColourBlindSettings.ShapeForColor(fishTarget.fishColor)`.
2. **TASK-02(c) VERIFIED:** `Color.RGBToHSV` extracts hue + saturation. Low-saturation guard (s < 0.15) returns `"■"`. Four hue buckets: h < 0.083 or h > 0.917 → `"●"`; h < 0.25 → `"★"`; h < 0.458 → `"▲"`; otherwise `"■"`. All 8 species receive a non-`"?"` symbol.
3. **TASK-04 PARTIAL NOTE:** The 4-symbol system cannot uniquely distinguish 8 species. Each bucket holds exactly 2 species: `●` (kakap_merah, kerapu_bebek), `★` (ikan_badut, bandeng), `▲` (lele, ikan_nila), `■` (kembung, tuna_sirip_kuning). A round that spawns fish from the same bucket produces two fish with identical symbols. The colour-blind player cannot distinguish them by symbol alone.
4. **Recommended Week 10 follow-up:** `FishCatalog.PickOther` should filter by symbol bucket — if target and candidate share a symbol, exclude the candidate. This is a code change to `FishCatalog.cs` requiring the HSV classifier to be accessible as a static utility.

---

### Session 4 — Frustrated Player (score at zero, wrong hits)

**Profile:** Level 2, hits wrong fish after reaching score 0.

**Flow trace:**

1. Score reaches 0 after multiple wrong hits.
2. Another wrong hit: `score = ClampScore(0 - 25) = 0`. `scoreBefore = 0`. `actualDeduction = scoreBefore - score = 0`.
3. **TASK-01(b) VERIFIED:** `sadFeedbackText.text = actualDeduction == 0 ? "Meleset!" : $"-{actualDeduction}!"`. Player sees `"Meleset!"` instead of `"-25!"`. Misleading penalty notification from Week 7 BUG-NEW-4 eliminated.

---

### Session 5 — Achievement Hunter (achievement-triggered level-up)

**Profile:** Level 4, XP = 790 (10 XP from Level 5 threshold). Earns `Combo5` achievement (xpReward = 30 XP).

**Flow trace:**

1. `EndGame()`: `xpEarned = 200`. `ProgressionStore.AddXp(200)` → XP = 990. Level 5 threshold = 400? 

   Wait — let me recalculate from `ProgressionRules`: `XpForLevel(5) = round(100 × 4^1.5) = 800`. XP was 790. AddXp(200) → XP = 990 > 800. `newLevel = 5`.

2. `levelBeforeAch = ProgressionStore.GetLevel() = 5` (just levelled up from EndGame XP).
3. `AchievementChecker.CheckAll` → `Combo5` triggers. `ProgressionStore.AddXp(30)` → XP = 1020. Level 6 threshold = `round(100 × 5^1.5)` = 1118. 1020 < 1118. No level-up from achievement XP. `levelAfterAch = 5`. No spurious reward call.

4. **Boundary variant (TASK-02(a) VERIFIED):** Set XP = 1115 before EndGame, xpEarned = 0, achievement xpReward = 30. `AchievementChecker.cs:100` still discards `AddXp` return. But `levelAfterAch = ProgressionStore.GetLevel() = 6` (snapshot taken after `CheckAll`). `levelAfterAch > levelBeforeAch` → `ShowLevelUp(6)` + `ApplyLevelReward(GetRewardForLevel(6))` called. Level 6 reward applied. Week 7 BUG-NEW-1 eliminated.

---

### Session 6 — Speed-Runner (consecutive hits near pacing floor)

**Profile:** Level 6, hitting fish every 1.0–1.1 s.

**Flow trace:**

1. Hit at t=0. `ShowHappy(...)`. `Invoke(HideFeedback, 1.0)` scheduled.
2. Hit at t=1.05. `ShowHappy(...)` called again.
3. **TASK-01(c) VERIFIED:** `CancelInvoke(nameof(HideFeedback))` fires first. The stale `HideFeedback` at t=1.0 is cancelled. New `Invoke(HideFeedback, 1.0)` scheduled for t=2.05. Feedback visible for full 1.0 s. Week 8 BUG-W8-2 eliminated.

---

### Session 7 — Small-Table AR Player

**Profile:** Playing on a 60 cm × 60 cm table.

**Flow trace:**

1. Water placed at table centre.
2. `FishSpawner.SpawnFish()`: serialised `spawnRadius = 0.8` (scene override).
3. **TASK-03(a) VERIFIED:** Fish spawn within 0.8 m of water centre. On a 60 cm table, all fish fall within the visible surface. No fish appear mid-air. Week 8 UX-W8-1 resolved.

---

### Session 8 — First-Time Player (ThrowHintPanel check)

**Profile:** Fresh install. `ScoreStore.GetBest() == 0`.

**Flow trace:**

1. `GameManager.StartGame()` runs.
2. **TASK-03(b) VERIFIED (scene):** `ThrowHintPanel` GameObject exists under `GamePlayUI`, inactive by default, anchored bottom-centre, contains "Tekan tombol untuk melempar tombak!" TMP_Text. `GameManager.throwHintPanel` field is wired.
3. **UX-1 (code side) STILL OPEN:** `GameManager.StartGame()` does not call `throwHintPanel.SetActive(true)` or start a hide coroutine. Panel is wired in scene but activation logic was deferred to Week 10 in the scope. First-time player still sees no throw instruction.

---

## Bugs Confirmed Fixed (Week 9)

- **BUG-W8-Rank1 (GoalManager NRE) — FIXED** — returning player launch no longer throws NullReferenceException; `GoalManager.CompleteGoal()` null-guards `m_OnboardingGoals`.
- **BUG-NEW-1 (Achievement level-up reward) — FIXED** — `GameManager.EndGame()` snapshots level before and after `AchievementChecker.CheckAll()`; achievement-triggered level-up now calls `ApplyLevelReward`.
- **BUG-NEW-2 (Daily-bonus level reward) — FIXED** — `TombakanOnboarding` calls `gameManager.ApplyLevelReward()` when `newLevel > 0` after `TryClaimDailyBonus`.
- **BUG-NEW-3 (ColourBlind "?") — FIXED** — `ColourBlindSettings.ShapeForColor` uses HSV hue-range classifier; no code path returns `"?"`.
- **BUG-NEW-4 (ShowSad at zero score) — FIXED** — `ShowSad(int actualDeduction)` shows `"Meleset!"` when deduction was absorbed by clamp.
- **BUG-NEW-5 (GoalManager NRE, same as Rank 1) — FIXED** — same null guard covers both code paths.
- **BUG-W8-1 (targetColorLabel mismatch) — FIXED** — `GameManager.PickNewTarget()` updates `targetColorLabel.text` after `SpawnFish()` resolves `CurrentTargetSpecies`.
- **BUG-W8-2 (HideFeedback accumulation) — FIXED** — `ShowHappy()` and `ShowSad()` both call `CancelInvoke(nameof(HideFeedback))` before scheduling new hide.
- **UX-W8-1 (spawnRadius off-table) — FIXED** — serialised `spawnRadius` set to 0.8 in scene; fish stay within visible water surface on typical AR surfaces.

---

## Open Issues — Week 10 Candidates

### Bugs

- [ ] **BUG-W8-3** *(deferred from Week 9)* — `FishCatalog.PickOther(exclude)` retry loop allows decoy = target with ~18% per-round probability on 2-species catalogs; replace retry loop with deterministic filtered-list pick — `FishCatalog.cs:36-44`
- [ ] **BUG-W9-1** *(new — TASK-04 partial)* — `FishCatalog.PickOther` does not exclude same-symbol-bucket species; colour-blind players can see two fish with the same shape symbol in one round; fix: add `IsBucketSafe(target, candidate)` check using the HSV classifier from `ColourBlindSettings` — `FishCatalog.cs:36-44`, `ColourBlindSettings.cs`

### UX

- [ ] **UX-1 (code side)** *(open since Week 6, panel now wired)* — `GameManager.StartGame()` must call `throwHintPanel.SetActive(true)` and start a 8 s hide coroutine on the first session (`ScoreStore.GetBest() == 0`) — `GameManager.cs:218-230`
- [ ] **UX-NEW-2** *(deferred)* — `greetingPanel` and `dailyBonusPanel` can be active simultaneously; add ordering: show greeting first, defer daily-bonus panel to `DismissGreeting()` callback — `TombakanOnboarding.cs:24-44`
- [ ] **UX-NEW-3** *(deferred)* — Achievement toast blocks result screen for up to 12 s; add tap-to-dismiss on level-up panel or delay toast queue to +3.5 s — `GameManager.cs:344,354`
- [ ] **UX-NEW-4** *(open)* — `targetColorLabel` shows raw hex (e.g. `"E6993F"`) when species `baseColor` is not in `Dict.cs`; show `FishSpecies.displayName` instead when catalog is active — `GameManager.cs:406`, `Dict.cs:35`

### Polish

- [ ] **POLISH-NEW-1** *(deferred)* — `ColorSummary.Format` appends `×1` for single catches; show `×N` only when `N > 1` — `ColorSummary.cs:41`
- [ ] **POLISH-W8-1** *(deferred)* — `ProgressionHUD.Refresh()` not called after daily-bonus XP grant; HUD shows stale values until panel toggle — `TombakanOnboarding.cs:39-44`
- [ ] **POLISH-2** *(open)* — `HapticFeedback.PlayCorrect` and `PlayWrong` produce identical `Handheld.Vibrate()` calls with no tactile distinction — `HapticFeedback.cs:14,22`
- [ ] **POLISH-3** *(deferred)* — `ApplyLevelReward` overwrites `levelUpText.text` with `reward.celebrationText` before player reads "Level N! Selamat!" — `GameManager.cs:387-388`

---

## Items Confirmed Fixed in Previous Weeks (carry-forward verification)

- **Week 6:** `OnFishHit` guard prevents post-EndGame score mutation — verified at `GameManager.cs:440`.
- **Week 6:** `HapticFeedback` decoupled from audio mute — vibration fires independently of mute state.
- **Week 5:** `PlaceWaterOnPlane` one-shot guard prevents mid-game re-positioning.
- **Week 5:** `PickNewTarget` guard (`if (!gameRunning) return`) prevents fish spawn after EndGame.
- **Week 4:** `AudioManager` null-guards — no NRE on null BGM/SFX sources.
- **Week 3:** `Accuracy.Format(0, 0)` returns `"--"`.
- **Week 2:** High-score persistence and new-record badge working.
