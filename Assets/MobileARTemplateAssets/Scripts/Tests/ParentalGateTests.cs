// QA — Parental gate tests (EditMode).
// Covers ParentalGate consent lifecycle and SpearStore premium-skin gate.
using System;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ParentalGateTests
{
    [SetUp]
    public void SetUp()
    {
        // Start each test with a clean slate.
        PlayerPrefs.DeleteKey(ParentalGate.ConsentKey);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(ParentalGate.ConsentKey);
    }

    [Test]
    public void HasConsent_ReturnsFalse_WhenNoKeySet()
    {
        Assert.IsFalse(ParentalGate.HasConsent);
    }

    [Test]
    public void HasConsent_ReturnsTrue_AfterGrantConsent()
    {
        ParentalGate.GrantConsent();
        Assert.IsTrue(ParentalGate.HasConsent);
    }

    [Test]
    public void HasConsent_ReturnsFalse_AfterRevokeConsent()
    {
        ParentalGate.GrantConsent();
        ParentalGate.RevokeConsent();
        Assert.IsFalse(ParentalGate.HasConsent);
    }

    [Test]
    public void HasConsent_ReturnsFalse_WhenTimestampOlderThan30Days()
    {
        // Store a timestamp that is 31 days in the past.
        long pastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (31L * 24 * 60 * 60);
        PlayerPrefs.SetString(ParentalGate.ConsentKey, pastTimestamp.ToString());

        Assert.IsFalse(ParentalGate.HasConsent);
    }

    [Test]
    public void HasConsent_ReturnsTrue_WhenTimestampWithin30Days()
    {
        // Store a timestamp that is 29 days in the past (still valid).
        long recentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (29L * 24 * 60 * 60);
        PlayerPrefs.SetString(ParentalGate.ConsentKey, recentTimestamp.ToString());

        Assert.IsTrue(ParentalGate.HasConsent);
    }

    [Test]
    public void IsPremiumSkin_ReturnsFalse_ForNullSkin()
    {
        Assert.IsFalse(ParentalGate.IsPremiumSkin(null));
    }

    [Test]
    public void IsPremiumSkin_ReturnsFalse_ForCoinSkin()
    {
        var skin = ScriptableObject.CreateInstance<SpearSkin>();
        skin.currency = SpearCurrency.Coins;
        Assert.IsFalse(ParentalGate.IsPremiumSkin(skin));
        UnityEngine.Object.DestroyImmediate(skin);
    }

    [Test]
    public void IsPremiumSkin_ReturnsTrue_ForPremiumSkin()
    {
        var skin = ScriptableObject.CreateInstance<SpearSkin>();
        skin.currency = SpearCurrency.Premium;
        Assert.IsTrue(ParentalGate.IsPremiumSkin(skin));
        UnityEngine.Object.DestroyImmediate(skin);
    }
}

[TestFixture]
public class SpearStorePremiumGateTests
{
    const string TestId = "test_premium_skin";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(ParentalGate.ConsentKey);
        // Clear ownership so the test skin is not already owned.
        PlayerPrefs.DeleteKey("tombakan_owned_skins");
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(ParentalGate.ConsentKey);
        PlayerPrefs.DeleteKey("tombakan_owned_skins");
    }

    [Test]
    public void Buy_PremiumSkin_WithoutConsent_ReturnsNeedsParentalApproval()
    {
        BuyResult result = SpearStore.Buy(TestId, 0, SpearCurrency.Premium);
        Assert.AreEqual(BuyResult.NeedsParentalApproval, result);
    }

    [Test]
    public void Buy_PremiumSkin_WithConsent_ReturnsSuccess()
    {
        ParentalGate.GrantConsent();
        BuyResult result = SpearStore.Buy(TestId, 0, SpearCurrency.Premium);
        Assert.AreEqual(BuyResult.Success, result);
    }

    [Test]
    public void Buy_CoinSkin_WithoutConsent_DoesNotRequireGate()
    {
        // Ensure enough coins for a paid coin skin.
        PlayerPrefs.SetInt("tombakan_coins", 100);
        BuyResult result = SpearStore.Buy(TestId, 10, SpearCurrency.Coins);
        Assert.AreEqual(BuyResult.Success, result);
        PlayerPrefs.DeleteKey("tombakan_coins");
    }
}
