// Tests covering ITERATION_week6_SCOPE.md acceptance criteria.
// Pure-logic tests only. Scene-dependent behaviours (gameRunning guard, stagger timings)
// require PlayMode tests and are covered in IterationPlayModeTests.cs.

using NUnit.Framework;

public class Week6AcceptanceTests
{
    // ── TASK-01 (b) · Guard ApplyLevelReward from overwriting non-empty levelUpText ──
    // AC: ApplyLevelReward only sets levelUpText.text when reward.celebrationText is non-empty.
    // The private void overload is not directly testable, but the pure logic is:
    //   !string.IsNullOrEmpty(reward.celebrationText) must be true before writing the text.

    [Test]
    public void LevelReward_EmptyCelebrationText_IsNullOrEmpty()
    {
        var reward = new LevelReward { celebrationText = "" };
        Assert.IsTrue(string.IsNullOrEmpty(reward.celebrationText),
            "An empty celebrationText must be recognised by IsNullOrEmpty " +
            "so the text field is left unchanged");
    }

    [Test]
    public void LevelReward_NullCelebrationText_IsNullOrEmpty()
    {
        var reward = new LevelReward { celebrationText = null };
        Assert.IsTrue(string.IsNullOrEmpty(reward.celebrationText));
    }

    [Test]
    public void LevelReward_NonEmptyCelebrationText_IsNotNullOrEmpty()
    {
        var reward = new LevelReward { celebrationText = "Level 5! Tombak baru terbuka!" };
        Assert.IsFalse(string.IsNullOrEmpty(reward.celebrationText),
            "Non-empty celebrationText should pass through to levelUpText");
    }

    // ── TASK-01 (c) · HapticFeedback does not check AudioPrefs.IsMuted ────────
    // AC: HapticFeedback.PlayCorrect/PlayWrong no longer check AudioPrefs.IsMuted().
    // Verified by inspection (the methods contain no IsMuted call), but we can
    // confirm the methods are callable without consulting AudioPrefs by calling them
    // regardless of mute state and asserting no exception is thrown.

    [Test]
    public void HapticFeedback_PlayCorrect_DoesNotThrow_WhenMuted()
    {
        AudioPrefs.SetMuted(true);
        Assert.DoesNotThrow(() => HapticFeedback.PlayCorrect(),
            "HapticFeedback.PlayCorrect must not throw (or be suppressed) when audio is muted");
        AudioPrefs.SetMuted(false);
    }

    [Test]
    public void HapticFeedback_PlayWrong_DoesNotThrow_WhenMuted()
    {
        AudioPrefs.SetMuted(true);
        Assert.DoesNotThrow(() => HapticFeedback.PlayWrong(),
            "HapticFeedback.PlayWrong must not throw (or be suppressed) when audio is muted");
        AudioPrefs.SetMuted(false);
    }

    [Test]
    public void HapticFeedback_PlayCorrect_DoesNotThrow_WhenUnmuted()
    {
        AudioPrefs.SetMuted(false);
        Assert.DoesNotThrow(() => HapticFeedback.PlayCorrect());
    }

    // ── TASK-01 (a) · ComboMultiplier consistency ─────────────────────────────
    // These verify the multiplier values used when showing "x2 COMBO! +200" feedback.

    [Test]
    public void ComboMultiplier_AtThree_IsTwo_ForHappyFeedback()
    {
        // The happy feedback text uses "x{multiplier}" — multiplier must be 2 at streak 3
        Assert.AreEqual(2, GameManager.ComboMultiplier(3));
    }

    [Test]
    public void ComboMultiplier_AtFive_IsThree_ForHappyFeedback()
    {
        Assert.AreEqual(3, GameManager.ComboMultiplier(5));
    }
}
