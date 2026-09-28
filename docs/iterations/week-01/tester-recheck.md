# Re-Tester Confirmation — Week 1

**Date:** 2026-06-02

## BUG-01 — Score floor ✅ FIXED
`GameManager.OnFishHit` now runs `score = Mathf.Max(0, score)` after every penalty.
Sessions 2 and 5 (previously −75 / negative display) would now show 0.

## BUG-02 — Dead `UpdateTimerUI` ✅ FIXED
Method removed from `GameManager`. No ghost `timer` variable reference.

## BUG-03 — `collectedFishColors[1000]` ✅ FIXED
Replaced with `List<string> collectedFishColors`. `EndGame` uses `.Count`; `OnFishHit` uses `.Add()`.

## UX-01 — Numeric timer ✅ IMPLEMENTED
`public TMP_Text timerCountdownText` field added. When wired in scene, displays
`CeilToInt(timeLeft)` each frame. Null-safe — no crash if not assigned.

## UX-02 — Combo streak ✅ IMPLEMENTED
`comboStreak` tracks consecutive correct hits. At 3: 2× multiplier (+200 per hit).
At 5+: 3× multiplier (+300 per hit). Wrong hit resets streak to 0. Feedback text
shows "x2 COMBO! +200!" etc.

## UX-03 — Dynamic fish count ✅ IMPLEMENTED
`FishCountForDifficulty(correctHitCount)` drives `fishSpawner.fishCount` before each
spawn: 3 → 4 → 5 → 6 → 7 fish as score increases. Default inspector value updated to 3.

## UX-04 — Tier thresholds ✅ RAISED
New thresholds: Empty=0, Low=1–4, Mid=5–9, High=10–14, Legend≥15.
`TierLegend` Image field added (null-safe; falls back to TierHigh if not wired).

## Regression Check
All 10 simulated sessions re-evaluated. No regressions detected in spear throw, fish
swim, AR surface detection, or audio playback paths. Changes are isolated to
`GameManager.cs` (scoring/UI/tier logic) and `FishSpawner.cs` (fish count field only).
