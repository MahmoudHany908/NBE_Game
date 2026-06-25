using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolable
{
    protected float moveSpeed;
    private PooledObject pooledObject;
    private HealthComponent healthComponent;
    
    public float MoveSpeed => moveSpeed;
    
    protected virtual void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        healthComponent = GetComponent<HealthComponent>();
    }

    public void OnSpawned()
    {
        EnemyRegistry.Instance.Register(this);
        HealthComponent.OnDied += HandleDeath;
    }

    public void OnDespawned()
    {
        EnemyRegistry.Instance.Unregister(this);
        HealthComponent.OnDied -= HandleDeath;
    }

    private void HandleDeath(GameObject obj)
    {
        if (obj != gameObject) return;
        pooledObject.ReturnToPool(0.1f);
    }
}
