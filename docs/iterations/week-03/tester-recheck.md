# Re-Tester Confirmation — Week 3

**Date:** 2026-06-02

## UX-10 — Target colour name label ✅ IMPLEMENTED
Optional `targetColorLabel` shows the Indonesian colour word each round via
`ColorHexLocalization.ToIndonesian(targetColor)`. Sessions 5, 6, 8 (glare / vocabulary /
colour-blind) now have a text cue. Null-safe: unassigned field changes nothing.

## UX-08 — Adaptive inter-round delay ✅ IMPLEMENTED
`PacingRules.HitDelayForProgress(hitDelay, correctHitCount)` shrinks the lock by 0.1 s per
correct hit, floored at 1.0 s and capped at the base. `OnFishHit` uses it for both
`LockThrow` and the retarget invoke (keeping the +0.8 s offset). Late-game pacing
complaints (sessions 1, 4, 9) resolved; early game unchanged.

## UX-09 — Accuracy stat ✅ IMPLEMENTED
`wrongHitCount` now tracked (reset in `StartGame`, incremented on wrong hits).
`Accuracy.Format(correct, wrong)` renders "9/12 (75%)" into optional `resultAccuracyText`
in `EndGame`. Sessions 2, 3, 7 get the requested feedback. Null-safe.

## BUG-07 — `WaterRipple` NRE ✅ FIXED
Renderer resolved via `TryGetComponent`; component disables itself with a warning when
absent and `Update` early-returns if null. `[RequireComponent(typeof(Renderer))]` added to
prevent the misconfiguration in the editor. No NRE under any configuration (session 10
clean).

## Automated Tests
`Week3Tests.cs` adds 9 EditMode tests over `PacingRules` and `Accuracy`, asserting the real
static logic (incl. floor/base clamps, negative-hit guard, and rounding). Total suite across
weeks 1–3 keeps growing; runs in CI via game-ci once `UNITY_LICENSE` is set.

## Regression Check
Changes isolated to `GameManager.cs` (target label, accuracy, adaptive delay), `WaterRipple.cs`,
and two new pure helper classes (same assembly — no asmdef changes). Spear throw, fish swim,
AR placement, audio, and the Week 1–2 scoring paths untouched. No regressions across the
10 re-simulated sessions.
