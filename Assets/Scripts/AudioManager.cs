using System.Collections;
using UnityEngine;

/// <summary>
/// Manages game audio including music and sound effects
/// Handles smooth cross-fading between normal and blink world music
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip normalWorldMusic;
    [SerializeField] private AudioClip blinkWorldMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip blinkEnterSFX;
    [SerializeField] private AudioClip blinkExitSFX;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip checkpointSFX;

    [Header("Music Settings")]
    [SerializeField] private float musicFadeDuration = 1f;
    [SerializeField] private float normalMusicVolume = 0.5f;
    [SerializeField] private float blinkMusicVolume = 0.5f;

    [Header("SFX Settings")]
    [SerializeField] private float sfxVolume = 0.7f;

    // State tracking
    private bool isInBlink = false;
    private Coroutine musicFadeCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Debug.LogWarning("Multiple AudioManagers detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        // Auto-create audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Set initial volumes
        musicSource.volume = normalMusicVolume;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        // Subscribe to world state events
        if (BlinkController.Instance != null)
        {
            BlinkController.Instance.enterBlink += HandleEnterBlink;
            BlinkController.Instance.exitBlink += HandleExitBlink;
        }
        else
        {
            Debug.LogWarning("AudioManager: WorldStateManager not found!");
        }

        // Start playing normal world music
        PlayMusic(normalWorldMusic, normalMusicVolume);
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (BlinkController.Instance != null)
        {
            BlinkController.Instance.enterBlink -= HandleEnterBlink;
            BlinkController.Instance.exitBlink -= HandleExitBlink;
        }
    }

    /// <summary>
    /// Handle entering blink state - cross-fade to blink music
    /// </summary>
    private void HandleEnterBlink()
    {
        isInBlink = true;
        PlaySFX(blinkEnterSFX);

        if (blinkWorldMusic != null)
        {
            CrossFadeMusic(blinkWorldMusic, blinkMusicVolume);
        }
    }

    /// <summary>
    /// Handle exiting blink state - cross-fade back to normal music
    /// </summary>
    private void HandleExitBlink()
    {
        isInBlink = false;
        PlaySFX(blinkExitSFX);

        if (normalWorldMusic != null)
        {
            CrossFadeMusic(normalWorldMusic, normalMusicVolume);
        }
    }

    /// <summary>
    /// Play music clip immediately (no fade)
    /// </summary>
    public void PlayMusic(AudioClip clip, float volume = 0.5f)
    {
        if (musicSource == null || clip == null) return;

        // Stop any active fade
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();

        Debug.Log($"Playing music: {clip.name}");
    }

    /// <summary>
    /// Cross-fade from current music to new music
    /// </summary>
    public void CrossFadeMusic(AudioClip newClip, float targetVolume = 0.5f)
    {
        if (musicSource == null || newClip == null) return;

        // Stop any active fade
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(CrossFadeMusicCoroutine(newClip, targetVolume));
    }

    /// <summary>
    /// Cross-fade music coroutine
    /// </summary>
    private IEnumerator CrossFadeMusicCoroutine(AudioClip newClip, float targetVolume)
    {
        float fadeOutDuration = musicFadeDuration / 2f;
        float fadeInDuration = musicFadeDuration / 2f;

        // Fade out current music
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        musicSource.volume = 0f;

        // Switch to new clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new music
        elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        Debug.Log($"Cross-faded to music: {newClip.name}");
        musicFadeCoroutine = null;
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    /// <summary>
    /// Play jump sound effect
    /// </summary>
    public void PlayJumpSFX()
    {
        PlaySFX(jumpSFX);
    }

    /// <summary>
    /// Play death sound effect
    /// </summary>
    public void PlayDeathSFX()
    {
        PlaySFX(deathSFX);
    }

    /// <summary>
    /// Play checkpoint sound effect
    /// </summary>
    public void PlayCheckpointSFX()
    {
        PlaySFX(checkpointSFX);
    }

    /// <summary>
    /// Stop all music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        normalMusicVolume = volume;
        blinkMusicVolume = volume;

        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;

        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }
}
