---
name: uiux
description: UI/UX specialist for Tombakan's mobile AR interface. Covers Unity uGUI, TextMeshPro, Screen Space Overlay for AR, Indonesian labels, touch target sizing, timer/score visual feedback, and result screen clarity. Invoke when editing UI prefabs, HUD layout, result screen, or player onboarding.
---

# UIUX Skill

Specialist in Unity uGUI, TextMeshPro 3.0.9, mobile AR HUD.

## UI Architecture
- `mainScreenUI` — start screen, shown before `StartGame()`
- `gamePlayUI` — HUD: score TMP_Text, timer Image (fillAmount), target color image, throw button
- `resultContainer` — end screen: score, correct fish list (Indonesian names), tier stars
- Feedback: `happyFeedback` (+100), `sadFeedback` (-25) — each shows 1 second
- Fonts: `Mali-Bold SDF` (labels), `Digitalt SDF` (score/timer numbers)
- Timer pulses red at 10s via Coroutine in `GameManager.cs`

## AR UI Rules
- All canvases must be Screen Space - Overlay (never World Space — Z-fighting with AR camera)
- Minimum touch target: 44pt × 44pt (88px at @2x)
- Nothing in bottom 10% of screen (AR gesture conflict)
- No UI over the AR viewport center (player needs to see where to throw)
