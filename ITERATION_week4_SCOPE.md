# Iteration Week 4 — Scope

**Source:** TESTER_REPORT_week4.md  
**Owner:** product-owner  
**Theme:** Robustness + depth — fail-soft audio with a persisted mute, a colour-difficulty
ramp, a combo-scaled time bonus, and a verified dead-code sweep.

---

## Selected Tasks (4)

### TASK-01 · Audio robustness + persisted mute API
**Agent:** dev  
**Files:** `Scripts/AudioManager.cs`, `Scripts/AudioPrefs.cs` (new)  
**Rationale:** BUG-08 + UX-11. NRE on unassigned sources; no mute / persistence.  
**Acceptance:**
- All `AudioManager` play methods null-guard their `AudioSource`/`AudioClip` (no NRE under
  any configuration).
- New pure static `AudioPrefs` wraps `PlayerPrefs`: `IsMuted()`, `SetMuted(bool)`.
- `AudioManager` exposes `SetMuted(bool)` / `ToggleMute()`; applies mute to both sources and
  persists via `AudioPrefs`; restores the saved state on `Awake`.
- `AudioPrefs` is unit-testable.

### TASK-02 · Progressive colour difficulty
**Agent:** dev  
**Files:** `Scripts/FishPalette.cs`, `Scripts/GameManager.cs`, `Scripts/FishSpawner.cs`  
**Rationale:** UX-12. All four colours appear immediately; no ramp.  
**Acceptance:**
- `FishPalette.CountForProgress(int correctHitCount)` → number of active colours
  (3 until the player warms up, then 4), clamped to the palette size.
- `FishPalette.ActiveOptions(int count)` and `FishPalette.RandomOther(Color exclude,
  int activeCount)` operate on the active subset.
- `GameManager.PickNewTarget` picks the target from the active subset and passes the active
  count to the spawner; `FishSpawner` decoys use the active subset.
- Pure helpers unit-testable; correct fish always present, decoys never equal the target.

### TASK-03 · Combo-scaled time bonus + reactive timer warning
**Agent:** dev  
**Files:** `Scripts/TimeBonus.cs` (new), `Scripts/GameManager.cs`  
**Rationale:** UX-13. No reward for hot streaks; round ends flat at 60 s. Also fixes the
latent bug where the warning pulse never stops if time rises back above the threshold.  
**Acceptance:**
- New pure static `TimeBonus.ForHit(int comboStreak)` → seconds added on a correct hit,
  scaled by the combo multiplier (base 0.5 s × multiplier).
- `OnFishHit` adds the bonus to `timeLeft` on a correct hit.
- Timer warning becomes reactive in `Update`: pulse starts when `timeLeft <= threshold`
  and stops (colour/scale reset) when `timeLeft` rises back above it.
- `TimeBonus` unit-testable.

### TASK-04 · Remove verified-dead code
**Agent:** dev  
**Files:** delete `Scripts/FishIdentity.cs`, `Scripts/FishHit.cs` (+ `.meta`)  
**Rationale:** BUG-09. Both confirmed unreferenced by GUID grep across scenes/prefabs/scripts.  
**Acceptance:**
- Files removed; project still compiles; no scene/prefab references a removed script GUID
  (re-verified before deletion).

---

## Out of Scope This Week
- HUD mute button wiring / volume sliders — scene work; this week ships the runtime API only
- Per-channel (music vs SFX) volume — defer; mute-all first
- C# linter in CI / `UNITY_LICENSE` secret — unchanged external blockers (backlog)
