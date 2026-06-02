# Spec — Leveling & Progression System

**Goal:** A persistent **player level** that ties play, learning, and rewards together —
giving kids long-term goals and pacing the unlock of species, spear skins, and modes.
This is the **meta-progression backbone** that sits across the Fish and Cosmetics systems.

> Not to be confused with the per-run **tier stars** (Empty/Low/Mid/High/Legend) shown on
> the result screen — those rate a *single game*. Leveling is the *cross-run account* that
> carries forward forever.

---

## Two flavours (pick one or both)

1. **Account level (XP-based)** — *recommended primary.* Earn XP every game; total XP →
   player level; levels unlock content + celebrate progress. Simple, flexible, art-light.
2. **Stage / level packs** — discrete stages (e.g. *Pantai → Terumbu → Laut Dalam*) with
   their own species sets and difficulty. Richer, but needs more content/art per stage.

This spec details flavour **1** and leaves hooks for **2**.

---

## Data model

- **XP sources** — pure rule, e.g. `ProgressionRules.XpForResult(...)`:
  | Source | XP (example) |
  |--------|--------------|
  | Correct hit | +10 |
  | Combo bonus | +5 × (multiplier−1) |
  | **New species discovered** | +50 (big — rewards learning) |
  | Accuracy bonus | +1 per accuracy % |
  Values live in a `ProgressionConfig` ScriptableObject so designers tune without code.

- **Level curve** — pure + testable:
  - `ProgressionRules.LevelForXp(totalXp)` → level.
  - `ProgressionRules.XpForLevel(level)` → cumulative XP needed.
  - `ProgressionRules.XpIntoLevel(totalXp)` / `XpToNextLevel(totalXp)` → for the XP bar.
  - Curve: rising thresholds (e.g. `base * level^1.5`, table-driven, or a `LevelCurve` SO).
    Must be **monotonic** and cover a sensible max level.

- **Level rewards** — `LevelReward` entries keyed by level: unlock species id / spear-skin
  id / mode / soft currency. A `LevelRewardTable` ScriptableObject.

## Persistence

- `ProgressionStore` (mirrors `ScoreStore`/`AudioPrefs`): `GetTotalXp()`, `AddXp(int)`,
  `GetLevel()`. Pure level/curve logic separated from storage for tests.

## Gameplay / UI integration

- **EndGame:** compute `XpForResult(score, accuracy, correctHits, newSpecies)`,
  `ProgressionStore.AddXp(...)`, detect level-up(s), reveal rewards (unlock via
  `FishdexStore` / `SpearStore` / `CurrencyStore`).
- **HUD / main / result:** level badge + XP progress bar (`XpIntoLevel` / `XpToNextLevel`).
- **Level-up celebration:** "Level 5! Tombak baru terbuka!" with the reward shown.
- **Difficulty tie-in (optional):** level can gate which species/colours appear, layered on
  top of the existing per-run ramps (`FishCountForDifficulty`, `FishPalette.CountForProgress`).

## Testability (no scene needed)

- `LevelForXp` / `XpForLevel` are inverse-consistent and monotonic.
- Level boundaries (XP just below/at/above a threshold).
- `XpForResult` rule (each source contributes correctly; new-species bonus applies once).
- Reward-table lookup (correct reward(s) granted on reaching a level; idempotent).

## Placeholder strategy

Ship the full XP/level/bar/celebration loop immediately. Rewards can point at the
**placeholder species/skins** from the other specs; real content slots in as data later.

---

## Open questions (need your input)

1. **Account level, stage packs, or both?** (Recommend account level first.)
2. **What unlocks per level** — species, spear skins, modes, currency, more time? In what order?
3. **Max level + curve feel** — fast early levels (kid-friendly dopamine) then slower?
4. Should level **gate difficulty/content**, or be purely cosmetic/celebratory at first?

## Acceptance (done)

- Persistent total XP + level survive app restarts.
- Each game awards XP; reaching a threshold levels up and grants the configured reward.
- XP bar + level badge shown; level-up celebration fires.
- XP curve, level boundaries, earn rule, and reward lookup covered by EditMode tests.
- No regression to per-run tier stars or existing scoring.
