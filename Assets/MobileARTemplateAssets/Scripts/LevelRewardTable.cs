using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Tombakan/Level Reward Table")]
public class LevelRewardTable : ScriptableObject
{
    public List<LevelReward> rewards = new List<LevelReward>();

    /// <summary>Returns the reward for the given level, or null if none is configured.</summary>
    public LevelReward GetRewardForLevel(int level) =>
        rewards.FirstOrDefault(r => r.level == level);
}
