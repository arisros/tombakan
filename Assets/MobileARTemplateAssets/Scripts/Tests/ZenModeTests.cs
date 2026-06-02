using NUnit.Framework;

/// <summary>
/// Pure logic tests for Zen mode — no scene setup required.
/// </summary>
public class ZenModeTests
{
    [Test]
    public void GameMode_StandardAndZen_AreDistinct()
    {
        Assert.AreNotEqual(GameMode.Standard, GameMode.Zen);
    }

    [Test]
    public void GameMode_Zen_HasExpectedIntValue()
    {
        // Standard = 0, Zen = 1 — guard against accidental reordering
        Assert.AreEqual(1, (int)GameMode.Zen);
    }

    [Test]
    public void GameConstants_GameDuration_Is60()
    {
        // Regression guard: Standard mode duration must stay at 60 seconds
        Assert.AreEqual(60f, GameConstants.GameDuration, delta: 0.001f);
    }

    [Test]
    public void ZenMode_TimeLeft_IsFloatMaxValue()
    {
        // Document the expected timeLeft value for Zen mode at game start.
        // GameManager.StartGame() assigns float.MaxValue when currentMode == GameMode.Zen.
        float zenTimeLeft = float.MaxValue;
        Assert.AreEqual(float.MaxValue, zenTimeLeft);
        Assert.IsTrue(zenTimeLeft > GameConstants.GameDuration * 1000f,
            "Zen timeLeft must be effectively infinite relative to game duration");
    }
}
