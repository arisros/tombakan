# Iteration Week 8 — Scope

**Date:** 2026-08-10
**Input:** TESTER_REPORT_week7.md
**Tasks:** 4

---

## TASK-01 — dev (P0) — Fix discarded AddXp return in DailyChallenge and AchievementChecker

**Bugs addressed:** BUG-1 (CRITICAL), BUG-2 (HIGH)

**Why first:** Level-up rewards (species unlock, spear skin, coin bonus) are silently dropped on every
daily-bonus level-up (BUG-1) and every achievement XP level-up (BUG-2). Both stem from the same
one-line mistake — `ProgressionStore.AddXp` return value discarded — and are together a P0 because
players permanently lose earned progression without any indication.

**What to do:**
1. In `DailyChallenge.TryClaimDailyBonus` (`DailyChallenge.cs:58`): capture the `AddXp` return as
   `int newLevel`. Add an `out int newLevel` parameter (or return it) so the caller can act on it.
2. In `TombakanOnboarding` (or wherever `TryClaimDailyBonus` is called): when `newLevel > 0`, call
   `GameManager.ApplyLevelReward(newLevel)`.
3. In `AchievementChecker.CheckAll` (`AchievementChecker.cs:100`): capture the `AddXp` return.
   When `> 0`, call `StartCoroutine(ShowLevelUp(...))` or `ApplyLevelReward` via a static/singleton
   path already available on `GameManager.I`.

**Acceptance criteria:**
- A player who crosses a level boundary via daily bonus receives the configured `LevelRewardTable`
  reward (species unlock, coins, skin) — verified by a new EditMode test with a fake `LevelRewardTable`
  confirming `ApplyLevelReward` is invoked with the correct new-level value.
- A player who crosses a level boundary via an achievement XP award sees the level-up panel and
  receives the reward — same test pattern in `AchievementChecker` tests.
- The existing Week 6 behaviour (daily-bonus toast text correct) is not regressed.

---

## TASK-02 — dev — Fix GoalManager null crash and throw-lock/spawn timing gap

**Bugs addressed:** BUG-8 (MEDIUM), BUG-3 (HIGH)

**Why together:** Both are one-liner or two-liner fixes in adjacent systems; batching them keeps the
task within a single agent turn without overlap.

**What to do:**

BUG-8 (`GoalManager.cs:196`, `TombakanOnboarding.cs:30`):
- Add a null guard in `GoalManager.CompleteGoal` (or `ForceCompleteGoal`):
  `if (m_OnboardingGoals == null) return;`
- Alternatively, initialise `m_OnboardingGoals = new Queue<Goal>();` in `GoalManager.Awake()` so
  it is never null regardless of call order.

BUG-3 (`GameManager.cs:485-486`):
- Change `spearThrower.LockThrow(delay)` to `spearThrower.LockThrow(delay + 0.8f)` so the throw
  unlock occurs at the same moment fish appear, eliminating the window where the player can throw
  into empty water.

**Acceptance criteria:**
- A returning player launching a fresh session does not trigger a `NullReferenceException` or
  `IndexOutOfRangeException` in `GoalManager` — verified with an EditMode test that calls
  `ForceCompleteGoal` without first calling `StartCoaching`.
- After a correct hit, the throw button remains locked until fish are present; a throw fired at the
  earliest possible moment after each round transition always finds at least one fish — confirmed
  by reading the `LockThrow` call sites and verifying `delay + 0.8f` matches the `PickNewTarget`
  invoke delay.

---

## TASK-03 — ui — Fix timer-bar saturation, stale colour label, and colour-blind shape in HUD

**Bugs addressed:** BUG-4 (HIGH), BUG-6 (MEDIUM), BUG-7 (MEDIUM)

**Why together:** All three are display-only changes in `GameManager.cs` and the HUD panel; none
requires new prefabs or assets. Combined, they fix the three highest-visibility HUD regressions
that affect skilled and accessibility players respectively.

**What to do:**

BUG-4 (`GameManager.cs:162`):
- Replace `timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / gameDuration)` with a denominator
  that tracks the current maximum:
  `timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / Mathf.Max(timeLeft, gameDuration));`
  This keeps the bar calibrated when bonus time pushes `timeLeft` above `gameDuration`.

BUG-6 (`GameManager.cs:415-420`):
- After the species colour-override block in `PickNewTarget`, re-run
  `targetColorLabel.text = ColorHexLocalization.ToIndonesian(targetColor);`
  so the label always matches the swatch. When `fishSpawner.CurrentTargetSpecies != null`,
  prefer `fishSpawner.CurrentTargetSpecies.displayName` as the label text and suppress the
  hex-fallback entirely.

BUG-7 (`GameManager.cs` `PickNewTarget`):
- When `ColourBlindSettings.IsEnabled()`, append the shape symbol to the target label:
  `targetColorLabel.text = $"{ColorHexLocalization.ToIndonesian(targetColor)} {ColourBlindSettings.ShapeForColor(targetColor)}";`
- If a dedicated `TMP_Text targetShapeLabel` field already exists on the HUD panel, set it there
  instead to keep colour name and shape symbol in separate elements.

**Acceptance criteria:**
- Timer bar fill never exceeds the current `timeLeft` fraction regardless of how many time bonuses
  have been earned; bar and countdown text always agree — confirmed by tracing the formula at
  `timeLeft = 80` with `gameDuration = 60`.
- `targetColorLabel` and `targetColorImage` always show the same colour after `PickNewTarget`
  completes, including when a catalog species overrides the palette pick.
- When colour-blind mode is on, the HUD target panel displays a shape symbol (●, ▲, ■, or ★)
  alongside the colour name.

---

## TASK-04 — artist — Create "Meleset!" miss-feedback floating text prefab

**Bug addressed:** BUG-10 (LOW-MEDIUM)

**Why now:** First-time players (tester persona "Raka") cannot tell whether the game registered
their throw when the spear hits empty water. A lightweight text-pop prefab unblocks the dev
implementation of `SpearHit.OnDestroy` miss detection with no code changes required in this task.

**What to do:**
- Create a Unity prefab (`Assets/Prefabs/UI/MissText.prefab`) containing:
  - A `TMP_Text` or `TextMeshPro` component displaying "Meleset!" in the game's existing UI font
    and colour scheme (white outline, warm-red or orange fill to distinguish from wrong-hit red).
  - A simple Animator or `Animation` clip: scale from 0 to 1 over 0.15 s, hold for 0.6 s, fade
    alpha to 0 over 0.4 s, then self-destroy (or pool-return).
  - A `Canvas` component set to World Space so it appears near the spear's last position in AR space.
- Place the prefab in `Assets/Prefabs/UI/` and name it exactly `MissText` so the dev task can
  reference it by path.

**Acceptance criteria:**
- `MissText.prefab` exists at `Assets/Prefabs/UI/MissText.prefab`.
- Previewing the prefab's animation clip in the Unity Editor shows the scale-in, hold, and
  fade-out sequence completing in approximately 1.15 s total.
- The text "Meleset!" is legible at 1 m distance in AR preview (font size scaled for world-space).
- Prefab has no missing-script or missing-asset references.
