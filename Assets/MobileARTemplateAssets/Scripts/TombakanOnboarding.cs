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

    [Header("Settings")]
    public bool skipOnboardingForReturning = true; // if true, veteran players skip AR steps

    // Pending daily bonus data — populated when greeting is blocking
    bool   _pendingBonus;
    int    _pendingXp;
    int    _pendingStreak;
    int    _pendingNewLevel;

    void Start()
    {
        bool isReturningPlayer = ScoreStore.GetBest() > 0 || ProgressionStore.GetTotalXp() > 0;

        if (isReturningPlayer && skipOnboardingForReturning && goalManager != null)
        {
            // Only fast-forward when coaching has been initialised; calling
            // ForceCompleteGoal before StartCoaching populates m_OnboardingGoals
            // causes a NullReferenceException on returning-player launches (TASK-01).
            if (goalManager.IsCoachingActive)
                goalManager.ForceCompleteGoal();
        }
        else if (!isReturningPlayer && greetingPanel != null)
        {
            greetingPanel.SetActive(true);
        }

        // Check for daily bonus — claim on every session start.
        // TryClaimDailyBonus now surfaces the new level directly so we no longer
        // need a manual before/after GetLevel snapshot (TASK-02b).
        if (DailyChallenge.TryClaimDailyBonus(out int xp, out int streak, out int newLevel))
        {
            // If the greeting panel is currently visible, defer the bonus until it is dismissed
            bool greetingVisible = greetingPanel != null && greetingPanel.activeSelf;
            if (greetingVisible)
            {
                _pendingBonus    = true;
                _pendingXp       = xp;
                _pendingStreak   = streak;
                _pendingNewLevel = newLevel;
            }
            else
            {
                ShowDailyBonus(xp, streak, newLevel);
                // Apply LevelRewardTable reward (coins, species unlock, spear skin)
                // so it is never silently skipped on a daily-bonus level-up (TASK-02c).
                if (newLevel > 0)
                    GameManager.I?.ApplyLevelReward(newLevel);
            }
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

        // Show any daily bonus that was deferred while the greeting was visible
        if (_pendingBonus)
        {
            _pendingBonus = false;
            ShowDailyBonus(_pendingXp, _pendingStreak, _pendingNewLevel);
            // Apply LevelRewardTable reward deferred alongside the panel (TASK-02c).
            if (_pendingNewLevel > 0)
                GameManager.I?.ApplyLevelReward(_pendingNewLevel);
        }
    }
}
