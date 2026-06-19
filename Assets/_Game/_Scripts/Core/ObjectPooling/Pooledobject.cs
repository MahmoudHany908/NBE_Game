using UnityEngine;



public class PooledObject : MonoBehaviour
{
    /// <summary>InstanceID of the prefab this object was spawned from. Set internally.</summary>
    public int PrefabId { get; internal set; }

    /// <summary>
    /// Incremented every time this instance is taken from the pool.
    /// Used to invalidate stale scheduled despawns if the object is
    /// returned and re-spawned before a delayed despawn fires.
    /// </summary>
    internal int Version { get; set; }


    public void ReturnToPool()
    {
        PoolManager.Instance.Despawn(gameObject);
    }
    public void ReturnToPool(float delay)
    {
        PoolManager.Instance.DespawnAfter(gameObject, delay);
    }
}
