---
name: game-tester
description: Simulates user sessions in Tombakan by tracing through game scripts from a beginner POV. Identifies friction points, logic bugs, and UX gaps. Does not run Unity — reads code and reasons about player experience. Produces structured tester reports.
---

# Game Tester Skill

Reads C# scripts and reasons through the full player flow:
**launch → AR scan → water placement → fish spawn → throw → hit/miss → timer → result**

## Session Simulation Method

For each simulated session, trace:
1. What the player sees/does at each step
2. What code path executes (cite file:line)
3. Where friction, confusion, or bugs occur

Simulate at least 10 sessions covering these profiles:
- First-time player (no AR experience)
- Casual player (2nd or 3rd session)
- Frustrated player (missed 3 throws in a row)
- Speed player (trying to maximize score)

## Key Scripts to Read Every Run
- `GameManager.cs` — game state, score, timer, UI
- `FishSwim.cs` — fish movement AI
- `FishSpawner.cs` — fish spawn logic and color assignment
- `SpearThrower.cs` — throw mechanics and cooldown
- `PlaceWaterOnPlane.cs` — AR plane placement
- `FishHitBox.cs` — collision and score response
- `Dict.cs` — Indonesian color name mapping

## Output Format

Save report as `TESTER_REPORT_weekN.md` in repo root:

```
# Tester Report — Week N

## Top Issues (ranked by player impact)
| Rank | Issue | File:Line | Impact |
|------|-------|-----------|--------|

## Bugs Found
- [ ] Bug description — File:Line — reproduction steps

## UX Gaps
- [ ] Gap description — player moment — suggested fix

## Polish Opportunities
- [ ] Polish item — expected feel improvement
```

## Rules
- Never mark an issue as fixed unless you read the changed code and confirm it
- Rank by: (frequency of encounter) x (frustration severity)
- A silent failure (tap with no response) always ranks higher than a visual glitch
- Read BACKLOG.md before each report — don't re-report already-known issues unless they got worse
