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

    void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
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
        if (bgmSource.clip == clip)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    // ===== SFX =====
    public void PlayCorrect()
    {
        sfxSource.PlayOneShot(sfxCorrect);
    }

    public void PlayWrong()
    {
        sfxSource.PlayOneShot(sfxWrong);
    }

    public void PlayEnd()
    {
        sfxSource.PlayOneShot(sfxEnd);
    }
}
