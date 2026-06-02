# Tester Report — Week 2

**Date:** 2026-06-02  
**Tester:** game-tester (simulated, 10 sessions)  
**Game:** Tombakan v0.2 — Mobile AR spear-fishing (Indonesian colour-matching)  
**Baseline:** Week 1 changes merged (score floor, combos, dynamic fish count, timer text, tiers, CI)

---

## Session Summary

| Session | Outcome | Score | Correct Hits | Notable Event |
|---------|---------|-------|--------------|---------------|
| 1 | Completed | 900 | 8 | Fish kept swimming behind the result screen after time ran out — looked broken |
| 2 | Completed | 1500 | 12 | Combo 3× felt great; but only ever saw red/green/blue — variety stale by 40 s |
| 3 | Completed | 400 | 5 | Result list "Merah, Merah, Merah, Biru, Merah" — ugly, no aggregation |
| 4 | Completed | 0 | 0 | Beat previous run but game didn't remember/celebrate any best score |
| 5 | Completed | 1100 | 10 | After EndGame, leftover fish overlapped the score panel |
| 6 | Completed | 700 | 7 | Wanted a "new record!" moment — none exists |
| 7 | Completed | 350 | 4 | Only 3 colours means a blind guess is 33% right — too easy to fluke |
| 8 | Completed | 1300 | 11 | Result colour text wrapped past the panel edge with 11 entries |
| 9 | Completed | 600 | 6 | Replays felt identical; no persistence between runs |
| 10 | Completed | 800 | 8 | Fish behind result panel still reacted to nothing — confusing |

---

## Bugs Found

### BUG-04 — Fish not cleared on game end (High severity)
**File:** `GameManager.cs` (`EndGame`), `FishSpawner.cs`  
When the timer reaches 0, `EndGame()` shows the result panel but never clears the active
fish. The last spawned shoal keeps swimming (via `FishSwim.Update`) behind/through the
result UI. Observed in sessions 1, 5, 10. `FishSpawner` has only a private `ClearFish()`
called at the start of the next spawn — there is no end-of-game cleanup path.

### BUG-05 — Duplicated colour palette can silently drift (Medium severity)
**Files:** `GameManager.cs:59` (`fishColorOptions = {green,red,blue}`),
`FishSpawner.cs` (`RandomOtherColor` hardcodes `{red,green,blue}`)  
The set of fish colours is defined independently in two places. If a designer adds a colour
to one list but not the other, decoys and targets desynchronise (e.g. a target colour that
decoys can never avoid, or vice-versa). This is a latent correctness bug, not just style.

### BUG-06 — `WaterRipple` throws if Renderer missing (Low severity)
**File:** `WaterRipple.cs:20`  
`rend` is fetched in `Start` with no null check; `Update` calls `rend.material...` every
frame. A misconfigured prefab spams `NullReferenceException`.

---

## UX / Feel Issues

### UX-05 — Only three colours; matching is too easy
`fishColorOptions` is red/green/blue. A pure guess is correct 1-in-3. The colour
vocabulary in `Dict.cs` already supports 20 names. Adding at least Yellow ("Kuning")
raises challenge and teaches more vocabulary — the game's actual educational goal.

### UX-06 — No high-score persistence / "new record" moment
Nothing is remembered between runs (sessions 4, 6, 9). Players have no long-term goal.
A persisted best score (PlayerPrefs) shown on the main and result screens, plus a "Rekor
Baru!" celebration, would close the loop.

### UX-07 — Result colour list is an unaggregated, overflowing dump
**File:** `GameManager.cs` (`EndGame`)  
`string.Join(", ", mappedColors)` lists every caught fish individually
("Merah, Merah, Merah, Biru"). With 10+ catches it overflows the panel (sessions 3, 8).
Aggregating to counts ("Merah ×3, Biru ×1") is cleaner and overflow-safe.

---

## Code Quality Notes

- `FishIdentity.cs` (`isCorrectFish`) is never read anywhere — dead component.
- `FishHit.cs` remains orphaned (all hit logic lives in `FishHitBox`).
- `RandomOtherColor`'s private palette is the root of BUG-05; centralising the palette
  removes the duplication.

---

## Recommended Priorities

1. **Clear fish on EndGame** (BUG-04) — visible, high-impact bug
2. **Centralise + expand colour palette** (BUG-05 + UX-05) — fixes a latent bug and adds variety
3. **High-score persistence + record moment** (UX-06) — long-term engagement
4. **Aggregate result colour summary** (UX-07) — readability + overflow safety
