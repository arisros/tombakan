using System.Collections.Generic;
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

    // Symbol bank — first four must stay in their existing positions so that the
    // four original palette entries keep the same symbols. Additional entries in
    // FishPalette.Options receive subsequent symbols from the bank.
    static readonly string[] SymbolBank = { "●", "▲", "■", "★", "◆", "▼", "✦", "⬟" };

    // Built once at class initialisation from the live FishPalette.Options array,
    // so any future palette extension is automatically covered.
    static readonly Dictionary<string, string> HexToShape = BuildMap();

    static Dictionary<string, string> BuildMap()
    {
        var map = new Dictionary<string, string>();
        for (int i = 0; i < FishPalette.Options.Length; i++)
        {
            string hex = ColorUtility.ToHtmlStringRGB(FishPalette.Options[i]).ToUpperInvariant();
            string symbol = i < SymbolBank.Length ? SymbolBank[i] : "?";
            // Only insert once in case two palette entries somehow produce the same hex.
            if (!map.ContainsKey(hex))
                map[hex] = symbol;
        }
        return map;
    }

    /// <summary>
    /// Maps a palette colour to a unique shape symbol for colour-blind mode.
    /// Returns "?" only for colours genuinely outside <see cref="FishPalette.Options"/>.
    /// </summary>
    public static string ShapeForColor(Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color).ToUpperInvariant();
        return HexToShape.TryGetValue(hex, out string shape) ? shape : "?";
    }
}
