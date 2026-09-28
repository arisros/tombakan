# Tester Report — Week 3

**Date:** 2026-06-02  
**Tester:** game-tester (simulated, 10 sessions)  
**Game:** Tombakan v0.3 — Mobile AR spear-fishing (Indonesian colour-matching)  
**Baseline:** Week 1 + Week 2 merged (score floor, combos, dynamic count, timers, tiers,
CI, FishPalette+Kuning, clear-on-end, best score, colour summary)

---

## Session Summary

| Session | Outcome | Score | Correct | Wrong | Notable Event |
|---------|---------|-------|---------|-------|---------------|
| 1 | Completed | 1500 | 12 | 3 | Late game felt sluggish — 2.2 s lock after every hit dragged the pace |
| 2 | Completed | 800 | 7 | 6 | Result screen shows colours caught but no sense of *how accurate* I was |
| 3 | Completed | 400 | 4 | 9 | Lots of misses; no accuracy feedback to learn from |
| 4 | Completed | 1700 | 14 | 2 | By hit 14 the fixed delay between rounds felt punishing for a skilled player |
| 5 | Completed | 600 | 6 | 4 | Couldn't tell target colour apart from a decoy in glare — needed the *word* |
| 6 | Completed | 900 | 8 | 5 | Kid tester named the wrong colour — no text label to reinforce vocabulary |
| 7 | Completed | 1100 | 9 | 3 | Wanted "you hit 9/12" type stat at the end |
| 8 | Completed | 300 | 3 | 8 | Colour-blind tester struggled to distinguish red vs green targets |
| 9 | Completed | 1300 | 11 | 2 | Pacing complaint again at high streak counts |
| 10 | Completed | 700 | 7 | 7 | NullReferenceException spam in log from a water plane with no Renderer |

---

## Bugs Found

### BUG-07 — `WaterRipple` NRE when Renderer missing (Low–Med severity)
**File:** `WaterRipple.cs`  
`rend` is fetched in `Start` with no null check and dereferenced every frame in `Update`.
A water-plane prefab without a `Renderer` (or before material assignment) spams
`NullReferenceException` (session 10). Carried over from Week 2 (BUG-06, deferred).

---

## UX / Feel Issues

### UX-08 — Fixed inter-round delay drags late game
**File:** `GameManager.cs` (`OnFishHit` uses constant `hitDelay = 2.2f`)  
The lock + retarget delay is constant regardless of skill. Skilled players at high streaks
(sessions 1, 4, 9) found the 2.2 s wait between rounds punishing. The delay should shrink
as the player improves, down to a sensible floor.

### UX-09 — No accuracy feedback
There is no record of *misses* (`OnFishHit` increments `correctHitCount` but discards wrong
hits after applying the penalty). Players (sessions 2, 3, 7) wanted an end-of-game accuracy
read ("9 of 12 = 75%") to gauge improvement — core to the educational loop.

### UX-10 — Target shown only as a colour swatch (vocabulary + accessibility gap)
**File:** `GameManager.cs` (`PickNewTarget` sets only `targetColorImage.color`)  
The target is communicated purely by colour. In glare (session 5), for colour-blind players
(session 8), and for the vocabulary goal (session 6), there is no Indonesian colour *word*.
`ColorHexLocalization` already provides the name — surfacing it as a label is low-effort,
high-value, and reinforces the game's actual teaching purpose.

---

## Code Quality Notes

- `wrongHitCount` is implicitly thrown away in `OnFishHit`; capturing it unlocks the
  accuracy stat (UX-09) at near-zero cost.
- `hitDelay` is a good candidate to become the *base/maximum* delay feeding a pure
  progress function (testable).
- Dead code still present (`FishHit.cs`, `FishIdentity.cs`) — defer; needs scene audit.

---

## Recommended Priorities

1. **Target colour name label** (UX-10) — accessibility + vocabulary, very low effort
2. **Adaptive inter-round delay** (UX-08) — fixes the most repeated pacing complaint
3. **Accuracy stat on result screen** (UX-09) — closes the educational feedback loop
4. **`WaterRipple` null-guard** (BUG-07) — kill the NRE spam
