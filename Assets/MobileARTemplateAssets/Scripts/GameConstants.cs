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
