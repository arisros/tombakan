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
    // TASK-02(b): needed to apply level rewards that come from daily-bonus XP grants.
    public GameManager gameManager;

    [Header("Greeting UI (optional)")]
    public GameObject greetingPanel;         // shown on first-ever launch
    public GameObject dailyBonusPanel;       // shown when a daily bonus is claimed
    public TMP_Text dailyBonusText;          // e.g. "Bonus harian +100 XP! Streak 3 hari!"

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
        int levelBefore = ProgressionStore.GetLevel();
        if (DailyChallenge.TryClaimDailyBonus(out int xp, out int streak))
        {
            int levelAfter = ProgressionStore.GetLevel();
            int newLevel = levelAfter > levelBefore ? levelAfter : 0;
            ShowDailyBonus(xp, streak, newLevel);
            // TASK-02(b): Apply the level reward so coins/unlocks from a daily-bonus
            // level-up are granted within the 3 s bonus panel window.
            if (newLevel > 0 && gameManager != null)
                gameManager.ApplyLevelReward(
                    gameManager.levelRewardTable?.GetRewardForLevel(newLevel));
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

        Invoke(nameof(HideDailyBonus), 3f);
    }

    void HideDailyBonus()
    {
        if (dailyBonusPanel != null)
            dailyBonusPanel.SetActive(false);
    }

    public void DismissGreeting()
    {
        if (greetingPanel != null) greetingPanel.SetActive(false);
        if (goalManager  != null) goalManager.StartCoaching();
    }
}
