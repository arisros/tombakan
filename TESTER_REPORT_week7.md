# Tombakan — Tester Report Week 7
**Date:** 2026-08-10  
**Tester:** AI Game Tester (10 simulated sessions)  
**Scope:** `GameManager.cs`, `FishSwim.cs`, `FishSpawner.cs`, `SpearThrower.cs`,
`PlaceWaterOnPlane.cs`, `FishHitBox.cs`, `Dict.cs` plus supporting scripts read for
context: `SpearHit`, `FishPalette`, `PacingRules`, `TimeBonus`, `DailyChallenge`,
`TombakanOnboarding`, `GoalManager`, `ColourBlindSettings`, `FishShapeOverlay`,
`ProgressionStore`, `ProgressionRules`, `Accuracy`, `AchievementChecker`

---

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Severity |
|------|-------|-----------|----------|
| 1 | `DailyChallenge.TryClaimDailyBonus` discards `ProgressionStore.AddXp` return — level reward (species unlock, skin, coins) silently not applied on daily-bonus level-up | `DailyChallenge.cs:58` | CRITICAL |
| 2 | `AchievementChecker.CheckAll` discards `ProgressionStore.AddXp` return at line 100 — achievement XP that crosses a level boundary produces no panel and no reward | `AchievementChecker.cs:100` | HIGH |
| 3 | 0.8 s window between spear-throw unlock and fish spawn — player can throw into empty water with zero feedback every single round | `GameManager.cs:485-486` | HIGH |
| 4 | Timer bar saturates at 100 % when combo time-bonuses push `timeLeft` above `gameDuration` — countdown text and bar disagree; skilled players lose visual pacing feedback | `GameManager.cs:162` | HIGH |
| 5 | `ColourBlindSettings.ShapeForColor` only handles four exact hex values; any `FishSpecies.baseColor` outside that set shows `"?"` on every fish in colour-blind mode | `ColourBlindSettings.cs:22-27` | MEDIUM |
| 6 | `targetColorLabel` not updated after species colour override in `PickNewTarget` — text label and colour swatch disagree when catalog species colour differs from the palette pick | `GameManager.cs:415-420` | MEDIUM |
| 7 | Colour-blind mode: target indicator shows no shape symbol — `FishShapeOverlay` marks fish correctly but the HUD target panel never shows which shape to aim for | `GameManager.cs` / `ColourBlindSettings.cs` | MEDIUM |
| 8 | `GoalManager.ForceCompleteGoal` called before `StartCoaching` initialises the queue — `NullReferenceException` or `IndexOutOfRangeException` risk for returning-player first launch | `TombakanOnboarding.cs:30`, `GoalManager.cs:196` | MEDIUM |
| 9 | `ShowSad()` always shows `"-25!"` even when `ClampScore` absorbed the full deduction and the displayed score did not change — misleading for players at score 0 | `GameManager.cs:507` | LOW-MEDIUM |
| 10 | No feedback when spear hits empty water — complete misses are invisible; first-timers cannot tell if the game registered their throw | `SpearHit.cs` | LOW-MEDIUM |

---

## Session Simulations

### Sessions 1–2 — First-Timer ("Raka", fresh install, no AR experience)

**Trace:**
1. App launches. `GameManager.Start()` plays main BGM. HUD shows `Lv 1`, XP bar empty.
2. `TombakanOnboarding.Start()` — `isReturningPlayer = false` — `greetingPanel` shown.
   Daily bonus: streak=1, xpAwarded=125. `ProgressionStore.AddXp(125)` inside
   `DailyChallenge.cs:58` — return value discarded. Still Level 1 at 125 XP (threshold 100),
   so level-up occurs but no reward runs. **BUG-1.**
3. Coaching begins. Player scans floor, taps. `PlaceWaterOnPlane.Update()` detects touch →
   places water → `enabled = false`. Water can never be repositioned. **BUG-7.**
4. `StartGame()`. Target = "Merah" (Red). No throw hint visible. **UX-1 (known, still open).**
5. Player taps a fish directly — nothing happens (touch is not a throw; throw requires the
   button). Player taps background — `PlaceWaterOnPlane` disabled, no response.
6. Player figures out the throw button. Spear flies, misses all fish. **Zero feedback.
   No sound, no UI, no vibration.** Player unsure if the game registered the input. **BUG-10.**
7. Player throws at wrong-colour fish. `OnFishHit(blue)` while target is Red. `comboStreak=0`,
   `wrongHitCount=1`. `score = ClampScore(0 - 25) = 0`. `ShowSad()` → "-25!" displayed,
   but score display stays 0. Confusing. **BUG-9 (ShowSad at score=0).**
8. Timer expires. `Accuracy.Format(0, 1) = "0/1 (0%)"`. Player threw 8 spears total; only 1
   wrong-colour hit is counted. Seven complete misses are invisible. **BUG-6.**

---

### Sessions 3–4 — Casual Returning Player ("Siti", Day 3, Level 2)

**Trace:**
1. `isReturningPlayer = true` → `goalManager.ForceCompleteGoal()` called.
   `GoalManager.m_OnboardingGoals` is a `Queue<Goal>` initialised only in `StartCoaching()`.
   No `Awake`/`Start` in `GoalManager` initialises it. If `StartCoaching` has not been called
   this session, `m_OnboardingGoals` is null → `NullReferenceException` in `CompleteGoal()`.
   If the list is non-null but step-list is not wired, `IndexOutOfRangeException` on
   `m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false)`. **BUG-8.**
2. Daily bonus. Siti at 95 XP (5 from Level-2 threshold). streak=3, xpAwarded=175.
   `ProgressionStore.AddXp(175)` inside `DailyChallenge.cs:58` → XP = 270, Level 3
   threshold ≈ 283; Level 2 threshold = 100. New level = 2. Return value discarded.
   No `ApplyLevelReward` call. Any Level-2 reward (coins, species, skin) silently lost. **BUG-1.**
   `TombakanOnboarding` catches `levelAfter=2 > levelBefore=1` via its own GetLevel calls and
   shows the level-up line in the daily panel — UI acknowledgement is correct (Week 6 partial fix
   confirmed), but the reward grant is still missing.
3. Game runs. Siti hits 5 correct (peak streak=3, x2 multiplier), 3 wrong.
   Score: 100+100+200+100+100 - 25 - 25 - 25 = 525. Correct.
4. EndGame. `xpEarned = XpForResult(5, 63, 0, 3) = 50+5+63 = 118 XP`.
   `AchievementChecker.CheckAll` — `first_catch` newly unlocked. `ProgressionStore.AddXp(50)`
   at `AchievementChecker.cs:100` — return value discarded. If the 50 XP crosses a level
   boundary, the level-up is invisible and no reward is applied. **BUG-2 (new find).**
5. Achievement toast fires at +1.2 s after result screen, 0.4 s after the level-up panel
   at +0.8 s. Both panels potentially visible simultaneously. **UX-3 (toast/level-up overlap).**

---

### Sessions 5–6 — Frustrated Player ("Budi", bad AR placement)

**Trace:**
1. Budi places water in a corner behind furniture. Fish spawn in `[waterPlane.position ± 1.5m]`.
   Fish swim behind the furniture from Budi's AR view.
2. Budi tries to re-tap the floor. `PlaceWaterOnPlane` is disabled. No re-position path.
   Only fix: close and relaunch the app. **BUG-7 (no re-place affordance).**
3. Target = "Biru". Blue fish is behind the sofa. Budi throws at other fish.
   Wrong hits pile up. Score stays at 0 (clamped). "-25!" shows each time. **BUG-9.**
4. Budi uninstalls the app after the game ends with 0 correct hits.

---

### Sessions 7–8 — Speed-Runner ("Dewi", 10+ games played, Level 6)

**Trace:**
1. Game starts. First fish spawned immediately by `PickNewTarget` inside `StartGame`.
2. Dewi hits correctly at t=3 s. `OnFishHit` → `LockThrow(delay)` + `Invoke(PickNewTarget, delay+0.8f)`.
   At minimum pacing (many prior correct hits), `delay = 1.0s`. Throw unlocks at t=4.0s.
   Fish spawn at t=4.8s. **0.8-second window where Dewi CAN throw but NO fish exist.**
   Dewi immediately throws at t=4.0s. Spear hits nothing. Zero feedback. Wastes the throw
   and the 1.2 s cooldown. This happens on every round transition. **BUG-3.**
3. Streak builds to 5. Each correct hit: `timeLeft += TimeBonus.ForHit(5) = 1.5s`. After
   20 correct hits, Dewi has earned +30s extra time. With 40s elapsed from a 60s game:
   `timeLeft = 60 - 40 + 30 = 50s`. Bar = 50/60 = 83%. Still calibrated.
4. After 40 correct hits at streak ≥5: `timeLeft = 60 - 58 + 40×1.5 = 62s`.
   `timerBarFill.fillAmount = Mathf.Clamp01(62/60) = 1.0`. Bar pegged at full.
   Countdown text shows "62". Bar and text disagree. Player watching the bar
   thinks time is full; player watching text sees a large number. **BUG-4.**
5. With enough hits, `timeLeft` could reach 120+s. Bar stays at 100% for the remainder of
   the session. The primary pacing visual is effectively disabled for skilled players.
6. `LockThrow` race trace: player throws at t=12s (canThrow=false, CooldownRoutine running).
   Spear hits fish at t=12.4s. `OnFishHit` → `LockThrow(delay)`. `StopAllCoroutines()`
   kills `CooldownRoutine` at t=12.4s (0.4s into its 1.2s wait). `LockRoutine` starts,
   eventually restores `canThrow=true` and shows `spearFake`. **Correct behaviour; no bug.**
   However: if `LockThrow` fires again before `LockRoutine` finishes (impossible with
   pacing guards), the second `StopAllCoroutines` would kill the first `LockRoutine`
   mid-flight, leaving `canThrow=false` and `spearFake` hidden. Latent risk only.

---

### Sessions 9–10 — Colour-Blind Player ("Andi", ColourBlindSettings enabled)

**Trace:**
1. `ColourBlindSettings.IsEnabled() = true`. Fish spawn. `FishShapeOverlay.Start()` runs.
   Reads `fishTarget.fishColor`. `ColourBlindSettings.ShapeForColor(fishColor)`:
   - `Color.red` → `FF0000` → `"●"`. Correct.
   - If `FishCatalog` assigns `new Color(0.9f, 0.6f, 0.2f)` (a custom orange species):
     `ColorUtility.ToHtmlStringRGB` → `"E6993F"`. Switch default → `"?"`.
     **Every non-palette-base species shows `"?"` in colour-blind mode. BUG-5.**
2. Target indicator: `targetColorImage` shows the colour swatch, `targetColorLabel` shows
   "Merah". **No shape symbol appears in the target indicator.** Andi knows the fish
   are marked ●▲■★ but cannot tell from the HUD which shape to aim for. **BUG-7 (shape).**
3. `targetColorLabel` is set BEFORE `SpawnFish` and the species override:
   ```
   targetColorLabel.text = ToIndonesian(targetColor);  // set with palette pick
   fishSpawner.SpawnFish(targetColor, activeColors);
   if (CurrentTargetSpecies != null)
       targetColor = targetSpecies.baseColor;           // image updated, label NOT updated
   ```
   If species `baseColor` differs from the palette pick, text and swatch disagree. **BUG-6.**
4. If species colour is not in `Dict.cs` (only 20 entries; most custom species colours
   will miss), `ToIndonesian` falls back to the raw hex string. Player sees `"E6993F"` as
   the target label. **UX-4.**

---

## Bug Register

### BUG-1 — CRITICAL: DailyChallenge discards AddXp level-up return — reward never applied
**File:** `DailyChallenge.cs:58`

```csharp
ProgressionStore.AddXp(xpAwarded);  // return value (new level, or 0) discarded
```

`AddXp` returns the new level when a level-up occurs, 0 otherwise. `DailyChallenge` discards
it. `TombakanOnboarding` detects the level-up via a before/after `GetLevel()` comparison and
shows the correct toast text (Week 6 partial fix) — but `GameManager.ApplyLevelReward` is
never called. Level rewards configured in `LevelRewardTable` (species unlock, spear skin,
coin bonus) are silently skipped on every daily-bonus level-up.

**Confirmed BACKLOG carry-over:** "DailyChallenge.cs:58 reward grant still skipped" (Week 7 candidates).

**Fix:** Capture the return value in `DailyChallenge.TryClaimDailyBonus` and expose it via
an `out int newLevel` parameter. Call `ApplyLevelReward` in `TombakanOnboarding` when `newLevel > 0`.

---

### BUG-2 — HIGH: AchievementChecker discards AddXp level-up return — achievement XP level-up silent
**File:** `AchievementChecker.cs:100`

```csharp
if (achievement != null && achievement.xpReward > 0)
    ProgressionStore.AddXp(achievement.xpReward);  // return value discarded
```

If achievement XP pushes the player past a level boundary, no `levelUpPanel` is shown and
no reward from `LevelRewardTable` is applied. Scenario: player ends game at Level 4, earns
350 XP from game (stays Level 4), then unlocks `combo_5` achievement for 50 XP bonus →
crosses Level 5 threshold → silent level-up, no reward.

**Fix:** Capture the return value. If `> 0`, call `StartCoroutine(ShowLevelUp(...))` or
expose a one-shot `ApplyLevelReward` path that does not require being inside `EndGame`.

---

### BUG-3 — HIGH: 0.8-second gap between throw-unlock and fish spawn
**File:** `GameManager.cs:485-486`

```csharp
if (spearThrower) spearThrower.LockThrow(delay);        // player can throw after `delay`
Invoke(nameof(PickNewTarget), delay + 0.8f);             // fish appear 0.8 s later
```

After a correct hit, the throw is unlocked `delay` seconds later, but new fish do not spawn
until `delay + 0.8f`. The 0.8 s gap exists on every round transition at every pacing level.
A player who throws immediately when unlocked fires a spear into empty water: `SpearHit`
finds no colliders, nothing happens, no feedback, and the 1.2 s cooldown is wasted.

**Fix:** Extend the lock to cover the full gap: `LockThrow(delay + 0.8f)`, so the throw
unlocks at the same time fish appear. Or keep the timing as-is and trigger a brief "waiting
for fish" visual indicator when the lock expires before fish spawn.

---

### BUG-4 — HIGH: Timer bar saturates when time-bonus pushes timeLeft above gameDuration
**File:** `GameManager.cs:162`

```csharp
timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / gameDuration);
```

`timeLeft += TimeBonus.ForHit(comboStreak)` adds 0.5–1.5 s per correct hit (uncapped).
Once `timeLeft > gameDuration (60 s)`, the bar clamps at 1.0 and is visually indistinct
from "full" at the start of the game. Countdown text and bar disagree. Skilled players
(streak ≥5, 20+ hits) reliably trigger this: 20 hits × 1.5 s = +30 s earned, easily
pushing `timeLeft` above 60 s. The primary timing visual is disabled for exactly the
players who most need pacing feedback.

**Fix (simple):** `timeLeft = Mathf.Min(timeLeft + bonus, gameDuration * 2f)` and remap
the bar to the capped range, or show a distinct "bonus time" glow when `timeLeft > gameDuration`.
**Fix (minimal):** `timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / Mathf.Max(timeLeft, gameDuration))`
— always calibrates to the current `timeLeft` as the denominator.

---

### BUG-5 — MEDIUM: ColourBlindSettings.ShapeForColor returns "?" for non-palette species colours
**File:** `ColourBlindSettings.cs:22-27`

```csharp
return hex switch
{
    "FF0000" => "●",
    "00FF00" => "▲",
    "0000FF" => "■",
    "FFFF00" => "★",
    _        => "?",   // all other colours
};
```

`FishSpecies.baseColor` is a free-form `Color` field. The starter catalog has 8 species
(BACKLOG); any species with a custom colour outside the four hard-coded hex values produces
`"?"` on the shape overlay. With a realistic catalog, the majority of species will fail this.
The fallback `"?"` is ambiguous: all non-palette species look identical in colour-blind mode.

**Fix:** Expand the switch to cover all `FishSpecies.baseColor` values in the catalog, or
derive the shape from the species' position in the catalog (e.g., species index mod 4) rather
than from the colour.

---

### BUG-6 — MEDIUM: targetColorLabel stale after species colour override
**File:** `GameManager.cs:405-420`

```csharp
// 1. Label set from palette pick:
if (targetColorLabel != null)
    targetColorLabel.text = ColorHexLocalization.ToIndonesian(targetColor);

fishSpawner.SpawnFish(targetColor, activeColors);

// 2. Image updated to species colour — label NOT updated:
if (fishSpawner.CurrentTargetSpecies != null)
{
    targetColor = fishSpawner.CurrentTargetSpecies.baseColor;
    targetColorImage.color = targetColor;
    // targetColorLabel.text is still showing the palette colour name
}
```

When a catalog is active and the species `baseColor` differs from the palette pick,
`targetColorImage` and `targetColorLabel` show different colours. If the species colour
is absent from `Dict.cs`, the label also degrades to a raw hex string.

**Fix:** After the species override block, re-run
`targetColorLabel.text = ColorHexLocalization.ToIndonesian(targetColor)`. If a catalog is
active, prefer showing `fishSpawner.CurrentTargetSpecies.displayName` (which `targetSpeciesLabel`
already shows) and suppress `targetColorLabel` entirely to avoid duplication.

---

### BUG-7 — MEDIUM: Colour-blind target indicator shows no shape symbol
**File:** `GameManager.cs` (PickNewTarget), `FishShapeOverlay.cs`

`FishShapeOverlay` correctly stamps shapes on individual fish when colour-blind mode is on.
The target indicator panel (targetColorImage + targetColorLabel) never shows which shape
corresponds to the target. A player who cannot distinguish colours sees fish with shapes
(●▲■★) but cannot determine which shape to aim for from the HUD alone.

**Fix:** In `PickNewTarget`, when `ColourBlindSettings.IsEnabled()`, append the shape to
the target label: `targetColorLabel.text = $"{ColorHexLocalization.ToIndonesian(targetColor)} {ColourBlindSettings.ShapeForColor(targetColor)}"`.
Better: add a dedicated `TMP_Text targetShapeLabel` GameObject to the HUD panel.

---

### BUG-8 — MEDIUM: GoalManager.ForceCompleteGoal before StartCoaching risks NRE / index crash
**File:** `TombakanOnboarding.cs:30`, `GoalManager.cs:196-213`

For returning players, `TombakanOnboarding.Start()` calls `goalManager.ForceCompleteGoal()`
without first calling `GoalManager.StartCoaching()`. `GoalManager.m_OnboardingGoals` is
declared as `Queue<Goal> m_OnboardingGoals;` — null until `StartCoaching()` populates it.
`CompleteGoal()` dereferences `m_OnboardingGoals.Count` → `NullReferenceException` if null.
It also accesses `m_StepList[m_CurrentGoalIndex - 1]` → `IndexOutOfRangeException` if the
step list is empty or not wired in the Inspector.

**Reproduction:** Returning player (non-zero score/XP) launches app on a fresh session
where no other script has called `StartCoaching()`.

**Fix:** Guard in `ForceCompleteGoal`: `if (m_OnboardingGoals == null) return;`. Or initialise
the queue to `new Queue<Goal>()` in `GoalManager.Awake()`.

---

### BUG-9 — LOW-MEDIUM: ShowSad displays "-25!" even when ClampScore absorbed the full deduction
**File:** `GameManager.cs:507`

```csharp
void ShowSad()
{
    sadFeedback.SetActive(true);
    sadFeedbackText.text = $"-{penaltyPerWrongHit}!";  // always shows -25
    // ...
}
```

`ClampScore(score - penaltyPerWrongHit)` correctly prevents negative scores. But `ShowSad`
is called unconditionally and always displays `"-25!"`. When the player's score is already 0,
the visible score does not change, yet the feedback says 25 points were deducted. Players
at zero score experience repeated "-25!" messages with no score movement, which reads as
a bug (the game "keeps lying to me").

**Fix:** In `OnFishHit`, compute `int actualDeduction = score - ClampScore(score - penaltyPerWrongHit)`
and pass it to `ShowSad`. Show `"-{actualDeduction}!"` or `"Salah!"` when `actualDeduction == 0`.

---

### BUG-10 — LOW-MEDIUM: No feedback on complete miss (spear hits empty water)
**File:** `SpearHit.cs`

When the spear expires without touching any fish collider, `SpearHit.CheckFishHit` never
fires, `GameManager.OnFishHit` is never called, and no UI/audio/haptic response occurs.
From the player's perspective the game simply does nothing. First-timers interpret this as
a bug in touch recognition or game state.

**Fix:** Detect "spear expired without hit" either in `SpearThrower.CooldownRoutine` (after
the spear lifetime) or by adding an `OnDestroy` to `SpearHit` that checks `!hasHit` and
triggers a "miss" SFX + brief floating "Meleset!" text near the spear's last position.

---

## Week 6 Fixes — Verification

| Fix | Status |
|-----|--------|
| `OnFishHit` guard — `if (!gameRunning) return` at `GameManager.cs:440` | CONFIRMED FIXED — mid-flight spear no longer corrupts score after EndGame |
| `HapticFeedback` decoupled from audio mute | CONFIRMED FIXED — `HapticFeedback.cs` has no reference to `AudioPrefs` or `AudioManager` |
| Achievement toast sequenced via `ShowAchievementsSequenced` coroutine | CONFIRMED FIXED — toast shows at +1.2 s, null-guarded |
| Result celebrations staggered (+0.4 s badge, +0.8 s level-up) | CONFIRMED FIXED — `StaggerResultCelebrations` coroutine works correctly |
| Daily-bonus level-up surfaced in `TombakanOnboarding` greeting text | CONFIRMED (partial) — UI text is correct; reward application still missing (BUG-1) |

---

## UX Friction

### UX-1 (Known, still open) — No throw-mechanic tutorial for first-time players
After water placement no hint explains how to throw the spear. `TombakanOnboarding` has no
game-mechanic coaching step. First-timers learn by trial and error only.
**Fix:** Show a timed hint panel `"Tekan tombol untuk melempar tombak!"` for 8 s on `StartGame()`;
add as a step in `TombakanOnboarding` gated by `!isReturningPlayer`.

### UX-2 — greetingPanel and dailyBonusPanel can both show simultaneously
`TombakanOnboarding.Start()` shows `greetingPanel` for new players, then unconditionally
checks the daily bonus. On a new player's very first launch, if the daily bonus happens to
also claim (e.g., device date is new day after initial install), both panels activate on the
same frame with no mutual exclusion. On small phones they overlap.
**Fix:** Trigger the daily-bonus check only after `greetingPanel` is dismissed, inside
`DismissGreeting()` or at the start of the next `Update` frame.

### UX-3 — Achievement toast fires 0.4 s after level-up panel, interrupting reading
Level-up panel at +0.8 s; first achievement toast at +1.2 s. Player has only 0.4 s to read
the level-up text before the toast overlaps it. If multiple achievements fire, the panel and
toasts compete for the same screen area for up to 5+ seconds.
**Fix:** Delay toast start to +3.0 s (after level-up panel has been readable for 2+ s),
or add tap-to-dismiss to the level-up panel.

### UX-4 — Dict.cs hex fallback shows raw hex string as target label
`ColorHexLocalization.ToIndonesian` falls back to the raw hex string when a species colour
is not in the 20-entry map. Players see `"E6993F"` as the target label for custom-coloured
species. The `targetSpeciesLabel` already shows the Indonesian species name; the colour label
is redundant and harmful in catalog mode.
**Fix:** When `fishSpawner.catalog != null` and `CurrentTargetSpecies != null`, suppress
`targetColorLabel` entirely (or set it to the species `displayName`) rather than showing a
potentially-unrecognised colour name.

---

## Summary Table

| ID | Severity | File | One-Line Description |
|----|----------|------|----------------------|
| BUG-1 | CRITICAL | `DailyChallenge.cs:58` | AddXp return discarded — daily level-up reward not applied |
| BUG-2 | HIGH | `AchievementChecker.cs:100` | AddXp return discarded — achievement XP level-up silent, no reward |
| BUG-3 | HIGH | `GameManager.cs:485-486` | 0.8s throw-unlock/fish-spawn gap — empty-water throws with zero feedback |
| BUG-4 | HIGH | `GameManager.cs:162` | Timer bar saturates at 100% when combo bonuses push timeLeft > gameDuration |
| BUG-5 | MEDIUM | `ColourBlindSettings.cs:22-27` | ShapeForColor returns "?" for any non-palette species colour |
| BUG-6 | MEDIUM | `GameManager.cs:415-420` | targetColorLabel not updated after species colour override |
| BUG-7 | MEDIUM | `GameManager.cs` / HUD | Colour-blind target indicator has no shape symbol |
| BUG-8 | MEDIUM | `TombakanOnboarding.cs:30` | ForceCompleteGoal before StartCoaching → NRE / index crash risk |
| BUG-9 | LOW-MED | `GameManager.cs:507` | ShowSad shows "-25!" even when clamp absorbed the deduction |
| BUG-10 | LOW-MED | `SpearHit.cs` | No miss feedback when spear hits empty water |
| UX-1 | UX | `TombakanOnboarding.cs` | No throw-mechanic tutorial for first-time players (known) |
| UX-2 | UX | `TombakanOnboarding.cs` | greetingPanel and dailyBonusPanel can show simultaneously |
| UX-3 | UX | `GameManager.cs:344,354` | Achievement toast fires 0.4 s after level-up panel, overlapping |
| UX-4 | UX | `GameManager.cs`, `Dict.cs` | Hex-string fallback shown as target label for custom species colours |

---

## Week 8 Recommendations

**Ship-blocking (one-line fixes):**
- BUG-1: Capture `ProgressionStore.AddXp` return in `DailyChallenge.TryClaimDailyBonus`;
  surface new level to `TombakanOnboarding` to call `ApplyLevelReward`.
- BUG-2: Same pattern in `AchievementChecker.cs:100`.
- BUG-8: Add `if (m_OnboardingGoals == null) return;` guard in `GoalManager.CompleteGoal`.

**High priority:**
- BUG-3: Extend `LockThrow` duration to `delay + 0.8f` or reduce `PickNewTarget` delay
  so fish are ready before the lock expires.
- BUG-4: Cap or remap `timerBarFill.fillAmount` when `timeLeft > gameDuration`.
- BUG-7: Add shape symbol to the target indicator panel when colour-blind mode is on.
- BUG-7 / BACKLOG: Implement "Re-position water" button (already in Week 7 candidates).

**Next sprint:**
- BUG-5: Expand `ShapeForColor` to cover all catalog species colours, or derive shape from
  species index.
- BUG-6: Re-run `targetColorLabel.text` after species colour override; suppress in catalog mode.
- BUG-9: Show `"Salah!"` instead of `"-25!"` when score is already 0.
- BUG-10: Add miss SFX triggered at spear lifetime expiry via `SpearHit.OnDestroy`.
- UX-1: Throw-mechanic tutorial hint (Week 7 candidate, still unimplemented).

---

## Validation Week 8

**Date:** 2026-08-10
**Re-tester:** AI Game Tester
**Input:** `ITERATION_week8_SCOPE.md` (4 tasks, 7 bugs addressed)
**Commit reviewed:** `7f4fddb` (TASK-01, TASK-02 committed); TASK-03 and TASK-04 in working tree, uncommitted.

| Task ID | Bug IDs | Status | Evidence |
|---------|---------|--------|----------|
| TASK-01 | BUG-1 (CRITICAL) | PASS | `DailyChallenge.TryClaimDailyBonus` signature changed to `(out int xpAwarded, out int streak, out int newLevel)`; line 71 captures `ProgressionStore.AddXp(xpAwarded)` into `newLevel` instead of discarding it. `TombakanOnboarding.Start` line 40 uses the new out-param and calls `GameManager.I?.ApplyLevelReward(newLevel)` at line 44 when `newLevel > 0`. Player session trace "Siti" (Day 3, Level 2, 95 XP): `TryClaimDailyBonus` out-param `newLevel` receives the new level from `AddXp(175)`; `ApplyLevelReward` is called; `LevelRewardTable` reward applied. Previous before/after `GetLevel()` workaround removed. |
| TASK-01 | BUG-2 (HIGH) | PASS | `AchievementChecker.cs` line 103: `int newLevel = ProgressionStore.AddXp(achievement.xpReward)` — return value captured. Line 105: `if (newLevel > 0) GameManager.I?.ApplyLevelReward(newLevel)` — level-up panel shown and reward applied. Player session trace "Siti" end-game: `first_catch` achievement awards 50 XP; if that crosses a level boundary, `ApplyLevelReward` fires with the new level. Public `ApplyLevelReward(int newLevel)` overload added to `GameManager` (line 383) so `AchievementChecker` can call it without a MonoBehaviour dependency. |
| TASK-02 | BUG-8 (MEDIUM) | PASS | `GoalManager.CompleteGoal` line 200: `if (m_OnboardingGoals == null) return;` guard added before any dereference of `m_OnboardingGoals`. Player session trace "Siti" fresh session: `TombakanOnboarding.Start` calls `ForceCompleteGoal` before `StartCoaching`; queue is null; guard returns immediately — no `NullReferenceException`, no `IndexOutOfRangeException`. |
| TASK-02 | BUG-3 (HIGH) | PASS | `GameManager.OnFishHit` lines 518-519: `spearThrower.LockThrow(delay + GameConstants.SpawnDelay)` and `Invoke(nameof(PickNewTarget), delay + GameConstants.SpawnDelay)` use the same `0.8 f` constant (`GameConstants.SpawnDelay = 0.8f` added). Throw unlock and fish spawn are simultaneous. Player session trace "Dewi" (Speed-Runner): after a correct hit at minimum pacing (`delay = 1.0 s`), throw unlocks at `t + 1.8 s`; fish spawn at `t + 1.8 s`; zero-fish window eliminated. |
| TASK-03 | BUG-4 (HIGH) | PARTIAL | `GameManager.cs` line 165 (uncommitted): formula changed to `Mathf.Clamp01(timeLeft / Mathf.Max(timeLeft, gameDuration))`. Code matches the SCOPE specification. However, the formula is mathematically equivalent to the original `Clamp01(timeLeft / gameDuration)` for all positive values: when `timeLeft > gameDuration`, `Max(timeLeft, gameDuration) = timeLeft`, so the result is always `1.0`, identical to the original. The `Week8HUDTests.cs` comment (line 60) explicitly confirms: "same observable value." At `timeLeft = 80, gameDuration = 60`, fill = 1.0 and countdown text = "80" — bar and text still disagree. Bar remains visually stuck at 100% for the full bonus-time window. The tests (monotonic decrease, extreme bonus cap, smooth decay) pass for the formula, but the acceptance criterion "bar and countdown text always agree" is not met. Changes also uncommitted. |
| TASK-03 | BUG-6 (MEDIUM) | PASS | `GameManager.PickNewTarget` (uncommitted): `targetColorLabel.text` assignment moved to after the species override block. When `fishSpawner.CurrentTargetSpecies != null`, label uses `fishSpawner.CurrentTargetSpecies.displayName` (not the palette hex-lookup). When species is null, falls back to `ColorHexLocalization.ToIndonesian(targetColor)`. Label and swatch now resolve from the same `targetColor` value. Player session trace "Andi" (colour-blind): species override sets `targetColor = species.baseColor`; label then reads `species.displayName`; label and swatch agree. |
| TASK-03 | BUG-7 (MEDIUM) | PASS | `GameManager.PickNewTarget` (uncommitted): `if (ColourBlindSettings.IsEnabled()) labelText = $"{labelText} {ColourBlindSettings.ShapeForColor(targetColor)}";` appends the shape symbol. Player session trace "Andi": colour-blind mode enabled; target is Red; HUD shows "Merah ●" (or species name + ●). Shape symbol now visible in the target indicator alongside the colour name. |
| TASK-04 | BUG-10 (LOW-MED) | PASS | `Assets/Prefabs/UI/MissText.prefab` exists (uncommitted, untracked `Assets/Prefabs/` directory). Prefab structure confirmed from YAML: root GameObject `MissText` with `Animator` component (controller GUID `a9b4e7f5...`); child `Canvas` GameObject with `Canvas` (`m_RenderMode: 2` = World Space), `CanvasGroup` (alpha=1, for fade-out); grandchild `MissText_Label` with `TMP_Text` displaying `"Meleset!"`, warm-orange fill `(r:1, g:0.447, b:0.098)`, font size 60, bold+italic. Root scale starts at `(x:0, y:0, z:1)` consistent with scale-from-zero animation. Canvas world scale `0.001` gives legible size at ~1 m AR distance. No null component references found in YAML. Acceptance criteria satisfied: prefab exists at the exact path, World Space Canvas confirmed, "Meleset!" text present, structure supports the animator scale-in / hold / fade-out sequence. |

### Validation Notes

**Commit coverage:** TASK-01 and TASK-02 are fully committed in `7f4fddb`. TASK-03 (`GameManager.cs` diff) and TASK-04 (`Assets/Prefabs/` directory and `Week8HUDTests.cs`) exist only in the working tree and are not committed. These should be committed before merging.

**BUG-4 — timer bar:** The formula change from `Clamp01(timeLeft / gameDuration)` to `Clamp01(timeLeft / Mathf.Max(timeLeft, gameDuration))` produces identical runtime output. Both formulas give `1.0` for any `timeLeft >= gameDuration`. The stated fix is semantically cleaner (denominator adapts to the current ceiling) but does not change any observable pixel. The bar-text disagreement ("80" text vs full bar) and the "primary pacing visual disabled for skilled players" symptom from the Week 7 report persist. Recommend a follow-on fix: cap `timeLeft` at `gameDuration * 1.5f` on addition, or remap the bar to show a "bonus glow" state when `timeLeft > gameDuration`.

**BUG-5 (not in scope):** `ColourBlindSettings.ShapeForColor` still returns `"?"` for non-palette species colours. `Week8HUDTests.cs` confirms this is expected (test `ShapeForColor_UnknownColor_ReturnsQuestionMark`). Carry forward to Week 9.
