# Iteration Log

## Week 4 — 2026-06-02

**Branch:** `iteration/week-4`  
**Tasks completed:** 4

| Task | Description | Files Changed |
|------|-------------|---------------|
| TASK-01 | Audio robustness (null-guards) + persisted mute API | `AudioManager.cs`, `AudioPrefs.cs` (new) |
| TASK-02 | Progressive colour difficulty (3 → 4 colours with progress) | `FishPalette.cs`, `GameManager.cs`, `FishSpawner.cs` |
| TASK-03 | Combo-scaled time bonus + reactive timer warning (fixes stuck-pulse) | `TimeBonus.cs` (new), `GameManager.cs` |
| TASK-04 | Removed verified-dead `FishIdentity.cs` + `FishHit.cs` | (deletions) |

**Note:** Dead-code removal was GUID-verified against all scenes/prefabs/scripts before
deletion. Mute HUD button left as scene work; runtime API shipped.

**Artefacts:** `TESTER_REPORT_week4.md`, `ITERATION_week4_SCOPE.md`,
`TESTER_RECHECK_week4.md`, `Assets/.../Tests/Week4Tests.cs`

---

## Week 3 — 2026-06-02

**Branch:** `iteration/week-3`  
**Tasks completed:** 4

| Task | Description | Files Changed |
|------|-------------|---------------|
| TASK-01 | Target colour name label (accessibility + vocabulary) | `GameManager.cs` |
| TASK-02 | Adaptive inter-round delay (shrinks with skill, 1.0 s floor) | `PacingRules.cs` (new), `GameManager.cs` |
| TASK-03 | Accuracy stat on result screen ("9/12 (75%)") | `Accuracy.cs` (new), `GameManager.cs` |
| TASK-04 | `WaterRipple` null-guard — kills NRE spam | `WaterRipple.cs` |

**Artefacts:** `TESTER_REPORT_week3.md`, `ITERATION_week3_SCOPE.md`,
`TESTER_RECHECK_week3.md`, `Assets/.../Tests/Week3Tests.cs`

---

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
