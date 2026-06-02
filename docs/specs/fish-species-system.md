# Spec — Fish Species System + Fishipedia (Phase 1)

**Goal:** Move from "spear the right *colour*" to "spear the right *fish*", so kids learn
to recognise (local Indonesian) species. Build it **data-driven** so it runs on placeholder
art today and accepts real models later with no code changes.

---

## Data model

`FishSpecies` (ScriptableObject), one asset per species:

| Field | Type | Notes |
|-------|------|-------|
| `id` | string | stable key, e.g. `"ikan_badut"` |
| `displayNameId` | string (Indonesian) | e.g. `"Ikan Badut"` |
| `englishName` / `latinName` | string (optional) | for older kids / parents |
| `modelPrefab` | GameObject | **falls back to the existing tuna prefab** if null |
| `baseColor` | Color | used by the colour layer + tint placeholder |
| `rarity` | enum (Common/Uncommon/Rare) | spawn weighting + collection flavour |
| `habitatId` | string | e.g. `"terumbu_karang"` (reef) |
| `funFact` | string (Indonesian) | one short, **verified** line for the Fishipedia |
| `icon` | Sprite | target card + collection card |

`FishCatalog` (ScriptableObject): ordered `List<FishSpecies>` + lookup by id.

---

## Gameplay integration

- `FishSpawner` spawns from a chosen `FishSpecies` (model + colour) instead of a raw colour.
  Keep the existing colour-tint path as the placeholder renderer until `modelPrefab` exists.
- `FishTarget` carries `speciesId` (in addition to current `fishColor`).
- **Difficulty layering** (pure, testable rule — mirrors `FishPalette.CountForProgress`):
  - Early: match by **colour** (current behaviour).
  - Mid: match by **species name** within one colour family.
  - Late: match species among similar-looking decoys.
- Target UI becomes a card: `🐟 [icon] + "Tangkap: <Indonesian name>"` (reuses the Week 3
  colour-word label pattern).

## Fishipedia (collection)

- New screen listing all catalog species; locked = silhouette, unlocked = icon + name + fact.
- A species unlocks the first time it's caught.
- Persisted via a small save store (mirror `ScoreStore`/`AudioPrefs` pattern) — keep the
  unlock-set logic **pure/static and unit-testable**.
- Entry points: main menu button + a "new species!" celebration on first catch.

## Persistence

- `FishdexStore` (PlayerPrefs or a JSON save): `IsUnlocked(id)`, `Unlock(id)`,
  `UnlockedCount()`, `UnlockedIds()`. Pure decision logic separated from storage for tests.

## Testability (no scene needed)

- Spawn-table weighting by rarity.
- Difficulty-layer rule (colour vs species by progress).
- Fishdex unlock set (idempotent unlock, count, ordering).

## Placeholder strategy

Ship with the **existing tuna mesh + `baseColor` tint** for every species. The catalog,
targets, Fishipedia, unlocks, and tests all work. Swapping in per-species `modelPrefab`
later is a data-only change.

---

## Open questions (need your input)

1. **Starter species list (CANDIDATES — VERIFY before shipping; names/facts must be checked):**
   `Tongkol`, `Kakap`, `Kerapu`, `Baronang`, `Ikan Badut` (clownfish), `Lele`, `Nila`,
   `Bandeng`. — Want these, a different set, or should I draft a verified shortlist?
2. Match-by **colour vs species** — keep colour as the early layer, or go straight to species?
3. Fishipedia depth — name + one fact, or richer (habitat, size, sound)?
4. Source for the per-species art (free low-poly pack / generative 3D / artist)?

## Acceptance (Phase 1 done)

- Catalog of ≥6 species drives spawning + targets (placeholder models OK).
- Caught species persist and appear unlocked in the Fishipedia.
- Difficulty layering + unlock logic covered by EditMode tests.
- No regressions to existing scoring/timer/audio systems.
