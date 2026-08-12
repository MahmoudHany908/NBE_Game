using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private PooledAudioSource soundSourcePrefab;

    [Header("Audio Settings")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float ambientVolume = 1f;
    private Coroutine fadeCoroutine;
    private bool isMuted = false;
    public bool IsMuted
    {
        get => isMuted;
        set { isMuted = value; ApplyMuteState(); }
    }

    private void ApplyMuteState()
    {
        // Debug.Log($"AudioManager: Mute state changed to {isMuted}");
        if (isMuted)
        {
            if (musicAudioSource != null) musicAudioSource.volume = 0f;
            foreach (var source in activeAmbientSources)
            {
                if (source != null && source.Source != null) source.Source.volume = 0f;
            }
        }
        else
        {
            UpdateMusicVolume();
            UpdateAmbientVolumes();
        }
    }

    private AudioSource musicAudioSource;

    // 1. Inject DI Dependencies
    [Inject] private PoolManager poolManager;
    [Inject] private ISaveSystem saveSystem;
    [Inject] private IEventSubscriber eventSubscriber;

    private List<PooledAudioSource> activeAmbientSources = new List<PooledAudioSource>();

    private void Awake()
    {
        musicAudioSource = new GameObject("MusicAudioSource").AddComponent<AudioSource>();
        musicAudioSource.transform.SetParent(transform);

        EnsureSoundSourcePrefab();
    }

    private void Start()
    {
        if (saveSystem != null)
        {
            var data = saveSystem.Load();
            if (data?.audio != null)
            {
                masterVolume = data.audio.masterVolume;
                musicVolume = data.audio.musicVolume;
                sfxVolume = data.audio.sfxVolume;
                ambientVolume = data.audio.ambientVolume;
                isMuted = data.audio.isMuted;
                ApplyMuteState();
            }
        }

        if (eventSubscriber != null)
        {
            eventSubscriber.Subscribe<OnMuteTogglePressedEvent>(OnMuteToggle);
            eventSubscriber.Subscribe<OnMasterVolumeChangedEvent>(OnMasterVolume);
            eventSubscriber.Subscribe<OnMusicVolumeChangedEvent>(OnMusicVolume);
            eventSubscriber.Subscribe<OnSFXSliderChangeEvent>(OnSFXVolume);
            eventSubscriber.Subscribe<OnAmbientSliderChangeEvent>(OnAmbientVolume);
        }
    }

    private void OnDestroy()
    {
        if (eventSubscriber != null)
        {
            eventSubscriber.Unsubscribe<OnMuteTogglePressedEvent>(OnMuteToggle);
            eventSubscriber.Unsubscribe<OnMasterVolumeChangedEvent>(OnMasterVolume);
            eventSubscriber.Unsubscribe<OnMusicVolumeChangedEvent>(OnMusicVolume);
            eventSubscriber.Unsubscribe<OnSFXSliderChangeEvent>(OnSFXVolume);
            eventSubscriber.Unsubscribe<OnAmbientSliderChangeEvent>(OnAmbientVolume);
        }
    }

    private void OnMuteToggle(OnMuteTogglePressedEvent cmd)
    {
        isMuted = cmd.newValue;
        if (saveSystem != null)
        {
            var data = saveSystem.Load();
            if (data?.audio != null)
            {
                data.audio.isMuted = isMuted;
                saveSystem.Save();
            }
        }
        ApplyMuteState();
    }

    private void OnMasterVolume(OnMasterVolumeChangedEvent cmd)
    {
        masterVolume = cmd.newValue;
        if (saveSystem != null)
        {
            var data = saveSystem.Load();
            if (data?.audio != null)
            {
                data.audio.masterVolume = masterVolume;
                saveSystem.Save();
            }
        }
        UpdateMusicVolume();
    }

    private void OnMusicVolume(OnMusicVolumeChangedEvent cmd)
    {
        musicVolume = cmd.newValue;
        if (saveSystem != null)
        {
            var data = saveSystem.Load();
            if (data?.audio != null)
            {
                data.audio.musicVolume = musicVolume;
                saveSystem.Save();
            }
        }
        UpdateMusicVolume();
    }

    private void OnSFXVolume(OnSFXSliderChangeEvent cmd)
    {
        sfxVolume = cmd.newValue;
        if (saveSystem != null)
        {
            var data = saveSystem.Load();
            if (data?.audio != null)
            {
                data.audio.sfxVolume = sfxVolume;
                saveSystem.Save();
            }
        }
    }

    private void OnAmbientVolume(OnAmbientSliderChangeEvent cmd)
    {
        ambientVolume = cmd.newValue;
        if (saveSystem != null)
        {
            var data = saveSystem.Load();
            if (data?.audio != null)
            {
                data.audio.ambientVolume = ambientVolume;
                saveSystem.Save();
            }
        }
        UpdateAmbientVolumes();
    }

    private void UpdateMusicVolume()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            // Approximate final volume. We don't have the original SoundData volume here, 
            // but normally music is played at volume 1. We just multiply it.
            musicAudioSource.volume = GetFinalVolume(1f, musicVolume);
        }
    }

    private void UpdateAmbientVolumes()
    {
        foreach (var source in activeAmbientSources)
        {
            if (source != null && source.Source != null && source.Source.isPlaying)
            {
                // Note: we can't perfectly recover the original clip volume, assuming 1f base
                source.Source.volume = GetFinalVolume(1f, ambientVolume);
            }
        }
    }

    private void EnsureSoundSourcePrefab()
    {
        if (soundSourcePrefab == null)
        {
            Debug.LogWarning("AudioManager: soundSourcePrefab was null. Creating a fallback 3D AudioSource prefab dynamically.");

            // Create a hidden GameObject to act as the prefab for PoolManager
            GameObject fallbackPrefab = new GameObject("FallbackSoundSourcePrefab");
            fallbackPrefab.SetActive(false);

            // Parent it to AudioManager so it persists and cleans up with it
            fallbackPrefab.transform.SetParent(transform);

            AudioSource audioSource = fallbackPrefab.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound by default
            audioSource.playOnAwake = false;

            fallbackPrefab.AddComponent<PooledObject>();
            soundSourcePrefab = fallbackPrefab.AddComponent<PooledAudioSource>();
        }
    }

    #region SFX

    public void PlaySoundAtPosition(SoundData data, Vector3 position)
    {
        AudioClip clip = data.GetClip();
        if (clip == null)
        {
            Debug.LogWarning("AudioManager.PlaySoundAtPosition: SoundData returned a null clip.");
            return;
        }

        if (poolManager == null)
        {
            Debug.LogError("AudioManager.PlaySoundAtPosition: poolManager is null! VContainer injection might have failed on AudioManager.", this);
            return;
        }

        if (soundSourcePrefab == null)
        {
            Debug.LogError("AudioManager.PlaySoundAtPosition: soundSourcePrefab is not assigned in the Inspector!", this);
            return;
        }

        // 2. Use the injected poolManager
        PooledAudioSource pooled = poolManager.Spawn<PooledAudioSource>(
            soundSourcePrefab.gameObject, position, Quaternion.identity);

        if (pooled != null)
        {
            pooled.Source.pitch = RandomizePitch(data.pitchRange);
            pooled.Source.PlayOneShot(clip, GetFinalVolume(data.volume, sfxVolume));

            pooled.ReturnToPool(clip.length + 0.1f);
        }
    }

    public PooledAudioSource PlayDialogue(SoundData data, Vector3 position)
    {
        AudioClip clip = data.GetClip();
        if (clip == null) return null;

        if (poolManager == null || soundSourcePrefab == null) return null;

        PooledAudioSource pooled = poolManager.Spawn<PooledAudioSource>(
            soundSourcePrefab.gameObject, position, Quaternion.identity);

        if (pooled != null)
        {
            pooled.Source.clip = clip;
            pooled.Source.volume = GetFinalVolume(data.volume, sfxVolume);
            pooled.Source.pitch = RandomizePitch(data.pitchRange);
            pooled.Source.loop = false;
            pooled.Source.Play();
        }

        return pooled;
    }

    #endregion

    #region AMBIENT

    public PooledAudioSource PlayAmbientAtPosition(SoundData data, Vector3 position)
    {
        AudioClip clip = data.GetClip();
        if (clip == null)
        {
            Debug.LogWarning("AudioManager.PlayAmbientAtPosition: SoundData returned a null clip.");
            return null;
        }

        if (poolManager == null)
        {
            Debug.LogError("AudioManager.PlayAmbientAtPosition: poolManager is null! VContainer injection might have failed.", this);
            return null;
        }

        if (soundSourcePrefab == null)
        {
            Debug.LogError("AudioManager.PlayAmbientAtPosition: soundSourcePrefab is not assigned in the Inspector!", this);
            return null;
        }

        PooledAudioSource pooled = poolManager.Spawn<PooledAudioSource>(
            soundSourcePrefab.gameObject, position, Quaternion.identity);

        if (pooled != null)
        {
            pooled.Source.clip = clip;
            pooled.Source.volume = GetFinalVolume(data.volume, ambientVolume);
            pooled.Source.loop = data.loop;
            pooled.Source.pitch = RandomizePitch(data.pitchRange);
            pooled.Source.Play();

            activeAmbientSources.Add(pooled);

            pooled.OnReturnedToPool += () => activeAmbientSources.Remove(pooled);
        }

        return pooled;
    }

    public void StopAmbient(PooledAudioSource source)
    {
        if (source == null) return;
        source.ReturnToPool();
    }

    public void StopAllAmbient()
    {
        for (int i = activeAmbientSources.Count - 1; i >= 0; i--)
            StopAmbient(activeAmbientSources[i]);
    }

    #endregion

    #region MUSIC

    public void PlayMusic(SoundData data)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToNewTrack(data));
    }

    public void PauseMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOut(() => musicAudioSource.Pause(), new SoundData { fade = 1f }));
    }

    public void ResumeMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        musicAudioSource.UnPause();
        float targetVolume = GetFinalVolume(1f, musicVolume);
        fadeCoroutine = StartCoroutine(FadeVolume(0f, targetVolume, 1f));
    }

    public void StopMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);


        fadeCoroutine = StartCoroutine(FadeOut(() => musicAudioSource.Stop(), new SoundData { fade = 1f }));
    }

    private IEnumerator FadeToNewTrack(SoundData data)
    {
        float targetVolume = GetFinalVolume(data.volume, musicVolume);

        if (musicAudioSource.isPlaying)
            yield return FadeOut(null, data);

        musicAudioSource.clip = data.GetClip();
        musicAudioSource.loop = data.loop;
        musicAudioSource.volume = 0f;
        musicAudioSource.Play();

        yield return FadeVolume(0f, targetVolume, data.fade);
    }

    private IEnumerator FadeOut(System.Action onComplete, SoundData data)
    {
        float startVolume = musicAudioSource.volume;
        yield return FadeVolume(startVolume, 0f, data.fade);
        onComplete?.Invoke();
    }

    private IEnumerator FadeVolume(float from, float to, float fadeDuration)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        musicAudioSource.volume = to;
    }

    #endregion

    #region HELPERS

    private float GetFinalVolume(float soundVolume, float categoryVolume)
    {
        return isMuted ? 0f : soundVolume * categoryVolume * masterVolume;
    }

    private float RandomizePitch(Vector2 pitchRange)
    {
        return Random.Range(pitchRange.x, pitchRange.y);
    }

    #endregion
}