using System;
using UnityEngine;

/// <summary>
/// Tracks parental consent for premium spear skin purchases.
/// Consent is valid for 30 days after it is granted, then expires automatically.
/// </summary>
public static class ParentalGate
{
    public const string ConsentKey = "parentalConsent_timestamp";
    const int ConsentValidDays = 30;

    /// <summary>
    /// Returns true if consent was granted within the last 30 days.
    /// </summary>
    public static bool HasConsent
    {
        get
        {
            if (!PlayerPrefs.HasKey(ConsentKey)) return false;
            long stored = long.Parse(PlayerPrefs.GetString(ConsentKey, "0"));
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long cutoff = (long)ConsentValidDays * 24 * 60 * 60;
            return (now - stored) < cutoff;
        }
    }

    /// <summary>Saves the current Unix timestamp as the consent grant time.</summary>
    public static void GrantConsent()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(ConsentKey, now.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>Removes the stored consent timestamp, revoking consent immediately.</summary>
    public static void RevokeConsent()
    {
        PlayerPrefs.DeleteKey(ConsentKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns true when <paramref name="skin"/> requires parental consent
    /// (i.e. its currency is <see cref="SpearCurrency.Premium"/>).
    /// Returns false if <paramref name="skin"/> is null.
    /// </summary>
    public static bool IsPremiumSkin(SpearSkin skin) =>
        skin != null && skin.currency == SpearCurrency.Premium;
}
