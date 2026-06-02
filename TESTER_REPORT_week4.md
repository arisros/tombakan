# Tester Report — Week 4

**Date:** 2026-06-02  
**Tester:** game-tester (simulated, 10 sessions)  
**Game:** Tombakan v0.4 — Mobile AR spear-fishing (Indonesian colour-matching)  
**Baseline:** Weeks 1–3 merged (scoring, combos, dynamic count, timers, tiers, CI,
palette+Kuning, clear-on-end, best score, colour summary, colour-word label, adaptive
pacing, accuracy stat, WaterRipple guard)

---

## Session Summary

| Session | Outcome | Score | Correct | Accuracy | Notable Event |
|---------|---------|-------|---------|----------|---------------|
| 1 | Completed | 1500 | 12 | 80% | Wanted to mute the game on the bus — no mute control anywhere |
| 2 | Completed | 900 | 8 | 73% | Mute preference wouldn't matter — there's nowhere to set it / no persistence |
| 3 | Completed | 1700 | 14 | 88% | All 4 colours from second 1 — wished difficulty *built up* instead |
| 4 | Completed | 400 | 4 | 50% | Beginner overwhelmed by 4 colours immediately; a gentler start would help |
| 5 | Completed | 1300 | 11 | 85% | Strong run ended at a hard 60 s — no reward for a hot streak |
| 6 | Completed | 1100 | 9 | 75% | Felt the timer should "breathe" when on fire; flat 60 s caps good play |
| 7 | Error logged | 600 | 6 | 67% | `NullReferenceException` at start — an AudioSource was unassigned in a test rig |
| 8 | Completed | 800 | 7 | 70% | BGM kept blaring; no way to quiet SFX vs music |
| 9 | Completed | 1000 | 9 | 82% | "New record!" felt good but the round still ended abruptly at 60 s |
| 10 | Completed | 350 | 3 | 43% | Two unused components on fish (noticed via inspector) — code smell |

---

## Bugs Found

### BUG-08 — `AudioManager` NRE when a source/clip is unassigned (Medium severity)
**File:** `AudioManager.cs`  
`PlayBGM` dereferences `bgmSource.clip` and `PlayOneShot` calls into `sfxSource` with no
null checks. A scene/prefab missing an `AudioSource` (session 7) throws
`NullReferenceException` at startup. The manager should fail soft.

### BUG-09 — Dead components shipped on fish / in project (Low severity)
**Files:** `FishIdentity.cs`, `FishHit.cs`  
Both classes are unreferenced — no scene, prefab, or script uses their script GUIDs
(verified by GUID grep). `FishHit.OnHit` is superseded by `FishHitBox`; `FishIdentity`'s
`isCorrectFish` is never read. Dead code (session 10 inspector smell).

---

## UX / Feel Issues

### UX-11 — No audio mute / persistence
There is no way to mute the game, and no preference is remembered between launches
(sessions 1, 2, 8). A persisted mute toggle (API + storage) is needed; the HUD button that
calls it is scene work, but the runtime contract should exist now.

### UX-12 — Colour difficulty does not ramp
All four palette colours appear from the first round (sessions 3, 4). Beginners are
overwhelmed and experts get no build-up. Introducing colours progressively (start with 3,
add the 4th once the player is warmed up) gives a difficulty curve, mirroring the existing
fish-count ramp.

### UX-13 — No reward for a hot streak; round ends abruptly
The 60 s limit is hard regardless of performance (sessions 5, 6, 9). Awarding a small time
bonus on each correct hit — scaled by combo — rewards skilled play with a little extra time
and makes the timer "breathe". (Note: this exposes a latent issue — the timer warning pulse
never turns off if time climbs back above the threshold; that must be handled.)

---

## Code Quality Notes

- `AudioManager` is a good place for a tiny persisted-preference helper (`AudioPrefs`),
  keeping the mute decision pure/testable.
- The colour-count ramp can reuse the `FishPalette` single-source pattern from Week 2.
- Time-bonus logic should be a pure rule so the combo scaling is testable.

---

## Recommended Priorities

1. **Audio robustness + persisted mute** (BUG-08 + UX-11)
2. **Progressive colour difficulty** (UX-12)
3. **Combo-scaled time bonus + reactive timer warning** (UX-13)
4. **Remove verified-dead `FishIdentity` / `FishHit`** (BUG-09)
