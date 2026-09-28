# Tester Report — Week 1

**Date:** 2026-06-02  
**Tester:** game-tester (simulated, 10 sessions)  
**Game:** Tombakan v0.1 — Mobile AR spear-fishing (Indonesian colour-matching)

---

## Session Summary

| Session | Outcome | Score | Correct Hits | Notable Event |
|---------|---------|-------|--------------|---------------|
| 1 | Completed | 375 | 5 | Wrong-hit spam drove score to −25 before recovery |
| 2 | Completed | 0 | 0 | Player hit only wrong fish; final display showed 0 after clamp would be needed — showed negative |
| 3 | Completed | 800 | 8 | Timer bar warning fired but player had no idea how many seconds remained |
| 4 | Completed | 1100 | 11 | Reached TierHigh with only 5 correct (threshold too low) — achieved at ~15 s |
| 5 | Completed | −75 | 0 | Score went deeply negative; discouraging for young learners |
| 6 | Completed | 200 | 3 | Player wanted to go faster; hit delay of 3 s felt sluggish late-game |
| 7 | Completed | 500 | 6 | No reward for 4-in-a-row streak; engagement dropped mid-session |
| 8 | Completed | 300 | 4 | Fish count always 5 felt repetitive; no sense of increasing challenge |
| 9 | Completed | 600 | 7 | Colour display shown on screen but no label for colour-blind players |
| 10 | Completed | 450 | 5 | Result screen fish list truncated when long (array[1000] overkill noted) |

---

## Bugs Found

### BUG-01 — Score goes negative (High severity)
**File:** `GameManager.cs:209`  
`score -= penaltyPerWrongHit` has no floor. A new player who misses repeatedly sees a
negative score on the HUD, which is confusing and demoralising for the target audience
(children learning Indonesian colour vocabulary).

### BUG-02 — Dead `UpdateTimerUI()` uses wrong variable (Medium severity)
**File:** `GameManager.cs:177`  
Method reads `timer` (never assigned) instead of `timeLeft`. The method is never called,
making it dead code that references a ghost variable.

### BUG-03 — `collectedFishColors` is a fixed `string[1000]` (Low severity)
**File:** `GameManager.cs:65`  
In a 60-second game (~20 rounds max), allocating 1000 slots is wasteful. Index arithmetic
using `correctHitCount - 1` also introduces an off-by-one risk if `correctHitCount`
ever exceeds the array length.

---

## UX / Feel Issues

### UX-01 — No numeric timer countdown
The timer bar pulses red at 10 s but shows no number. Players cannot plan their last
throws without guessing how much time remains.

### UX-02 — No combo/streak reward
Consecutive correct hits give no extra points. Sessions 7 and 8 showed a flat reward
curve; after the first 3 correct hits players lose motivation to aim carefully.

### UX-03 — Constant fish count, no difficulty ramp
`FishSpawner.fishCount` is always 5 (1 correct, 4 decoys). The challenge never
increases. Experienced players find it trivial after the first 20 seconds.

### UX-04 — TierHigh threshold too low
`correctHitCount > 4` unlocks the highest tier. Achievable before the 30-second mark.
The result screen loses its sense of achievement.

---

## Code Quality Notes

- `FishHit.cs` — `OnHit(Transform spear)` is defined but never called by `SpearHit.cs`.
  `FishHitBox.cs` handles all hit logic. `FishHit` is orphaned dead code.
- `GameManager.fishColorOptions` is only 3 colours (red/green/blue) even though
  `Dict.cs` maps 20 Indonesian colour names. Opportunity to expand colour vocabulary.

---

## Recommended Priorities

1. **Fix score floor** (BUG-01) — critical for player experience
2. **Add numeric timer text** (UX-01) — low effort, high clarity gain
3. **Combo streak multiplier** (UX-02) — adds engagement depth
4. **Dynamic fish count** (UX-03) — increases replayability
5. **Fix dead code / array** (BUG-02, BUG-03) — code health
