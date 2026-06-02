using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// EditMode tests for Phase 1: Fish Species + Fishipedia system.
/// All tests exercise pure/static logic — no scene, no MonoBehaviour required.
/// </summary>
public class Phase1Tests
{
    // ── FishdexStore pure logic ───────────────────────────────────────────

    [Test]
    public void Fishdex_EmptySet_IsNotUnlocked()
    {
        var set = new HashSet<string>();
        Assert.IsFalse(FishdexStore.IsUnlocked("ikan_badut", set));
    }

    [Test]
    public void Fishdex_Unlock_AddsToSet()
    {
        var set = new HashSet<string>();
        bool isNew = FishdexStore.Unlock("ikan_badut", set);
        Assert.IsTrue(isNew);
        Assert.IsTrue(FishdexStore.IsUnlocked("ikan_badut", set));
    }

    [Test]
    public void Fishdex_Unlock_Idempotent_ReturnsFalseOnDuplicate()
    {
        var set = new HashSet<string>();
        FishdexStore.Unlock("tongkol", set);
        bool secondCall = FishdexStore.Unlock("tongkol", set);
        Assert.IsFalse(secondCall, "Second unlock of same species must return false");
        Assert.AreEqual(1, FishdexStore.Count(set));
    }

    [Test]
    public void Fishdex_UnlockMultiple_CountMatches()
    {
        var set = new HashSet<string>();
        FishdexStore.Unlock("tongkol",  set);
        FishdexStore.Unlock("kakap",    set);
        FishdexStore.Unlock("kerapu",   set);
        Assert.AreEqual(3, FishdexStore.Count(set));
    }

    [Test]
    public void Fishdex_UnlockEmptyId_Ignored()
    {
        var set = new HashSet<string>();
        bool result = FishdexStore.Unlock("", set);
        Assert.IsFalse(result);
        Assert.AreEqual(0, FishdexStore.Count(set));
    }

    [Test]
    public void Fishdex_UnlockNull_Ignored()
    {
        var set = new HashSet<string>();
        bool result = FishdexStore.Unlock(null, set);
        Assert.IsFalse(result);
        Assert.AreEqual(0, FishdexStore.Count(set));
    }

    // ── FishCatalog rarity weighting ─────────────────────────────────────

    [Test]
    public void FishCatalog_WeightFor_Common_Is6()
    {
        Assert.AreEqual(6, FishCatalog.WeightFor(FishRarity.Common));
    }

    [Test]
    public void FishCatalog_WeightFor_Uncommon_Is3()
    {
        Assert.AreEqual(3, FishCatalog.WeightFor(FishRarity.Uncommon));
    }

    [Test]
    public void FishCatalog_WeightFor_Rare_Is1()
    {
        Assert.AreEqual(1, FishCatalog.WeightFor(FishRarity.Rare));
    }

    [Test]
    public void FishCatalog_WeightFor_Rare_LessThan_Common()
    {
        Assert.Less(FishCatalog.WeightFor(FishRarity.Rare), FishCatalog.WeightFor(FishRarity.Common));
    }
}
