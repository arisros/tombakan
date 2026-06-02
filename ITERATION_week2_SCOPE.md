# Iteration Week 2 — Scope

**Source:** TESTER_REPORT_week2.md  
**Owner:** product-owner  
**Theme:** Polish the loop — fix the visible end-game bug, deepen the colour challenge,
add persistence, and clean up the result screen.

---

## Selected Tasks (4)

### TASK-01 · Centralise + expand the fish-colour palette
**Agent:** dev  
**Files:** `Scripts/FishPalette.cs` (new), `Scripts/GameManager.cs`, `Scripts/FishSpawner.cs`  
**Rationale:** BUG-05 + UX-05. One source of truth removes the drift bug between
`GameManager.fishColorOptions` and `FishSpawner.RandomOtherColor`; adding Yellow ("Kuning",
already in `Dict.cs`) raises the guess difficulty from 1-in-3 to 1-in-4 and teaches more
vocabulary.  
**Acceptance:**
- New static `FishPalette` exposes `Options` (Red, Green, Blue, Yellow) and
  `RandomOther(Color exclude)`.
- `GameManager.PickNewTarget` picks from `FishPalette.Options`.
- `FishSpawner` decoys use `FishPalette.RandomOther(target)`; its private
  `RandomOtherColor` is removed.
- All palette colours have a `Dict.cs` Indonesian mapping (regression-tested).

### TASK-02 · Clear fish when the game ends
**Agent:** dev  
**Files:** `Scripts/FishSpawner.cs`, `Scripts/GameManager.cs`  
**Rationale:** BUG-04. Fish currently keep swimming behind the result panel.  
**Acceptance:**
- `FishSpawner` gains a `public void ClearAll()` that destroys all active fish.
- `GameManager.EndGame()` calls `fishSpawner.ClearAll()`.
- No `FishSwim` instances remain active after EndGame.

### TASK-03 · High-score persistence + "new record" moment
**Agent:** dev  
**Files:** `Scripts/GameManager.cs`, `Scripts/ScoreStore.cs` (new)  
**Rationale:** UX-06. No long-term goal across runs.  
**Acceptance:**
- New static `ScoreStore` wraps `PlayerPrefs`: `GetBest()`, `TrySetBest(int) -> bool`
  (returns true when a new record is set).
- `EndGame` calls `ScoreStore.TrySetBest(score)`; if a record, shows `newRecordBadge`
  (optional, null-safe `GameObject`).
- Best score rendered into optional, null-safe `TMP_Text bestScoreText` (main screen) and
  `resultBestScoreText` (result screen).
- `TrySetBest` logic is pure/static and unit-testable (record only when strictly greater).

### TASK-04 · Aggregated result colour summary
**Agent:** dev  
**Files:** `Scripts/GameManager.cs`, `Scripts/ColorSummary.cs` (new)  
**Rationale:** UX-07. Flat per-fish list overflows the panel.  
**Acceptance:**
- New static `ColorSummary.Format(IList<string> hexColors)` returns an aggregated,
  count-suffixed, overflow-safe string (e.g. "Merah ×3, Biru ×1"), preserving first-seen
  order and localising via `ColorHexLocalization`.
- Empty input returns "Tidak ada ikan dikumpulkan".
- `EndGame` uses `ColorSummary.Format(...)` instead of `string.Join`.
- Pure/static and unit-testable.

---

## Out of Scope This Week
- BUG-06 `WaterRipple` null-guard — trivial, deferred to a cleanup batch
- Removing dead `FishIdentity.cs` / `FishHit.cs` — needs prefab/scene audit (scene work)
- Adding a C# linter to CI — still blocked on Unity-generated `.csproj` (backlog)
