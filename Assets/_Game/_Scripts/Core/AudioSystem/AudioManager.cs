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

    private bool isMuted = false;
    public bool IsMuted
    {
        get => isMuted;
        set { isMuted = value; ApplyMuteState(); }
    }

    private void ApplyMuteState()
    {
        Debug.Log($"AudioManager: Mute state changed to {isMuted}");
    }

    private AudioSource musicAudioSource;

    // 1. Inject PoolManager via DI instead of using PoolManager.Instance
    [Inject] private PoolManager poolManager;

    private List<PooledAudioSource> activeAmbientSources = new List<PooledAudioSource>();

    private void Awake()
    {
        musicAudioSource = new GameObject("MusicAudioSource").AddComponent<AudioSource>();
        musicAudioSource.transform.SetParent(transform);

        EnsureSoundSourcePrefab();
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
        musicAudioSource.clip = data.GetClip();
        musicAudioSource.volume = GetFinalVolume(data.volume, musicVolume);
        musicAudioSource.loop = data.loop;
        musicAudioSource.Play();
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
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