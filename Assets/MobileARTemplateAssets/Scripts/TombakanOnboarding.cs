using TMPro;
using UnityEngine;

/// <summary>
/// Tombakan-specific onboarding wrapper.
/// Skips AR coaching steps for returning players (those with a saved best score or XP)
/// and shows a daily-streak greeting on first daily login.
/// Wire this up alongside GoalManager in the scene.
/// </summary>
public class TombakanOnboarding : MonoBehaviour
{
    [Header("References")]
    public GoalManager goalManager;          // the AR template onboarding manager

    [Header("Greeting UI (optional)")]
    public GameObject greetingPanel;         // shown on first-ever launch
    public GameObject dailyBonusPanel;       // shown when a daily bonus is claimed
    public TMP_Text dailyBonusText;          // e.g. "Bonus harian +100 XP! Streak 3 hari!"

    [Header("Throw Hint (first-time only)")]
    public GameObject throwHintPanel;        // panel with "Sentuh tombol untuk melempar tombak"
    public RectTransform throwHintPointer;   // optional visual arrow pointing at the throw button

    [Header("Settings")]
    public bool skipOnboardingForReturning = true; // if true, veteran players skip AR steps

    void Start()
    {
        bool isReturningPlayer = ScoreStore.GetBest() > 0 || ProgressionStore.GetTotalXp() > 0;

        if (isReturningPlayer && skipOnboardingForReturning && goalManager != null)
        {
            // Fast-forward through all coaching steps
            goalManager.ForceCompleteGoal();
        }
        else if (!isReturningPlayer && greetingPanel != null)
        {
            greetingPanel.SetActive(true);
        }

        // Check for daily bonus — claim on every session start
        if (DailyChallenge.TryClaimDailyBonus(out int xp, out int streak, out int newLevel))
        {
            ShowDailyBonus(xp, streak, newLevel);

            // Apply the level-up reward immediately so coins/unlocks are not lost
            if (newLevel > 0 && GameManager.I != null)
                GameManager.I.ApplyLevelReward(newLevel);
        }
    }

    void ShowDailyBonus(int xp, int streak, int newLevel = 0)
    {
        if (dailyBonusPanel == null) return;
        dailyBonusPanel.SetActive(true);

        if (dailyBonusText != null)
        {
            string baseText = streak > 1
                ? $"Bonus harian +{xp} XP!\nStreak {streak} hari berturut-turut!"
                : $"Bonus harian +{xp} XP!\nSelamat datang kembali!";
            dailyBonusText.text = newLevel > 0
                ? $"{baseText}\nLevel {newLevel}! Selamat!"
                : baseText;
        }

        if (newLevel > 0 && GameManager.I != null)
            GameManager.I.ApplyLevelReward(newLevel);

        Invoke(nameof(HideDailyBonus), 3f);
    }

    void HideDailyBonus()
    {
        if (dailyBonusPanel != null)
            dailyBonusPanel.SetActive(false);
    }

    public void DismissGreeting()
    {
        if (greetingPanel  != null) greetingPanel.SetActive(false);
        if (goalManager    != null) goalManager.StartCoaching();
        if (throwHintPanel != null) throwHintPanel.SetActive(true);
    }

    public void DismissThrowHint()
    {
        if (throwHintPanel != null) throwHintPanel.SetActive(false);
    }

    /// <summary>
    /// Called after AR placement succeeds (wire via UnityEvent or PlaceWaterOnPlane callback).
    /// Shows the throw-mechanic hint for first-time players only.
    /// </summary>
    public void StartCoaching()
    {
        bool isFirstTimePlayer = ProgressionStore.GetTotalXp() == 0;
        if (isFirstTimePlayer)
            ShowThrowHint();
    }

    /// <summary>
    /// Activates the throw hint panel and registers a one-shot listener that
    /// auto-dismisses it on the first throw event. Safe to call when panel is null.
    /// </summary>
    public void ShowThrowHint()
    {
        if (throwHintPanel == null) return;
        throwHintPanel.SetActive(true);
        if (throwHintPointer != null) throwHintPointer.gameObject.SetActive(true);
        SpearThrower.OnThrowFired += HideThrowHint;
    }

    /// <summary>
    /// Deactivates the throw hint panel and unsubscribes the one-shot listener.
    /// </summary>
    public void HideThrowHint()
    {
        SpearThrower.OnThrowFired -= HideThrowHint;
        if (throwHintPanel != null) throwHintPanel.SetActive(false);
        if (throwHintPointer != null) throwHintPointer.gameObject.SetActive(false);
    }
}
