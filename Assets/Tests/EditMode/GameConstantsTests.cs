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
