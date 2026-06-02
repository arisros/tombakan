---
name: game-developer
description: Unity C# game development specialist for Tombakan. Handles AR game mechanics, scoring, fish/spear systems, coroutines, MonoBehaviour architecture, and AR session configuration. Invoke when editing scripts in Assets/MobileARTemplateAssets/Scripts/, diagnosing AR issues, or changing game loop behavior.
---

# Game Developer Skill

Specialist in Unity 2022.3 LTS, URP 14.0.12, ARFoundation 5.2.0, C# game scripting.

## Project Architecture
- Singletons: `GameManager.I`, `AudioManager.I` — both set in Awake()
- Game loop: `StartGame()` → `PickNewTarget()` → `FishSpawner.SpawnFish(color)` → player throws spear → `SpearHit` overlap sphere → `FishHitBox.OnHit()` → `GameManager.OnFishHit()` → timer 0 → `EndGame()`
- Scoring: `GameConstants.PointPerCorrectHit` (+100), `GameConstants.PenaltyPerWrongHit` (-25)
- Fish: 5 per round, 1 target color, 4 wrong; colors from {Color.red, Color.green, Color.blue}
- Spear cooldown: 1.2s after throw, 2.2s lockout after hit
- All scripts: `Assets/MobileARTemplateAssets/Scripts/`

## Sub-Agents
- `unity-gameplay-engineer`: Scoring, timer, hit logic, spawn logic, result screen
- `ar-systems-engineer`: ARFoundation config, ARKit/ARCore platform differences, plane detection
