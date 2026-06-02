using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Persistent HUD overlay showing player level and XP bar.
/// Attach anywhere in the scene; call Refresh() after XP changes.
/// All references are optional/null-safe.
/// </summary>
public class ProgressionHUD : MonoBehaviour
{
    public TMP_Text levelText;    // e.g. "Lv 3"
    public Image xpBarFill;       // fill amount 0–1
    public TMP_Text xpText;       // e.g. "120 / 316 XP" (optional)
    public TMP_Text coinText;     // e.g. "Koin: 80" (optional)

    void OnEnable() => Refresh();

    public void Refresh()
    {
        int lv      = ProgressionStore.GetLevel();
        int xpIn    = ProgressionStore.XpIntoCurrentLevel();
        int xpNeeds = ProgressionStore.XpToNextLevel();

        if (levelText != null)
            levelText.text = $"Lv {lv}";

        if (xpBarFill != null)
        {
            int span  = xpIn + xpNeeds;
            xpBarFill.fillAmount = span > 0 ? (float)xpIn / span : 1f;
        }

        if (xpText != null)
        {
            int nextLvXp = ProgressionRules.XpForLevel(lv + 1);
            xpText.text  = lv >= ProgressionRules.MaxLevel
                ? "MAX LEVEL"
                : $"{xpIn} / {xpIn + xpNeeds} XP";
        }

        if (coinText != null)
            coinText.text = $"Koin: {CurrencyStore.GetCoins()}";
    }
}
