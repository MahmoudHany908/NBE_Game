using UnityEngine;



public class PooledObject : MonoBehaviour
{
    public int PrefabId { get; internal set; }
    internal int Version { get; set; }


    public void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy) return;
        PoolManager.Instance.Despawn(gameObject);
    }
    public void ReturnToPool(float delay)
    {
        if (!gameObject.activeInHierarchy) return;
        PoolManager.Instance.DespawnAfter(gameObject, delay);
    }
}
