# Spec — Spear Cosmetics + Shop (Phase 2)

**Goal:** Cosmetic spear customisation as an engagement + (ethical) monetisation layer.
Built data-driven and art-decoupled like the fish system.

---

## Data model

`SpearSkin` (ScriptableObject), one asset per skin:

| Field | Type | Notes |
|-------|------|-------|
| `id` | string | stable key, e.g. `"spear_bamboo"` |
| `displayNameId` | string (Indonesian) | e.g. `"Tombak Bambu"` |
| `prefab` / `material` | refs | **falls back to the current spear** if null |
| `price` | int | in soft currency |
| `currency` | enum (Coins / Premium) | premium only if real-money is enabled |
| `unlockedByDefault` | bool | starter skin = true |
| `previewIcon` | Sprite | shop tile |

`SpearShopCatalog` (ScriptableObject): ordered `List<SpearSkin>`.

---

## Systems

- **Ownership + persistence:** `SpearStore` — `IsOwned(id)`, `Buy(id)`, `Equip(id)`,
  `EquippedId()`. Keep the **purchase/equip decision logic pure + unit-testable**; storage
  (PlayerPrefs/JSON) behind it.
- **Soft currency:** earned per game (e.g. tied to score/accuracy). `CurrencyStore` with a
  pure `EarnedForResult(score, accuracy)` rule (testable).
- **Equip → gameplay:** `SpearThrower` instantiates the **equipped** skin's prefab/material
  instead of the hard-coded one (null-safe fallback to current spear).
- **Shop UI:** grid of skins (owned / buyable / equipped states), buy + equip actions.

## Monetisation (decide first)

- **Default: cosmetic-only + soft currency**, earned by play. Safe for a kids' title.
- **Optional real-money IAP:** scaffold **Unity IAP** (`com.unity.purchasing`) for premium
  skins/currency. Requires Google Play / App Store developer accounts + product config
  (yours). Gate the shop behind a **parental gate** (e.g. a simple maths challenge) per
  app-store rules for children.

## Legal / ethical flags

- COPPA / GDPR-K and store policies restrict ads + IAP targeted at children.
- No pay-to-win (cosmetics only). No manipulative dark patterns.
- Keep premium purchases behind the parental gate; make soft-currency the primary path.

## Testability (no scene needed)

- Buy logic (enough currency, idempotent ownership, can't double-charge).
- Equip logic (only owned skins equippable; default always owned).
- Currency `EarnedForResult` rule.

## Placeholder strategy

Ship with 2–3 skins defined as data, all using the **current spear** prefab tinted by
`material`/colour. The shop, currency, ownership, equip, and tests work before any new
spear models exist.

## Open questions (need your input)

1. Confirm **cosmetic-only** as the model (recommended).
2. Real-money IAP now, or soft-currency only for v1?
3. Currency earn rate / sources (score? accuracy? species collected?).
4. Source for spear-skin art (free pack / generative 3D / artist)?

## Acceptance (Phase 2 done)

- ≥3 skins in catalog; default owned + equipped; others buyable with soft currency.
- Equipped skin is used by `SpearThrower` (placeholder art OK).
- Buy/equip/currency logic covered by EditMode tests.
- Parental gate present if any real-money path is enabled.
