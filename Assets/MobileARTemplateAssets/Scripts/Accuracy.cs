using UnityEngine;

/// <summary>
/// Pure accuracy maths for the end-of-game stat (Week 3 UX-09).
/// Static + dependency-free so it is unit-testable.
/// </summary>
public static class Accuracy
{
    /// <summary>
    /// Rounded integer accuracy percent over all fish-hit attempts
    /// (correct + wrong). Returns 0 when there were no attempts.
    /// </summary>
    public static int Percent(int correct, int wrong)
    {
        int total = correct + wrong;
        if (total <= 0)
            return 0;

        return Mathf.RoundToInt(100f * correct / total);
    }

    /// <summary>Result-screen readout, e.g. "9/12 (75%)".</summary>
    public static string Format(int correct, int wrong)
    {
        int total = correct + wrong;
        return $"{correct}/{total} ({Percent(correct, wrong)}%)";
    }
}
