# Iteration Week 9 — Scope

**Based on:** TESTER_REPORT_week8.md (2026-07-27)
**Previous implementation:** Week 6 (last entry in ITERATION_LOG.md)

---

## Selected Tasks

| ID | Owner | Task | Acceptance Criteria |
|----|-------|------|---------------------|
| TASK-01 | dev | **P0 + defensive fixes in GoalManager / GameManager:** (a) null-guard `m_OnboardingGoals` in `GoalManager.CompleteGoal()` — if queue is null, set `m_AllGoalsFinished = true` and return without deref; (b) fix `ShowSad()` to compute actual absorbed penalty and display `"Meleset!"` when `ClampScore` fully absorbs it; (c) add `CancelInvoke(nameof(HideFeedback))` at the top of both `ShowHappy()` and `ShowSad()` to prevent stale hide calls; (d) after `SpawnFish` resolves `CurrentTargetSpecies`, update `targetColorLabel.text` using the resolved species colour | (a) A returning player (`ScoreStore.GetBest() > 0`) launches without NullReferenceException; (b) hitting a wrong fish when score is already 0 shows `"Meleset!"` not `"-25!"`; (c) two fish hits within 1.0 s always show feedback for the full 1 s (no 0 ms flash); (d) in catalog mode, `targetColorLabel.text` matches `targetColorImage.color` every round immediately after fish spawn |
| TASK-02 | dev | **Level-reward propagation + ColourBlind hue-range fix:** (a) in `GameManager.EndGame()`, record level before and after `AchievementChecker.CheckAll()`; call `ApplyLevelReward` + `ShowLevelUp` if level increased; (b) make `ApplyLevelReward` public and add a `GameManager` reference to `TombakanOnboarding`; in `TombakanOnboarding.Start()`, call `gameManager.ApplyLevelReward(levelRewardTable.GetRewardForLevel(newLevel))` when `newLevel > 0`; (c) replace `ColourBlindSettings.ShapeForColor` exact-hex switch with an HSV hue-range classifier: red (h < 0.083 or h > 0.917) → `"●"`, orange-yellow (h < 0.25) → `"★"`, green (h < 0.458) → `"▲"`, blue-cyan-purple (h < 0.917) → `"■"`, with a low-saturation (s < 0.15) neutral override | (a) Triggering an achievement whose XP crosses a level boundary shows the level-up panel and applies `LevelReward` in the same end-game flow; `CurrencyStore.GetCoins()` increases if the reward has a coin bonus; (b) a daily-bonus level-up applies its `LevelReward` within 3 s of the bonus panel appearing; (c) enabling colour-blind mode with any of the 8 catalog species active shows one of `●▲■★` (never `"?"`) on every fish overlay |
| TASK-03 | ui | **Spawn radius and throw-tutorial panel (UX-W8-1 + UX-1):** In `GamePlay.unity` scene: (a) find the `FishSpawner` component and set its serialised `spawnRadius` field from `1.5` to `0.8`; (b) add a child `GameObject` named `ThrowHintPanel` under `GamePlayUI` (Screen Space Overlay), inactive by default, with a semi-transparent background `Image` and a centred `TMP_Text` reading `"Tekan tombol untuk melempar tombak!"` anchored to bottom-centre of screen; wire `ThrowHintPanel` to the `GameManager.throwHintPanel` field | `FishSpawner` serialised `spawnRadius` is `0.8` in the scene; fish spawned on a 60 cm table stay within the visible water surface; `ThrowHintPanel` GameObject exists under `GamePlayUI`, is inactive by default, anchors to bottom-centre, and contains the Indonesian throw instruction |
| TASK-04 | artist | **FishSpecies colour audit for ColourBlind hue separation:** Read each of the 8 FishSpecies `.asset` files created by `TombakanSetupWizard`. For each species, determine which hue bucket its `baseColor` falls in (red/orange-yellow/green/blue). If two or more species share the same bucket — making them indistinguishable in colour-blind mode — update the `baseColor` of the lower-priority species to a representative hue from an under-used bucket, keeping saturation ≥ 0.6 and value ≥ 0.5 for AR visibility. Record any changes made. | Each of the 8 species maps to a distinct `ShapeForColor` symbol after the hue-range fix in TASK-02; no two species produce the same symbol; all updated `baseColor` values are saturated (s ≥ 0.6) and bright (v ≥ 0.5) |

---

## Deferred

- **UX-NEW-2** — `greetingPanel` / `dailyBonusPanel` mutual exclusion; no ordering guard. Low frequency (edge case for players with XP but no best score). Defer to Week 10.
- **UX-NEW-3** — Achievement toast blocks result screen for up to 12 s; no tap-to-skip. Requires layout and coroutine changes across two tasks; scope for Week 10 after toast timing stabilises post-TASK-01 fixes.
- **BUG-W8-3** — `FishCatalog.PickOther` retry loop allows decoy = target with ~18% per-round probability on 2-species catalogs. Deferred; requires replacing the retry loop with deterministic filtered-list pick; scope for Week 10.
- **POLISH-3** — `ApplyLevelReward` can overwrite `levelUpText.text` before player reads "Level N! Selamat!" — making text in two separate UI elements would require a scene change and layout review; defer.
- **POLISH-NEW-1** — `ColorSummary.Format` always appends `×1` for single catches; cosmetic; defer.
- **UX-1 (code side)** — `GameManager.StartGame()` should show `throwHintPanel` for 8 s on first session. The panel itself is added in TASK-03 (ui); the show/hide coroutine in `GameManager` is a follow-on dev task in Week 10 once the panel is wired in scene.

---

## Rationale

**TASK-01** addresses the single CRITICAL bug (GoalManager NRE, Rank 1): every returning player crashes on launch, making the game unshippable for anyone past session 1. The three additional changes (ShowSad, CancelInvoke, label update) are all 1–5 line modifications to the same two files and share no dependencies with other tasks; bundling avoids a second dev pass over GameManager.cs next week.

**TASK-02** tackles the two HIGH reward-loss bugs (BUG-NEW-1, BUG-NEW-2) and the HIGH accessibility regression (BUG-NEW-3). All three have been confirmed across Week 7 and Week 8 tester sessions and compound each other: a player who levels up via an achievement AND has colour-blind mode active loses both their reward and their gameplay signal simultaneously. The ColourBlind fix is a pure logic change confined to `ColourBlindSettings.cs` and does not require scene or asset work.

**TASK-03** is a pure scene task: changing one float and adding one panel object. It unblocks UX-W8-1 (fish leaving the table surface on AR) and lays the scene groundwork for UX-1 (throw tutorial), which only needs a `GameManager.StartGame()` coroutine to become fully functional.

**TASK-04** ensures that the hue-range fix in TASK-02 actually provides discrimination across all species. Without colour separation in the data, the code fix is correct but ineffective — two orange-ish species would both map to `"★"`. The artist agent can complete this in one pass over the 8 `.asset` files.
