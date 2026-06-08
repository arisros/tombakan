# Iteration Week 7 — Scope

**Date:** 2026-06-08  
**Source:** TESTER_REPORT_week7.md  
**Branch:** iteration/week-7

## Selected Tasks (5 + 1 bonus)

| ID | Task | Files |
|----|------|-------|
| TASK-01 | `AchievementChecker` silent level-up — `GameManager.ApplyLevelReward` made public; level-up from achievement XP now applies `LevelRewardTable` rewards | `GameManager.cs` |
| TASK-02 | Daily-bonus `ApplyLevelReward` gap — `DailyChallenge.TryClaimDailyBonus` now returns `out int newLevel`; `TombakanOnboarding.ShowDailyBonus` calls `ApplyLevelReward` when daily XP crosses level boundary | `DailyChallenge.cs`, `TombakanOnboarding.cs`, `GameManager.cs` |
| TASK-03 | `ColourBlindSettings.ShapeForColor` — palette dictionary first, hue-proximity fallback for catalog species; no more `"?"` for any colour | `ColourBlindSettings.cs` |
| TASK-04 | `ShowSad()` truthful penalty — `int actualDeduction = Mathf.Min(penalty, score)`; shows `"Miss!"` when score was already 0 | `GameManager.cs` |
| TASK-05 | Throw-mechanic tutorial hint — `TombakanOnboarding.NotifyGameStarted` + `ShowThrowHint` coroutine; `SpearThrower.NotifyFirstThrow` dismisses on first throw | `TombakanOnboarding.cs`, `SpearThrower.cs`, `GameManager.cs` |
| BONUS | `ColorSummary ×1` suppression — single catches show `"Merah"` not `"Merah ×1"` | `ColorSummary.cs`, `Tests/Week2Tests.cs` |

## Deferred

- `GoalManager.ForceCompleteGoal` NRE risk — needs scene wiring audit
- Platform-specific haptic differentiation — needs device test matrix
- "Re-position water" button — needs Unity Editor scene access
- Colour-blind target indicator shape — needs UI label addition in scene
- Achievement toast / level-up panel overlap — lower severity, addressed partially by delayed coroutines
