using NUnit.Framework;
using UnityEngine;

public class ColorLocalizationTests
{
    [TestCase("00FF00", "Hijau")]
    [TestCase("FF0000", "Merah")]
    [TestCase("0000FF", "Biru")]
    [TestCase("FFFF00", "Kuning")]
    [TestCase("000000", "Hitam")]
    [TestCase("FFFFFF", "Putih")]
    [TestCase("FFA500", "Oranye")]
    [TestCase("800080", "Ungu")]
    [TestCase("FFC0CB", "Merah Muda")]
    [TestCase("A52A2A", "Cokelat")]
    [TestCase("808080", "Abu-abu")]
    [TestCase("00FFFF", "Sian")]
    [TestCase("FF00FF", "Magenta")]
    [TestCase("32CD32", "Hijau Muda")]
    [TestCase("000080", "Biru Tua")]
    [TestCase("800000", "Marun")]
    [TestCase("808000", "Zaitun")]
    [TestCase("008080", "Hijau Kebiruan")]
    [TestCase("C0C0C0", "Perak")]
    [TestCase("FFD700", "Emas")]
    public void ToIndonesian_KnownHex_ReturnsIndonesianName(string hex, string expected)
    {
        Assert.AreEqual(expected, ColorHexLocalization.ToIndonesian(hex));
    }

    [Test]
    public void ToIndonesian_UnknownHex_ReturnsFallbackHex()
    {
        Assert.AreEqual("ABCDEF", ColorHexLocalization.ToIndonesian("ABCDEF"));
    }

    [Test]
    public void ToIndonesian_LowercaseHex_StillMatches()
    {
        Assert.AreEqual("Merah", ColorHexLocalization.ToIndonesian("ff0000"));
    }

    [Test]
    public void ToIndonesian_NullInput_ReturnsNull()
    {
        Assert.IsNull(ColorHexLocalization.ToIndonesian((string)null));
    }

    [Test]
    public void ToIndonesian_EmptyString_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, ColorHexLocalization.ToIndonesian(string.Empty));
    }

    [Test]
    public void ToIndonesian_ColorGreen_ReturnsHijau()
    {
        Assert.AreEqual("Hijau", ColorHexLocalization.ToIndonesian(Color.green));
    }

    [Test]
    public void ToIndonesian_ColorRed_ReturnsMerah()
    {
        Assert.AreEqual("Merah", ColorHexLocalization.ToIndonesian(Color.red));
    }

    [Test]
    public void ToIndonesian_ColorBlue_ReturnsBiru()
    {
        Assert.AreEqual("Biru", ColorHexLocalization.ToIndonesian(Color.blue));
    }

    [Test]
    public void ToIndonesian_UnknownColor_ReturnsFallbackHex()
    {
        string result = ColorHexLocalization.ToIndonesian(new Color(0.1f, 0.2f, 0.3f, 1f));
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual(6, result.Length);
    }

    [Test]
    public void Map_HasExactlyTwentyEntries()
    {
        Assert.AreEqual(20, ColorHexLocalization.Map.Count);
    }

    [Test]
    public void Map_AllKeysAreUppercaseSixChars()
    {
        foreach (var key in ColorHexLocalization.Map.Keys)
        {
            Assert.AreEqual(6, key.Length, $"Key '{key}' is not 6 chars");
            Assert.AreEqual(key.ToUpperInvariant(), key, $"Key '{key}' is not uppercase");
        }
    }

    [Test]
    public void Map_AllValuesAreNonEmpty()
    {
        foreach (var value in ColorHexLocalization.Map.Values)
            Assert.IsFalse(string.IsNullOrWhiteSpace(value), "A map value is null or whitespace");
    }
}
