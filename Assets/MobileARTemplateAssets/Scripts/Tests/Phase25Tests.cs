using NUnit.Framework;

/// <summary>
/// EditMode tests for Phase 2.5: Leveling &amp; Progression system.
/// All tests exercise pure/static ProgressionRules — no scene required.
/// </summary>
public class Phase25Tests
{
    // ── Level curve: XpForLevel ───────────────────────────────────────────

    [Test]
    public void XpForLevel_Level1_IsZero()
    {
        Assert.AreEqual(0, ProgressionRules.XpForLevel(1));
    }

    [Test]
    public void XpForLevel_Level0_IsZero()
    {
        Assert.AreEqual(0, ProgressionRules.XpForLevel(0));
    }

    [Test]
    public void XpForLevel_Level2_IsPositive()
    {
        Assert.Greater(ProgressionRules.XpForLevel(2), 0);
    }

    [Test]
    public void XpForLevel_Monotonic()
    {
        for (int lv = 2; lv <= ProgressionRules.MaxLevel; lv++)
            Assert.Greater(ProgressionRules.XpForLevel(lv), ProgressionRules.XpForLevel(lv - 1),
                $"XpForLevel({lv}) must exceed XpForLevel({lv - 1})");
    }

    // ── Level curve: LevelForXp ───────────────────────────────────────────

    [Test]
    public void LevelForXp_Zero_IsLevel1()
    {
        Assert.AreEqual(1, ProgressionRules.LevelForXp(0));
    }

    [Test]
    public void LevelForXp_Negative_IsLevel1()
    {
        Assert.AreEqual(1, ProgressionRules.LevelForXp(-100));
    }

    [Test]
    public void LevelForXp_AtThreshold_MatchesLevel()
    {
        for (int lv = 2; lv <= 10; lv++)
        {
            int xp = ProgressionRules.XpForLevel(lv);
            Assert.AreEqual(lv, ProgressionRules.LevelForXp(xp),
                $"LevelForXp(XpForLevel({lv})) should be {lv}");
        }
    }

    [Test]
    public void LevelForXp_JustBelowThreshold_IsOneLevelLower()
    {
        for (int lv = 2; lv <= 10; lv++)
        {
            int xp = ProgressionRules.XpForLevel(lv) - 1;
            Assert.AreEqual(lv - 1, ProgressionRules.LevelForXp(xp),
                $"LevelForXp({xp}) should be {lv - 1}");
        }
    }

    [Test]
    public void LevelForXp_LevelForXp_Inverse_Consistency()
    {
        // Round-trip: LevelForXp(XpForLevel(n)) == n for all reasonable levels
        for (int lv = 1; lv <= ProgressionRules.MaxLevel; lv++)
            Assert.AreEqual(lv, ProgressionRules.LevelForXp(ProgressionRules.XpForLevel(lv)));
    }

    // ── XpIntoLevel and XpToNextLevel ────────────────────────────────────

    [Test]
    public void XpIntoLevel_AtLevelThreshold_IsZero()
    {
        int xp = ProgressionRules.XpForLevel(5);
        Assert.AreEqual(0, ProgressionRules.XpIntoLevel(xp));
    }

    [Test]
    public void XpToNextLevel_PlusXpIntoLevel_EqualsLevelRange()
    {
        int totalXp = 150;
        int lv       = ProgressionRules.LevelForXp(totalXp);
        int into     = ProgressionRules.XpIntoLevel(totalXp);
        int toNext   = ProgressionRules.XpToNextLevel(totalXp);

        if (lv < ProgressionRules.MaxLevel)
            Assert.AreEqual(ProgressionRules.XpForLevel(lv + 1) - ProgressionRules.XpForLevel(lv),
                into + toNext, "into + toNext should span the full level range");
    }

    [Test]
    public void XpToNextLevel_AtMaxLevel_IsZero()
    {
        int xp = ProgressionRules.XpForLevel(ProgressionRules.MaxLevel);
        Assert.AreEqual(0, ProgressionRules.XpToNextLevel(xp));
    }

    // ── XpForResult ──────────────────────────────────────────────────────

    [Test]
    public void XpForResult_ZeroInputs_IsZero()
    {
        Assert.AreEqual(0, ProgressionRules.XpForResult(0, 0, 0, 0));
    }

    [Test]
    public void XpForResult_CorrectHits_Contribute()
    {
        int xp = ProgressionRules.XpForResult(10, 0, 0, 0);
        Assert.AreEqual(10 * ProgressionRules.XpPerCorrectHit, xp);
    }

    [Test]
    public void XpForResult_AccuracyBonus_Contributes()
    {
        int xp = ProgressionRules.XpForResult(0, 100, 0, 0);
        Assert.AreEqual(100 * ProgressionRules.XpPerAccuracyPct, xp);
    }

    [Test]
    public void XpForResult_NewSpecies_BigBonus()
    {
        int xp = ProgressionRules.XpForResult(0, 0, 3, 0);
        Assert.AreEqual(3 * ProgressionRules.XpNewSpecies, xp);
    }

    [Test]
    public void XpForResult_ComboBonus_OnlyAboveStreak1()
    {
        int xpNoCombo   = ProgressionRules.XpForResult(0, 0, 0, 1); // multiplier 1 → 0 bonus
        int xpWithCombo = ProgressionRules.XpForResult(0, 0, 0, 3); // multiplier 2 → +5
        Assert.Greater(xpWithCombo, xpNoCombo);
    }

    [Test]
    public void XpForResult_IsNeverNegative()
    {
        Assert.GreaterOrEqual(ProgressionRules.XpForResult(-5, -20, -1, -1), 0);
    }

    // ── ComboMultiplier ───────────────────────────────────────────────────

    [Test]
    public void ComboMultiplier_Streak0_Is1()
    {
        Assert.AreEqual(1, ProgressionRules.ComboMultiplierForStreak(0));
    }

    [Test]
    public void ComboMultiplier_Streak3_Is2()
    {
        Assert.AreEqual(2, ProgressionRules.ComboMultiplierForStreak(3));
    }

    [Test]
    public void ComboMultiplier_Streak5_Is3()
    {
        Assert.AreEqual(3, ProgressionRules.ComboMultiplierForStreak(5));
    }
}
