# Ship Plan: Tombakan → Public Release
## QA Architecture + Claude Role-Agents + Store Release

### Context
Tombakan is a Unity 2022.3 LTS mobile AR fishing game (ARFoundation 5.2, C#, single scene `GamePlay.unity`). Goal: ship publicly on Google Play + App Store. All scripts are in `Assets/MobileARTemplateAssets/Scripts/`. No tests, CI, or `.claude/` directory exists yet. `com.unity.test-framework 1.1.33` is already installed — only assembly definitions are needed to start writing tests.

**Confirmed decisions:**
- `AndroidMinSdkVersion`: change 35 → 24 (ARCore minimum, max device reach)
- Skills location: project-level at `.claude/skills/`

**Key facts discovered from reading source files:**
- `score` and `correctHitCount` in `GameManager.cs` are already `public` — no change needed
- `targetColor`, `timeLeft`, `gameRunning` are package-private — need `internal` for PlayMode tests
- `ColorHexLocalization.Map` in `Dict.cs` is already `public static readonly` — no change needed
- `OnFishHit(Color)` is already `public` — callable from tests

---

## Execution Order

```
Phase 5+6 ─── start immediately (no dependencies)
  ├── Agent F: Create .claude/skills/ (13 files)
  └── Agent J: Fix ProjectSettings + PRIVACY.md

Phase 1 ─── after 5+6 complete (or run independently)
  └── Agent A: GameConstants.cs + GameManager refactor

Phase 2 ─── after Phase 1 compiles
  └── Agent B: 3 assembly definition files

Phase 3+4 ─── parallel after Phase 2
  ├── Agent C: EditMode tests (2 files)
  ├── Agent D: PlayMode tests (1 file)
  └── Agent E: GitHub Actions CI
```

---

## PHASE 1 — Testability Refactor
*Prerequisite for all test writing.*

### Files to Create/Edit

#### NEW: `Assets/MobileARTemplateAssets/Scripts/GameConstants.cs`
```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tombakan.Tests.PlayMode")]

public static class GameConstants
{
    public const int PointPerCorrectHit = 100;
    public const int PenaltyPerWrongHit = 25;
    public const float GameDuration = 60f;
    public const float WarningTimeThreshold = 10f;

    // Returns 0=Empty, 1=Low, 2=Mid, 3=High
    public static int GetTier(int correctHits)
    {
        if (correctHits <= 0) return 0;
        if (correctHits <= 2) return 1;
        if (correctHits <= 4) return 2;
        return 3;
    }
}
```

#### EDIT: `Assets/MobileARTemplateAssets/Scripts/GameManager.cs`

| Current | Change to |
|---|---|
| `int pointPerCorrectHit = 100;` | delete field; use `GameConstants.PointPerCorrectHit` inline |
| `int penaltyPerWrongHit = 25;` | delete field; use `GameConstants.PenaltyPerWrongHit` inline |
| `Color targetColor;` | `internal Color targetColor;` |
| `float timeLeft;` | `internal float timeLeft;` |
| `bool gameRunning;` | `internal bool gameRunning;` |
| `score += pointPerCorrectHit;` | `score += GameConstants.PointPerCorrectHit;` |
| `score -= penaltyPerWrongHit;` | `score -= GameConstants.PenaltyPerWrongHit;` |
| `happyFeedbackText.text = $"+{pointPerCorrectHit}!";` | `happyFeedbackText.text = $"+{GameConstants.PointPerCorrectHit}!";` |
| `sadFeedbackText.text = $"-{penaltyPerWrongHit}!";` | `sadFeedbackText.text = $"-{GameConstants.PenaltyPerWrongHit}!";` |
| if/else tier block in `UpdateTierStars()` | `int tier = GameConstants.GetTier(correctHitCount);` + if/else on tier |

---

## PHASE 2 — Assembly Definitions
*Run after Phase 1. Creates named assemblies so tests can reference game code.*

### NEW: `Assets/MobileARTemplateAssets/Scripts/Tombakan.Runtime.asmdef`
```json
{
    "name": "Tombakan.Runtime",
    "rootNamespace": "",
    "references": [
        "Unity.InputSystem",
        "Unity.TextMeshPro",
        "Unity.XR.ARFoundation",
        "Unity.XR.ARSubsystems",
        "Unity.XR.Interaction.Toolkit",
        "Unity.XR.Interaction.Toolkit.Samples.ARStarterAssets",
        "Unity.XR.Interaction.Toolkit.Samples.StarterAssets",
        "Unity.RenderPipelines.Universal.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

> **Critical:** Must include `Unity.XR.Interaction.Toolkit.Samples.ARStarterAssets` — `ARTemplateMenuManager.cs` imports that namespace. Missing this causes a full project compile failure.

### NEW: `Assets/Tests/EditMode/Tombakan.Tests.EditMode.asmdef`
```json
{
    "name": "Tombakan.Tests.EditMode",
    "rootNamespace": "",
    "references": [
        "Tombakan.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "optionalUnityReferences": ["TestAssemblies"]
}
```

### NEW: `Assets/Tests/PlayMode/Tombakan.Tests.PlayMode.asmdef`
```json
{
    "name": "Tombakan.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Tombakan.Runtime",
        "UnityEngine.TestRunner"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "optionalUnityReferences": ["TestAssemblies"]
}
```

---

## PHASE 3 — Write Tests
*Parallel: EditMode and PlayMode agents run simultaneously after Phase 2.*

### NEW: `Assets/Tests/EditMode/ColorLocalizationTests.cs`

> `ColorHexLocalization.Map` is already `public static readonly` — no Dict.cs changes needed.
> Correct Indonesian values (from actual Dict.cs): "Cokelat" not "Coklat", "Marun" not "Merah Tua", "Hijau Kebiruan" not "Teal", lime hex is "32CD32" not "00FF7F".

```csharp
using NUnit.Framework;
using UnityEngine;

public class ColorLocalizationTests
{
    [TestCase("00FF00", "Hijau")]
    [TestCase("FF0000", "Merah")]
    [TestCase("0000FF", "Biru")]
    [TestCase("FFFF00", "Kuning")]
    [TestCase("000000", "Hitam")]
    [TestCase("FFFFFF", "Putih")]
    [TestCase("FFA500", "Oranye")]
    [TestCase("800080", "Ungu")]
    [TestCase("FFC0CB", "Merah Muda")]
    [TestCase("A52A2A", "Cokelat")]
    [TestCase("808080", "Abu-abu")]
    [TestCase("00FFFF", "Sian")]
    [TestCase("FF00FF", "Magenta")]
    [TestCase("32CD32", "Hijau Muda")]
    [TestCase("000080", "Biru Tua")]
    [TestCase("800000", "Marun")]
    [TestCase("808000", "Zaitun")]
    [TestCase("008080", "Hijau Kebiruan")]
    [TestCase("C0C0C0", "Perak")]
    [TestCase("FFD700", "Emas")]
    public void ToIndonesian_KnownHex_ReturnsIndonesianName(string hex, string expected)
    {
        Assert.AreEqual(expected, ColorHexLocalization.ToIndonesian(hex));
    }

    [Test]
    public void ToIndonesian_UnknownHex_ReturnsFallbackHex()
    {
        Assert.AreEqual("ABCDEF", ColorHexLocalization.ToIndonesian("ABCDEF"));
    }

    [Test]
    public void ToIndonesian_LowercaseHex_StillMatches()
    {
        Assert.AreEqual("Merah", ColorHexLocalization.ToIndonesian("ff0000"));
    }

    [Test]
    public void ToIndonesian_NullInput_ReturnsNull()
    {
        Assert.IsNull(ColorHexLocalization.ToIndonesian((string)null));
    }

    [Test]
    public void ToIndonesian_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, ColorHexLocalization.ToIndonesian(string.Empty));
    }

    [Test]
    public void ToIndonesian_ColorGreen_ReturnsHijau()
    {
        Assert.AreEqual("Hijau", ColorHexLocalization.ToIndonesian(Color.green));
    }

    [Test]
    public void ToIndonesian_ColorRed_ReturnsMerah()
    {
        Assert.AreEqual("Merah", ColorHexLocalization.ToIndonesian(Color.red));
    }

    [Test]
    public void ToIndonesian_ColorBlue_ReturnsBiru()
    {
        Assert.AreEqual("Biru", ColorHexLocalization.ToIndonesian(Color.blue));
    }

    [Test]
    public void ToIndonesian_UnknownColor_ReturnsFallbackHex()
    {
        string result = ColorHexLocalization.ToIndonesian(new Color(0.1f, 0.2f, 0.3f, 1f));
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual(6, result.Length);
    }

    [Test]
    public void Map_HasExactlyTwentyEntries()
    {
        Assert.AreEqual(20, ColorHexLocalization.Map.Count);
    }

    [Test]
    public void Map_AllKeysAreUppercaseSixChars()
    {
        foreach (var key in ColorHexLocalization.Map.Keys)
        {
            Assert.AreEqual(6, key.Length, $"Key '{key}' is not 6 chars");
            Assert.AreEqual(key.ToUpperInvariant(), key, $"Key '{key}' is not uppercase");
        }
    }

    [Test]
    public void Map_AllValuesAreNonEmpty()
    {
        foreach (var value in ColorHexLocalization.Map.Values)
            Assert.IsFalse(string.IsNullOrWhiteSpace(value), "A map value is null or whitespace");
    }
}
```

### NEW: `Assets/Tests/EditMode/GameConstantsTests.cs`
```csharp
using NUnit.Framework;

public class GameConstantsTests
{
    [Test]
    public void PointPerCorrectHit_Is100()
    {
        Assert.AreEqual(100, GameConstants.PointPerCorrectHit);
    }

    [Test]
    public void PenaltyPerWrongHit_Is25()
    {
        Assert.AreEqual(25, GameConstants.PenaltyPerWrongHit);
    }

    [Test]
    public void GameDuration_Is60Seconds()
    {
        Assert.AreEqual(60f, GameConstants.GameDuration, 0.001f);
    }

    [Test]
    public void WarningThreshold_Is10Seconds()
    {
        Assert.AreEqual(10f, GameConstants.WarningTimeThreshold, 0.001f);
    }

    [TestCase(0, 0)]
    [TestCase(-1, 0)]
    [TestCase(1, 1)]
    [TestCase(2, 1)]
    [TestCase(3, 2)]
    [TestCase(4, 2)]
    [TestCase(5, 3)]
    [TestCase(100, 3)]
    public void GetTier_ReturnsCorrectTier(int correctHits, int expectedTier)
    {
        Assert.AreEqual(expectedTier, GameConstants.GetTier(correctHits));
    }
}
```

### NEW: `Assets/Tests/PlayMode/GameManagerPlayModeTests.cs`

> **Manual setup required before running:** Create `Assets/Tests/PlayMode/TombakanTestScene.unity` in the Unity Editor.
> The scene needs: GameManager + AudioManager (with AudioSource) GameObjects wired together.
> No ARSession or ARFoundation components needed.

```csharp
// ═══════════════════════════════════════════════════════════
// MANUAL SETUP REQUIRED before running PlayMode tests:
// In Unity Editor, create: Assets/Tests/PlayMode/TombakanTestScene.unity
// Scene contents:
//   - GameObject "GameManager" with GameManager.cs component
//     Wire all SerializeField UI references to lightweight stub objects
//   - GameObject "AudioManager" with AudioManager.cs + AudioSource
//     Wire mainBGMSource, gameplayBGMSource, sfxSource
//   - No ARSession, ARRaycastManager, or ARFoundation components needed
// Add scene to Build Settings before running PlayMode tests
// ═══════════════════════════════════════════════════════════

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class GameManagerPlayModeTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("TombakanTestScene", LoadSceneMode.Single);
        yield return null;
    }

    [UnityTest]
    public IEnumerator StartGame_ResetsScoreToZero()
    {
        GameManager.I.score = 999;
        GameManager.I.StartGame();
        yield return null;
        Assert.AreEqual(0, GameManager.I.score);
    }

    [UnityTest]
    public IEnumerator StartGame_ResetsCorrectHitCount()
    {
        GameManager.I.correctHitCount = 10;
        GameManager.I.StartGame();
        yield return null;
        Assert.AreEqual(0, GameManager.I.correctHitCount);
    }

    [UnityTest]
    public IEnumerator StartGame_SetsGameRunningTrue()
    {
        GameManager.I.StartGame();
        yield return null;
        Assert.IsTrue(GameManager.I.gameRunning);
    }

    [UnityTest]
    public IEnumerator OnFishHit_CorrectColor_AddsHundredPoints()
    {
        GameManager.I.StartGame();
        yield return null;
        Color target = GameManager.I.targetColor;
        int before = GameManager.I.score;
        GameManager.I.OnFishHit(target);
        yield return null;
        Assert.AreEqual(before + GameConstants.PointPerCorrectHit, GameManager.I.score);
    }

    [UnityTest]
    public IEnumerator OnFishHit_CorrectColor_IncrementsCorrectHitCount()
    {
        GameManager.I.StartGame();
        yield return null;
        Color target = GameManager.I.targetColor;
        int before = GameManager.I.correctHitCount;
        GameManager.I.OnFishHit(target);
        yield return null;
        Assert.AreEqual(before + 1, GameManager.I.correctHitCount);
    }

    [UnityTest]
    public IEnumerator OnFishHit_WrongColor_SubtractsTwentyFivePoints()
    {
        GameManager.I.StartGame();
        yield return null;
        Color wrong = GameManager.I.targetColor == Color.red ? Color.green : Color.red;
        int before = GameManager.I.score;
        GameManager.I.OnFishHit(wrong);
        yield return null;
        Assert.AreEqual(before - GameConstants.PenaltyPerWrongHit, GameManager.I.score);
    }

    [UnityTest]
    public IEnumerator Timer_WhenExpired_SetsGameNotRunning()
    {
        GameManager.I.StartGame();
        yield return null;
        GameManager.I.timeLeft = 0.016f;
        yield return new WaitForSeconds(0.1f);
        Assert.IsFalse(GameManager.I.gameRunning);
    }
}
```

---

## PHASE 4 — GitHub Actions CI
*Can start after Phase 2; independent of Phase 3.*

### NEW: `.github/workflows/unity-tests.yml`

```yaml
name: Unity Tests

on:
  push:
    branches: [master]
  pull_request:
    branches: [master]

jobs:
  test:
    name: Run {{ matrix.testMode }} Tests
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix:
        testMode: [editmode, playmode]

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          lfs: false

      - name: Cache Library
        uses: actions/cache@v4
        with:
          path: Library
          key: Library-{{ matrix.testMode }}-{{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
          restore-keys: |
            Library-{{ matrix.testMode }}-
            Library-

      - name: Run Tests
        uses: game-ci/unity-test-runner@v4
        env:
          UNITY_LICENSE: {{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: {{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: {{ secrets.UNITY_PASSWORD }}
        with:
          projectPath: .
          unityVersion: 2022.3.62f1
          testMode: {{ matrix.testMode }}
          artifactsPath: TestResults/{{ matrix.testMode }}
          githubToken: {{ secrets.GITHUB_TOKEN }}
          checkName: "{{ matrix.testMode }} Test Results"

      - name: Upload Results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: TestResults-{{ matrix.testMode }}
          path: TestResults/{{ matrix.testMode }}
```

> **Note:** Replace all `{{ }}` with `${{ }}` in the actual YAML file — the `$` is omitted here to prevent markdown rendering issues.

**Required GitHub Secrets** (repo Settings → Secrets → Actions → New repository secret):

| Secret | Value |
|---|---|
| `UNITY_LICENSE` | Full contents of your `.ulf` Unity personal license file |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password |

**How to get `.ulf`:** Follow https://game-ci.com/docs/github/activation — generates a `.alf` file, upload to license.unity3d.com, download the `.ulf`, paste contents into the secret.

---

## PHASE 5 — Claude Skills
*Fully parallel with Phases 3 & 4. All files in `.claude/skills/`.*

### Directory Structure
```
.claude/skills/
  game-developer/
    SKILL.md
    agents/unity-gameplay-engineer.md
    agents/ar-systems-engineer.md
  qa/
    SKILL.md
    agents/test-writer.md
    agents/bug-reporter.md
  uiux/
    SKILL.md
    agents/ui-reviewer.md
    agents/ux-flow-auditor.md
  3d-artist/
    SKILL.md
    agents/material-inspector.md
    agents/prefab-auditor.md
  release-checklist/
    SKILL.md
```

See the approved plan at `/root/.claude/plans/sleepy-gathering-meadow.md` for full content of all 13 skill files.

### Skills Summary

| Skill | Agents | Purpose |
|---|---|---|
| `/game-developer` | unity-gameplay-engineer, ar-systems-engineer | C#, AR, game loop, physics |
| `/qa` | test-writer, bug-reporter | Test writing, bug reports, CI |
| `/uiux` | ui-reviewer, ux-flow-auditor | HUD, onboarding, AR UX gaps |
| `/3d-artist` | material-inspector, prefab-auditor | URP materials, prefab structure |
| `/release-checklist` | (no agents) | Live audit of all ship requirements |

---

## PHASE 6 — Store Configuration
*Parallel with Phase 5.*

### EDIT: `ProjectSettings/ProjectSettings.asset`

| Field | Current | Fix |
|---|---|---|
| Android bundle ID | `com.unity.template.ar_mobile` | `com.aris.tombakan` |
| `AndroidMinSdkVersion` | `35` | `24` |
| `iOSCameraUsageDescription` | empty | `Tombakan menggunakan kamera untuk pengalaman AR memancing.` |

### NEW: `PRIVACY.md` (repo root)
```markdown
# Privacy Policy — Tombakan

**Effective date:** June 2, 2026

## Camera
Tombakan uses your device camera solely for Augmented Reality surface detection.
No images or video are captured, stored, or transmitted.

## Data Collection
Tombakan does not collect, store, or share any personal data.
There are no analytics, advertising, or tracking SDKs in this application.

## Contact
For privacy questions: arisjirat@gmail.com

## Changes
If this policy changes, the updated version will be posted before the change takes effect.
```

---

## Verification Checklist

| Check | How |
|---|---|
| EditMode tests pass | Unity: Window → General → Test Runner → EditMode → Run All |
| PlayMode tests pass | Unity: Window → General → Test Runner → PlayMode → Run All |
| CI passes | Push to master → GitHub Actions both matrix jobs green |
| Bundle ID fixed | Confirm `Android: com.aris.tombakan` in ProjectSettings.asset |
| Skills work | Type `/game-developer`, `/qa`, `/uiux`, `/3d-artist`, `/release-checklist` |
| Release audit | Run `/release-checklist` — all items DONE |
| On-device AR | APK on ARCore device: plane detection, 60s loop, Indonesian result names |

## Store Asset Requirements (manual — needs physical device)

**Google Play:**
- App icon: 512×512 PNG
- Feature graphic: 1024×500 PNG
- Screenshots: ≥2 phone screenshots showing AR in action (must be on real device)
- Short description: ≤80 chars, Indonesian + English
- Full description: ≤4000 chars
- Privacy policy URL (can use GitHub raw URL of PRIVACY.md)

**App Store:**
- App icon: 1024×1024 PNG, no alpha channel
- Screenshots: ≥3 at 1242×2688 (iPhone 6.5")
- Privacy policy URL

> Unity splash screen (`m_ShowUnitySplashScreen: 1`) cannot be removed on Personal license — it will appear on every launch.
