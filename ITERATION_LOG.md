# Iteration Log

## Week 2 — 2026-06-02

**Branch:** `iteration/week-2`  
**Tasks completed:** 4

| Task | Description | Files Changed |
|------|-------------|---------------|
| TASK-01 | Centralised + expanded fish-colour palette (add Kuning); fixes GameManager/FishSpawner drift bug | `FishPalette.cs` (new), `GameManager.cs`, `FishSpawner.cs` |
| TASK-02 | Clear active fish on game end (no shoal behind result screen) | `FishSpawner.cs`, `GameManager.cs` |
| TASK-03 | High-score persistence (PlayerPrefs) + "new record" badge | `ScoreStore.cs` (new), `GameManager.cs` |
| TASK-04 | Aggregated, overflow-safe result colour summary ("Merah ×3") | `ColorSummary.cs` (new), `GameManager.cs` |

**Notable catch:** `Color.yellow` serialises to `FFEB04`, not `FFFF00` — used a pure
`new Color(1,1,0)` so the "Kuning" localisation resolves; locked in by a regression test.

**Artefacts:** `TESTER_REPORT_week2.md`, `ITERATION_week2_SCOPE.md`,
`TESTER_RECHECK_week2.md`, `Assets/.../Tests/Week2Tests.cs`

---

## Week 1 — 2026-06-02

**Branch:** `iteration/week-1`  
**Tasks completed:** 5

| Task | Description | Files Changed |
|------|-------------|---------------|
| TASK-01 | Score floor (≥0) + remove dead `UpdateTimerUI` + `collectedFishColors` → `List<string>` | `GameManager.cs` |
| TASK-02 | Combo streak multiplier (2× at 3, 3× at 5+) | `GameManager.cs` |
| TASK-03 | Dynamic fish count scaled with `correctHitCount` (3→7) | `GameManager.cs`, `FishSpawner.cs` |
| TASK-04 | Numeric timer countdown `TMP_Text` field (null-safe) | `GameManager.cs` |
| TASK-05 | Raised tier thresholds + optional `TierLegend` image field | `GameManager.cs` |

**Artefacts:**  
- `TESTER_REPORT_week1.md`  
- `ITERATION_week1_SCOPE.md`  
- `TESTER_RECHECK_week1.md`  
- `Assets/MobileARTemplateAssets/Scripts/Tests/GameManagerTests.cs`
