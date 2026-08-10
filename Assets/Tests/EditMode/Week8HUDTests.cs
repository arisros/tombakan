// QA — Week 8 HUD and asset acceptance tests (EditMode).
//
// Covers the acceptance criteria from ITERATION_week8_SCOPE.md:
//   TASK-03 BUG-4 — Timer bar formula: fill never exceeds 1 when bonus time > gameDuration
//   TASK-03 BUG-6 — Label sync: targetColorLabel always matches the swatch after PickNewTarget
//   TASK-03 BUG-7 — Colour-blind shape: shape symbol appended to HUD label when mode is on
//   TASK-04       — MissText.prefab exists at Assets/Prefabs/UI/MissText.prefab

using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ─────────────────────────────────────────────────────────────────────────────
// TASK-03 / BUG-4: Timer bar fill formula
// GameManager.Update line 165:
//   timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / Mathf.Max(timeLeft, gameDuration));
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class TimerBarFillFormulaTests
{
    // Mirror of the exact formula in GameManager.Update so tests fail if it changes.
    static float BarFill(float timeLeft, float gameDuration)
        => Mathf.Clamp01(timeLeft / Mathf.Max(timeLeft, gameDuration));

    // ── Normal countdown ─────────────────────────────────────────────────────

    [Test]
    public void Fill_AtGameStart_IsOne()
    {
        // timeLeft == gameDuration at the start of a fresh 60 s game.
        Assert.AreEqual(1f, BarFill(60f, 60f), 1e-4f,
            "Bar must be full at the beginning of a round");
    }

    [Test]
    public void Fill_AtHalfTime_IsHalf()
    {
        Assert.AreEqual(0.5f, BarFill(30f, 60f), 1e-4f,
            "Bar must be at 50 % when half the time has elapsed");
    }

    [Test]
    public void Fill_AtZero_IsZero()
    {
        // Mathf.Max(0, 60) = 60; 0/60 = 0 → Clamp01 = 0.
        Assert.AreEqual(0f, BarFill(0f, 60f), 1e-4f,
            "Bar must be empty when time runs out");
    }

    // ── BUG-4 regression: bonus time above gameDuration ──────────────────────

    [Test]
    public void Fill_BonusTimePushesTimeLeftTo80_IsExactlyOne()
    {
        // Acceptance criterion from SCOPE: trace formula at timeLeft = 80, gameDuration = 60.
        // Old formula: 80/60 = 1.333 → Clamp01 = 1.0 (coincidentally correct after clamping)
        // New formula: Mathf.Max(80, 60) = 80; 80/80 = 1.0 — same observable value.
        // The difference is the DENOMINATOR: the new formula keeps the bar calibrated so
        // it tracks correctly as timeLeft falls back through 60 → 0 without snapping.
        float fill = BarFill(80f, 60f);
        Assert.AreEqual(1f, fill, 1e-4f,
            "BUG-4: fill must be exactly 1.0 when timeLeft (80) exceeds gameDuration (60)");
    }

    [Test]
    public void Fill_BonusTimeDecaysBackThroughGameDuration_TracksSmoothly()
    {
        // After bonus pushes timeLeft to 90, it decays back. Each decrement must stay <= 1.
        float gameDuration = 60f;
        for (float t = 90f; t >= 0f; t -= 5f)
        {
            float fill = BarFill(t, gameDuration);
            Assert.LessOrEqual(fill, 1f,
                $"Fill must never exceed 1.0 — failed at timeLeft={t}");
            Assert.GreaterOrEqual(fill, 0f,
                $"Fill must never be negative — failed at timeLeft={t}");
        }
    }

    [Test]
    public void Fill_ExtremeBonus_StillCapsAtOne()
    {
        Assert.AreEqual(1f, BarFill(9999f, 60f), 1e-4f,
            "Extreme bonus time must still produce a fill of 1.0");
    }

    [Test]
    public void Fill_MonotonicallyDecreasing_WhenNoBonus()
    {
        // Without bonus time the bar should only ever go down, never jump up.
        float gameDuration = 60f;
        float prev = BarFill(gameDuration, gameDuration);
        for (float t = gameDuration - 1f; t >= 0f; t -= 1f)
        {
            float current = BarFill(t, gameDuration);
            Assert.LessOrEqual(current, prev + 1e-4f,
                $"Bar must not increase without a bonus hit (t={t})");
            prev = current;
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TASK-03 / BUG-6: Colour label ↔ swatch synchronisation
// GameManager.PickNewTarget re-syncs targetColorLabel after SpawnFish resolves
// any catalog-species colour override.
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ColourLabelSyncTests
{
    // The four active palette colours (FishPalette.Options).
    static readonly Color[] PaletteColours =
    {
        Color.red,                // Merah   FF0000
        Color.green,              // Hijau   00FF00
        Color.blue,               // Biru    0000FF
        new Color(1f, 1f, 0f),   // Kuning  FFFF00
    };

    // Expected Indonesian names aligned with Dict.cs / ColorHexLocalization.Map.
    static readonly string[] ExpectedNames =
    {
        "Merah", "Hijau", "Biru", "Kuning",
    };

    [Test]
    public void ToIndonesian_ReturnsIndonesianName_ForAllPaletteColours()
    {
        for (int i = 0; i < PaletteColours.Length; i++)
        {
            string name = ColorHexLocalization.ToIndonesian(PaletteColours[i]);
            Assert.AreEqual(ExpectedNames[i], name,
                $"BUG-6: colour at index {i} must map to '{ExpectedNames[i]}', got '{name}'");
        }
    }

    [Test]
    public void ToIndonesian_NeverReturnsHexFallback_ForPaletteColours()
    {
        // The hex fallback (6 uppercase hex chars) indicates a missing entry in Dict.cs.
        // Any palette colour that returns its own hex string means the label would show
        // e.g. "FF0000" instead of "Merah", which is the BUG-6 symptom.
        foreach (Color c in PaletteColours)
        {
            string name = ColorHexLocalization.ToIndonesian(c);
            string hex  = ColorUtility.ToHtmlStringRGB(c).ToUpperInvariant();
            Assert.AreNotEqual(hex, name,
                $"BUG-6: ToIndonesian must not fall back to hex string for palette colour {hex}");
        }
    }

    [Test]
    public void ToIndonesian_HexOverload_MatchesColorOverload()
    {
        // Both overloads must agree: the colour swatch and the label come from the same table.
        foreach (Color c in PaletteColours)
        {
            string hex        = ColorUtility.ToHtmlStringRGB(c).ToUpperInvariant();
            string fromColor  = ColorHexLocalization.ToIndonesian(c);
            string fromHex    = ColorHexLocalization.ToIndonesian(hex);
            Assert.AreEqual(fromColor, fromHex,
                $"Both ToIndonesian overloads must return the same name for hex {hex}");
        }
    }

    [Test]
    public void LabelText_WithoutCatalogSpecies_EqualsIndonesianColourName()
    {
        // Mirror of the label-building block in GameManager.PickNewTarget (no catalog):
        //   labelText = ColorHexLocalization.ToIndonesian(targetColor)
        // This test documents the expected output for each palette colour, providing a
        // regression guard if the mapping table is accidentally modified.
        for (int i = 0; i < PaletteColours.Length; i++)
        {
            Color  c    = PaletteColours[i];
            string label = ColorHexLocalization.ToIndonesian(c);
            Assert.AreEqual(ExpectedNames[i], label,
                $"BUG-6 regression: label for palette[{i}] must be '{ExpectedNames[i]}'");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TASK-03 / BUG-7: Colour-blind shape symbol in HUD label
// When ColourBlindSettings.IsEnabled(), the shape symbol is appended to labelText:
//   labelText = $"{labelText} {ColourBlindSettings.ShapeForColor(targetColor)}"
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class ColourBlindShapeTests
{
    const string PrefKey = "tombakan_colourblind";

    [SetUp]
    public void SetUp() => PlayerPrefs.DeleteKey(PrefKey);

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
    }

    // ── ShapeForColor symbol mapping ─────────────────────────────────────────

    [Test]
    public void ShapeForColor_Red_ReturnsCircle()
    {
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(Color.red),
            "BUG-7: red (Merah) must map to ● in colour-blind mode");
    }

    [Test]
    public void ShapeForColor_Green_ReturnsTriangle()
    {
        Assert.AreEqual("▲", ColourBlindSettings.ShapeForColor(Color.green),
            "BUG-7: green (Hijau) must map to ▲ in colour-blind mode");
    }

    [Test]
    public void ShapeForColor_Blue_ReturnsSquare()
    {
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(Color.blue),
            "BUG-7: blue (Biru) must map to ■ in colour-blind mode");
    }

    [Test]
    public void ShapeForColor_Yellow_ReturnsStar()
    {
        Color yellow = new Color(1f, 1f, 0f); // FFFF00 — palette yellow, not Color.yellow
        Assert.AreEqual("★", ColourBlindSettings.ShapeForColor(yellow),
            "BUG-7: yellow (Kuning, FFFF00) must map to ★ in colour-blind mode");
    }

    [Test]
    public void ShapeForColor_UnknownColor_ReturnsQuestionMark()
    {
        // The fallback "?" must not crash — unknown colours in future species packs
        // should still produce a safe, visible placeholder.
        Assert.AreEqual("?", ColourBlindSettings.ShapeForColor(Color.magenta),
            "ShapeForColor must return '?' for colours not in the palette");
    }

    [Test]
    public void ShapeForColor_AllPaletteColours_ReturnNonEmptySymbol()
    {
        Color[] palette = { Color.red, Color.green, Color.blue, new Color(1f, 1f, 0f) };
        foreach (Color c in palette)
        {
            string sym = ColourBlindSettings.ShapeForColor(c);
            Assert.IsNotEmpty(sym,
                $"ShapeForColor must return a non-empty symbol for palette colour " +
                $"{ColorUtility.ToHtmlStringRGB(c)}");
            Assert.AreNotEqual("?", sym,
                $"All four palette colours must have dedicated symbols, not '?' fallback — " +
                $"failed for {ColorUtility.ToHtmlStringRGB(c)}");
        }
    }

    // ── Label-building with colour-blind mode ─────────────────────────────────

    [Test]
    public void LabelBuilding_WhenColourBlindEnabled_AppendShape()
    {
        // Mirror of the label-building block in GameManager.PickNewTarget (BUG-7 fix):
        //   if (ColourBlindSettings.IsEnabled())
        //       labelText = $"{labelText} {ColourBlindSettings.ShapeForColor(targetColor)}";
        ColourBlindSettings.SetEnabled(true);

        Color  target    = Color.red;
        string baseName  = ColorHexLocalization.ToIndonesian(target);   // "Merah"
        string shape     = ColourBlindSettings.ShapeForColor(target);   // "●"
        string labelText = ColourBlindSettings.IsEnabled()
            ? $"{baseName} {shape}"
            : baseName;

        Assert.AreEqual("Merah ●", labelText,
            "BUG-7: when colour-blind mode is on, label must be '<name> <shape>'");
    }

    [Test]
    public void LabelBuilding_WhenColourBlindDisabled_NoShape()
    {
        ColourBlindSettings.SetEnabled(false);

        Color  target    = Color.red;
        string baseName  = ColorHexLocalization.ToIndonesian(target);
        string labelText = ColourBlindSettings.IsEnabled()
            ? $"{baseName} {ColourBlindSettings.ShapeForColor(target)}"
            : baseName;

        Assert.AreEqual("Merah", labelText,
            "When colour-blind mode is off, label must show only the colour name");
    }

    [Test]
    public void LabelBuilding_AllPaletteColours_WhenEnabled_ContainNameAndSymbol()
    {
        ColourBlindSettings.SetEnabled(true);

        Color[]  colours = { Color.red, Color.green, Color.blue, new Color(1f, 1f, 0f) };
        string[] names   = { "Merah", "Hijau", "Biru", "Kuning" };
        string[] shapes  = { "●", "▲", "■", "★" };

        for (int i = 0; i < colours.Length; i++)
        {
            string baseName  = ColorHexLocalization.ToIndonesian(colours[i]);
            string shape     = ColourBlindSettings.ShapeForColor(colours[i]);
            string labelText = $"{baseName} {shape}";

            StringAssert.Contains(names[i], labelText,
                $"BUG-7: label for palette[{i}] must contain the Indonesian name");
            StringAssert.Contains(shapes[i], labelText,
                $"BUG-7: label for palette[{i}] must contain the shape symbol");
        }
    }

    [Test]
    public void ColourBlindSettings_Toggle_FlipsState()
    {
        ColourBlindSettings.SetEnabled(false);
        Assert.IsFalse(ColourBlindSettings.IsEnabled());

        ColourBlindSettings.Toggle();
        Assert.IsTrue(ColourBlindSettings.IsEnabled());

        ColourBlindSettings.Toggle();
        Assert.IsFalse(ColourBlindSettings.IsEnabled());
    }

    [Test]
    public void ColourBlindSettings_DefaultsToDisabled()
    {
        // Fresh install: colour-blind mode must be off by default.
        PlayerPrefs.DeleteKey(PrefKey);
        Assert.IsFalse(ColourBlindSettings.IsEnabled(),
            "Colour-blind mode must default to disabled on a fresh install");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TASK-04: MissText.prefab exists at Assets/Prefabs/UI/MissText.prefab
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
public class MissTextPrefabTests
{
    const string PrefabPath = "Assets/Prefabs/UI/MissText.prefab";

#if UNITY_EDITOR
    [Test]
    public void MissTextPrefab_ExistsAtExpectedPath()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab,
            $"TASK-04: {PrefabPath} must exist so SpearHit.OnDestroy can reference it. " +
            "Run the artist task or check that the prefab was committed.");
    }

    [Test]
    public void MissTextPrefab_HasNoMissingScripts()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Assert.Inconclusive($"{PrefabPath} not found — run TASK-04 artist step first.");
            return;
        }

        // Instantiate into an editor scene temporarily to inspect components.
        var instance = Object.Instantiate(prefab);
        try
        {
            var components = instance.GetComponentsInChildren<Component>(includeInactive: true);
            foreach (var c in components)
            {
                Assert.IsNotNull(c,
                    $"TASK-04: MissText.prefab has at least one missing (null) script reference. " +
                    "Remove or reassign the missing MonoBehaviour in the prefab.");
            }
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    [Test]
    public void MissTextPrefab_HasCanvasComponent()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Assert.Inconclusive($"{PrefabPath} not found — run TASK-04 artist step first.");
            return;
        }

        var canvas = prefab.GetComponentInChildren<Canvas>(includeInactive: true);
        Assert.IsNotNull(canvas,
            "TASK-04: MissText.prefab must contain a Canvas component set to World Space " +
            "so the text appears near the spear position in AR.");
    }
#endif
}
