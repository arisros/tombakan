using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Mobile vibration / haptic feedback for hit events (Phase 3 juice).
/// Falls back to no-op on non-mobile platforms. Respects the mute preference.
/// Call from GameManager on correct/wrong fish hit.
/// </summary>
public static class HapticFeedback
{
    /// <summary>Short tap for a correct hit.</summary>
    public static void PlayCorrect()
    {
        if (AudioPrefs.IsMuted()) return; // respect mute (user silence preference)
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    /// <summary>Longer buzz for a wrong hit.</summary>
    public static void PlayWrong()
    {
        if (AudioPrefs.IsMuted()) return;
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
