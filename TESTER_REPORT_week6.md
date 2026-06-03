# Tester Report — Week 6

## Top Issues (ranked by player impact)

| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|
| 1 | `OnFishHit` has no `gameRunning` guard — mid-flight spear after `EndGame` silently alters score without updating result screen | `GameManager.cs:399` | HIGH: result score shown is wrong; player may see wrong tier |
| 2 | Achievement unlocks are invisible — `Debug.Log` only, no in-game notification | `GameManager.cs:293` | HIGH: players earn achievements and never know |
| 3 | Level-up panel and new-record badge fire simultaneously with no ordering — visual clutter on result screen | `GameManager.cs:265,281` | MED: two celebrations compete for attention at the same instant |
| 4 | `DailyChallenge.TryClaimDailyBonus` calls `ProgressionStore.AddXp` but discards the level-up return value — silent level-up from daily bonus | `DailyChallenge.cs:58` | MED: player levels up from daily XP with no acknowledgement |
| 5 | `HapticFeedback.PlayCorrect` and `PlayWrong` both call `Handheld.Vibrate()` identically — haptics give no directional signal | `HapticFeedback.cs:12,21` | LOW-MED: correct and wrong hits feel the same on device |

---

## Session Simulations

### Session 1 — First-timer (no AR experience)

**Flow:** Launches → AR coaching → scans floor → taps plane → `PlaceWaterOnPlane.Update()` detects plane → `enabled = false` (Week 5 fix confirmed) → fish spawn → player sees target colour swatch with Indonesian label → tries to figure out how to throw.

**Friction:** No on-screen hint exists for throw gesture. `SpearThrower.ThrowSpear()` is tied to a UI button (or input), but there's no tutorial text visible in code. `TombakanOnboarding` handles AR coaching but has no game-mechanic hints. First-timer may tap the fish, tap the background, or simply wait. Silent failure (no response to random taps) is the worst kind.

**Known issue (BACKLOG UX-1):** No throw tutorial. Deferred from Week 5, still present.

### Session 2 — Casual player (3rd session), daily login

**Flow:** `TombakanOnboarding.Start()` detects returning player → `goalManager.ForceCompleteGoal()` → `DailyChallenge.TryClaimDailyBonus(out xp, out streak)` fires → XP awarded → daily panel shown.

**Critical path — DailyChallenge level-up:**
```
DailyChallenge.cs:58: ProgressionStore.AddXp(xpAwarded)
```
`AddXp` returns the new level if a level-up occurred, else 0. `TryClaimDailyBonus` **discards this return value**. If the bonus XP bridges a level boundary, the player levels up silently before the game even starts. No `levelUpPanel`, no celebration text, no reward applied. `LevelRewardTable.GetRewardForLevel` is only called inside `GameManager.EndGame` — level rewards tied to this level are never granted. **BUG-2 confirmed: silent level-up from daily bonus, rewards skipped.**

### Session 3 — Speed runner (correctHitCount = 16, Level 4)

**Flow:** Fast game, hits 10 fish in 60 s. At `EndGame`:

**Simultaneous celebration trace:**
```
GameManager.cs:263: bool isRecord = ScoreStore.TrySetBest(score)  → true
GameManager.cs:265: newRecordBadge.SetActive(true)                 ← fires NOW
...
GameManager.cs:281: if (newLevel > 0) ShowLevelUp(newLevel)        ← fires same frame
```
Both `newRecordBadge` and `levelUpPanel` activate in the same synchronous call within `EndGame`. Neither has a delay, timer, or sequencer. On a device, both panels appear at the same millisecond. Player focus splits between the badge and the panel; neither moment lands properly.

**Also:** `ShowLevelUp` shows `"Level {newLevel}! Selamat!"` via `levelUpText`, but `ApplyLevelReward` may then overwrite `levelUpText.text` with `reward.celebrationText` (GameManager.cs:348). If both fire in the same call, the text changes within one frame — player may never read the original level-up message.

### Session 4 — Achievement hunter (combo of 5)

**Flow:** Player builds a combo streak of 5. `EndGame` fires. `AchievementChecker.CheckAll` (GameManager.cs:291) finds `Combo5` condition met → `AchievementStore.Unlock("combo_5")` → XP granted (if catalog assigned).

**BUG-2 trace:**
```
GameManager.cs:292: string[] newlyUnlocked = AchievementChecker.CheckAll(this, achievementCatalog)
GameManager.cs:293-294:
  foreach (string id in newlyUnlocked)
      Debug.Log($"[Tombakan] Achievement unlocked: {id}");
```
Nothing is shown to the player. The `Achievement` ScriptableObject has `titleIndonesian` ("Combo Master") and `descriptionIndonesian` fields populated in the catalog — this content is never surfaced. A player on a release build has no `Debug.Log` output at all. The achievement is permanently silently granted.

Frequency: every game where a milestone is first reached. Severity: high — gamification value of achievements is completely lost.

### Session 5 — Edge case: spear mid-flight at timer expiry

**Flow:** Player throws at T=60.2s. Timer ticks to 0 at T=60.0s. `Update()` sets `gameRunning=false` and calls `EndGame()`. Result screen appears at T=60.0s with `resultScoreText.text = "1200"`. Spear continues flying (spearLifeTime=2.5s, Destroy at T=62.7s). At T=60.4s, spear hits a fish.

**Bug trace:**
```
SpearHit.Update()  →  CheckFishHit()  →  FishHitBox.OnHit()
  →  GameManager.I.OnFishHit(fishColor, speciesId)   // gameRunning is FALSE
     GameManager.cs:399: bool correct = fishColor == targetColor  — no guard
     ...score += 100
     ...correctHitCount++
     UpdateScoreUI()  →  scoreText.text = "1300"      // HUD score (hidden)
     Invoke(nameof(PickNewTarget), ...)               // guarded by !gameRunning ✓
```
Result screen still shows "1200". HUD score shows "1300". Score, `correctHitCount`, `maxComboStreak`, tier, and accuracy are all stale. The player is shown the wrong result. **BUG-1 confirmed.**

Frequency: any time a fish is hit within 2.5 s after timer expiry (i.e., after EndGame fires but before the in-flight spear is destroyed). Reproducible by hitting a fish in the final second.

### Session 6 — Wrong hit immediately before timer expiry

**Flow:** Player hits wrong fish at T=59s. `score = ClampScore(1200 - 25) = 1175`. Timer expires 1 second later. `EndGame` shows 1175. OK — no race condition here since the hit happened before EndGame.

But `Invoke(nameof(PickNewTarget), delay + 0.8f)` fires at T=60.8s. `PickNewTarget` now has `!gameRunning` guard — returns immediately. ✅ Week 5 fix working correctly.

### Session 7 — Colour-blind player toggles mode mid-game

**Flow:** Player enables colour-blind mode mid-game via `ColourBlindToggleUI`.

**Trace:**
```
ColourBlindToggleUI.OnClick()
  ColourBlindSettings.Toggle()          // persists
  foreach FishShapeOverlay in scene:
      overlay.OnSettingChanged()        // refreshes ALL active fish overlays
```
`FindObjectsOfType<FishShapeOverlay>()` finds all active fish overlays — confirmed correct. New fish spawned after the toggle also read `ColourBlindSettings.IsEnabled()` in their `Start()`. **No bug.** Mid-game toggle works correctly.

### Session 8 — Veteran player, full session

**Flow:** 10+ games played. Best score 2400. This game: 18 correct hits, no wrong, new record.

Result screen shows:
- Score: 1800 (correct)
- `resultAccuracyText`: "18/18 (100%)" ✅
- `resultXpText`: "+285 XP" ✅ (xpEarned > 0)
- `newRecordBadge`: visible ✅
- `levelUpPanel`: also visible (simultaneous) ← POLISH-1
- `resultCorrectFishText`: `CollectSummary()` — if `collectedSpeciesIds.Count > 0`, shows all species caught (Week 5 fix) ✅

All Week 5 fixes confirmed working in this path.

### Session 9 — Mute player, haptic feedback

**Flow:** Player has muted audio. `AudioPrefs.IsMuted()` → true. `HapticFeedback.PlayCorrect()` → `if (AudioPrefs.IsMuted()) return` — no vibration on correct hit. Player expects the vibration to be independent of mute, since haptics are tactile, not audio. Muting audio should not mute haptics.

This is a design decision, but tying haptics to the audio mute flag is confusing. A player who mutes to play in public still wants tactile feedback.

### Session 10 — Long session, high combo

**Flow:** Player reaches combo × 3 at 5+ streak. Achievement `Combo5` unlocks. `ProgressionRules.XpForResult` awards bonus XP. Result screen shows nothing about the achievement. Player has no idea. The XP total in the HUD increments, which is the only visible sign, but there's no cause-and-effect shown.

---

## Bugs Found

- [x] **BUG-1** — `OnFishHit` runs after `EndGame` (no `gameRunning` guard); mid-flight spear updates score/stats but not result screen display — `GameManager.cs:399` — throw spear at final second, let it land after timer expires; result score differs from final HUD score
- [x] **BUG-2** — `DailyChallenge.TryClaimDailyBonus` discards `ProgressionStore.AddXp` return value; level-up from daily bonus is silent and level rewards are not applied — `DailyChallenge.cs:58` — accumulate XP close to a level threshold; on next day login the level-up silently occurs

## UX Gaps

- [ ] **UX-1** — No throw-mechanic tutorial for first-time players — first game, after water is placed — add a brief "Sentuh tombol untuk melempar tombak" hint visible for the first 8 seconds *(known BACKLOG item)*
- [ ] **UX-2** — Achievement unlock has no in-game notification; `titleIndonesian`/`descriptionIndonesian` fields on `Achievement` are populated but never shown — post-game result screen — add a brief toast panel using existing achievement data *(top BACKLOG candidate)*
- [ ] **UX-3** — Haptic feedback is silenced when audio is muted (`HapticFeedback` checks `AudioPrefs.IsMuted()`); players who mute for a quiet environment lose all tactile feedback — every hit while muted — decouple haptics from audio mute (add a separate HapticPrefs or remove the mute check)

## Polish Opportunities

- [ ] **POLISH-1** — Level-up panel and new-record badge activate simultaneously; text on `levelUpText` can also be overwritten by `ApplyLevelReward` in the same frame — result screen — stagger: show tier/score → badge (0.4 s delay) → level-up panel (0.8 s delay) *(known BACKLOG item)*
- [ ] **POLISH-2** — `HapticFeedback.PlayCorrect` and `PlayWrong` produce identical vibration; both call `Handheld.Vibrate()` with no duration/pattern difference — on any fish hit — use `AndroidJavaObject` for duration on Android; use `UnityEngine.InputSystem.Haptics` or `HapticPattern` on iOS
- [ ] **POLISH-3** — `ShowLevelUp` text is immediately overwritten by `reward.celebrationText` in `ApplyLevelReward` if called the same frame — level-up moment — apply reward text only if `celebrationText` is non-empty; otherwise preserve the `"Level {n}! Selamat!"` text
