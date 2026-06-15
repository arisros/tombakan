using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("Game State")]
    public int score;
    public int correctHitCount;
    public float gameDuration = 60f;
    public GameMode currentMode = GameMode.Standard;

    [Header("UI Screens")]
    public GameObject mainScreenUI;
    public GameObject gamePlayUI;

    [Header("HUD Score UI")]
    public TMP_Text scoreText;

    [Header("Timer UI")]
    public Image timerBarFill;
    public TMP_Text timerCountdownText;
    public float warningTimeThreshold = 10f;
    public Color timerNormalColor = Color.white;
    public Color timerWarningColor = Color.red;

    Coroutine timerPulseRoutine;
    bool timerWarningActive;

    [Header("Target Color UI")]
    public Image targetColorImage;
    public TMP_Text targetColorLabel;   // Indonesian colour/species word
    public TMP_Text targetSpeciesLabel; // optional — Indonesian species name when catalog active

    [Header("Spear")]
    public SpearThrower spearThrower;

    public float hitDelay = 2.2f;

    [Header("HUD Feedback UI")]
    public GameObject happyFeedback;
    public TMP_Text happyFeedbackText;
    public GameObject sadFeedback;
    public TMP_Text sadFeedbackText;

    [Header("Result UI")]
    public GameObject resultContainer;
    public TMP_Text resultScoreText;
    public TMP_Text resultCorrectFishText;
    public TMP_Text resultAccuracyText;   // e.g. "9/12 (75%)"
    public TMP_Text resultXpText;         // optional — XP earned this round

    public Image TierEmpty;
    public Image TierLow;
    public Image TierMid;
    public Image TierHigh;
    public Image TierLegend;

    [Header("Best Score UI (optional/null-safe)")]
    public TMP_Text bestScoreText;
    public TMP_Text resultBestScoreText;
    public GameObject newRecordBadge;

    [Header("Zen Mode Exit (optional/null-safe)")]
    public GameObject zenEndButton;   // shown only in Zen mode; onClick → EndGameManual()

    [Header("Progression HUD (optional/null-safe)")]
    public TMP_Text levelBadgeText;     // shows "Lv 3"
    public Image xpBarFill;             // fill amount 0–1

    [Header("Level-Up Celebration (optional/null-safe)")]
    public GameObject levelUpPanel;
    public TMP_Text levelUpText;        // e.g. "Level 5! Tombak baru terbuka!"

    [Header("Fish Managers")]
    public FishSpawner fishSpawner;

    [Header("Data")]
    public LevelRewardTable levelRewardTable;

    [Header("Achievements (optional/null-safe)")]
    [SerializeField] AchievementCatalog achievementCatalog;

    [Header("Achievement Toast (optional/null-safe)")]
    public GameObject achievementToastPanel;
    public TMP_Text achievementToastText;

    // --- Internal state ---

    Color targetColor;

    int pointPerCorrectHit = 100;
    int penaltyPerWrongHit = 25;

    int comboStreak;
    int maxComboStreak;
    int wrongHitCount;

    // --- Public read-only accessors for AchievementChecker ---
    public int MaxComboStreak => maxComboStreak;
    public int WrongHitCount  => wrongHitCount;

    List<string> collectedFishColors = new List<string>();

    // Species tracking (Phase 1)
    HashSet<string> newSpeciesThisGame = new HashSet<string>();
    List<string> collectedSpeciesIds   = new List<string>();

    float timeLeft;
    bool gameRunning;

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        if (AudioManager.I != null) AudioManager.I.PlayMainBGM();
        RefreshBestScoreUI();
        RefreshProgressionHUD();
    }

    void RefreshBestScoreUI()
    {
        int best = ScoreStore.GetBest();
        if (bestScoreText != null)
            bestScoreText.text = best.ToString();
        if (resultBestScoreText != null)
            resultBestScoreText.text = best.ToString();
    }

    void RefreshProgressionHUD()
    {
        int lv      = ProgressionStore.GetLevel();
        int xpIn    = ProgressionStore.XpIntoCurrentLevel();
        int xpNeeds = ProgressionStore.XpToNextLevel();

        if (levelBadgeText != null)
            levelBadgeText.text = $"Lv {lv}";

        if (xpBarFill != null)
        {
            float ratio = (xpIn + xpNeeds) > 0 ? (float)xpIn / (xpIn + xpNeeds) : 0f;
            xpBarFill.fillAmount = ratio;
        }
    }

    void Update()
    {
        if (!gameRunning) return;

        if (currentMode == GameMode.Zen)
        {
            // Zen mode: no countdown, no auto-end; timer bar stays full
            timerBarFill.fillAmount = 1f;
            return;
        }

        timeLeft -= Time.unscaledDeltaTime;

        timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / gameDuration);

        if (timerCountdownText != null)
            timerCountdownText.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)).ToString();

        if (timeLeft <= warningTimeThreshold && !timerWarningActive)
        {
            timerWarningActive = true;
            timerPulseRoutine  = StartCoroutine(TimerPulseWarning());
        }
        else if (timeLeft > warningTimeThreshold && timerWarningActive)
        {
            StopTimerWarning();
        }

        if (timeLeft <= 0f)
        {
            gameRunning = false;
            EndGame();
        }
    }

    /// <summary>
    /// Ends the game on demand. Intended for Zen mode where there is no countdown,
    /// so a UI button calls this instead of the timer expiring automatically.
    /// </summary>
    public void EndGameManual()
    {
        if (!gameRunning) return;
        gameRunning = false;
        EndGame();
    }

    void StopTimerWarning()
    {
        timerWarningActive = false;
        if (timerPulseRoutine != null)
        {
            StopCoroutine(timerPulseRoutine);
            timerPulseRoutine = null;
        }
        timerBarFill.color           = timerNormalColor;
        timerBarFill.transform.localScale = Vector3.one;
    }

    public void StartGame()
    {
        mainScreenUI.SetActive(false);
        gamePlayUI.SetActive(true);

        if (AudioManager.I != null) AudioManager.I.PlayGameplayBGM();

        score          = 0;
        correctHitCount = 0;
        wrongHitCount  = 0;
        comboStreak    = 0;
        maxComboStreak = 0;
        collectedFishColors.Clear();
        newSpeciesThisGame.Clear();
        collectedSpeciesIds.Clear();

        timeLeft    = currentMode == GameMode.Zen ? float.MaxValue : gameDuration;
        gameRunning = true;

        timerWarningActive = false;
        if (timerPulseRoutine != null)
        {
            StopCoroutine(timerPulseRoutine);
            timerPulseRoutine = null;
        }

        timerBarFill.fillAmount   = 1f;
        timerBarFill.color        = timerNormalColor;
        timerBarFill.transform.localScale = Vector3.one;

        if (timerCountdownText != null)
            timerCountdownText.text = currentMode == GameMode.Zen
                ? "--"
                : Mathf.CeilToInt(gameDuration).ToString();

        resultContainer.SetActive(false);
        if (newRecordBadge != null) newRecordBadge.SetActive(false);
        if (levelUpPanel   != null) levelUpPanel.SetActive(false);
        if (zenEndButton   != null) zenEndButton.SetActive(currentMode == GameMode.Zen);

        UpdateScoreUI();
        PickNewTarget();

        happyFeedback.SetActive(false);
        sadFeedback.SetActive(false);

        TombakanOnboarding.I?.NotifyGameStarted();
    }

    void EndGame()
    {
        if (fishSpawner  != null) fishSpawner.ClearAll();
        if (zenEndButton != null) zenEndButton.SetActive(false);

        resultContainer.SetActive(true);
        resultScoreText.text = score.ToString();

        if (AudioManager.I != null) AudioManager.I.PlayEnd();

        resultCorrectFishText.text = CollectSummary();

        if (resultAccuracyText != null)
            resultAccuracyText.text = Accuracy.Format(correctHitCount, wrongHitCount);

        bool isRecord = ScoreStore.TrySetBest(score);
        RefreshBestScoreUI();

        UpdateTierStars();

        // --- XP & Coins ---
        int accuracy  = Accuracy.Percent(correctHitCount, wrongHitCount);
        int xpEarned  = ProgressionRules.XpForResult(correctHitCount, accuracy, newSpeciesThisGame.Count, maxComboStreak);
        int newLevel  = ProgressionStore.AddXp(xpEarned);
        int coins     = CurrencyStore.EarnedForResult(correctHitCount, accuracy);
        CurrencyStore.AddCoins(coins);

        if (resultXpText != null)
            resultXpText.text = xpEarned > 0 ? $"+{xpEarned} XP" : "";

        RefreshProgressionHUD();

        // Stagger badge and level-up panel (0.4 s and 0.8 s after result screen)
        StartCoroutine(StaggerResultCelebrations(isRecord, newLevel));

        // --- Achievements ---
        int levelBeforeAchievements = ProgressionStore.GetLevel();
        if (achievementCatalog != null)
        {
            string[] newlyUnlocked = AchievementChecker.CheckAll(this, achievementCatalog);
            StartCoroutine(ShowAchievementsSequenced(newlyUnlocked, achievementCatalog));
        }
        // Achievement XP grants can silently level up; surface with panel + rewards after toasts
        int levelAfterAchievements = ProgressionStore.GetLevel();
        if (levelAfterAchievements > levelBeforeAchievements && newLevel == 0)
            StartCoroutine(DelayedAchievementLevelUp(levelAfterAchievements));

        if (timerPulseRoutine != null)
        {
            StopCoroutine(timerPulseRoutine);
            timerPulseRoutine = null;
        }
        timerBarFill.color            = timerNormalColor;
        timerBarFill.transform.localScale = Vector3.one;
    }

    string CollectSummary()
    {
        // When catalog active and species were caught, aggregate all catches (not only new ones)
        if (collectedSpeciesIds.Count > 0 && fishSpawner != null && fishSpawner.catalog != null)
        {
            var order  = new List<string>();
            var counts = new Dictionary<string, int>();
            foreach (var id in collectedSpeciesIds)
            {
                var sp   = fishSpawner.catalog.GetById(id);
                string n = sp != null ? sp.displayName : id;
                if (!counts.ContainsKey(n)) { counts[n] = 0; order.Add(n); }
                counts[n]++;
            }
            var parts = new List<string>();
            foreach (var n in order)
                parts.Add(counts[n] > 1 ? $"{n} ×{counts[n]}" : n);
            return string.Join(", ", parts);
        }
        return ColorSummary.Format(collectedFishColors);
    }

    void ShowLevelUp(int newLevel)
    {
        if (levelUpPanel == null) return;
        levelUpPanel.SetActive(true);
        if (levelUpText != null)
            levelUpText.text = $"Level {newLevel}! Selamat!";
    }

    System.Collections.IEnumerator StaggerResultCelebrations(bool isRecord, int newLevel)
    {
        yield return new WaitForSecondsRealtime(0.4f);

        if (isRecord && newRecordBadge != null)
        {
            newRecordBadge.SetActive(true);
            PunchScale(newRecordBadge.transform);
        }

        yield return new WaitForSecondsRealtime(0.4f);

        if (newLevel > 0)
        {
            ShowLevelUp(newLevel);
            ApplyLevelReward(levelRewardTable?.GetRewardForLevel(newLevel));
        }
    }

    // Fires level-up panel after achievement toasts have had time to display
    System.Collections.IEnumerator DelayedAchievementLevelUp(int level)
    {
        yield return new WaitForSecondsRealtime(3.5f);
        ShowLevelUp(level);
        ApplyLevelReward(levelRewardTable?.GetRewardForLevel(level));
    }

    System.Collections.IEnumerator ShowAchievementsSequenced(string[] ids, AchievementCatalog catalog)
    {
        yield return new WaitForSecondsRealtime(3.5f);

        foreach (string id in ids)
        {
            if (achievementToastPanel == null) yield break;

            var achievement = catalog?.GetById(id);
            string title = (achievement != null && !string.IsNullOrEmpty(achievement.titleIndonesian))
                ? achievement.titleIndonesian
                : id;

            if (achievementToastText != null)
                achievementToastText.text = title;

            achievementToastPanel.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            achievementToastPanel.SetActive(false);
        }
    }

    public void ApplyLevelReward(LevelReward reward)
    {
        if (reward == null) return;

        if (!string.IsNullOrEmpty(reward.unlockedSpeciesId))
            FishdexStore.Unlock(reward.unlockedSpeciesId);

        if (!string.IsNullOrEmpty(reward.unlockedSpearSkinId))
            SpearStore.EnsureDefault(reward.unlockedSpearSkinId);

        if (reward.softCurrencyBonus > 0)
            CurrencyStore.AddCoins(reward.softCurrencyBonus);

        if (levelUpText != null && !string.IsNullOrEmpty(reward.celebrationText))
            levelUpText.text = reward.celebrationText;
    }

    void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

    public void PickNewTarget()
    {
        if (!gameRunning) return;

        int activeColors = FishPalette.CountForProgress(correctHitCount);
        Color[] active   = FishPalette.ActiveOptions(activeColors);
        targetColor      = active[Random.Range(0, active.Length)];

        targetColorImage.color = targetColor;
        if (targetColorLabel != null)
            targetColorLabel.text = ColorHexLocalization.ToIndonesian(targetColor);

        fishSpawner.fishCount = FishCountForDifficulty(correctHitCount);
        fishSpawner.SpawnFish(targetColor, activeColors);

        // Update target UI with resolved species colour/name now that SpawnFish has run
        if (fishSpawner.CurrentTargetSpecies != null)
        {
            targetColor = fishSpawner.CurrentTargetSpecies.baseColor;
            targetColorImage.color = targetColor;
        }

        if (targetSpeciesLabel != null)
            targetSpeciesLabel.text = fishSpawner.CurrentTargetSpecies != null
                ? fishSpawner.CurrentTargetSpecies.displayName
                : "";
    }

    public static int FishCountForDifficulty(int correct)
    {
        if (correct >= 15) return 7;
        if (correct >= 10) return 6;
        if (correct >= 6)  return 5;
        if (correct >= 3)  return 4;
        return 3;
    }

    public static int ClampScore(int rawScore)
    {
        return Mathf.Max(0, rawScore);
    }

    public void OnFishHit(Color fishColor, string speciesId)
    {
        if (!gameRunning) return;
        bool correct = fishColor == targetColor;

        if (correct)
        {
            comboStreak++;
            if (comboStreak > maxComboStreak) maxComboStreak = comboStreak;

            int multiplier = ComboMultiplier(comboStreak);
            int earned     = pointPerCorrectHit * multiplier;

            score += earned;
            correctHitCount++;

            timeLeft += TimeBonus.ForHit(comboStreak);

            collectedFishColors.Add(ColorUtility.ToHtmlStringRGB(fishColor));

            // Species discovery (Phase 1)
            if (!string.IsNullOrEmpty(speciesId))
            {
                collectedSpeciesIds.Add(speciesId);
                bool isNew = FishdexStore.Unlock(speciesId);
                if (isNew) newSpeciesThisGame.Add(speciesId);
            }

            ShowHappy(earned, multiplier);
            if (AudioManager.I != null) AudioManager.I.PlayCorrect();
            if (ScreenShake.I != null) ScreenShake.I.ShakeOnCorrect();
            HapticFeedback.PlayCorrect();
        }
        else
        {
            comboStreak = 0;
            wrongHitCount++;
            int actualDeduction = Mathf.Min(penaltyPerWrongHit, score);
            score = ClampScore(score - penaltyPerWrongHit);
            ShowSad(actualDeduction);
            if (AudioManager.I != null) AudioManager.I.PlayWrong();
            if (ScreenShake.I != null) ScreenShake.I.ShakeOnWrong();
            HapticFeedback.PlayWrong();
        }

        UpdateScoreUI();

        float delay = PacingRules.HitDelayForProgress(hitDelay, correctHitCount);
        if (spearThrower) spearThrower.LockThrow(delay);
        Invoke(nameof(PickNewTarget), delay + 0.8f);
    }

    public static int ComboMultiplier(int streak)
    {
        if (streak >= 5) return 3;
        if (streak >= 3) return 2;
        return 1;
    }

    void ShowHappy(int earned, int multiplier)
    {
        happyFeedback.SetActive(true);
        happyFeedbackText.text = multiplier > 1
            ? $"x{multiplier} COMBO! +{earned}!"
            : $"+{earned}!";
        Invoke(nameof(HideFeedback), 1f);
    }

    void ShowSad(int actualDeduction)
    {
        sadFeedback.SetActive(true);
        sadFeedbackText.text = actualDeduction > 0 ? $"-{actualDeduction}!" : "Miss!";
        Invoke(nameof(HideFeedback), 1f);
    }

    void HideFeedback()
    {
        happyFeedback.SetActive(false);
        sadFeedback.SetActive(false);
    }

    void ResetTierStars()
    {
        TierEmpty.gameObject.SetActive(false);
        TierLow.gameObject.SetActive(false);
        TierMid.gameObject.SetActive(false);
        TierHigh.gameObject.SetActive(false);
        if (TierLegend != null) TierLegend.gameObject.SetActive(false);
    }

    public static int TierIndex(int correctHitCount)
    {
        if (correctHitCount <= 0)  return 0;
        if (correctHitCount <= 4)  return 1;
        if (correctHitCount <= 9)  return 2;
        if (correctHitCount <= 14) return 3;
        return 4;
    }

    void UpdateTierStars()
    {
        ResetTierStars();

        Image activeTier = TierIndex(correctHitCount) switch
        {
            0 => TierEmpty,
            1 => TierLow,
            2 => TierMid,
            3 => TierHigh,
            _ => TierLegend != null ? TierLegend : TierHigh,
        };

        activeTier.gameObject.SetActive(true);
        PunchScale(activeTier.transform);
    }

    void PunchScale(Transform target, float punchScale = 1.2f, float duration = 0.25f)
    {
        StartCoroutine(PunchRoutine(target, punchScale, duration));
    }

    System.Collections.IEnumerator PunchRoutine(Transform target, float punchScale, float duration)
    {
        Vector3 original = target.localScale;
        Vector3 big      = original * punchScale;
        float t = 0f;

        while (t < duration) { t += Time.unscaledDeltaTime; target.localScale = Vector3.Lerp(original, big, t / duration); yield return null; }
        t = 0f;
        while (t < duration) { t += Time.unscaledDeltaTime; target.localScale = Vector3.Lerp(big, original, t / duration); yield return null; }
        target.localScale = original;
    }

    System.Collections.IEnumerator TimerPulseWarning()
    {
        float pd = 0.4f;
        Vector3 n = Vector3.one;
        Vector3 p = Vector3.one * 1.1f;

        while (gameRunning)
        {
            timerBarFill.color = timerWarningColor;
            yield return StartCoroutine(ScaleLerp(timerBarFill.transform, n, p, pd * 0.5f));
            timerBarFill.color = timerNormalColor;
            yield return StartCoroutine(ScaleLerp(timerBarFill.transform, p, n, pd * 0.5f));
        }
    }

    System.Collections.IEnumerator ScaleLerp(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        target.localScale = to;
    }
}
