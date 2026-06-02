using UnityEngine;

/// <summary>
/// Pure pacing rules for the game loop (Week 3 UX-08).
/// Kept static so the tempo curve is unit-testable without a running scene.
/// </summary>
public static class PacingRules
{
    public const float MinHitDelay = 1.0f;
    public const float DelayReductionPerHit = 0.1f;

    /// <summary>
    /// Shrinks the inter-round lock delay as the player lands more correct hits,
    /// clamped between <see cref="MinHitDelay"/> and the supplied base delay.
    /// </summary>
    public static float HitDelayForProgress(float baseDelay, int correctHitCount)
    {
        float floor = Mathf.Min(MinHitDelay, baseDelay);
        float reduced = baseDelay - DelayReductionPerHit * Mathf.Max(0, correctHitCount);
        return Mathf.Clamp(reduced, floor, baseDelay);
    }
}
