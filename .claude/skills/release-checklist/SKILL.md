---
name: release-checklist
description: Tracks Tombakan's release readiness for Google Play and Apple App Store. Run /release-checklist to audit current state of all ship requirements. Reads ProjectSettings.asset live and reports DONE or PENDING per item.
---

# Release Checklist Skill

When invoked, perform the following audit live (do not rely on cached knowledge):

## Audit Steps
1. Read `ProjectSettings/ProjectSettings.asset` — check bundle IDs, SDK versions, camera description
2. Check if `.github/workflows/unity-tests.yml` exists
3. Check if `PRIVACY.md` exists in repo root
4. Check if `Assets/Tests/EditMode/` and `Assets/Tests/PlayMode/` directories exist
5. Check if `Assets/MobileARTemplateAssets/Scripts/GameConstants.cs` exists

## Report Format
For each item below, output: ✅ DONE — <evidence> OR ⏳ PENDING — <what's needed>

### Identity
- Android bundle ID is `com.aris.tombakan` (not `com.unity.template.ar_mobile`)
- iOS bundle ID is `com.aris.tombakan`
- Version is `1.0.0`, build number is `1`
- `AndroidMinSdkVersion` is `24`
- `iOSCameraUsageDescription` is set to a meaningful Indonesian string

### QA
- `GameConstants.cs` exists
- `Assets/Tests/EditMode/` exists with at least 2 test files
- `Assets/Tests/PlayMode/` exists with at least 1 test file
- `.github/workflows/unity-tests.yml` exists

### Privacy & Legal
- `PRIVACY.md` exists in repo root
- Camera usage justification is documented

### Store Assets (manual check — report what's needed)
- Google Play: app icon 512×512 PNG, feature graphic 1024×500, ≥2 phone screenshots, short desc (≤80 chars), full desc, privacy policy URL
- App Store: app icon 1024×1024 PNG (no alpha), ≥3 iPhone 6.5" screenshots (1242×2688), description, privacy policy URL

### Technical Gates
- All EditMode tests passing
- All PlayMode tests passing
- AR plane detection confirmed on physical Android device
- AR plane detection confirmed on physical iOS device
- Full 60s game loop completes without crash
- Result screen shows Indonesian color names correctly

### Release Tag
- `git tag v1.0.0` has been created
