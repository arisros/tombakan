# Iteration Week 7 — Scope

## Selected Tasks

| ID | Owner | Task | Acceptance Criteria |
|----|-------|------|---------------------|
| T1 | dev | **BUG-W7-2** — `AchievementChecker`: propagate `AddXp` return value and call `ApplyLevelReward`. In `AchievementChecker.cs` line 100, capture the return value of `ProgressionStore.AddXp(achievement.xpReward)`; if it returns a non-zero level, look up the reward via `GameManager.I?.levelRewardTable?.GetRewardForLevel(newLevel)` and call a new internal helper `GameManager.I?.ApplyLevelReward(reward)` (make `ApplyLevelReward` `internal` or `public`). | After unlocking an achievement whose XP crosses a level boundary, `CurrencyStore.GetCoins()` reflects the soft-currency bonus and `LevelRewardTable` species/skin unlocks are applied. Covered by a new `AchievementCheckerRewardTests` EditMode test. |
| T2 | dev | **BUG-W7-5** — `GameManager.ShowSad()`: suppress penalty text when score was already 0. Compute `scoreBefore` before calling `ClampScore`; pass a boolean `deductionAbsorbed` into `ShowSad`. If absorbed, show `"Miss!"` instead of `"-{penaltyPerWrongHit}!"`. | When score is 0 and a wrong fish is hit, the sad-feedback label reads `"Miss!"` and the displayed score stays at 0. When score is above the penalty floor, the label still reads `"-25!"`. Covered by a new `GameManagerFeedbackTests` EditMode test for the absorbed case. |
| T3 | dev | **BUG-W7-1** — `DailyChallenge`/`TombakanOnboarding`: apply level reward after daily-bonus level-up. In `TombakanOnboarding.Start()`, after `DailyChallenge.TryClaimDailyBonus` returns and a level-up is detected (`levelAfter > levelBefore`), call `GameManager.I?.ApplyLevelReward(GameManager.I?.levelRewardTable?.GetRewardForLevel(levelAfter))`. This requires `ApplyLevelReward` to be made `public` (done as part of T1). | On a day-boundary test where daily XP crosses a level threshold, `CurrencyStore.GetCoins()` increases by the configured `softCurrencyBonus` and any configured unlock is applied. Covered by a new EditMode test that stubs `ProgressionStore` near a level boundary. |
| T4 | ui | **Throw-mechanic tutorial hint** — First-time players get no indication of how to throw. Add a `hintPanel` (`GameObject`) and `hintText` (`TMP_Text`) field to `TombakanOnboarding`. In `GameManager.StartGame()` (or via a new `public void NotifyGameStarted()` called from `GameManager`), start a `2f`-second delayed `Invoke` that activates `hintPanel` only when `correctHitCount == 0 && wrongHitCount == 0`. Wire `SpearThrower` to call `TombakanOnboarding.I?.DismissHint()` on first successful throw. Set `hintText` to `"Sentuh tombol untuk melempar tombak!"`. Auto-dismiss after 5 s if not already dismissed. | `hintPanel` becomes active 2 s after `StartGame()` on a fresh session with zero throws, shows the Indonesian hint string, and is hidden the moment the first spear is thrown. On returning sessions where a throw has already occurred, the panel never appears. |

## Deferred (picked up next iteration)

- **BUG-W7-4** (`FishShapeOverlay` runtime toggle has no effect) — Re-evaluated against live code: `ColourBlindToggleUI.OnClick()` already calls `FindObjectsOfType<FishShapeOverlay>()` and invokes `OnSettingChanged()` on each instance. The tester report describes a pre-Week-6 state of the file; the fix is already present. No code change needed.
- **BUG-W7-3** (`ColourBlindSettings.ShapeForColor` returns `"?"` for non-palette colours) — Valid bug but depends on `FishCatalog` species `baseColor` values being finalised. Deferred until art assets are confirmed.
- **BUG-W7-6** (`GoalManager.ForceCompleteGoal` potential NRE for returning players) — Requires reading `GoalManager.cs` scene wiring context; deferred to avoid scope creep.
- **Platform-specific haptic differentiation** (POLISH-2) — Requires a device test matrix before merge; deferred per backlog note.
- **"Re-position water" button** — Involves scene YAML (UI Button wiring) and is blocked on Unity Editor access for the HUD; deferred.
- **Daily bonus panel overlaps greeting panel** — UX polish; lower severity than P0 bugs selected this iteration.

## Rationale

**P0 bugs first.** T1 and T3 are both silent reward-loss failures: the player earns XP, crosses a level boundary, but receives nothing. T1 (achievement path) and T3 (daily-bonus path) share the same root cause — `ProgressionStore.AddXp` return value discarded outside `GameManager.EndGame` — and are both fixable by making `ApplyLevelReward` public and calling it from the two callsites.

**BUG-W7-5** (T2) is a player-trust bug: displaying `-25!` when the score did not change erodes confidence in the scoring system. One-line conditional change with a clear test oracle.

**T4 (throw tutorial)** is the highest-ranked UX gap and the single most likely cause of first-session drop-off. Pure `TombakanOnboarding.cs` + `GameManager.cs` C# — no scene YAML, no Unity Editor required.

**BUG-W7-4 is not a real task this week**: live inspection of `ColourBlindToggleUI.cs` confirms the `FindObjectsOfType` broadcast is already in place.
