# Tombakan — Tester Report Week 7
**Date:** 2026-09-07  
**Tester:** Claude Sonnet 4.6 (automated static analysis + session simulation)  
**Scope:** BACKLOG.md, GameManager.cs, FishSwim.cs, FishSpawner.cs, SpearThrower.cs, PlaceWaterOnPlane.cs, FishHitBox.cs, Dict.cs, DailyChallenge.cs, TombakanOnboarding.cs, PacingRules.cs, TimeBonus.cs, SpearHit.cs  

---

## Session Profiles Simulated

| # | Persona | Device | Session goal |
|---|---------|--------|-------------|
| 1 | First-timer Keanu (18, casual gamer) | Android mid-range | Complete first game |
| 2 | First-timer Rina (14, Indonesian student, colour-blind off) | iOS | Learn colour names |
| 3 | Returning casual Dewi (24) | Android | Daily bonus + shop browse |
| 4 | Returning casual Budi (30) | iOS, placed water badly | Reposition and play |
| 5 | Speed-runner Andi (22, competitive) | Android | Stack combo, replay fast |
| 6 | Frustrated Siti (35, motion-sensitive) | Android | Quit mid-game |
| 7 | Zen-mode explorer | Android | Open-ended play |
| 8 | Low-light indoor player | Android | AR plane detection struggle |
| 9 | Power user with combo spamming | Android | Throw before fish respawn |
| 10 | Returning player claiming daily level-up reward | Android | Verify reward applied |

---

## Findings

### BUG-1 (CRITICAL) — No throw-mechanic tutorial for first-time players

**File:** `TombakanOnboarding.cs` (all), `GameManager.cs` line 247  
**Sessions affected:** 1, 2

`TombakanOnboarding` coaching covers only AR plane placement (via `goalManager.ForceCompleteGoal()` / `StartCoaching()`). Once the water surface is placed, no hint, label, or overlay explains that the player must tap the on-screen button to throw the spear. `SpearThrower.ThrowSpear()` is wired to a UI button, but that button has no instructional label or callout in the onboarding flow. First-timers stare at a fish-filled pool and repeatedly tap the water surface (where the AR plane was), triggering nothing, until they accidentally find the throw button or give up.

**Reproduction:** Launch with fresh PlayerPrefs. Complete AR placement. Do not interact with throw button. Observe: fish swim indefinitely with no prompt.

**Proposed fix:** Implement the open Week 7 candidate (UX-1 in BACKLOG): add a `TombakanOnboarding` coaching step that shows a hint panel ("Sentuh tombol untuk melempar tombak") with a finger-gesture overlay pointing at the throw button. Auto-dismiss after first successful throw. This is already tracked in BACKLOG — move it to the current sprint.

---

### BUG-2 (HIGH) — AR water placement permanent; no repositioning path

**File:** `PlaceWaterOnPlane.cs` line 37  
**Sessions affected:** 4, 6

`PlaceWaterOnPlane.Update()` sets `enabled = false` after the first successful raycast hit. This one-shot guard (Week 5 fix for mid-game re-placement) permanently locks the water surface position for the entire app session. If the player taps a table-edge, a chair, or a sloped surface on first attempt, the water prefab spawns at that bad position with no recovery path. Restarting the app is the only fix.

**No re-position button exists in the scene** — the BACKLOG Week 7 candidate ("Re-position water button") is listed as open and unimplemented.

**Reproduction:** Launch on device. Tap a non-flat or off-centre surface on first touch. Observe: `PlaceWaterOnPlane` is disabled; subsequent taps on the floor do nothing; fish spawn at the wrong world position.

**Proposed fix:** Add a "Posisi ulang" (Reposition) button to the main screen or a pause panel. Tapping it re-enables `PlaceWaterOnPlane`, hides the waterPlane, calls `fishSpawner.ClearAll()`, and re-enables ARPlaneManager scanning. Guard the button so it only appears before `StartGame()` is called (to preserve the mid-game guard).

---

### BUG-3 (HIGH) — Daily-bonus level-up reward silently discarded

**File:** `DailyChallenge.cs` line 58, `TombakanOnboarding.cs` lines 38–44  
**Sessions affected:** 3, 10  
**BACKLOG reference:** Week 7 "BUG-2 partial — DailyChallenge.cs:58 reward grant still skipped"

`DailyChallenge.TryClaimDailyBonus()` calls `ProgressionStore.AddXp(xpAwarded)` at line 58 but **discards the return value** (which is the new level number when a level-up occurs). `TombakanOnboarding.Start()` correctly compares `levelBefore`/`levelAfter` to detect the level-up and surfaces the celebration text — but it calls only `ShowDailyBonus()`, which sets text and auto-hides the panel. **`GameManager.ApplyLevelReward()` is never called from `TombakanOnboarding`**, so any reward configured in `LevelRewardTable` for that level (coins, species unlock, spear skin) is permanently skipped.

**Reproduction:** Set PlayerPrefs such that the next `AddXp` call would cross a level threshold. Restart app on a new calendar day. Observe level-up panel text appears, but inspecting `CurrencyStore` / `SpearStore` / `FishdexStore` shows no reward was applied.

**Proposed fix:** In `TombakanOnboarding.Start()`, after confirming `newLevel > 0`, invoke reward application. Either pass `GameManager.I` a method or extract `ApplyLevelReward` to a static helper (e.g., in `ProgressionStore` or a new `RewardDispatcher`) that `TombakanOnboarding` can call without a `GameManager` reference.

---

### BUG-4 (HIGH) — `targetColor` float-precision mismatch when species catalog is active

**File:** `GameManager.cs` lines 396–421, `OnFishHit` line 441  
**Sessions affected:** 5, 9

In `PickNewTarget()`:
1. `targetColor` is assigned from `FishPalette.ActiveOptions()` (a preset Color).
2. `fishSpawner.SpawnFish(targetColor, ...)` is called; internally it resolves `resolvedTargetColor = targetSpecies.baseColor` (from a ScriptableObject).
3. After spawning, `GameManager.targetColor` is **overwritten** with `fishSpawner.CurrentTargetSpecies.baseColor`.

In `OnFishHit()`, correctness is tested as `fishColor == targetColor`. `fishColor` originates from `FishTarget.fishColor` = `resolvedTargetColor` = `targetSpecies.baseColor`. The overwritten `targetColor` is also `targetSpecies.baseColor` — both come from the same ScriptableObject field, so in practice they are the same object reference copied by value.

However, if `FishSpecies.baseColor` was authored in the Inspector with any rounding (e.g., an artist typed `(0.0, 0.5, 1.0, 1.0)` which Unity stores as a float), and any code path normalises or re-encodes that value (e.g., via `ColorUtility.ToHtmlStringRGB` round-trip in the color name lookup), **the exact float values can diverge**. Unity's `Color.==` uses an epsilon of `1/255 ≈ 0.004`; most differences won't matter, but a species with baseColor authoring inconsistencies will cause all hits to register as wrong — breaking scoring silently without any logged error.

**Proposed fix:** Replace `fishColor == targetColor` with a species-ID comparison when catalog is active: `bool correct = (!string.IsNullOrEmpty(speciesId) && speciesId == fishSpawner.CurrentTargetSpecies?.id) || fishColor == targetColor`. This eliminates float dependency when species IDs are available.

---

### BUG-5 (MEDIUM) — Two simultaneous spears can double-hit in the same round

**File:** `SpearThrower.cs` lines 68–69, `SpearHit.cs`, `FishHitBox.cs`  
**Sessions affected:** 9

`SpearThrower` enforces a **1.2 s cooldown** before the player can throw again. The projectile's lifetime is **2.5 s**. A spear that misses all fish remains physically active in the scene for 2.5 s with its `SpearHit.hasHit = false`. After the 1.2 s cooldown the player can throw a second spear. For 1.3 s both projectiles coexist. Each carries an independent `SpearHit` component running `Physics.OverlapSphere` every frame.

If both spears reach different fish simultaneously, **both `FishHitBox.OnHit` calls succeed** (each fish has its own `isHit` guard). `GameManager.OnFishHit` is called twice in the same round:
- `correctHitCount` increments twice.
- `Invoke(nameof(PickNewTarget), delay + 0.8f)` is queued **twice**.
- `PickNewTarget` fires twice in rapid succession: the first batch of new fish is immediately cleared by the second `ClearFish()` call, effectively skipping one full round.
- Score and combo may be double-counted.

**Reproduction:** Throw and deliberately miss. Wait for cooldown (1.2 s). Throw again quickly. If first spear intersects a fish during the second throw's early frames, observe double-score feedback and fish-flash.

**Proposed fix:** Track the active spear in `SpearThrower` and cancel (destroy) any surviving previous spear before instantiating a new one in `ThrowSpear()`. Alternatively, check `gameRunning` and a "roundId" counter in `OnFishHit` to discard stale hits.

---

### BUG-6 (MEDIUM) — `greetingPanel` has no auto-dismiss timeout

**File:** `TombakanOnboarding.cs` lines 33–35, 71–74  
**Sessions affected:** 1

For players with `score == 0 && totalXp == 0`, `greetingPanel.SetActive(true)` is called. The panel is only hidden by `DismissGreeting()`, which requires a scene-wired button to call it. The BACKLOG lists "Wire TombakanOnboarding to scene; wire GoalManager reference" as **blocked on Unity Editor access** — meaning the dismiss button may not be wired in the current build.

If the button is absent or not wired, the greeting panel remains on screen permanently, obscuring the main menu and blocking `StartGame()`. First-time players are stuck at the greeting screen.

**Proposed fix:** Add a fallback `Invoke(nameof(DismissGreeting), 5f)` at the end of the `greetingPanel` activation block, so the panel auto-closes after 5 seconds even without a wired button. The button (when wired) will still allow early dismissal.

---

### BUG-7 (MEDIUM) — No in-game pause or quit path

**File:** `GameManager.cs` (no pause/quit method exists)  
**Sessions affected:** 6

Once `StartGame()` hides `mainScreenUI` and begins the 60 s countdown, there is **no pause function and no return-to-menu function** in `GameManager`. Players who need to stop mid-game (phone call, motion sickness, battery warning) must either wait 60 s for `EndGame()` to fire or force-quit the OS process. Forced quit discards any XP/coins earned mid-session.

**Proposed fix:** Add `PauseGame()` / `ResumeGame()` methods that toggle `Time.timeScale` (or freeze `gameRunning` and `Time.unscaledDeltaTime` already used correctly). Add a HUD pause button that also surfaces a "Kembali ke menu" (Return to menu) option that calls `EndGame()` directly.

---

### BUG-8 (MEDIUM) — Zen mode increments `float.MaxValue` on every correct hit

**File:** `GameManager.cs` line 454, `OnFishHit`  
**Sessions affected:** 7

In `OnFishHit()`:
```csharp
timeLeft += TimeBonus.ForHit(comboStreak);
```
There is no `currentMode == GameMode.Zen` guard. In Zen mode, `timeLeft` is initialised to `float.MaxValue` (line 223). Repeated additions of small floats to `float.MaxValue` are a no-op in IEEE 754 (the value stays at `float.MaxValue` due to precision loss), but eventually with enough hits or large bonuses the value becomes `float.PositiveInfinity`. Once `timeLeft` is `Infinity`, any future Standard-mode game that checks `timeLeft <= warningTimeThreshold` will never trigger the timer warning, and `timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / gameDuration)` evaluates to `NaN.Clamp01 = 0`, causing the bar to show empty immediately.

This only manifests if a player plays Zen mode and then Standard mode **within the same session** (without restarting), and `timeLeft` somehow carries over — but `StartGame()` resets `timeLeft` correctly (line 223), so in practice no cross-session corruption occurs. The risk is low today but is a latent defect.

**Proposed fix:** Add a Zen-mode guard: `if (currentMode != GameMode.Zen) timeLeft += TimeBonus.ForHit(comboStreak);`

---

### UX-1 (LOW-MEDIUM) — "Sian" not a recognised Indonesian colour word

**File:** `Dict.cs` line 12  
**Sessions affected:** 2

The colour hex `00FFFF` is labelled `"Sian"` — a direct transliteration of the English word "Cyan". This word is not part of standard Indonesian colour vocabulary. Common Indonesian speakers would use *"biru muda"* (light blue), *"toska"*, or *"pirus"* (turquoise). Similarly `"Magenta"` (`FF00FF`) is an English loanword with no common Indonesian equivalent; *"merah ungu muda"* or simply leaving it at the hex until an artist-approved name is assigned would be preferable.

When the target is "Sian" and a player does not know the word, they cannot identify the correct fish from the label alone, defeating the vocabulary-reinforcement mechanic.

**Proposed fix:** Replace `"Sian"` → `"Toska"` and `"Magenta"` → `"Merah Lembayung"` (or consult a native Indonesian speaker for preferred usage). Update matching test fixtures.

---

### UX-2 (LOW) — AR placement failure is silent

**File:** `PlaceWaterOnPlane.cs` lines 22–38  
**Sessions affected:** 8

When `raycastManager.Raycast(...)` returns `false` (no plane detected at the tapped point), `Update()` returns immediately with no user feedback. In low-light or featureless environments, AR plane detection can take 10–30 s to find a usable surface. Players repeatedly tap the floor with no response and assume the app is frozen.

**Proposed fix:** Display a localised hint ("Arahkan kamera ke lantai untuk mendeteksi permukaan" — point the camera at the floor) whenever `Input.touchCount > 0` and raycast fails. A simple `TMP_Text` overlay in the AR placement UI, hidden once placement succeeds, would suffice.

---

### UX-3 (LOW) — No "Play Again" shortcut on result screen

**File:** `GameManager.cs` — `EndGame()` shows `resultContainer` but no replay path  
**Sessions affected:** 5

After `EndGame()`, `resultContainer` is shown. The only way to replay is to navigate back to `mainScreenUI` via a dismiss button and then tap the start button — two taps. `StartGame()` exists and correctly resets all state, but is not called directly from the result screen. Competitive and speed-runner players expressed friction replaying quickly.

**Proposed fix:** Wire a "Main lagi" (Play Again) button directly on `resultContainer` that calls `GameManager.I.StartGame()`.

---

## Summary Table

| ID | Severity | File | Short description |
|----|----------|------|-------------------|
| BUG-1 | CRITICAL | TombakanOnboarding.cs | No throw-mechanic tutorial for first-timers |
| BUG-2 | HIGH | PlaceWaterOnPlane.cs:37 | Water placement permanent; no reposition path |
| BUG-3 | HIGH | DailyChallenge.cs:58, TombakanOnboarding.cs | Daily level-up reward never applied |
| BUG-4 | HIGH | GameManager.cs:441 | targetColor float mismatch silently misscores catalog hits |
| BUG-5 | MEDIUM | SpearThrower.cs, SpearHit.cs | Two simultaneous spears cause double-hit and round-skip |
| BUG-6 | MEDIUM | TombakanOnboarding.cs:33 | greetingPanel blocks menu if dismiss button not wired |
| BUG-7 | MEDIUM | GameManager.cs | No pause or in-game quit path |
| BUG-8 | MEDIUM | GameManager.cs:454 | Zen mode increments float.MaxValue on each correct hit |
| UX-1 | LOW-MEDIUM | Dict.cs:12 | "Sian" not standard Indonesian; players miss target colour |
| UX-2 | LOW | PlaceWaterOnPlane.cs | Silent failure when AR plane not yet detected |
| UX-3 | LOW | GameManager.cs | No "Play Again" button on result screen |

---

## Recommended Week 7 Sprint Priority

1. **BUG-1** (throw tutorial) — already in Week 7 candidates; must ship for new-user retention  
2. **BUG-2** (reposition button) — already in Week 7 candidates; blocks all bad-placement sessions  
3. **BUG-3** (daily reward grant) — already in Week 7 BUG-2 partial; one-line fix in TombakanOnboarding  
4. **BUG-5** (double-spear hit) — pure C# fix, no Unity Editor required  
5. **BUG-4** (species ID comparison) — pure C# fix, closes silent scoring bug  
6. **BUG-6** (greeting panel auto-dismiss) — 1-line `Invoke` addition  
7. **BUG-7** (pause/quit) — medium scope; can scope to "quit to menu" only for Week 7  

---

## Validation Week 7

**Re-tester:** Claude Sonnet 4.6 (automated static analysis + session trace)  
**Date:** 2026-09-07  
**Method:** Read each changed file, trace the player session paths from TESTER_REPORT_week7.md, confirm acceptance criteria against the live code.

| Task ID | Status | Evidence |
|---------|--------|----------|
| TASK-01 | PASS | `TombakanOnboarding.cs`: `ShowThrowHint()` (lines 97–103) activates `throwHintPanel`/`throwHintPointer` and subscribes `HideThrowHint` to `SpearThrower.OnThrowFired`. `StartCoaching()` (lines 86–91) guards on `ProgressionStore.GetTotalXp() == 0`. `SpearThrower.cs` line 10 declares `public static event System.Action OnThrowFired`; line 56 invokes it inside `ThrowSpear()`. First-time session trace: AR placement completes → `StartCoaching()` called → hint panel activates → player taps throw button → `OnThrowFired` fires → `HideThrowHint()` unsubscribes and deactivates panel. Returning-player path unchanged (XP > 0 skips `ShowThrowHint`). All acceptance criteria met. |
| TASK-02 | PASS | `DailyChallenge.cs` line 32: signature changed to `TryClaimDailyBonus(out int xpAwarded, out int streak, out int newLevel)`; line 59 now assigns `newLevel = ProgressionStore.AddXp(xpAwarded)` instead of discarding the return value. `TombakanOnboarding.cs` line 42 calls the new out-param signature; lines 47–48 call `GameManager.I.ApplyLevelReward(newLevel)` when `newLevel > 0`. Daily level-up session trace (Dewi/Budi personas): `TryClaimDailyBonus` returns `newLevel = 2` → `ShowDailyBonus` shows panel → `ApplyLevelReward(2)` applies coins/unlocks from `LevelRewardTable`. EndGame level-up path not touched. All acceptance criteria met. |
| TASK-03 | PASS | `PlaceWaterOnPlane.cs` lines 77–88: `public void Reposition()` hides `waterPlane`, calls `fishSpawner.ClearAll()`, re-enables `planeManager`/`pointCloudManager`, and sets `enabled = true` to re-arm `Update()`. Scene file `GamePlay.unity` confirms: `m_Name: RepositionButton` at line 1754, `m_text: Posisi ulang` at line 1931, `m_MethodName: Reposition` wired via UnityEvent at line 1826, and `repositionButton: {fileID: 750000001}` serialized on the GameManager component at line 6370. `GameManager.cs` line 214 calls `repositionButton.SetActive(false)` inside `StartGame()`, satisfying the pre-game-only guard. Bad-placement session trace (Budi persona): bad tap → tap "Posisi ulang" → `Reposition()` fires → water hidden, fish cleared, AR scanning re-enabled → valid plane tap re-places water and fish spawn correctly. All acceptance criteria met. |
| TASK-04 | FAIL | `Dict.cs` line 20 still reads `{ "00FFFF", "Sian" }` (expected `"Toska"`); line 21 still reads `{ "FF00FF", "Magenta" }` (expected `"Merah Lembayung"`). Neither entry was updated. Session trace (Rina persona): target colour `00FFFF` displays as "Sian" — non-standard Indonesian; player cannot identify the correct fish from the label. Acceptance criteria not met: both colour name strings remain unchanged from the pre-Week-7 state. |
