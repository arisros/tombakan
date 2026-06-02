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

    // Maps palette colour → shape symbol shown on fish in colour-blind mode
    public static string ShapeForColor(Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color).ToUpperInvariant();
        return hex switch
        {
            "FF0000" => "●",   // Merah
            "00FF00" => "▲",   // Hijau
            "0000FF" => "■",   // Biru
            "FFFF00" => "★",   // Kuning
            _        => "?",
        };
    }
}
