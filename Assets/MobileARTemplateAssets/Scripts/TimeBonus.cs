using UnityEngine;

/// <summary>
/// Pure time-bonus rule (Week 4 UX-13). A correct hit grants a little extra time,
/// scaled by the current combo so hot streaks "breathe". Static + dependency-free
/// (beyond the shared combo rule) so it is unit-testable.
/// </summary>
public static class TimeBonus
{
    public const float BaseSeconds = 0.5f;

    /// <summary>Seconds to add to the timer for a correct hit at the given streak.</summary>
    public static float ForHit(int comboStreak)
    {
        return BaseSeconds * GameManager.ComboMultiplier(comboStreak);
    }
}
