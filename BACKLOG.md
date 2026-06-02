# Backlog

## Completed

### Phase 1 — Fish Species + Fishipedia (2026-06-02)
- [x] `FishSpecies` + `FishRarity` ScriptableObject
- [x] `FishCatalog` with weighted-random pick + PickOther
- [x] `FishdexStore` persistent species unlock (idempotent, tested)
- [x] `FishTarget` + `FishHitBox` + `SpearHit` carry speciesId through hit chain
- [x] `FishSpawner` species-catalog-aware (null catalog = colour-only fallback)
- [x] `FishipediaUI` collection screen (lock/unlock display, count label)
- [x] `GameManager` tracks new species per game, unlocks Fishipedia on catch

### Phase 2 — Spear Cosmetics + Shop (2026-06-02)
- [x] `SpearSkin` ScriptableObject + `SpearCurrency` enum
- [x] `SpearShopCatalog` ScriptableObject
- [x] `CurrencyStore` (soft coins) with pure earn rule (tested)
- [x] `SpearStore` buy/equip/own with idempotent purchase guard (tested)
- [x] `SpearShopUI` shop screen (buy, equip, coin display)
- [x] `SpearThrower` reads equipped skin prefab/material

### Phase 2.5 — Leveling & Progression (2026-06-02)
- [x] `ProgressionRules` pure XP/level curve (monotonic, inverse-consistent, tested)
- [x] `ProgressionStore` persistent XP + level (AddXp returns new level on level-up)
- [x] `LevelReward` + `LevelRewardTable` ScriptableObject
- [x] `GameManager.EndGame` awards XP + coins, shows level-up celebration, applies rewards
- [x] `ProgressionHUD` level badge + XP bar fill

### Phase 3 — Juice (2026-06-02)
- [x] `ScreenShake` — damped camera shake on correct/wrong hit
- [x] `HapticFeedback` — mobile vibration on hits (respects mute)
- [x] `PerformanceSettings` — low-end mode (30 fps cap, shadow disable)
- [x] `AudioManager` restored with null-guards + mute + `AudioPrefs` integration

### Phase 4 — Modes & Retention (2026-06-02)
- [x] `DailyChallenge` — daily bonus XP (100 + 25/streak day, capped at 7 days)
- [x] `TombakanOnboarding` — skip AR coaching for returning players

### Phase 5 — Onboarding & Accessibility (2026-06-02)
- [x] `ColourBlindSettings` — persistent toggle with colour→shape mapping (●▲■★)
- [x] `FishShapeOverlay` — shape symbol on fish when mode active
- [x] `ColourBlindToggleUI` — runtime toggle button
- [x] `MuteButtonUI` — mute button tied to `AudioManager.IsMuted`/`ToggleMute`

### Editor Tooling (2026-06-02)
- [x] `TombakanSetupWizard` — Tools > Tombakan > Create Starter Data
  Creates FishCatalog (8 species), SpearShopCatalog (3 skins), LevelRewardTable as .asset files

### CI Fix (2026-06-02)
- [x] Fix CI trigger branch `main` → `master + main` so all PRs against master run CI

### Week 5 (2026-06-02)
- [x] `PickNewTarget` guard — no fish spawn after EndGame
- [x] `FishSpawner` duplicate `FishSwim` fix — no more erratic fish movement
- [x] Stale species label — `targetSpeciesLabel` updated after `SpawnFish`
- [x] `PlaceWaterOnPlane` one-shot — mid-game re-placement disabled
- [x] `CollectSummary` fixed for returning players — shows all-caught species/colours
- [x] `Accuracy.Format(0,0)` returns `"--"` instead of `"0/0 (0%)"`
- [x] Suppress `"+0 XP"` label when no XP earned

### Week 4 (2026-06-02)
- [x] Audio robustness (null-guards) + persisted mute API
- [x] Progressive colour difficulty (3 → 4 colours)
- [x] Combo-scaled time bonus + reactive timer warning
- [x] Removed verified-dead FishIdentity.cs / FishHit.cs

### Week 3
- [x] Target colour name label (accessibility + vocabulary)
- [x] Adaptive inter-round delay (pacing scales with skill)
- [x] Accuracy stat on result screen
- [x] WaterRipple null-guard (NRE fix)

### Week 2
- [x] Centralise + expand fish-colour palette (FishPalette, +Kuning) — fixes palette drift bug
- [x] Clear fish on game end (no shoal behind result screen)
- [x] High-score persistence (PlayerPrefs) + new-record badge
- [x] Aggregated, overflow-safe result colour summary

### Week 1
- [x] Score floor — prevent negative score display
- [x] Combo streak multiplier (2× / 3×)
- [x] Dynamic fish count difficulty ramp (3→7)
- [x] Numeric timer countdown text field
- [x] Raised tier thresholds + TierLegend
- [x] CI/CD — GitHub Actions: EditMode test runner + Android build (game-ci)
- [x] Assembly definitions (`Tombakan.Runtime` + `Tombakan.Tests`) so tests are discoverable

---

## Open — Needs Unity Editor or Art Assets

### Blocked on Unity Editor Access
- [ ] **Wire all new scripts in Unity scene(s)** — run `Tools > Tombakan > Create Starter Data` then assign FishCatalog to FishSpawner, SpearShopCatalog to SpearThrower, LevelRewardTable to GameManager
- [ ] Create Fishipedia screen panel UI (ScrollView + entry prefab) and wire FishipediaUI component
- [ ] Create SpearShop panel UI and wire SpearShopUI component
- [ ] Add level badge + XP bar UI elements to HUD; wire ProgressionHUD
- [ ] Add level-up celebration panel; wire GameManager.levelUpPanel
- [ ] Add mute button to main menu; wire MuteButtonUI
- [ ] Add colour-blind toggle to settings; wire ColourBlindToggleUI
- [ ] Add ScreenShake to AR Camera (or parent) in scene
- [ ] Add TombakanOnboarding to scene; wire GoalManager reference
- [ ] Wire dailyBonusPanel on main menu (TombakanOnboarding)

### Blocked on Art / Content
- [ ] Real low-poly species models — assign to `FishSpecies.modelPrefab` when available
- [ ] Spear skin materials/prefabs — assign to `SpearSkin.prefab`/`material` when available
- [ ] Species icons (Sprite) — assign to `FishSpecies.icon` for Fishipedia cards
- [ ] Spear skin preview icons — assign to `SpearSkin.previewIcon`

### Continuous
- [ ] **Add `UNITY_LICENSE` repo secret** so CI test/build jobs actually run
- [ ] C# lint in CI (`dotnet format`) once `.csproj` is generated by Unity
- [ ] Expand colour vocabulary beyond 4 (Dict.cs has 20; palette ready to grow)
- [ ] Hit animation on fish — "caught" squish/flop in Animator
- [ ] Per-channel volume sliders (music vs SFX)

## Week 6 Candidates (from Week 5 tester report)
- [ ] Achievement unlock in-game notification toast/banner (UX-2) — requires new UI panel + C# show/hide
- [ ] Throw-mechanic tutorial hint for first-time players (UX-1) — `TombakanOnboarding` integration
- [ ] Stagger level-up panel + new-record badge on result screen (POLISH-1) — 0.3–0.5 s delay
- [ ] "Re-position water" button — complement the one-shot placement guard (TASK-02a partial)
- [ ] ScreenShake wrong-hit magnitude — reduce lurch on low-end devices (POLISH-3)
