using UnityEngine;
using VContainer;

[RequireComponent(typeof(PooledObject))]
[RequireComponent(typeof(EnemyHealthComponent))]
public class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] private EnemyDataSO enemyData;

    private PooledObject pooledObject;
    private EnemyHealthComponent enemyHealthComponent;
    private IEventSubscriber eventSubscriber;
    private PoolManager poolManager;

    private bool isSubscribed = false;

    private ITargetProvider targetProvider;
    private Transform target;

    public float TargetHeight { get; set; }
    public float AttackTimer { get; set; }

    public EnemyDataSO Data => enemyData;

    [Inject]
    public void Construct(IEventSubscriber subscriber, PoolManager pool)
    {
        eventSubscriber = subscriber;
        poolManager = pool;
        SubscribeToEvents();
    }

    protected virtual void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        enemyHealthComponent = GetComponent<EnemyHealthComponent>();
        targetProvider = GetComponentInChildren<ITargetProvider>();

        if (targetProvider == null)
            Debug.LogError($"[Enemy] Missing ITargetProvider on {gameObject.name}!", this);
    }

    void OnEnable()
    {
        if (enemyData != null && enemyHealthComponent != null)
        {
            enemyHealthComponent.SetMaxHealth(enemyData.maxHealth);
        }

        TargetHeight = 0f;
        AttackTimer = 0f;

        if (targetProvider != null)
            targetProvider.TargetChanged += SetTarget;

        SubscribeToEvents();
    }

    void OnDisable()
    {
        if (targetProvider != null)
            targetProvider.TargetChanged -= SetTarget;

        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if (target == null || enemyData == null) return;

        FaceTarget();

        enemyData.movementStrategy?.Move(this, target);

        AttackTimer -= Time.deltaTime;
        if (AttackTimer <= 0f)
        {
            enemyData.attackStrategy?.HandleAttack(this, target, poolManager);
            AttackTimer = enemyData.attackCooldown;
        }
    }

    private void FaceTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void SetTarget(GameObject obj)
    {
        target = obj != null ? obj.transform : null;
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