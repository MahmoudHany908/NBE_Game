using UnityEngine;

public class PooledAudioSource : MonoBehaviour, IPoolable
{
    private PooledObject pooledObject;
    private AudioSource source;

    public AudioSource Source => source;
    public System.Action OnReturnedToPool;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        source = GetComponent<AudioSource>();

        if (pooledObject == null)
            Debug.LogError("PooledAudioSource requires a PooledObject component.");
        if (source == null)
            Debug.LogError("PooledAudioSource requires an AudioSource component.");
    }

    public void OnSpawned()
    {
        // Intentionally empty.
    }

    public void OnDespawned()
    {
        source.Stop();
        source.clip = null;

        OnReturnedToPool?.Invoke();
        OnReturnedToPool = null;
    }

    public void ReturnToPool()
    {
        pooledObject.ReturnToPool();
    }

    public void ReturnToPool(float delay)
    {
        pooledObject.ReturnToPool(delay);
    }
}