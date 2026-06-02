using UnityEngine;

/// <summary>
/// ScriptableObject container for all achievements in the game.
/// Assign Achievement assets to the <see cref="achievements"/> array in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "AchievementCatalog", menuName = "Tombakan/Achievement Catalog")]
public class AchievementCatalog : ScriptableObject
{
    [Tooltip("All achievements available in the game")]
    public Achievement[] achievements = new Achievement[0];

    /// <summary>
    /// Returns the Achievement with the given id, or null if not found.
    /// </summary>
    public Achievement GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var achievement in achievements)
            if (achievement != null && achievement.id == id)
                return achievement;
        return null;
    }
}
