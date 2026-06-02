using NUnit.Framework;

/// <summary>
/// EditMode tests for Phase 4/5: daily challenge, colour-blind settings logic.
/// All tests use pure/static methods — no scene, no PlayerPrefs.
/// </summary>
public class Phase45Tests
{
    // ── DailyChallenge pure logic ─────────────────────────────────────────

    [Test]
    public void DailyChallenge_IsNewDay_DifferentDate_True()
    {
        Assert.IsTrue(DailyChallenge.IsNewDay("2026-06-01", "2026-06-02"));
    }

    [Test]
    public void DailyChallenge_IsNewDay_SameDate_False()
    {
        Assert.IsFalse(DailyChallenge.IsNewDay("2026-06-02", "2026-06-02"));
    }

    [Test]
    public void DailyChallenge_IsNewDay_EmptyStored_True()
    {
        Assert.IsTrue(DailyChallenge.IsNewDay("", "2026-06-02"));
    }

    [Test]
    public void DailyChallenge_TotalBonusXp_ZeroStreak_IsBase()
    {
        Assert.AreEqual(DailyChallenge.DailyBonusXp, DailyChallenge.TotalBonusXp(0));
    }

    [Test]
    public void DailyChallenge_TotalBonusXp_Streak1_AddsBonus()
    {
        Assert.AreEqual(DailyChallenge.DailyBonusXp + DailyChallenge.StreakBonusXp,
                        DailyChallenge.TotalBonusXp(1));
    }

    [Test]
    public void DailyChallenge_TotalBonusXp_Streak7_Capped()
    {
        int at7  = DailyChallenge.TotalBonusXp(7);
        int at10 = DailyChallenge.TotalBonusXp(10);
        Assert.AreEqual(at7, at10, "Streak beyond 7 should not add more XP");
    }

    [Test]
    public void DailyChallenge_TotalBonusXp_NegativeStreak_IsBase()
    {
        Assert.AreEqual(DailyChallenge.DailyBonusXp, DailyChallenge.TotalBonusXp(-5));
    }

    // ── ColourBlindSettings shape mapping ─────────────────────────────────

    [Test]
    public void ColourBlind_ShapeForColor_Red_IsCircle()
    {
        Assert.AreEqual("●", ColourBlindSettings.ShapeForColor(UnityEngine.Color.red));
    }

    [Test]
    public void ColourBlind_ShapeForColor_Green_IsTriangle()
    {
        Assert.AreEqual("▲", ColourBlindSettings.ShapeForColor(UnityEngine.Color.green));
    }

    [Test]
    public void ColourBlind_ShapeForColor_Blue_IsSquare()
    {
        Assert.AreEqual("■", ColourBlindSettings.ShapeForColor(UnityEngine.Color.blue));
    }

    [Test]
    public void ColourBlind_ShapeForColor_Yellow_IsStar()
    {
        Assert.AreEqual("★", ColourBlindSettings.ShapeForColor(new UnityEngine.Color(1f, 1f, 0f)));
    }

    [Test]
    public void ColourBlind_ShapeForColor_Unknown_IsQuestionMark()
    {
        Assert.AreEqual("?", ColourBlindSettings.ShapeForColor(UnityEngine.Color.cyan));
    }

    [Test]
    public void ColourBlind_AllPaletteColors_HaveNonQuestionShape()
    {
        foreach (var color in FishPalette.Options)
        {
            string shape = ColourBlindSettings.ShapeForColor(color);
            Assert.AreNotEqual("?", shape, $"Palette colour {color} should have a dedicated shape");
        }
    }
}
