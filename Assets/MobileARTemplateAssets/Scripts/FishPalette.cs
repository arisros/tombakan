using UnityEngine;

/// <summary>
/// Single source of truth for the fish colours used by the game.
/// Centralising this removes the prior drift risk between GameManager's
/// target list and FishSpawner's decoy list (Week 2 BUG-05).
/// Every colour here MUST have an Indonesian mapping in ColorHexLocalization.
/// </summary>
public static class FishPalette
{
    public static readonly Color[] Options =
    {
        Color.red,                 // Merah  -> FF0000
        Color.green,               // Hijau  -> 00FF00
        Color.blue,                // Biru   -> 0000FF
        new Color(1f, 1f, 0f),     // Kuning -> FFFF00 (NOT Color.yellow, which is FFEB04)
    };

    public const int MinActiveColors = 3;

    /// <summary>
    /// How many palette colours are active at the player's current progress (Week 4 UX-12).
    /// Starts at <see cref="MinActiveColors"/> and adds one colour every 5 correct hits,
    /// clamped to the palette size.
    /// </summary>
    public static int CountForProgress(int correctHitCount)
    {
        int count = MinActiveColors + Mathf.Max(0, correctHitCount) / 5;
        return Mathf.Clamp(count, MinActiveColors, Options.Length);
    }

    /// <summary>Clamps a requested active-colour count into the valid palette range.</summary>
    public static int ClampActiveCount(int count)
    {
        return Mathf.Clamp(count, 1, Options.Length);
    }

    /// <summary>The first <paramref name="count"/> palette colours (clamped).</summary>
    public static Color[] ActiveOptions(int count)
    {
        int n = ClampActiveCount(count);
        var result = new Color[n];
        for (int i = 0; i < n; i++)
            result[i] = Options[i];
        return result;
    }

    /// <summary>Pick a random palette colour that is not <paramref name="exclude"/>.</summary>
    public static Color RandomOther(Color exclude)
    {
        return RandomOther(exclude, Options.Length);
    }

    /// <summary>
    /// Pick a random colour from the active subset that is not <paramref name="exclude"/>.
    /// </summary>
    public static Color RandomOther(Color exclude, int activeCount)
    {
        int n = ClampActiveCount(activeCount);

        // Fallback guard: a single active colour cannot exclude itself.
        if (n <= 1)
            return Options[0];

        Color c;
        do
        {
            c = Options[Random.Range(0, n)];
        } while (c == exclude);

        return c;
    }
}
