# Tester Report — Week 7

Tested: 2026-09-28
Scripts read: GameManager.cs, FishSwim.cs, FishSpawner.cs, SpearThrower.cs,
PlaceWaterOnPlane.cs, FishHitBox.cs, SpearHit.cs, Dict.cs, FishPalette.cs,
TombakanOnboarding.cs, DailyChallenge.cs, ProgressionStore.cs

---

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | No feedback when AR plane not yet scanned — tap does nothing, looks broken | PlaceWaterOnPlane.cs:22-37 | First-timer can't start; silent failure |
| 2 | No throw-mechanic hint after water placement — player stares at fish with timer running | TombakanOnboarding.cs (missing step) | First-timer wastes full 60 s round |
| 3 | SpearHit.OverlapSphere uses fishLayer mask — silent zero-hit if layer unassigned in scene | SpearHit.cs:18 | Spear passes through all fish; core mechanic fails |
| 4 | FishSpawner.ApplyColor only colors first child Renderer | FishSpawner.cs:103 | Multi-mesh fish appear wrong color; color-ID mechanic unreliable |
| 5 | Daily-bonus level-up reward (species/skin/coins) not applied — BACKLOG BUG-2 still present | DailyChallenge.cs:58, TombakanOnboarding.cs:39 | Returning player gets level-up toast but reward is silently skipped |
| 6 | No water re-positioning option — bad first placement requires full app restart | PlaceWaterOnPlane.cs:37 | Every player who mis-taps is permanently stuck |
| 7 | FishHitBox.StickSpear teleports spear to fish center, not impact point | FishHitBox.cs:32 | Visual break: spear snaps instead of sticks |
| 8 | targetColorLabel shows raw hex when species baseColor is not in Dict.cs | Dict.cs:34, FishSpawner.cs:30 | Target label becomes unreadable for catalog-based species |
| 9 | "COMBO!" feedback string is English in otherwise Indonesian UI | GameManager.cs:499 | Breaks tone and language consistency |
| 10 | Timer-bar pulse color can bleed onto result screen for ~0.2 s | GameManager.cs:179-298 | Minor visual glitch on game-over |

---

## Bugs Found

- [ ] **BUG-A: Daily bonus level-up reward silently skipped** — `DailyChallenge.TryClaimDailyBonus` (line 58) calls `ProgressionStore.AddXp(xpAwarded)` and discards the returned new-level value. `TombakanOnboarding.Start()` detects the level change (lines 38-43) and shows the level-up text, but never calls `ApplyLevelReward`. Unlocked species, spear skin, and coin bonus are never granted. **Reproduction**: reach a level boundary, close app, open next day; daily bonus triggers level-up; result screen shows no reward applied. This is BACKLOG BUG-2 partial fix — the `TombakanOnboarding` greeting now surfaces the level number but the reward itself is still not applied.

- [ ] **BUG-B: SpearHit OverlapSphere silently misses all fish when fishLayer is unset** — `SpearHit.cs:18`: `Physics.OverlapSphere(transform.position, hitRadius, fishLayer)`. If `fishLayer` is left at default (0) and fish objects are not on a matching layer, the sphere finds no colliders. No fallback, no log. Spear flies through all fish with zero response. **Reproduction**: in a fresh scene where fish layer has not been set in the Inspector, throw any number of spears; none register a hit.

- [ ] **BUG-C: FishSpawner.ApplyColor only paints first child Renderer** — `FishSpawner.cs:103`: `fish.GetComponentInChildren<Renderer>()` returns a single renderer. A typical low-poly fish model has separate renderers for body, fins, and tail. Only the first in depth-first order is recolored; the rest keep the default material. The target fish may show mixed or mostly grey appearance, making color-based targeting ambiguous. **Reproduction**: use a multi-mesh fish prefab; observe 1 of N meshes colored, rest default.

- [ ] **BUG-D: FishHitBox.StickSpear places spear at fish center, not impact point** — `FishHitBox.cs:32`: `spear.position = transform.position`. This ignores where the spear actually was when `OnHit` fired. The spear teleports to the FishHitBox GameObject's origin. **Reproduction**: throw and observe the spear snap position relative to where it entered the fish.

- [ ] **BUG-E: FishHitBox.hitRadius (0.12 m) and SpearHit.hitRadius (0.1 m) are independent and mismatched** — `FishHitBox.cs:8` declares `hitRadius = 0.12f` and draws the Gizmo with it. `SpearHit.cs:6` uses its own `hitRadius = 0.1f` for the actual overlap check. The Inspector gizmo on FishHitBox misleads when tuning hit size; the effective hit radius is 0.02 m smaller. **Reproduction**: draw Gizmos in Scene view; the red sphere on FishHitBox is larger than the actual detection sphere on SpearHit.

---

## UX Gaps

- [ ] **UX-1: No "scanning" status or feedback during AR plane detection** — `PlaceWaterOnPlane.Update()` (lines 14-38) returns silently when `Input.touchCount == 0` or when the raycast finds no planes. First-time players tap the floor repeatedly and receive no response. No UI text like "Arahkan kamera ke lantai" (Point camera at floor), no scan animation, no plane-found indicator. Suggested fix: show a scanning overlay and display a prompt when `planeManager.trackables.count > 0`, swap it to "Ketuk untuk menempatkan air" (Tap to place water) once planes are ready.

- [ ] **UX-2: No throw-mechanic hint for first-time players** — `TombakanOnboarding.Start()` calls `goalManager.StartCoaching()` via `DismissGreeting()`, but GoalManager coaching covers AR placement, not the throw action. After water is placed the timer immediately starts counting down and fish spawn, but there is no onscreen prompt explaining that a tap anywhere triggers `SpearThrower.ThrowSpear()`. The BACKLOG Week 7 candidate for a "Sentuh tombol untuk melempar tombak" coaching step (BACKLOG line 126) is not yet present. A frustrated first-timer may not discover the throw mechanic until ~20 s of the 60 s round has elapsed.

- [ ] **UX-3: No water re-positioning option — one-shot placement is permanent** — `PlaceWaterOnPlane.cs:37` sets `enabled = false` after first successful placement, which is intentional to prevent mid-game drift. But there is no "Pindahkan air" (Reposition water) button exposed to the player before the game starts. BACKLOG Week 7 candidate still open. Suggested fix: add a pre-game confirmation step with a reposition button that re-enables `PlaceWaterOnPlane` and clears `waterPlane`.

- [ ] **UX-4: targetColorLabel shows raw hex string for catalog-based species** — `ColorHexLocalization.ToIndonesian()` (Dict.cs:34) falls back to returning the raw 6-character hex when the color is not in `Map`. `FishSpawner.SpawnFish` (line 30) uses `targetSpecies.baseColor` for the resolved target color. If a species `ScriptableObject` has a `baseColor` that is not one of the 4 exact FishPalette values (e.g., an artist-tuned coral color), the target label shown to the player reads "FF6B3D" instead of an Indonesian name. Suggested fix: every `FishSpecies` ScriptableObject should store a display name and `GameManager.PickNewTarget` should prefer `targetSpecies.displayName` over `ColorHexLocalization.ToIndonesian(targetColor)` for the label when a species is active. Currently the species name label (`targetSpeciesLabel`) is set correctly on line 419-421, but `targetColorLabel` on line 406 runs BEFORE the species is resolved, and the post-resolve re-assignment only sets `targetColorImage.color`, not `targetColorLabel.text`.

---

## Session Simulations

### Session 1 — First-timer A (never played AR before)
1. Opens app, sees camera view with AR scanning mesh. Does not know what to do.
2. Taps floor immediately — `PlaceWaterOnPlane.Update` fires, `raycastManager.Raycast` returns false (planes not scanned yet), nothing happens (**UX-1 hit**).
3. Taps 4 more times in different spots — still nothing. Player assumes game is broken.
4. Waits ~10 s, AR plane mesh appears. Taps — water placement succeeds.
5. Fish appear, 60 s timer starts. Player stares. No hint visible (**UX-2 hit**).
6. Player discovers tap-to-throw at ~40 s remaining.
7. Throws spear — `SpearThrower.ThrowSpear()` fires, spear travels, hits blue fish (target is Merah/Red). `GameManager.OnFishHit` → wrong hit, `score -= 25`, `ShowSad`.
8. 60 s expire. Score = -25, clamped to 0 by `ClampScore`. TierEmpty shown.
9. Net experience: confusion → frustration → mild curiosity. No retention.

### Session 2 — First-timer B (water placement worked immediately)
1. AR plane detected within 3 s. Taps, water placed.
2. Sees fish. No hint. Waits 10 s looking for a button (**UX-2 hit**, 10 s wasted).
3. Accidentally taps screen — spear flies. Hits correct fish (lucky). Score +100. Happy feedback: "+100!".
4. New fish spawn. Timer at 45 s. Player now understands tap = throw.
5. Throws repeatedly during LockThrow period — `canThrow = false`, nothing happens. Player panics ("broken again").
6. After lock expires, throws again, hits wrong. `-25`.
7. Ends with 2 correct hits, TierLow. Curious enough to replay.

### Session 3 — Casual Player (3rd session, familiar with placement)
1. Skips onboarding (returning player, `skipOnboardingForReturning = true`).
2. Daily bonus claimed: `DailyChallenge.TryClaimDailyBonus` returns true. Shows "Bonus harian +125 XP! Streak 2 hari berturut-turut!" in dailyBonusPanel for 3 s.
3. If this XP tips them over a level boundary: `TombakanOnboarding.Start()` detects `levelAfter > levelBefore`, shows "Level 4! Selamat!" in bonus text. BUT `ApplyLevelReward` is never called (**BUG-A hit**). Reward (e.g., new spear skin) silently skipped.
4. Places water confidently. Throws well. Combo streak to 3, feedback "x2 COMBO! +200!" — English "COMBO!" noted as odd (**POLISH-1**).
5. Ends at TierMid (6 correct). XP and coins awarded. Progression HUD updates. Session feels good minus the missed level reward.

### Session 4 — Casual Player (fish look wrong color)
1. Scene configured with catalog (FishCatalog assigned). Species-based spawning active.
2. `FishSpawner.SpawnFish` picks targetSpecies from catalog. `ApplyColor` runs on each fish.
3. Fish prefab has 3 child renderers (body, fins, eye). `GetComponentInChildren<Renderer>()` returns `body` only.
4. Fins and eye stay default white. All fish look partially white regardless of assigned color (**BUG-C hit**).
5. Player cannot reliably identify the target fish by color. Core mechanic is compromised.
6. Throws at a fish that looks "most colored." Gets wrong hit 3 times in a row. Score drops. Frustration.

### Session 5 — Frustrated Player (3 wrong hits in a row, minimal time left)
1. 3 wrong hits: score = 0 (clamped), comboStreak = 0. sadFeedback shown 3 times.
2. LockThrow + `Invoke(PickNewTarget, delay + 0.8f)` means 3+ seconds between rounds.
3. Timer at 8 s. `timerWarningActive = true`, `TimerPulseWarning` coroutine starts.
4. Timer bar pulses red. Psychological pressure adds to frustration.
5. Player throws frantically during cooldown. No response to any tap. Feels like the app froze.
6. Timer hits 0. `gameRunning = false`, `EndGame()`. Result: 0 score, TierEmpty, 3 wrong hits.
7. `resultAccuracyText` shows "0/3 (0%)". Demoralizing but honest.
8. `StaggerResultCelebrations`: no new record, no level-up. Clean exit. No "play again" shortcut visible (scene-level).

### Session 6 — Frustrated Player (spear hits fish, nothing registered)
1. Scene where fishLayer not correctly assigned in SpearHit inspector (**BUG-B scenario**).
2. Player throws spear. `SpearHit.Update` → `CheckFishHit` → `Physics.OverlapSphere(..., fishLayer)` returns empty array.
3. Spear visually passes through fish at close range. Disappears after 2.5 s (`Destroy(spear, spearLifeTime)`).
4. `spearFake` re-activates. Player can throw again. Same result.
5. Player throws 8 times, zero hits registered. Timer expires. Score 0.
6. Player uninstalls app.

### Session 7 — Speed-Runner (maximizing score in 60 s)
1. Knows placement. Places water immediately.
2. Throws as fast as possible. Hits correct fish. `comboStreak` builds.
3. At streak 5: `ComboMultiplier` returns 3. Hit = 300 pts. `TimeBonus.ForHit(5)` adds time.
4. `LockThrow(delay)` with `PacingRules.HitDelayForProgress` — at high `correctHitCount`, delay decreases. Speed-runner benefits from adaptive pacing.
5. `fishCount` ramps 3→4→5→6→7 — more fish to dodge as difficulty increases.
6. At 10+ correct hits: 4 active colors. Color variety increases cognitive load.
7. Speed-runner finishes at 15 correct, TierLegend (if `TierLegend` object assigned; else falls back to `TierHigh` per GameManager.cs:546).
8. `newSpeciesThisGame` adds XP bonus. Multiple achievements may fire at +1.2 s.
9. Achievement toasts sequence with 2 s gaps — at 3 achievements, toasts take 6 s after result screen. This can overlap with player starting a new round if they tap quickly (**POLISH gap: achievement dismissal not player-controlled**).

### Session 8 — Speed-Runner (noticing spear/fish visual)
1. Hits fish perfectly. Spear snaps to fish center (**BUG-D**). Sees rubber-band visual in SpearLeash LineRenderer (`leash.spearTip = null` at end of cooldown).
2. Fish + spear destroyed at 1.2 s (`FishHitBox.DestroyAfterDelay`). `Destroy(spear, 2.5f)` in SpearThrower is now a no-op on already-destroyed object. Harmless but redundant.
3. Speed-runner is fine with this; casual player would notice the snap and comment on it.

### Session 9 — Returning Player (checking Fishipedia)
1. `TombakanOnboarding` skips coaching. `goalManager.ForceCompleteGoal()`.
2. Player wants to view Fishipedia. `FishipediaUI` not yet wired in scene per BACKLOG (open item). Panel doesn't appear. Player gives up on collection feature.
3. Same for SpearShop — scene wiring not done. All progression UI (level badge, XP bar) also depend on scene wiring per BACKLOG open items.
4. Player experience of Phase 1/2/2.5 features depends entirely on scene setup blocked in BACKLOG. Net: returning player sees no new content until Unity Editor work is done.

### Session 10 — Returning Player (bad water placement)
1. Places water on top of a coffee table (1 m off ground). Fish spawn at -0.08 to -0.3 m below table surface — too high to aim at comfortably.
2. Player wants to reposition. Taps floor: `PlaceWaterOnPlane.enabled = false`, script is dead (**UX-3 hit**).
3. Player closes and reopens app to retry placement.
4. Second attempt: places on floor. Good angle. Plays normally.
5. Effort cost: full app restart for a 1-tap mistake.

---

## Polish Opportunities

- [ ] **POLISH-1: "COMBO!" → Indonesian equivalent** — `GameManager.cs:499`: `$"x{multiplier} COMBO! +{earned}!"` should use "KOMBO!" or simply `$"×{multiplier}! +{earned}!"` to stay consistent with the Indonesian-first UI.

- [ ] **POLISH-2: Platform-specific haptic durations (BACKLOG Week 7 candidate)** — `HapticFeedback` currently uses a single vibration call. Android supports duration-based `Handheld.Vibrate()` via reflection; iOS supports `InputSystem.Haptics`. Without differentiation, correct-hit and wrong-hit feel identical on some devices.

- [ ] **POLISH-3: Achievement toast has no dismiss gesture** — `ShowAchievementsSequenced` (GameManager.cs:352) auto-dismisses after 2 s per achievement with a fixed `WaitForSecondsRealtime`. A speed-runner finishing a game and tapping "play again" immediately cannot dismiss the toast sequence. The toasts continue on the main menu screen after scene transition if scenes share the GameManager.

- [ ] **POLISH-4: Timer bar color not reset before result screen for ~one pulse half-cycle** — `gameRunning = false` at GameManager.cs:179 stops the TimerPulseWarning loop condition, but the loop runs one final `ScaleLerp` half-cycle (0.2 s) before exiting. `EndGame` cleanup at lines 294-300 forces reset after that. The result screen may briefly appear with a red timer bar before it resets to white. Very brief; player perception depends on frame timing.

- [ ] **POLISH-5: spearFake re-activates during LockRoutine even though throw is still locked** — `SpearThrower.LockRoutine` (lines 105-112): `spearFake.SetActive(false)` on start, then `spearFake.SetActive(true)` and `canThrow = true` together at the end. But `canThrow` is the actual gate. The visible spear (spearFake) being visible is a visual proxy for "ready to throw." These are correctly in sync here; this is a note that they remain tightly coupled, and any future change to the lock timing needs to update both.
