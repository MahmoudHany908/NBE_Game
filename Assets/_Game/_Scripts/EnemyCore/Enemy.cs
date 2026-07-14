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

    private bool isSubscribed = false;

    [Inject]
    public void Construct(IEventSubscriber subscriber)
    {
        eventSubscriber = subscriber;
        SubscribeToEvents();
    }

    protected virtual void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        healthComponent = GetComponent<HealthComponent>();
    }

    void OnEnable()
    {
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (isSubscribed || eventSubscriber == null) return;
        eventSubscriber.Subscribe<EnemyDiedEvent>(HandleDeath);
        isSubscribed = true;
    }

    private void UnsubscribeFromEvents()
    {
        if (!isSubscribed || eventSubscriber == null) return;
        eventSubscriber.Unsubscribe<EnemyDiedEvent>(HandleDeath);
        isSubscribed = false;
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