using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip mainBGM;
    public AudioClip gameplayWaves;

    [Header("SFX Clips")]
    public AudioClip sfxCorrect;
    public AudioClip sfxWrong;
    public AudioClip sfxEnd;

    bool muted;

    void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        // Restore persisted mute preference (Week 4 UX-11)
        muted = AudioPrefs.IsMuted();
        ApplyMute();
    }

    // ===== Mute =====
    public bool IsMuted => muted;

    public void ToggleMute()
    {
        SetMuted(!muted);
    }

    public void SetMuted(bool value)
    {
        muted = value;
        ApplyMute();
        AudioPrefs.SetMuted(muted);
    }

    void ApplyMute()
    {
        if (bgmSource != null)
            bgmSource.mute = muted;
        if (sfxSource != null)
            sfxSource.mute = muted;
    }

    // ===== BGM =====
    public void PlayMainBGM()
    {
        PlayBGM(mainBGM, true);
    }

    public void PlayGameplayBGM()
    {
        PlayBGM(gameplayWaves, true);
    }

    void PlayBGM(AudioClip clip, bool loop)
    {
        // Fail soft when source/clip are unassigned (Week 4 BUG-08)
        if (bgmSource == null || clip == null)
            return;

        if (bgmSource.clip == clip)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.mute = muted;
        bgmSource.Play();
    }

    // ===== SFX =====
    public void PlayCorrect()
    {
        PlaySfx(sfxCorrect);
    }

    public void PlayWrong()
    {
        PlaySfx(sfxWrong);
    }

    public void PlayEnd()
    {
        PlaySfx(sfxEnd);
    }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}
