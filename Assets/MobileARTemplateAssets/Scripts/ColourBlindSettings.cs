using UnityEngine;

/// <summary>
/// Colour-blind accessibility mode.
/// When enabled, fish display a shape symbol (●, ▲, ■, ★) alongside their colour
/// so players can identify the target without relying on colour perception alone.
/// </summary>
public static class ColourBlindSettings
{
    const string Key = "tombakan_colourblind";

    public static bool IsEnabled()    => PlayerPrefs.GetInt(Key, 0) == 1;
    public static void SetEnabled(bool v) { PlayerPrefs.SetInt(Key, v ? 1 : 0); PlayerPrefs.Save(); }
    public static void Toggle()       => SetEnabled(!IsEnabled());

    /// <summary>
    /// Maps any colour to a shape symbol using HSV hue-range classification so that
    /// all catalog species — not just the four legacy palette colours — get a symbol.
    /// TASK-02(c): replaces the exact-hex switch that returned "?" for every species colour.
    ///
    /// Buckets (hue in [0, 1)):
    ///   h &lt; 0.083 or h &gt; 0.917  →  ● (red)
    ///   h &lt; 0.250               →  ★ (orange / yellow)
    ///   h &lt; 0.458               →  ▲ (green)
    ///   otherwise               →  ■ (blue / cyan / purple)
    ///
    /// Low-saturation (s &lt; 0.15) override: near-greyscale colours map to ■.
    /// </summary>
    public static string ShapeForColor(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float _);

        // Near-greyscale: hue is meaningless, pick a neutral symbol.
        if (s < 0.15f) return "■";

        if (h < 0.083f || h > 0.917f) return "●";   // red
        if (h < 0.250f)               return "★";   // orange / yellow
        if (h < 0.458f)               return "▲";   // green
        return                                "■";   // blue / cyan / purple
    }
}
