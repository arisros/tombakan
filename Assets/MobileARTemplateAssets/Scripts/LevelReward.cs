using System;

[Serializable]
public class LevelReward
{
    public int level;
    public string unlockedSpeciesId;   // empty = no species unlock
    public string unlockedSpearSkinId; // empty = no skin unlock
    public int softCurrencyBonus;      // 0 = no currency reward
    public string celebrationText;     // Indonesian, e.g. "Level 5! Tombak baru terbuka!"
}
