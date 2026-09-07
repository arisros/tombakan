using NUnit.Framework;

/// <summary>
/// Acceptance tests for TASK-04: Fix non-standard Indonesian colour names in Dict.cs.
/// 00FFFF must return "Toska"; FF00FF must return "Merah Lembayung".
/// No other entries may change.
/// </summary>
public class Week7_Task04_DictColorNameTests
{
    // AC: Dict.cs entry for 00FFFF returns "Toska".
    [Test]
    public void Cyan_00FFFF_ReturnsToska()
    {
        Assert.AreEqual("Toska", ColorHexLocalization.ToIndonesian("00FFFF"),
            "Cyan (00FFFF) must map to standard Indonesian 'Toska', not the English transliteration 'Sian'.");
    }

    // AC: Dict.cs entry for 00FFFF returns "Toska" — checked via Map directly.
    [Test]
    public void MapKey_00FFFF_IsToska()
    {
        Assert.AreEqual("Toska", ColorHexLocalization.Map["00FFFF"],
            "Map[\"00FFFF\"] must be \"Toska\".");
    }

    // AC: Dict.cs entry for FF00FF returns "Merah Lembayung".
    [Test]
    public void Magenta_FF00FF_ReturnsMerahLembayung()
    {
        Assert.AreEqual("Merah Lembayung", ColorHexLocalization.ToIndonesian("FF00FF"),
            "Magenta (FF00FF) must map to standard Indonesian 'Merah Lembayung', not the English loanword 'Magenta'.");
    }

    // AC: Dict.cs entry for FF00FF returns "Merah Lembayung" — checked via Map directly.
    [Test]
    public void MapKey_FF00FF_IsMerahLembayung()
    {
        Assert.AreEqual("Merah Lembayung", ColorHexLocalization.Map["FF00FF"],
            "Map[\"FF00FF\"] must be \"Merah Lembayung\".");
    }

    // Non-regression: old value "Sian" must not be present.
    [Test]
    public void Cyan_MapValue_IsNotOldSian()
    {
        Assert.AreNotEqual("Sian", ColorHexLocalization.Map["00FFFF"],
            "Old value 'Sian' must have been replaced with 'Toska'.");
    }

    // Non-regression: old value "Magenta" (English loanword) must not be present.
    [Test]
    public void Magenta_MapValue_IsNotOldMagentaLoanword()
    {
        Assert.AreNotEqual("Magenta", ColorHexLocalization.Map["FF00FF"],
            "Old value 'Magenta' must have been replaced with 'Merah Lembayung'.");
    }

    // AC: No other Dict.cs entries are altered.
    [Test]
    public void AllOtherEntries_AreUnchanged()
    {
        Assert.AreEqual("Hijau",          ColorHexLocalization.ToIndonesian("00FF00"));
        Assert.AreEqual("Merah",          ColorHexLocalization.ToIndonesian("FF0000"));
        Assert.AreEqual("Biru",           ColorHexLocalization.ToIndonesian("0000FF"));
        Assert.AreEqual("Kuning",         ColorHexLocalization.ToIndonesian("FFFF00"));
        Assert.AreEqual("Hitam",          ColorHexLocalization.ToIndonesian("000000"));
        Assert.AreEqual("Putih",          ColorHexLocalization.ToIndonesian("FFFFFF"));
        Assert.AreEqual("Oranye",         ColorHexLocalization.ToIndonesian("FFA500"));
        Assert.AreEqual("Ungu",           ColorHexLocalization.ToIndonesian("800080"));
        Assert.AreEqual("Merah Muda",     ColorHexLocalization.ToIndonesian("FFC0CB"));
        Assert.AreEqual("Cokelat",        ColorHexLocalization.ToIndonesian("A52A2A"));
        Assert.AreEqual("Abu-abu",        ColorHexLocalization.ToIndonesian("808080"));
        Assert.AreEqual("Hijau Muda",     ColorHexLocalization.ToIndonesian("32CD32"));
        Assert.AreEqual("Biru Tua",       ColorHexLocalization.ToIndonesian("000080"));
        Assert.AreEqual("Marun",          ColorHexLocalization.ToIndonesian("800000"));
        Assert.AreEqual("Zaitun",         ColorHexLocalization.ToIndonesian("808000"));
        Assert.AreEqual("Hijau Kebiruan", ColorHexLocalization.ToIndonesian("008080"));
        Assert.AreEqual("Perak",          ColorHexLocalization.ToIndonesian("C0C0C0"));
        Assert.AreEqual("Emas",           ColorHexLocalization.ToIndonesian("FFD700"));
    }

    // AC: Map still has exactly 20 entries after the substitution.
    [Test]
    public void Map_StillHasExactlyTwentyEntries()
    {
        Assert.AreEqual(20, ColorHexLocalization.Map.Count,
            "Dict.cs must still contain exactly 20 colour entries after the fix.");
    }
}
