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

    // Exact palette lookup — built once from FishPalette.Options at class init.
    static readonly Dictionary<string, string> HexToShape = BuildMap();

    static Dictionary<string, string> BuildMap()
    {
        var map = new Dictionary<string, string>();
        for (int i = 0; i < FishPalette.Options.Length; i++)
        {
            string hex = ColorUtility.ToHtmlStringRGB(FishPalette.Options[i]).ToUpperInvariant();
            string symbol = i < SymbolBank.Length ? SymbolBank[i] : "?";
            if (!map.ContainsKey(hex))
                map[hex] = symbol;
        }
        return map;
    }

    /// <summary>
    /// Maps any colour to a shape symbol for colour-blind mode.
    /// Exact palette hex matches use the assigned symbol; any other colour
    /// (e.g. FishSpecies.baseColor for catalog species) falls back to hue proximity.
    /// </summary>
    public static string ShapeForColor(Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color).ToUpperInvariant();
        if (HexToShape.TryGetValue(hex, out string shape)) return shape;

        // Hue-proximity fallback for catalog species with non-palette colours
        Color.RGBToHSV(color, out float h, out float s, out float v);

        if (s < 0.15f || v < 0.15f)
            return v >= 0.5f ? "●" : "■";

        if (h >= 0.95f || h < 0.08f) return "●";   // Red
        if (h < 0.20f)               return "★";   // Yellow / Orange
        if (h < 0.50f)               return "▲";   // Green / Cyan
        return "■";                                 // Blue / Purple / Magenta
    }
}
