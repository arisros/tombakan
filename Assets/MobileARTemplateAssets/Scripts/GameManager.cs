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
    public float warningTimeThreshold = 10f; // detik terakhir
    public Color timerNormalColor = Color.white;
    public Color timerWarningColor = Color.red;

    Coroutine timerPulseRoutine;
    bool timerWarningActive;

    [Header("Target Color UI")]
    public Image targetColorImage;

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

    [Header("Fish Managers")]
    public FishSpawner fishSpawner;

    // Target color internal
    Color targetColor;
    Color[] fishColorOptions = { Color.green, Color.red, Color.blue };

    int pointPerCorrectHit = 100;
    int penaltyPerWrongHit = 25;

    // Collected fish colors for stats
    string[] collectedFishColors = new string[1000];

    float timer;
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

        string[] mappedColors = new string[correctHitCount];

        // ColorHexLocalization
        for (int i = 0; i < correctHitCount; i++)
        {
            mappedColors[i] = ColorHexLocalization.ToIndonesian(collectedFishColors[i]);
        }

        resultCorrectFishText.text =
            correctHitCount > 0 ? string.Join(", ", mappedColors) : "Tidak ada ikan dikumpulkan";

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

    void UpdateTimerUI()
    {
        float normalized = Mathf.Clamp01(timer / gameDuration);
        timerBarFill.fillAmount = normalized;
    }

    public void PickNewTarget()
    {
        // randomize target color
        int i = Random.Range(0, fishColorOptions.Length);
        targetColor = fishColorOptions[i];

        // set UI target image
        targetColorImage.color = targetColor;
        // spawn fish with target color
        fishSpawner.SpawnFish(targetColor);
    }

    public void OnFishHit(Color fishColor)
    {
        bool correct = fishColor == targetColor;

        if (correct)
        {
            score += pointPerCorrectHit;
            correctHitCount++;
            ShowHappy();
            AudioManager.I.PlayCorrect();

            collectedFishColors[correctHitCount - 1] = ColorUtility.ToHtmlStringRGB(fishColor);
        }
        else
        {
            score -= penaltyPerWrongHit;
            ShowSad();
            AudioManager.I.PlayWrong();
        }

        UpdateScoreUI();
        Invoke(nameof(PickNewTarget), 3f);
    }

    void ShowHappy()
    {
        happyFeedback.SetActive(true);
        happyFeedbackText.text = $"+{pointPerCorrectHit}!";
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
    }

    void UpdateTierStars()
    {
        ResetTierStars();

        Image activeTier;

        if (correctHitCount <= 0)
            activeTier = TierEmpty;
        else if (correctHitCount <= 2)
            activeTier = TierLow;
        else if (correctHitCount <= 4)
            activeTier = TierMid;
        else
            activeTier = TierHigh;

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

        // scale up
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            yield return null;
        }

        t = 0f;

        // scale down
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
            // merah + scale up
            timerBarFill.color = timerWarningColor;
            yield return StartCoroutine(
                ScaleLerp(timerBarFill.transform, normalScale, pulseScale, pulseDuration * 0.5f)
            );

            // normal + scale down
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
