using UnityEngine;


/// <summary>
/// Optional interface for components that need to reset state when
/// spawned from a pool or clean up when returned to it.
/// Implement on any MonoBehaviour attached to a pooled prefab
/// (e.g. reset health, clear particle trails, stop coroutines).
/// </summary>
public interface IPoolable
{
    void OnSpawned();

    void OnDespawned();
}
