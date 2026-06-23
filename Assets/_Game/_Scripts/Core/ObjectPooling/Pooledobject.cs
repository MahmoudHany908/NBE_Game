using UnityEngine;



public class PooledObject : MonoBehaviour
{
    public int PrefabId { get; internal set; }
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
