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

    public Image TierEmpty;
    public Image TierLow;
    public Image TierMid;
    public Image TierHigh;
    public Image TierLegend;

    [Header("Fish Managers")]
    public FishSpawner fishSpawner;

    // Target color internal
    Color targetColor;
    Color[] fishColorOptions = { Color.green, Color.red, Color.blue };

    int pointPerCorrectHit = 100;
    int penaltyPerWrongHit = 25;

    // Combo streak
    int comboStreak;

    // Collected fish colors for result screen
    List<string> collectedFishColors = new List<string>();

    float timeLeft;

    // Game running state
    bool gameRunning;

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        AudioManager.I.PlayMainBGM();
    }

    void Update()
    {
        if (!gameRunning)
            return;

        timeLeft -= Time.unscaledDeltaTime;

        timerBarFill.fillAmount = Mathf.Clamp01(timeLeft / gameDuration);

        if (timerCountdownText != null)
            timerCountdownText.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)).ToString();

        if (timeLeft <= warningTimeThreshold && !timerWarningActive)
        {
            timerWarningActive = true;
            timerPulseRoutine = StartCoroutine(TimerPulseWarning());
        }

        if (timeLeft <= 0f)
        {
            gameRunning = false;
            EndGame();
        }
    }

    public void StartGame()
    {
        mainScreenUI.SetActive(false);
        gamePlayUI.SetActive(true);

        AudioManager.I.PlayGameplayBGM();

        // RESET STATE
        score = 0;
        correctHitCount = 0;
        comboStreak = 0;
        collectedFishColors.Clear();

        timeLeft = gameDuration;
        gameRunning = true;

        timerWarningActive = false;

        if (timerPulseRoutine != null)
        {
            StopCoroutine(timerPulseRoutine);
            timerPulseRoutine = null;
        }

        // RESET VISUAL TIMER
        timerBarFill.fillAmount = 1f;
        timerBarFill.color = timerNormalColor;
        timerBarFill.transform.localScale = Vector3.one;

        if (timerCountdownText != null)
            timerCountdownText.text = Mathf.CeilToInt(gameDuration).ToString();

        resultContainer.SetActive(false);

        UpdateScoreUI();
        PickNewTarget();

        happyFeedback.SetActive(false);
        sadFeedback.SetActive(false);
    }

    void EndGame()
    {
        resultContainer.SetActive(true);
        resultScoreText.text = score.ToString();

        AudioManager.I.PlayEnd();

        string[] mappedColors = new string[collectedFishColors.Count];

        for (int i = 0; i < collectedFishColors.Count; i++)
        {
            mappedColors[i] = ColorHexLocalization.ToIndonesian(collectedFishColors[i]);
        }

        resultCorrectFishText.text =
            collectedFishColors.Count > 0 ? string.Join(", ", mappedColors) : "Tidak ada ikan dikumpulkan";

        UpdateTierStars();

        if (timerPulseRoutine != null)
        {
            StopCoroutine(timerPulseRoutine);
            timerPulseRoutine = null;
        }

        timerBarFill.color = timerNormalColor;
        timerBarFill.transform.localScale = Vector3.one;
    }

    void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

    public void PickNewTarget()
    {
        int i = Random.Range(0, fishColorOptions.Length);
        targetColor = fishColorOptions[i];

        targetColorImage.color = targetColor;

        fishSpawner.fishCount = FishCountForDifficulty(correctHitCount);
        fishSpawner.SpawnFish(targetColor);
    }

    // Scales fish count with player progress. Pure + static so it is unit-testable.
    public static int FishCountForDifficulty(int correct)
    {
        if (correct >= 15) return 7;
        if (correct >= 10) return 6;
        if (correct >= 6)  return 5;
        if (correct >= 3)  return 4;
        return 3;
    }

    // Clamps score to a non-negative floor. Pure + static so it is unit-testable.
    public static int ClampScore(int rawScore)
    {
        return Mathf.Max(0, rawScore);
    }

    public void OnFishHit(Color fishColor)
    {
        bool correct = fishColor == targetColor;

        if (correct)
        {
            comboStreak++;
            int multiplier = ComboMultiplier(comboStreak);
            int earned = pointPerCorrectHit * multiplier;

            score += earned;
            correctHitCount++;

            collectedFishColors.Add(ColorUtility.ToHtmlStringRGB(fishColor));

            ShowHappy(earned, multiplier);
            AudioManager.I.PlayCorrect();
        }
        else
        {
            comboStreak = 0;
            score = ClampScore(score - penaltyPerWrongHit);
            ShowSad();
            AudioManager.I.PlayWrong();
        }

        UpdateScoreUI();

        if (spearThrower)
            spearThrower.LockThrow(hitDelay);

        Invoke(nameof(PickNewTarget), hitDelay + 0.8f);
    }

    // Returns score multiplier for the current streak. Pure + static so it is unit-testable.
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

    void ShowSad()
    {
        sadFeedback.SetActive(true);
        sadFeedbackText.text = $"-{penaltyPerWrongHit}!";
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
        if (TierLegend != null)
            TierLegend.gameObject.SetActive(false);
    }

    // Maps correct-hit count to a tier index. Pure + static so it is unit-testable.
    // 0 = Empty, 1 = Low, 2 = Mid, 3 = High, 4 = Legend
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
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * punchScale;

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            yield return null;
        }

        t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
            yield return null;
        }

        target.localScale = originalScale;
    }

    System.Collections.IEnumerator TimerPulseWarning()
    {
        float pulseDuration = 0.4f;
        Vector3 normalScale = Vector3.one;
        Vector3 pulseScale = Vector3.one * 1.1f;

        while (gameRunning)
        {
            timerBarFill.color = timerWarningColor;
            yield return StartCoroutine(
                ScaleLerp(timerBarFill.transform, normalScale, pulseScale, pulseDuration * 0.5f)
            );

            timerBarFill.color = timerNormalColor;
            yield return StartCoroutine(
                ScaleLerp(timerBarFill.transform, pulseScale, normalScale, pulseDuration * 0.5f)
            );
        }
    }

    System.Collections.IEnumerator ScaleLerp(
        Transform target,
        Vector3 from,
        Vector3 to,
        float duration
    )
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
