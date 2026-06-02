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
- [ ] Confirm the project compiles in Unity (first real CI run validates the asmdef refs).
- [ ] Establish a real playtest path (local build / screenshots) to replace simulated reports.

Detailed specs: `docs/specs/fish-species-system.md`,
`docs/specs/spear-cosmetics-system.md`, `docs/specs/leveling-system.md`.

### Phase 1 — Fish Species System + Fishipedia *(educational core)*
See `docs/specs/fish-species-system.md`.
- [ ] `FishSpecies` ScriptableObject + catalog.
- [ ] Species-driven spawning; target = species (name + icon), colour as early layer.
- [ ] Collection / "Fishipedia" screen with unlock + facts.
- [ ] Persistence of caught species; tests. Placeholder models until art lands.

### Phase 2 — Spear Cosmetics + Shop *(monetisation)*
See `docs/specs/spear-cosmetics-system.md`.
- [ ] `SpearSkin` ScriptableObject + ownership/persistence.
- [ ] Soft currency earned by play; shop + equip UI; parental gate.
- [ ] `SpearThrower` uses the equipped skin. Optional Unity IAP scaffold.

### Phase 2.5 — Leveling & Progression *(meta-backbone — buildable in parallel)*
See `docs/specs/leveling-system.md`.
- [ ] Persistent player XP + level (`ProgressionStore`), pure XP/level curve.
- [ ] XP awarded at end-of-game; level-up detection + celebration.
- [ ] Level reward table → unlocks species / spear skins / currency / modes.
- [ ] XP bar + level badge on HUD/main/result; tests for curve, boundaries, rewards.
- Built data-driven against placeholders; rewards point at Phase 1/2 content as it lands.

### Phase 3 — Art & juice pass *(visible quality)*
- [ ] Implement the UI redesign (`design/tombakan_ui_v0.svg`) in Unity.
- [ ] Real low-poly species models + spear skins (free packs / generative 3D / artist).
- [ ] Shader/particle juice: water, "caught" dissolve/flop, splash, screen-shake, haptics.

### Phase 4 — Modes & retention
- [ ] Modes beyond the single 60 s round (zen / challenge / **stage packs** — leveling flavour 2).
- [ ] Daily challenge + achievements (feed XP into the Phase 2.5 leveling system).

### Phase 5 — Onboarding & accessibility
- [ ] Streamline AR onboarding (`GoalManager`) for players who know AR placement.
- [ ] Colour-blind support: shape/symbol overlays on fish (target word label already shipped).

### Continuous
- [ ] Robustness, low-end-device performance, CI C# lint (`dotnet format`, once `.csproj` is generated).

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
