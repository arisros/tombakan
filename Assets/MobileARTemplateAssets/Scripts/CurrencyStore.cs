using UnityEngine;

/// <summary>
/// Soft-currency (Coins) store. Pure earn-rule separated from storage for testability.
/// </summary>
public static class CurrencyStore
{
    const string Key = "tombakan_coins";

    // --- Storage ---

    public static int GetCoins() => PlayerPrefs.GetInt(Key, 0);

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(Key, GetCoins() + amount);
        PlayerPrefs.Save();
    }

    /// <summary>Deducts <paramref name="amount"/> if funds are sufficient. Returns success.</summary>
    public static bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        int current = GetCoins();
        if (current < amount) return false;
        PlayerPrefs.SetInt(Key, current - amount);
        PlayerPrefs.Save();
        return true;
    }

    // --- Pure earn rule (testable) ---

    public const int CoinsPerCorrectHit   = 5;
    public const int AccuracyBonusDivisor = 10; // 1 coin per 10 accuracy %

    public static int EarnedForResult(int correctHits, int accuracy)
    {
        int coins = Mathf.Max(0, correctHits) * CoinsPerCorrectHit;
        coins += Mathf.Clamp(accuracy, 0, 100) / AccuracyBonusDivisor;
        return Mathf.Max(0, coins);
    }
}
