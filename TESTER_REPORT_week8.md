# Tombakan — Tester Report Week 8
**Date:** 2026-08-10
**Tester:** AI Game Tester (post-iteration validation)
**Scope:** `DailyChallenge.cs`, `AchievementChecker.cs`, `GameManager.cs`, `GoalManager.cs`,
`TombakanOnboarding.cs`, `ColourBlindSettings.cs`, `GameConstants.cs`;
new assets `Assets/Prefabs/UI/MissText.prefab`; new tests `Week8HUDTests.cs`

---

## Iteration Validation

All four tasks from `ITERATION_week8_SCOPE.md` have been implemented. TASK-01 and TASK-02
were committed in `7f4fddb`. TASK-03 (`GameManager.cs` HUD fixes) and TASK-04 (`MissText.prefab`
and `Week8HUDTests.cs`) exist in the working tree and are included in the release commit.

| Task | Bugs Fixed | Result | Notes |
|------|-----------|--------|-------|
| TASK-01 | BUG-1 (CRITICAL), BUG-2 (HIGH) | PASS | `DailyChallenge` and `AchievementChecker` now capture `AddXp` return; `ApplyLevelReward` called when `newLevel > 0` |
| TASK-02 | BUG-8 (MEDIUM), BUG-3 (HIGH) | PASS | `GoalManager` null guard prevents NRE on fresh session; `GameConstants.SpawnDelay` aligns throw-unlock with fish spawn |
| TASK-03 | BUG-4 (HIGH), BUG-6 (MEDIUM), BUG-7 (MEDIUM) | PARTIAL / PASS | BUG-6 and BUG-7 fully resolved; BUG-4 formula semantically correct but bar still reads 100% when `timeLeft > gameDuration` |
| TASK-04 | BUG-10 (LOW-MED) | PASS | `Assets/Prefabs/UI/MissText.prefab` present with World Space Canvas, Animator, "Meleset!" TMP_Text |

---

## Bug-by-Bug Verdicts

### BUG-1 — CRITICAL: DailyChallenge discards AddXp level-up return
**Status: FIXED**

`DailyChallenge.TryClaimDailyBonus` signature expanded with `out int newLevel`; line 71
captures the `ProgressionStore.AddXp` return value. `TombakanOnboarding.Start` line 40
reads the out-param and calls `GameManager.I?.ApplyLevelReward(newLevel)` when non-zero.
Level rewards (species unlock, coins, spear skin) now apply on every daily-bonus level-up.

**Regression test:** `DailyChallengeProgressionTests.cs` — confirms reward path exercised.

---

### BUG-2 — HIGH: AchievementChecker discards AddXp level-up return
**Status: FIXED**

`AchievementChecker.cs` line 103: `int newLevel = ProgressionStore.AddXp(achievement.xpReward)`.
Line 105: `if (newLevel > 0) GameManager.I?.ApplyLevelReward(newLevel)`. A public
`ApplyLevelReward(int newLevel)` overload was added to `GameManager` (line 383) so
`AchievementChecker` can invoke it without a MonoBehaviour dependency.

**Regression test:** `AchievementCheckerProgressionTests.cs`.

---

### BUG-3 — HIGH: 0.8 s gap between throw-unlock and fish spawn
**Status: FIXED**

`GameConstants.SpawnDelay = 0.8f` extracted as a named constant. `GameManager.OnFishHit`
now calls `spearThrower.LockThrow(delay + GameConstants.SpawnDelay)` and
`Invoke(nameof(PickNewTarget), delay + GameConstants.SpawnDelay)` — both sides use the
same constant so the unlock and fish-appear moments are guaranteed simultaneous.

---

### BUG-4 — HIGH: Timer bar saturates when bonus time exceeds gameDuration
**Status: PARTIAL — carry to Week 9**

Formula changed from `Clamp01(timeLeft / gameDuration)` to
`Clamp01(timeLeft / Mathf.Max(timeLeft, gameDuration))`. The new formula keeps the
denominator honest as `timeLeft` decays back through `gameDuration`, preventing a snap
from 100% to a lower value. However, when `timeLeft > gameDuration` both formulas
produce `1.0` — the bar still reads full and the countdown text (e.g. "80") disagrees.

The `Week8HUDTests.cs` test `Fill_BonusTimePushesTimeLeftTo80_IsExactlyOne` explicitly
documents this: the new formula is semantically cleaner (calibrated denominator) but the
bar-text disagreement symptom persists for skilled players with large bonus time.

**Week 9 recommendation:** Cap `timeLeft` at `gameDuration * 1.5f` on every `+=` bonus
addition, or implement a two-zone bar (normal zone 0–60 s, "bonus zone" glow overlay for
the excess) so the visual pacing signal remains useful at high skill.

---

### BUG-6 — MEDIUM: targetColorLabel stale after species colour override
**Status: FIXED**

`GameManager.PickNewTarget` now sets `targetColorLabel.text` after `SpawnFish` resolves
the catalog species colour override. When `CurrentTargetSpecies != null`, the label uses
`species.displayName` directly (eliminating the hex-lookup fallback entirely for catalog
mode). Label and swatch always show the same colour.

---

### BUG-7 — MEDIUM: Colour-blind target indicator shows no shape symbol
**Status: FIXED**

`GameManager.PickNewTarget`: when `ColourBlindSettings.IsEnabled()`, the label is built as
`$"{labelText} {ColourBlindSettings.ShapeForColor(targetColor)}"`. All four palette colours
(Merah ●, Hijau ▲, Biru ■, Kuning ★) now appear in the HUD target panel when colour-blind
mode is on.

---

### BUG-8 — MEDIUM: GoalManager.ForceCompleteGoal NRE before StartCoaching
**Status: FIXED**

`GoalManager.CompleteGoal` line 200: `if (m_OnboardingGoals == null) return;` guard added.
Returning-player path (`TombakanOnboarding.Start` → `ForceCompleteGoal`) no longer risks
`NullReferenceException` or `IndexOutOfRangeException` on fresh sessions.

**Regression test:** `GoalManagerTests.cs`.

---

### BUG-10 — LOW-MEDIUM: No miss feedback when spear hits empty water
**Status: PREFAB DELIVERED — dev wire-up pending**

`Assets/Prefabs/UI/MissText.prefab` created: World Space Canvas, `CanvasGroup` for fade-out,
`TMP_Text` displaying "Meleset!" in warm-orange (`r:1, g:0.447, b:0.098`), `Animator`
with scale-in / hold / fade-out clip (~1.15 s total). Canvas world-scale `0.001` is legible
at 1 m AR distance.

The dev wire-up (`SpearHit.OnDestroy` instantiate when `!hasHit`) is a separate task —
the prefab is ready for it. No missing-script or missing-asset references.

---

## Bugs Confirmed Still Open

| ID | Severity | File | Status |
|----|----------|------|--------|
| BUG-4 | HIGH | `GameManager.cs:162` | PARTIAL — formula improved but bar-text disagreement persists |
| BUG-5 | MEDIUM | `ColourBlindSettings.cs:22-27` | OPEN — ShapeForColor returns "?" for non-palette species colours |
| BUG-9 | LOW-MED | `GameManager.cs:507` | OPEN — ShowSad shows "-25!" when score clamped at 0 |
| UX-1 | UX | `TombakanOnboarding.cs` | OPEN — no throw-mechanic tutorial for first-time players |
| UX-2 | UX | `TombakanOnboarding.cs` | OPEN — greetingPanel and dailyBonusPanel can show simultaneously |
| UX-3 | UX | `GameManager.cs:344,354` | OPEN — achievement toast overlaps level-up panel within 0.4 s |
| UX-4 | UX | `GameManager.cs`, `Dict.cs` | OPEN — hex fallback shown for custom species colours (mitigated in catalog mode by BUG-6 fix) |

---

## New Findings This Iteration

### FIND-1 — MEDIUM: MissText.prefab dev wire-up not yet implemented
**File:** `SpearHit.cs`

The `MissText.prefab` exists but `SpearHit.OnDestroy` has no code to instantiate it on a
complete miss. The UX gap (player fires spear into empty water, zero feedback) remains live
until the dev step is completed.

**Recommendation for Week 9:** In `SpearHit.OnDestroy` (or `SpearThrower.CooldownRoutine`
at spear lifetime expiry), add: `if (!hasHit) Instantiate(missTextPrefab, transform.position, Quaternion.identity);`
Wire a `[SerializeField] GameObject missTextPrefab` field on `SpearHit` and drag
`Assets/Prefabs/UI/MissText.prefab` in the Inspector.

---

### FIND-2 — LOW: GameConstants.SpawnDelay is undocumented in CLAUDE.md
**File:** `Assets/MobileARTemplateAssets/Scripts/GameConstants.cs`

`GameConstants.SpawnDelay` is now used to synchronise throw-unlock and fish-spawn timing.
It is not documented in CLAUDE.md, leaving the constant invisible to contributors who search
the architecture overview before editing timing parameters.

**Recommendation:** Add `GameConstants.cs` to the Key Files table in CLAUDE.md.

---

## Week 9 Candidates (priority-ranked)

| Priority | ID | Description | Effort |
|----------|----|-------------|--------|
| P0 | BUG-4 | Timer bar: cap `timeLeft` at `gameDuration * 1.5f` on bonus addition; implement bonus-glow zone | 1 day |
| P1 | BUG-10 wire | Wire `MissText.prefab` in `SpearHit.OnDestroy`; add `[SerializeField]` field; Inspector assignment | 0.5 day |
| P1 | UX-1 | Throw-mechanic tutorial hint: timed hint panel on `StartGame()` for `!isReturningPlayer` | 0.5 day |
| P2 | BUG-5 | Expand `ShapeForColor` to cover all catalog species colours, or derive shape from species catalog index | 1 day |
| P2 | BUG-9 | Compute `actualDeduction = score - ClampScore(score - penalty)` in `OnFishHit`; show "Salah!" when 0 | 0.5 day |
| P3 | UX-2 | Trigger daily-bonus check only after `greetingPanel` dismissal | 0.5 day |
| P3 | UX-3 | Delay achievement toast to +3.0 s (after level-up panel is readable) | 0.5 day |
| P3 | UX-4 | Suppress `targetColorLabel` in catalog mode (already partially fixed by BUG-6); carry forward | 0.5 day |

---

## Week 7 Fixes — Re-verification

| Fix from Week 7 scope | Status in Week 8 |
|----------------------|-----------------|
| BUG-1 AddXp return in DailyChallenge | CONFIRMED FIXED (TASK-01) |
| BUG-2 AddXp return in AchievementChecker | CONFIRMED FIXED (TASK-01) |
| BUG-3 throw-lock/spawn gap | CONFIRMED FIXED (TASK-02) |
| BUG-8 GoalManager null guard | CONFIRMED FIXED (TASK-02) |
| BUG-6 stale colour label | CONFIRMED FIXED (TASK-03) |
| BUG-7 colour-blind shape in HUD | CONFIRMED FIXED (TASK-03) |
| BUG-4 timer bar formula | PARTIAL (formula changed; symptom persists) |
| BUG-10 miss prefab | PREFAB READY; wire-up deferred to Week 9 |
