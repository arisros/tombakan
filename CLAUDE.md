# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Tombakan** is a mobile Augmented Reality fishing game built with Unity. Players place a virtual water surface in the real world via AR, then throw a spear to hit fish matching a displayed target color. The name and UI labels are in Indonesian (e.g., "Hijau" = Green, "Merah" = Red).

- Engine: Unity (URP 14.0.12)
- Language: C#
- AR: ARFoundation 5.2.0 + ARKit/ARCore 5.2.0
- Build targets: Android and iOS

## How to Run

1. Open the project folder in **Unity Editor** (LTS version compatible with URP 14/ARFoundation 5).
2. The only scene is `Assets/Scenes/GamePlay.unity` — open and press **Play** to test in Editor (AR plane detection is simulated).
3. To run on device:
   - **Android**: File → Build Settings → Android → Build → deploy the `.apk`
   - **iOS**: File → Build Settings → iOS → Build → open in Xcode → Archive & deploy
4. AR requires a physical device with ARCore (Android) or ARKit (iOS) support. Editor Play mode uses simulated touch input.

## How to Test

Unity Test Framework (`com.unity.test-framework 1.1.33`) is included.

- Place tests under `Assets/Tests/`
- Run via **Window → General → Test Runner** in the Unity Editor
- No automated CI pipeline is configured

## Code Architecture

All game scripts live in `Assets/MobileARTemplateAssets/Scripts/`.

### Singleton Managers

| Class | Accessor | Responsibility |
|---|---|---|
| `GameManager.cs` | `GameManager.I` | Game state, score (+100/-25), 60 s timer, UI coordination |
| `AudioManager.cs` | `AudioManager.I` | BGM (menu/gameplay) and SFX (hit correct, hit wrong, end) |

### Game Loop

```
GameManager.StartGame()
  └─ PickNewTarget()          — picks random color (Red/Green/Blue)
       └─ FishSpawner.SpawnFish(targetColor)
            ├─ Destroy previous fish
            └─ Instantiate 5 fish; 1 gets target color, rest get wrong colors

Each frame:
  FishSwim.Update()           — Perlin-noise steering, depth variation, boundary clamping
  FishAnimate.Update()        — sine-wave tail bone animation
  SpearHit.Update()           — overlap-sphere collision check at spear tip

On hit:
  SpearHit → FishHitBox.OnHit()
    ├─ GameManager.OnFishHit() → score update, emoji feedback, PickNewTarget after delay
    └─ Spear parented to fish, then fish + spear destroyed after 1.2 s

Timer reaches 0:
  GameManager.EndGame()       — shows result screen, star tier (0 / 1-2 / 3-4 / 5+ correct hits)
```

### AR Placement

`PlaceWaterOnPlane.cs` handles touch-raycast against `ARRaycastManager` to position the water prefab on a detected `ARPlane`. Once placed, AR plane visualizations are hidden.

### Spear System

- `SpearThrower.cs` — instantiates a spear prefab from a shoulder-mounted origin, applies forward velocity, enforces 1.2 s cooldown
- `SpearHit.cs` — sphere overlap (radius 0.1 m) each frame; calls `FishHitBox.OnHit()` on contact
- `SpearLeash.cs` — `LineRenderer`-based visual rope from holder to spear

### Fish System

- `FishSpawner.cs` — manages fish lifecycle; destroys all fish before each new round
- `FishSwim.cs` — procedural movement: Perlin noise for steering angle, speed variance, banking on turns, boundary radius enforcement
- `FishAnimate.cs` — drives tail root + tip bones with sine-wave at random amplitude/frequency
- `FishHitBox.cs` — collision response; prevents double-hit; sticks spear to fish; schedules destruction

### Localization

`Dict.cs` (`ColorHexLocalization`) maps Unity `Color` values → Indonesian color names for results display (20 color entries).

### Camera Rigs

- `ShoulderRigFollowCamera.cs` — offsets a rig transform relative to the main camera
- `SpareHolderFollowCamera.cs` — keeps the spear holder attached to the camera each frame

## Key Files

| File | Purpose |
|---|---|
| `Assets/MobileARTemplateAssets/Scripts/GameManager.cs` | Core game loop (343 lines) |
| `Assets/MobileARTemplateAssets/Scripts/FishSwim.cs` | Fish AI movement |
| `Assets/MobileARTemplateAssets/Scripts/SpearThrower.cs` | Throw mechanics |
| `Assets/MobileARTemplateAssets/Scripts/PlaceWaterOnPlane.cs` | AR plane placement |
| `Assets/MobileARTemplateAssets/Scripts/Dict.cs` | Indonesian color names |
| `Assets/Scenes/GamePlay.unity` | Only scene in project |
| `Packages/manifest.json` | All UPM dependencies |
