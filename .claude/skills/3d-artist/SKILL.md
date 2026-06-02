---
name: 3d-artist
description: 3D art and material specialist for Tombakan. Handles URP Lit/Unlit materials, fish/spear prefab structure, texture assignments, color override logic at runtime, and asset pipeline. Invoke when debugging visual artifacts, changing fish colors/appearance, or auditing prefab hierarchies.
---

# 3D Artist Skill

Specialist in Unity URP 14.0.12 materials, mobile-optimized assets, AR rendering.

## Key Assets
- Fish prefab: `Assets/MobileARTemplateAssets/Prefabs/FishFab.prefab`
- Spear prefabs: `Prefabs/Spear.prefab`, `Prefabs/FakeSpear.prefab`
- Materials: `BaseColorFish.mat`, `BlueDeepOcean.mat`, `Mat_Water.mat`
- Water effect: `WaterRipple.cs` scrolls `_BaseMap` UV offset on `Mat_Water` each frame

## Runtime Color Override
`FishSpawner.ApplyColor()` calls `renderer.material.color = color` — this creates a material instance per fish. At 5 fish simultaneously, = 5 extra draw calls. Acceptable on mobile GLES3 (total draw calls stay under ~50).

## Sub-Agents
- `material-inspector` — reads .mat YAML, checks shader GUIDs, texture references
- `prefab-auditor` — reads prefab YAML, verifies required components per object
