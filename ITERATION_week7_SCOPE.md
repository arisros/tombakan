# Iteration Week 7 — Scope

**Date:** 2026-09-07  
**Branch:** `iteration/week-7`  
**Input:** TESTER_REPORT_week7.md, BACKLOG.md, ITERATION_LOG.md  

---

## Selected Tasks

### TASK-01 — Throw-mechanic tutorial hint for first-time players (P0)
**Owner:** dev  
**Source:** BUG-1 (CRITICAL), BACKLOG Week 7 candidate UX-1  
**Files:** `Assets/MobileARTemplateAssets/Scripts/TombakanOnboarding.cs`

Add a coaching step in `TombakanOnboarding` that activates immediately after AR placement succeeds and before the player can interact with fish. The step shows a hint panel with text "Sentuh tombol untuk melempar tombak" and a visual pointer at the throw button. The panel auto-dismisses after the first successful `SpearThrower.ThrowSpear()` call. Only runs for players with zero prior throws (first-time onboarding path); returning-player flow is unchanged.

**Acceptance criteria:**
- `TombakanOnboarding.cs` exposes a `ShowThrowHint()` method that activates the throw hint panel and registers a one-shot listener on `SpearThrower.OnThrowFired` (or equivalent event) to call `HideThrowHint()`.
- `StartCoaching()` (called after placement) invokes `ShowThrowHint()` when `totalXp == 0 && throwCount == 0`.
- Hint panel is inactive by default; auto-dismissed on first throw event with no player action required.
- No changes to flow for players with `totalXp > 0`.

---

### TASK-02 — Fix daily level-up reward never applied
**Owner:** dev  
**Source:** BUG-3 (HIGH), BACKLOG Week 7 candidate "DailyChallenge.cs:58 reward grant still skipped"  
**Files:** `Assets/MobileARTemplateAssets/Scripts/DailyChallenge.cs`, `Assets/MobileARTemplateAssets/Scripts/TombakanOnboarding.cs`

`DailyChallenge.TryClaimDailyBonus()` discards the new-level return value from `ProgressionStore.AddXp()`. `TombakanOnboarding.Start()` surfaces the level-up text but never calls `GameManager.ApplyLevelReward()`. Fix both: (1) capture and return the level from `TryClaimDailyBonus`, (2) in `TombakanOnboarding.Start()` call reward application when `newLevel > 0`.

**Acceptance criteria:**
- `DailyChallenge.TryClaimDailyBonus()` returns (or exposes via out-param) the new level number when a level-up occurs, instead of discarding it.
- `TombakanOnboarding.Start()` calls `GameManager.I.ApplyLevelReward(newLevel)` (or an equivalent extracted helper) when it detects `newLevel > 0` from the daily claim.
- After a daily level-up, the reward configured in `LevelRewardTable` for that level (coins, species unlock, spear skin) is applied to `CurrencyStore` / `SpearStore` / `FishdexStore`.
- The `EndGame` level-up reward path (existing) is not regressed.

---

### TASK-03 — "Posisi ulang" reposition button before game start
**Owner:** ui  
**Source:** BUG-2 (HIGH), BACKLOG Week 7 candidate "Re-position water button"  
**Files:** `Assets/MobileARTemplateAssets/Scripts/PlaceWaterOnPlane.cs`, `Assets/Scenes/GamePlay.unity`

`PlaceWaterOnPlane` permanently disables after first placement with no recovery path. Add a `Reposition()` method to `PlaceWaterOnPlane` that re-enables the component, hides the water prefab, clears active fish, and re-enables `ARPlaneManager` scanning. Add a "Posisi ulang" button to the pre-game HUD in the scene, wired to this method. The button must be hidden once `StartGame()` is called (preserving the mid-game guard).

**Acceptance criteria:**
- `PlaceWaterOnPlane.cs` has a public `Reposition()` method implementing the reset described above.
- A "Posisi ulang" UI button is present in `GamePlay.unity`, active only when `gameRunning == false` (pre-game), hidden after `StartGame()` fires.
- The button calls `PlaceWaterOnPlane.Reposition()` via UnityEvent or direct reference.
- After tapping Reposition, subsequent AR taps on a valid plane re-place the water correctly and fish spawn at the new position.

---

### TASK-04 — Fix non-standard Indonesian colour names in Dict.cs
**Owner:** artist  
**Source:** UX-1 (LOW-MEDIUM), TESTER_REPORT_week7.md  
**Files:** `Assets/MobileARTemplateAssets/Scripts/Dict.cs`

`Dict.cs` labels hex `00FFFF` as "Sian" (English transliteration) and `FF00FF` as "Magenta" (English loanword). Neither is standard Indonesian vocabulary; players unfamiliar with English colour terms cannot identify the target fish from the label alone.

**Acceptance criteria:**
- `Dict.cs`: entry for `00FFFF` returns `"Toska"`.
- `Dict.cs`: entry for `FF00FF` returns `"Merah Lembayung"`.
- All existing unit tests that reference these colour names are updated to match the new strings.
- No other Dict.cs entries are altered.

---

## Task Summary

| ID | Owner | Severity | Source |
|----|-------|----------|--------|
| TASK-01 | dev | P0 CRITICAL | BUG-1 |
| TASK-02 | dev | HIGH | BUG-3 |
| TASK-03 | ui | HIGH | BUG-2 |
| TASK-04 | artist | LOW-MEDIUM | UX-1 |

**Not in scope this week (deferred):**
- BUG-4 (targetColor float mismatch) — valid but low real-world impact without baseColor authoring inconsistencies; defer to Week 8
- BUG-5 (double spear double-hit) — medium scope, affects only speed-runners; defer to Week 8
- BUG-6 (greetingPanel no auto-dismiss) — 1-line fix, but greeting panel scene wiring is still blocked; defer
- BUG-7 (no pause/quit) — medium scope, scope to "quit to menu" in Week 8
- BUG-8 (Zen mode float.MaxValue) — low real-world risk (StartGame resets timeLeft); defer
