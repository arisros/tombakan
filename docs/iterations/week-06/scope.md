# Iteration Week 6 — Scope

## Selected Tasks

| ID | Owner | Task | Acceptance Criteria |
|----|-------|------|---------------------|
| TASK-01 | dev | Fix four small bugs: (a) add `if (!gameRunning) return;` guard to `OnFishHit`; (b) surface daily-bonus level-up in `TombakanOnboarding` greeting; (c) guard `ApplyLevelReward` from overwriting non-empty `levelUpText`; (d) decouple `HapticFeedback` from audio mute | `OnFishHit` returns immediately when `!gameRunning`; `TombakanOnboarding.Start` detects and shows level-up text alongside daily bonus; `ApplyLevelReward` only sets `levelUpText.text` when `reward.celebrationText` is non-empty; `HapticFeedback.PlayCorrect/PlayWrong` no longer check `AudioPrefs.IsMuted()` |
| TASK-02 | dev | Achievement toast + stagger result celebrations: show `Achievement.titleIndonesian` in a toast panel after EndGame; delay `newRecordBadge` by 0.4 s and `levelUpPanel` by 0.8 s relative to result screen appearance | `GameManager` has null-safe `achievementToastPanel` (GameObject) and `achievementToastText` (TMP_Text) fields; newly unlocked achievements are shown in toast (2 s visible) not just Debug.Log; `newRecordBadge` activates 0.4 s after result screen; `levelUpPanel` activates 0.8 s after result screen |
| TASK-03 | ui | Add achievement toast panel to the gameplay canvas in `Assets/Scenes/GamePlay.unity`: a Screen Space Overlay child of `gamePlayUI`, inactive by default, with a semi-transparent background Image and a centred TMP_Text | Scene contains a GameObject named `AchievementToastPanel` as child of `gamePlayUI`, with `active: 0`; a child TMP_Text named `AchievementToastText` is present; panel anchors to top-centre of screen |

## Deferred (next iteration)

- **UX-1** — First-timer throw tutorial ("Sentuh tombol untuk melempar tombak") — needs `TombakanOnboarding` coaching step and a dedicated hint panel; scope for Week 7
- **POLISH-2** — Platform-specific haptic differentiation (`AndroidJavaObject` duration on Android, `InputSystem.Haptics` on iOS) — device-only validation required; defer until device test matrix is run
- **Re-position water button** — complement to the Week 5 one-shot placement fix; needs scene YAML button + code path; scope for Week 7

## Rationale

**TASK-01** clears four bugs in one turn because they are each one-to-five line changes across three files. All are P1: `OnFishHit` without a guard lets mid-flight spears corrupt the result; the daily-bonus level-up is silent; `ApplyLevelReward` can erase the level-up message before the player reads it; muted haptics break the correct-vs-wrong signal for players who silence audio in public.

**TASK-02** tackles the top BACKLOG item (achievement toast, UX-2) bundled with POLISH-1 (result stagger) since both require coroutine changes to the same `EndGame` code path. Doing them together avoids a second pass over the same function next week. The toast uses null-safe fields so it degrades gracefully until TASK-03 wires the panel.

**TASK-03** is the UI counterpart to TASK-02 — adds the panel that TASK-02 references. Keeping it separate lets the ui agent focus on scene YAML structure without touching C#. TASK-02 is fully functional without TASK-03 (null-safe); TASK-03 makes the toast visible to players.
