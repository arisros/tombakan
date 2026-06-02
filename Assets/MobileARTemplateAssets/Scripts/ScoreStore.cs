using UnityEngine;

/// <summary>
/// Persistent best-score storage backed by PlayerPrefs (Week 2 UX-06).
/// Kept tiny and static so the "is this a new record?" decision is unit-testable.
/// </summary>
public static class ScoreStore
{
    public const string BestScoreKey = "tombakan_best_score";

    public static int GetBest()
    {
        return PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    /// <summary>
    /// Stores <paramref name="score"/> as the new best when it strictly beats the
    /// stored value. Returns true when a new record was set.
    /// </summary>
    public static bool TrySetBest(int score)
    {
        if (!IsNewRecord(score, GetBest()))
            return false;

        PlayerPrefs.SetInt(BestScoreKey, score);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>Pure record rule: a strictly higher score beats the current best.</summary>
    public static bool IsNewRecord(int score, int currentBest)
    {
        return score > currentBest;
    }
}
