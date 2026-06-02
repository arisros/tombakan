using UnityEngine;

/// <summary>
/// Pure data ScriptableObject describing a single achievement.
/// No logic — assign fields in the Inspector or via the catalog asset.
/// </summary>
[CreateAssetMenu(fileName = "Achievement", menuName = "Tombakan/Achievement")]
public class Achievement : ScriptableObject
{
    [Tooltip("Unique machine-readable identifier, e.g. \"first_catch\"")]
    public string id;

    [Tooltip("Achievement title shown to the player (Indonesian)")]
    public string titleIndonesian;

    [Tooltip("Achievement description shown to the player (Indonesian)")]
    public string descriptionIndonesian;

    [Tooltip("XP awarded when this achievement is first unlocked")]
    public int xpReward;

    [Tooltip("Optional icon displayed in the achievement list")]
    public Sprite icon;
}
