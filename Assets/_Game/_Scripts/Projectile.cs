using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifeTime;



    private PooledObject pooledObject;


    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void OnDespawned()
    {

    }

    public void OnSpawned()
    {

        pooledObject.ReturnToPool(lifeTime);

    }
}