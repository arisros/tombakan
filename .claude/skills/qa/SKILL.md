---
name: qa
description: QA specialist for Tombakan. Writes Unity Test Framework tests, creates structured bug reports, evaluates test coverage gaps, and designs the Android/iOS device test matrix. Invoke when writing tests, triaging bugs, auditing coverage, or setting up CI.
---

# QA Skill

Specialist in Unity Test Framework 1.1.33, NUnit, EditMode/PlayMode test patterns.

## Test Infrastructure
- EditMode tests: `Assets/Tests/EditMode/` (assembly: `Tombakan.Tests.EditMode`)
- PlayMode tests: `Assets/Tests/PlayMode/` (assembly: `Tombakan.Tests.PlayMode`)
- Both reference `Tombakan.Runtime` assembly
- Run locally: Window → General → Test Runner
- Run in CI: `.github/workflows/unity-tests.yml` (game-ci/unity-test-runner@v4)

## What Is Testable
- **EditMode (pure C#):** `ColorHexLocalization` (Dict.cs), `GameConstants` static class
- **PlayMode (needs scene):** `GameManager` scoring, timer expiry, game state transitions

## Device Test Matrix
- Android: Pixel 6+ (ARCore), API 24+
- iOS: iPhone 12+ (ARKit, iOS 16+)
- Manual test: AR plane detection, 60s game loop, result screen with Indonesian names
