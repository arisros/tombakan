# Iteration Log

## Week 9 — 2026-07-27

**Branch:** `iteration/week-9`
**Tasks completed:** 4

| Task | Description | Files Changed |
|------|-------------|---------------|
| TASK-01 | P0 + defensive fixes: (a) null-guard `m_OnboardingGoals` in `GoalManager.CompleteGoal()` — returning-player NRE eliminated; (b) `ShowSad(actualDeduction)` shows `"Meleset!"` when clamp absorbs penalty; (c) `CancelInvoke(nameof(HideFeedback))` at top of `ShowHappy()`/`ShowSad()` prevents stale hide-calls; (d) `targetColorLabel.text` updated after `SpawnFish()` resolves `CurrentTargetSpecies` | `GoalManager.cs`, `GameManager.cs` |
| TASK-02 | Level-reward propagation + ColourBlind hue-range fix: (a) snapshot level before/after `AchievementChecker.CheckAll()` in `EndGame` — achievement-triggered level-ups now call `ApplyLevelReward`; (b) `ApplyLevelReward` made public; `TombakanOnboarding` calls it when daily-bonus triggers level-up; (c) `ShapeForColor` replaced exact-hex switch with HSV hue-range classifier — no code path returns `"?"` | `GameManager.cs`, `TombakanOnboarding.cs`, `ColourBlindSettings.cs` |
| TASK-03 | Spawn radius + throw-tutorial panel (scene): serialised `spawnRadius` lowered from 1.5 to 0.8 on `FishSpawner` in `GamePlay.unity`; `ThrowHintPanel` GameObject added under `GamePlayUI` (inactive by default, bottom-centre anchored, Indonesian throw instruction, wired to `GameManager.throwHintPanel`) | `Assets/Scenes/GamePlay.unity` |
| TASK-04 | FishSpecies colour audit: verified all 8 starter species produce a valid non-`"?"` symbol via new HSV classifier; updated `baseColor` of any species whose hue bucket was over-represented to redistribute across four buckets (s ≥ 0.6, v ≥ 0.5 maintained); 4-symbol limit means 2 species per bucket — same-bucket discrimination deferred to Week 10 `FishCatalog.PickOther` fix | `Assets/ScriptableObjects/FishSpecies/*.asset` |

**Note:** Week 7 and Week 8 were tester-only passes (no code committed); their findings were incorporated into the Week 9 scope.

**Artefacts:** `TESTER_REPORT_week9.md`, `ITERATION_week9_SCOPE.md`,
`Assets/Tests/EditMode/Week9Task01ScoreLogicTests.cs`,
`Assets/Tests/EditMode/Week9Task02AchievementLevelUpTests.cs`,
`Assets/Tests/EditMode/Week9Task02ColourBlindTests.cs`,
`Assets/Tests/EditMode/Week9Task04SpeciesAuditTests.cs`,
`Assets/Tests/PlayMode/Week9Task01PlayModeTests.cs`,
`Assets/Tests/PlayMode/Week9Task03SceneTests.cs`

---

## Week 6 — 2026-06-03

**Branch:** `iteration/week-6`
**Tasks completed:** 3

| Task | Description | Files Changed |
|------|-------------|---------------|
| TASK-01 | Four small bugs: `OnFishHit` `!gameRunning` guard; daily-bonus level-up surfaced in `TombakanOnboarding`; `HapticFeedback` decoupled from audio mute | `GameManager.cs`, `TombakanOnboarding.cs`, `HapticFeedback.cs` |
| TASK-02 | Achievement toast (null-safe) + stagger result celebrations (0.4 s badge, 0.8 s level-up) | `GameManager.cs` |
| TASK-03 | Add `AchievementToastPanel` + `AchievementToastText` to `GamePlayUI` in scene; wire GameManager fields | `Assets/Scenes/GamePlay.unity` |

**Artefacts:** `TESTER_REPORT_week6.md`, `ITERATION_week6_SCOPE.md`

---

## Week 5 — 2026-06-02

**Branch:** `iteration/week-5`
**Tasks completed:** 4

| Task | Description | Files Changed |
|------|-------------|---------------|
| TASK-01 | Spawn-pipeline bugs: `PickNewTarget` post-game guard + duplicate `FishSwim` fix + stale species label reorder | `GameManager.cs`, `FishSpawner.cs` |
| TASK-02 | Defensive guards: `PlaceWaterOnPlane` one-shot placement + `CollectSummary` shows all-caught species for returning players | `PlaceWaterOnPlane.cs`, `GameManager.cs` |
| TASK-03 | Result-screen polish: `Accuracy.Format(0,0)` → `"--"`; suppress `"+0 XP"` label | `Accuracy.cs`, `GameManager.cs` |
| TASK-04 | Remove dead Zen-mode TODO comment | `GameManager.cs` |

**Artefacts:** `TESTER_REPORT_week5.md`, `ITERATION_week5_SCOPE.md`,
`Assets/.../Tests/Week5Tests.cs`, Week3Tests.cs updated (Accuracy contract change)

---

## Phase 1 + 2 + 2.5 — 2026-06-02

**Branch:** `master` (direct)
**Tasks completed:** 17 new files, 6 modified

### Summary
Implemented all three roadmap systems in one pass — Fish Species + Fishipedia (Phase 1),
Spear Cosmetics + Shop (Phase 2), Leveling & Progression (Phase 2.5) — plus CI fix.

| Task | Description | Files |
|------|-------------|-------|
| P0   | Fix CI `main`→`master` branch mismatch | `.github/workflows/ci.yml` |
| P1-A | `FishSpecies` ScriptableObject + `FishRarity` enum | `FishSpecies.cs` (new) |
| P1-B | `FishCatalog` ScriptableObject with weighted random + PickOther | `FishCatalog.cs` (new) |
| P1-C | `FishdexStore` persistent species unlock | `FishdexStore.cs` (new) |
| P1-D | Species-aware `FishTarget`, `FishHitBox`, `SpearHit` | 3 files modified |
| P1-E | Species-driven `FishSpawner` (catalog optional; null = colour-only) | `FishSpawner.cs` modified |
| P2-A | `SpearSkin` ScriptableObject + `SpearCurrency` enum | `SpearSkin.cs` (new) |
| P2-B | `SpearShopCatalog` ScriptableObject | `SpearShopCatalog.cs` (new) |
| P2-C | `CurrencyStore` (soft coins) with pure earn rule | `CurrencyStore.cs` (new) |
| P2-D | `SpearStore` ownership/equip with idempotent buy | `SpearStore.cs` (new) |
| P25-A| `ProgressionRules` pure XP/level curve (testable) | `ProgressionRules.cs` (new) |
| P25-B| `ProgressionStore` persistent XP+level | `ProgressionStore.cs` (new) |
| P25-C| `LevelReward` + `LevelRewardTable` ScriptableObject | 2 files (new) |
| P25-D| `GameManager` — species tracking, XP award, level-up, coins, HUD | `GameManager.cs` modified |
| T-1  | `Phase1Tests` — FishdexStore pure logic, catalog weights (12 tests) | `Tests/Phase1Tests.cs` (new) |
| T-2  | `Phase2Tests` — SpearStore + CurrencyStore pure logic (11 tests) | `Tests/Phase2Tests.cs` (new) |
| T-25 | `Phase25Tests` — ProgressionRules XP/level curve (22 tests) | `Tests/Phase25Tests.cs` (new) |



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
