# Re-Tester Confirmation — Week 2

**Date:** 2026-06-02

## BUG-04 — Fish not cleared on game end ✅ FIXED
`FishSpawner.ClearAll()` added and called from `GameManager.EndGame()`. The active shoal is
destroyed the moment the timer hits 0, so no `FishSwim` instances remain behind the result
panel. Sessions 1, 5, 10 re-run clean.

## BUG-05 — Duplicated colour palette ✅ FIXED
Palette now lives only in `FishPalette.Options`. `GameManager.PickNewTarget` and
`FishSpawner` decoys both read it; `FishSpawner.RandomOtherColor` deleted. No drift possible.

## UX-05 — Only three colours ✅ IMPLEMENTED
Yellow ("Kuning") added. Pure-guess odds drop from 1-in-3 to 1-in-4.
**Caught during implementation:** `Color.yellow` serialises to `FFEB04`, which has no
`Dict.cs` mapping — used `new Color(1,1,0)` (`FFFF00`) instead. Regression test
`EveryPaletteColor_HasIndonesianMapping` now guards this permanently.

## UX-06 — High-score persistence + record moment ✅ IMPLEMENTED
`ScoreStore` (PlayerPrefs) added. `EndGame` calls `TrySetBest(score)`, refreshes optional
`bestScoreText` / `resultBestScoreText`, and toggles `newRecordBadge` on a record. Best
score also shown on the main screen at `Start`. All UI fields null-safe.

## UX-07 — Result colour list overflow ✅ IMPLEMENTED
`ColorSummary.Format` aggregates to counts in first-seen order ("Merah ×3, Biru ×1"),
overflow-safe. `EndGame` uses it instead of `string.Join`. Empty input returns the existing
"Tidak ada ikan dikumpulkan" placeholder.

## Automated Tests
`Week2Tests.cs` adds 11 EditMode tests across `FishPalette`, `ScoreStore`, `ColorSummary`,
all asserting against the real static logic. Combined with Week 1's suite they run in CI via
game-ci once `UNITY_LICENSE` is set.

## Regression Check
Changes isolated to scoring/spawn/result logic plus three new pure helper classes (same
assembly, no asmdef changes). Spear throw, fish swim, AR placement, and audio paths
untouched. No regressions observed across the 10 re-simulated sessions.
