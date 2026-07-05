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
        DontDestroyOnLoad(gameObject);
        musicAudioSource = new GameObject("MusicAudioSource").AddComponent<AudioSource>();
        musicAudioSource.transform.SetParent(transform);
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

        // 2. Use the injected poolManager
        PooledAudioSource pooled = poolManager.Spawn<PooledAudioSource>(
            soundSourcePrefab.gameObject, position, Quaternion.identity);

        pooled.Source.pitch = RandomizePitch(data.pitchRange);
        pooled.Source.PlayOneShot(clip, GetFinalVolume(data.volume, sfxVolume));

        pooled.ReturnToPool(clip.length + 0.1f);
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

        PooledAudioSource pooled = poolManager.Spawn<PooledAudioSource>(
            soundSourcePrefab.gameObject, position, Quaternion.identity);

        pooled.Source.clip = clip;
        pooled.Source.volume = GetFinalVolume(data.volume, ambientVolume);
        pooled.Source.loop = data.loop;
        pooled.Source.pitch = RandomizePitch(data.pitchRange);
        pooled.Source.Play();

        activeAmbientSources.Add(pooled);

        // Your original lambda is perfectly fine and correctly captures the specific instance!
        pooled.OnReturnedToPool += () => activeAmbientSources.Remove(pooled);

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