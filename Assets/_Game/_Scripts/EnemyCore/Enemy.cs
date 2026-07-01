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


    void OnEnable()
    {
        EventBus.Subscribe<EnemyDiedEvent>(HandleDeath);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<EnemyDiedEvent>(HandleDeath);
    }

    public void OnSpawned()
    {

    }

    public void OnDespawned()
    {

    }

    private void HandleDeath(EnemyDiedEvent obj)
    {
        if (obj.gameObject != gameObject) return;
        Debug.Log($"{obj.gameObject.name} died{obj.DeathPosition}");
        pooledObject.ReturnToPool(0.1f);
    }
}