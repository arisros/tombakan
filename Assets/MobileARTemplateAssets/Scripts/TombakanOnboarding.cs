using System.Collections;
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
    public static TombakanOnboarding I { get; private set; }

    [Header("References")]
    public GoalManager goalManager;          // the AR template onboarding manager

    [Header("Greeting UI (optional)")]
    public GameObject greetingPanel;         // shown on first-ever launch
    public GameObject dailyBonusPanel;       // shown when a daily bonus is claimed
    public TMP_Text dailyBonusText;          // e.g. "Bonus harian +100 XP! Streak 3 hari!"

    [Header("Throw Hint UI (optional)")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMPro.TMP_Text hintText;

    [Header("Settings")]
    public bool skipOnboardingForReturning = true; // if true, veteran players skip AR steps

    Coroutine _autoDismissRoutine;

    void Awake()
    {
        I = this;
    }

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
        if (DailyChallenge.TryClaimDailyBonus(out int xp, out int streak, out int bonusNewLevel))
        {
            // Apply rewards for every level crossed, not just the final one
            if (bonusNewLevel > 0 && GameManager.I != null)
            {
                int levelAfter = ProgressionStore.GetLevel();
                for (int lvl = levelBefore + 1; lvl <= levelAfter; lvl++)
                    GameManager.I.ApplyLevelReward(GameManager.I.levelRewardTable?.GetRewardForLevel(lvl));
            }
            ShowDailyBonus(xp, streak, bonusNewLevel);
        }
    }

    void ShowDailyBonus(int xp, int streak, int newLevel = 0)
    {
        // Rewards already applied via loop in Start() — UI only here

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

    // --- Throw hint ---

    /// <summary>
    /// Called by GameManager.StartGame() to trigger the delayed throw hint
    /// for players who haven't thrown yet.
    /// </summary>
    public void NotifyGameStarted()
    {
        StartCoroutine(ShowThrowHint());
    }

    /// <summary>
    /// Called by SpearThrower the moment the first spear is thrown.
    /// Cancels the pending auto-dismiss and hides the hint immediately.
    /// </summary>
    public void NotifyFirstThrow()
    {
        if (_autoDismissRoutine != null)
        {
            StopCoroutine(_autoDismissRoutine);
            _autoDismissRoutine = null;
        }
        if (hintPanel != null) hintPanel.SetActive(false);
    }

    IEnumerator ShowThrowHint()
    {
        yield return new WaitForSeconds(2f);

        // Only show if no throws have happened yet
        if (GameManager.I == null) yield break;
        if (GameManager.I.correctHitCount != 0 || GameManager.I.WrongHitCount != 0) yield break;

        if (hintText  != null) hintText.text = "Sentuh tombol untuk melempar tombak!";
        if (hintPanel != null) hintPanel.SetActive(true);

        _autoDismissRoutine = StartCoroutine(AutoDismissHint());
    }

    IEnumerator AutoDismissHint()
    {
        yield return new WaitForSeconds(5f);
        if (hintPanel != null) hintPanel.SetActive(false);
        _autoDismissRoutine = null;
    }
}
