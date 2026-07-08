using System;
using UnityEngine;
using VContainer; 

[RequireComponent(typeof(PooledObject))]
[RequireComponent(typeof(HealthComponent))]
public class Enemy : MonoBehaviour, IPoolable
{
    protected float moveSpeed;
    private PooledObject pooledObject;
    private HealthComponent healthComponent;

    private IEventSubscriber eventSubscriber;

    public float MoveSpeed => moveSpeed;

    [Inject]
    public void Construct(IEventSubscriber subscriber)
    {
        eventSubscriber = subscriber;
    }

    protected virtual void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        healthComponent = GetComponent<HealthComponent>();
    }

    void OnEnable()
    {
        eventSubscriber?.Subscribe<EnemyDiedEvent>(HandleDeath);
    }

    void OnDisable()
    {
        eventSubscriber?.Unsubscribe<EnemyDiedEvent>(HandleDeath);
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
        Debug.Log($"{obj.gameObject.name} died at {obj.DeathPosition}");
        pooledObject.ReturnToPool(0.1f);
    }
}