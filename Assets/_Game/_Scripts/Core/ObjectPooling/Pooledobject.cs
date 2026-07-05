using UnityEngine;
using VContainer;



public class PooledObject : MonoBehaviour
{
    public int PrefabId { get; internal set; }
    internal int Version { get; set; }

    [Inject] private PoolManager poolManager;

    public void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy) return;
        poolManager.Despawn(gameObject);
    }
    public void ReturnToPool(float delay)
    {
        if (!gameObject.activeInHierarchy) return;
        poolManager.DespawnAfter(gameObject, delay);
    }
}
