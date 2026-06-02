# Tombakan — Product Roadmap

> **Tombakan** = *menombak ikan* (spearfishing). The game teaches children to
> **recognise the fish they catch** — starting with colour, growing into real
> (local Indonesian) species identification — and lets them **collect** what
> they learn and **customise** their spear.

Status date: 2026-06-02 · Owner: arisros

---

## 1. Vision (revised)

From a *colour-matching drill* → a **marine-life learning + collection game** with
**cosmetic customisation**.

Three pillars:

| Pillar | Player experience | Why it matters |
|--------|-------------------|----------------|
| **Learn** | Identify the fish you spear (colour → species name + picture) | The educational core; differentiates from generic tap games |
| **Collect** | A "Fishipedia" of caught species with names + facts | Retention + reinforces learning |
| **Progress** | A persistent **player level** that unlocks content and sets long-term goals | The meta-backbone tying play → learning → rewards |
| **Customise** | Cosmetic spear skins earned/bought | Engagement + (ethical) monetisation |

---

## 2. Guiding principles

1. **Data-driven, art-decoupled.** All fish and spears are defined as data
   (ScriptableObjects). Gameplay/UI/persistence are built around the data and run
   with **placeholder art** (existing tuna mesh + colour tint). Real models drop in
   later with **zero code changes** — so art never blocks progress.
2. **Cosmetic-only monetisation + parental gate.** No pay-to-win; respect app-store
   and children's-privacy rules (COPPA / GDPR-K). Real-money IAP is optional and
   gated.
3. **Educational accuracy.** A small, *verified* species set beats a large sloppy
   one. Never teach a child the wrong fish.
4. **Mobile-AR performance.** Prefer **low-poly stylised** assets; the colour-tint
   mechanic favours simple meshes.
5. **Verify before claiming.** Land the `UNITY_LICENSE` secret so CI actually
   compiles/builds and runs the test suite (today the Unity jobs *skip*).

---

## 3. Phases

### Phase 0 — Verification foundation *(unblocks everything)*
- [ ] Add `UNITY_LICENSE` (+ `UNITY_EMAIL`/`UNITY_PASSWORD`) secret → CI builds + runs tests.
- [x] CI branch mismatch (`main`→`master+main`) fixed; future PRs run CI.
- [x] Establish a real playtest path (local build / screenshots) to replace simulated reports.

Detailed specs: `docs/specs/fish-species-system.md`,
`docs/specs/spear-cosmetics-system.md`, `docs/specs/leveling-system.md`.

### Phase 1 — Fish Species System + Fishipedia *(educational core)* ✅ CODE COMPLETE
See `docs/specs/fish-species-system.md`.
- [x] `FishSpecies` ScriptableObject + `FishCatalog` with weighted-random pick.
- [x] Species-driven spawning; `FishSpawner` catalog-aware (null = colour-only fallback).
- [x] `FishipediaUI` collection screen with lock/unlock display + count label.
- [x] `FishdexStore` persistence; species unlock on first catch; tests.
- [ ] Wire catalog in Unity scene + create art assets (use `Tools > Tombakan > Create Starter Data`).

### Phase 2 — Spear Cosmetics + Shop *(monetisation)* ✅ CODE COMPLETE
See `docs/specs/spear-cosmetics-system.md`.
- [x] `SpearSkin` SO + `SpearShopCatalog` + `SpearStore` ownership/equip/persistence.
- [x] `CurrencyStore` soft coins with pure earn rule.
- [x] `SpearShopUI` — shop screen with buy / equip / coin display.
- [x] `SpearThrower` reads equipped skin prefab/material; `EnsureDefault` on Start.
- [ ] Parental gate for premium skins (soft-currency only for v1 → safe for kids).
- [ ] Wire shop in Unity scene + assign skin art when available.

### Phase 2.5 — Leveling & Progression *(meta-backbone)* ✅ CODE COMPLETE
See `docs/specs/leveling-system.md`.
- [x] `ProgressionRules` pure XP curve (monotonic, inverse-consistent, 22 tests).
- [x] `ProgressionStore` persistent XP + level; `AddXp` returns new level on level-up.
- [x] `LevelReward` + `LevelRewardTable` SO; rewards fire on level-up in EndGame.
- [x] `ProgressionHUD` level badge + XP bar; `GameManager` tracks maxComboStreak + newSpecies.
- [ ] Wire HUD elements in Unity scene.

### Phase 3 — Art & juice pass *(visible quality)* ✅ CODE COMPLETE (art TBD)
- [x] `ScreenShake` — damped camera shake on correct/wrong hit.
- [x] `HapticFeedback` — Handheld.Vibrate on Android/iOS.
- [x] `PerformanceSettings` — low-end mode (30 fps, shadow disable, fixed 1/30).
- [ ] Implement the UI redesign (`design/tombakan_ui_v0.svg`) in Unity.
- [ ] Real low-poly species models + spear skins (art TBD).
- [ ] Shader/particle juice: water, "caught" dissolve/flop, splash (art TBD).

### Phase 4 — Modes & retention ✅ CODE COMPLETE
- [x] `DailyChallenge` — daily bonus XP (100 + 25/streak day, capped at 7).
- [ ] Additional game modes (zen / challenge / stage packs) — content decision needed.
- [ ] Achievements system — hooks exist via XP/level; UI TBD.

### Phase 5 — Onboarding & accessibility ✅ CODE COMPLETE
- [x] `TombakanOnboarding` — skips AR coaching for returning players (score/XP > 0).
- [x] `ColourBlindSettings` + `FishShapeOverlay` + `ColourBlindToggleUI` — ●▲■★ per colour.
- [ ] Wire onboarding + accessibility buttons in Unity scene.

### Continuous
- [x] Low-end device performance mode.
- [ ] `UNITY_LICENSE` secret — CI test/build jobs currently skip.
- [ ] C# lint (`dotnet format`) once `.csproj` is generated by Unity in CI.

---

## 4. What's already shipped (weeks 1–4)

Scoring (floor, combos, dynamic fish count, adaptive pacing, time bonus), timers + numeric
countdown, tier stars, best-score persistence, colour summary, target colour-word label,
accuracy stat, centralised + progressive colour palette, audio robustness + persisted mute,
dead-code removal, GitHub Actions CI, and an EditMode test suite (~55 tests). See
`ITERATION_LOG.md`.

These are the **logic foundation** the systems above build on.

---

## 5. Dependencies / ownership

| Need | Who | Blocks |
|------|-----|--------|
| `UNITY_LICENSE` secret | **You** | CI verification (Phase 0) |
| Verified species list + facts | **You** (I can draft) | Phase 1 educational accuracy |
| 3D models (fish species, spear skins) | **You / tool / artist** — I integrate | Phase 3 visuals |
| Store / billing accounts for IAP | **You** | Phase 2 real-money (optional) |
| Monetisation model decision (cosmetic-only + gate) | **You** | Phase 2 scope |

Everything else (data systems, gameplay, UI logic, persistence, shaders, tests,
integration of supplied art) can be built and committed without leaving this environment.

---

## 6. Parallelisation

The data-driven seam lets work run in parallel:
- **Track A (systems, no art):** Phase 1 + Phase 2 with placeholders — fully buildable now.
- **Track B (art):** species models + spear skins + UI implementation — slot into Track A.
- **Track C (foundation):** Phase 0 license/CI — independent, do anytime.

Recommended start: **Track A → Phase 1 (Fish Species + Fishipedia)**, in parallel with
**Track C (license)**.
