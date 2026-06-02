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

    /// <summary>Pick a random palette colour that is not <paramref name="exclude"/>.</summary>
    public static Color RandomOther(Color exclude)
    {
        // Fallback guard: if the palette somehow has a single colour, return it.
        if (Options.Length <= 1)
            return Options[0];

        Color c;
        do
        {
            c = Options[Random.Range(0, Options.Length)];
        } while (c == exclude);

        return c;
    }
}
