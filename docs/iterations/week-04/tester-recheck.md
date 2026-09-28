# Re-Tester Confirmation — Week 4

**Date:** 2026-06-02

## BUG-08 — AudioManager NRE ✅ FIXED
All play paths null-guard their `AudioSource`/`AudioClip` (`PlayBGM`, `PlaySfx`). A scene
with an unassigned source no longer throws — it fails soft. Session 7 startup clean.

## UX-11 — Audio mute + persistence ✅ IMPLEMENTED
`AudioPrefs` (PlayerPrefs) stores the mute flag. `AudioManager.SetMuted/ToggleMute` mute
both sources and persist; `Awake` restores the saved state and applies it. `PlayBGM` re-
applies mute when swapping clips. (HUD button to call `ToggleMute()` is scene work — runtime
API is ready.)

## UX-12 — Progressive colour difficulty ✅ IMPLEMENTED
`FishPalette.CountForProgress` starts at 3 colours and adds one every 5 correct hits,
clamped to the palette size (4). `PickNewTarget` picks the target from
`ActiveOptions(count)`; decoys use `RandomOther(target, count)`. Beginners start with 3
(sessions 4), the 4th colour unlocks once warmed up (sessions 3). Correct fish always
present; decoys never equal the target and stay within the active subset.

## UX-13 — Combo-scaled time bonus + reactive warning ✅ IMPLEMENTED
`TimeBonus.ForHit(comboStreak)` adds 0.5 s × combo multiplier on each correct hit
(0.5 / 1.0 / 1.5 s). `OnFishHit` applies it to `timeLeft`. The timer warning is now
reactive: the pulse starts at/below the threshold AND stops (colour + scale reset via
`StopTimerWarning`) if a bonus pushes time back above it — fixing the latent stuck-pulse
bug. Sessions 5, 6, 9 now reward hot streaks.

## BUG-09 — Dead code removed ✅ DONE
`FishIdentity.cs` and `FishHit.cs` (+ `.meta`) deleted after re-verifying their script
GUIDs are referenced by no scene, prefab, or script. Project compiles; no missing-script
warnings possible since nothing referenced them.

## Automated Tests
`Week4Tests.cs` adds 11 EditMode tests over `AudioPrefs`, `TimeBonus`, and the progressive
`FishPalette` helpers (ramp, clamp, subset exclusion). The Week 2 `RandomOther(exclude)`
test still passes via the backward-compatible overload.

## Regression Check
Changes touch `GameManager.cs` (palette pick, time bonus, reactive warning),
`FishSpawner.cs` (+1 param), `FishPalette.cs` (additive), `AudioManager.cs` (rewrite,
same public surface + mute), plus three new pure helpers and two deletions. No asmdef
changes. Spear throw, fish swim, AR placement untouched. No regressions across the 10
re-simulated sessions.
