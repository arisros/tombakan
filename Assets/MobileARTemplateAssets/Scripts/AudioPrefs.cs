using UnityEngine;

/// <summary>
/// Persistent audio preferences backed by PlayerPrefs (Week 4 UX-11).
/// Static + tiny so the mute state is unit-testable and shared by any UI hook.
/// </summary>
public static class AudioPrefs
{
    public const string MuteKey = "tombakan_audio_muted";

    public static bool IsMuted()
    {
        // PlayerPrefs has no bool; store 0/1.
        return PlayerPrefs.GetInt(MuteKey, 0) == 1;
    }

    public static void SetMuted(bool muted)
    {
        PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
        PlayerPrefs.Save();
    }
}
