using UnityEngine;

/// <summary>
/// Persistent player XP and level. Backed by PlayerPrefs.
/// Pure curve logic lives in <see cref="ProgressionRules"/>.
/// </summary>
public static class ProgressionStore
{
    const string XpKey = "tombakan_total_xp";

    public static int GetTotalXp() => PlayerPrefs.GetInt(XpKey, 0);

    public static int GetLevel() => ProgressionRules.LevelForXp(GetTotalXp());

    /// <summary>
    /// Adds XP and persists. Returns the new level if a level-up occurred, else 0.
    /// </summary>
    public static int AddXp(int amount)
    {
        if (amount <= 0) return 0;
        int oldLevel = GetLevel();
        int newXp    = GetTotalXp() + amount;
        PlayerPrefs.SetInt(XpKey, newXp);
        PlayerPrefs.Save();
        int newLevel = ProgressionRules.LevelForXp(newXp);
        return newLevel > oldLevel ? newLevel : 0;
    }

    public static int XpIntoCurrentLevel() => ProgressionRules.XpIntoLevel(GetTotalXp());
    public static int XpToNextLevel()       => ProgressionRules.XpToNextLevel(GetTotalXp());
}
