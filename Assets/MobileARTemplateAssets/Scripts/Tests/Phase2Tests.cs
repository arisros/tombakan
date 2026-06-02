using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// EditMode tests for Phase 2: Spear Cosmetics + Shop.
/// Exercises pure SpearStore and CurrencyStore logic — no scene, no MonoBehaviour.
/// </summary>
public class Phase2Tests
{
    // ── SpearStore pure logic ─────────────────────────────────────────────

    [Test]
    public void SpearStore_IsOwned_EmptySet_ReturnsFalse()
    {
        var owned = new HashSet<string>();
        Assert.IsFalse(SpearStore.IsOwned("spear_bamboo", owned));
    }

    [Test]
    public void SpearStore_Buy_AddsToSet()
    {
        var owned = new HashSet<string>();
        bool bought = SpearStore.Buy("spear_bamboo", owned);
        Assert.IsTrue(bought);
        Assert.IsTrue(SpearStore.IsOwned("spear_bamboo", owned));
    }

    [Test]
    public void SpearStore_Buy_Idempotent_ReturnsFalseIfAlreadyOwned()
    {
        var owned = new HashSet<string>();
        SpearStore.Buy("spear_bamboo", owned);
        bool secondBuy = SpearStore.Buy("spear_bamboo", owned);
        Assert.IsFalse(secondBuy, "Buying an already-owned skin must return false");
    }

    [Test]
    public void SpearStore_Buy_DoesNotDuplicateSet()
    {
        var owned = new HashSet<string>();
        SpearStore.Buy("spear_bamboo", owned);
        SpearStore.Buy("spear_bamboo", owned);
        Assert.AreEqual(1, owned.Count);
    }

    [Test]
    public void SpearStore_CanEquip_OnlyIfOwned()
    {
        var owned = new HashSet<string>();
        Assert.IsFalse(SpearStore.CanEquip("spear_bamboo", owned));
        SpearStore.Buy("spear_bamboo", owned);
        Assert.IsTrue(SpearStore.CanEquip("spear_bamboo", owned));
    }

    [Test]
    public void SpearStore_Buy_EmptyId_Ignored()
    {
        var owned = new HashSet<string>();
        bool result = SpearStore.Buy("", owned);
        Assert.IsFalse(result);
        Assert.AreEqual(0, owned.Count);
    }

    // ── CurrencyStore pure earn rule ─────────────────────────────────────

    [Test]
    public void Currency_EarnedForResult_ZeroInputs_IsZero()
    {
        Assert.AreEqual(0, CurrencyStore.EarnedForResult(0, 0));
    }

    [Test]
    public void Currency_EarnedForResult_CorrectHits_Contribute()
    {
        int coins = CurrencyStore.EarnedForResult(10, 0);
        Assert.AreEqual(10 * CurrencyStore.CoinsPerCorrectHit, coins);
    }

    [Test]
    public void Currency_EarnedForResult_AccuracyBonus_Adds1CoinPer10Pct()
    {
        int coins = CurrencyStore.EarnedForResult(0, 100);
        Assert.AreEqual(100 / CurrencyStore.AccuracyBonusDivisor, coins);
    }

    [Test]
    public void Currency_EarnedForResult_ClampedAccuracy_IgnoresOver100()
    {
        int coinsAt100 = CurrencyStore.EarnedForResult(0, 100);
        int coinsAt200 = CurrencyStore.EarnedForResult(0, 200);
        Assert.AreEqual(coinsAt100, coinsAt200);
    }

    [Test]
    public void Currency_EarnedForResult_IsNeverNegative()
    {
        Assert.GreaterOrEqual(CurrencyStore.EarnedForResult(-5, -50), 0);
    }

    [Test]
    public void Currency_EarnedForResult_Combined_CorrectTotal()
    {
        // 5 hits + 80% accuracy = 25 + 8 = 33
        int expected = 5 * CurrencyStore.CoinsPerCorrectHit + 80 / CurrencyStore.AccuracyBonusDivisor;
        Assert.AreEqual(expected, CurrencyStore.EarnedForResult(5, 80));
    }
}
