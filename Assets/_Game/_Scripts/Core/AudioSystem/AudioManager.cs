using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject soundSourcePrefab;
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
    public void PlayerSoundAtPostion (SoundData data, Vector3 position)
    {
        GameObject sfxGO = PoolManager.Instance.Spawn(soundSourcePrefab, position, Quaternion.identity);
        AudioSource audioSource = sfxGO.GetComponent<AudioSource>();
        audioSource.PlayOneShot(data.GetClip(), data.volume);
    }
    public void PlayAmbianAtPostion (SoundData data, Vector3 position)
    {
        GameObject GO = PoolManager.Instance.Spawn(soundSourcePrefab, position, Quaternion.identity);
        AudioSource audioSource = GO.GetComponent<AudioSource>();
        audioSource.clip = data.GetClip();
        audioSource.volume = data.volume;

    }

}
