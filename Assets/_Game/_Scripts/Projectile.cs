using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    private PooledObject pooledObject;


    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void OnDespawned()
    {
        Debug.Log("Projectile Despawned");
    }

    public void OnSpawned()
    {
        Debug.Log("Projectile Spawned");
        pooledObject.ReturnToPool(2f);

    }
}