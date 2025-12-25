using System.Collections.Generic;
using UnityEngine;

public static class ColorHexLocalization
{
    // Key = resulting from ColorUtility.ToHtmlStringRGB(color)
    public static readonly Dictionary<string, string> Map = new Dictionary<string, string>
    {
        { "00FF00", "Hijau" },
        { "FF0000", "Merah" },
        { "0000FF", "Biru" },
        { "FFFF00", "Kuning" },
        { "000000", "Hitam" },
        { "FFFFFF", "Putih" },
        { "FFA500", "Oranye" },
        { "800080", "Ungu" },
        { "FFC0CB", "Merah Muda" },
        { "A52A2A", "Cokelat" },
        { "808080", "Abu-abu" },
        { "00FFFF", "Sian" },
        { "FF00FF", "Magenta" },
        { "32CD32", "Hijau Muda" },
        { "000080", "Biru Tua" },
        { "800000", "Marun" },
        { "808000", "Zaitun" },
        { "008080", "Hijau Kebiruan" },
        { "C0C0C0", "Perak" },
        { "FFD700", "Emas" },
    };

    public static string ToIndonesian(Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);

        return Map.TryGetValue(hex, out var result) ? result : hex; // fallback aman
    }

    public static string ToIndonesian(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return hex;

        hex = hex.Replace("#", "").ToUpperInvariant();

        return Map.TryGetValue(hex, out var result) ? result : hex;
    }
};
