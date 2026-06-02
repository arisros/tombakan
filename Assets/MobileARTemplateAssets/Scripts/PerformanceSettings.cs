using UnityEngine;

/// <summary>
/// Low-end device performance mode (Continuous roadmap item).
/// When enabled: caps frame rate at 30, reduces physics rate, disables shadows.
/// Applied on game startup; persisted in PlayerPrefs.
/// </summary>
public class PerformanceSettings : MonoBehaviour
{
    const string Key = "tombakan_low_perf";

    [Header("Quality Caps (Low-End Mode)")]
    public int lowPerfTargetFps     = 30;
    public int normalTargetFps      = 60;

    void Awake()
    {
        Apply(IsLowPerfMode());
    }

    public static bool IsLowPerfMode() => PlayerPrefs.GetInt(Key, 0) == 1;

    public static void SetLowPerfMode(bool v)
    {
        PlayerPrefs.SetInt(Key, v ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void Apply(bool lowPerf)
    {
        Application.targetFrameRate = lowPerf ? lowPerfTargetFps : normalTargetFps;

        if (lowPerf)
        {
            QualitySettings.shadows       = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            Time.fixedDeltaTime           = 1f / 30f;
        }
        else
        {
            QualitySettings.shadows       = ShadowQuality.All;
            QualitySettings.shadowDistance = 50f;
            Time.fixedDeltaTime           = 1f / 60f;
        }
    }

    public void Toggle()
    {
        bool newVal = !IsLowPerfMode();
        SetLowPerfMode(newVal);
        Apply(newVal);
    }
}
