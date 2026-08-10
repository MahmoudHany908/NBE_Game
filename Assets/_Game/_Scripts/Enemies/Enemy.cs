using UnityEngine;
using UnityEngine.AI;
using VContainer;

[RequireComponent(typeof(PooledObject))]
[RequireComponent(typeof(EnemyHealthComponent))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] private EnemyDataSO enemyData;
    [SerializeField] private GameObject destroyedModel;

    private PooledObject pooledObject;
    private EnemyHealthComponent enemyHealthComponent;
    private IEventSubscriber eventSubscriber;
    private PoolManager poolManager;

    private bool isSubscribed = false;

    private ITargetProvider targetProvider;
    private Transform target;
    [SerializeField] private Animator animator;

    public NavMeshAgent Agent { get; private set; }

    private bool isStunned;
    private float stunTimer;

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
        animator = GetComponentInChildren<Animator>();
        Agent = GetComponent<NavMeshAgent>();

        if (Agent != null)
        {
            Agent.updateRotation = false; // We use FaceTarget() to rotate
        }

        if (targetProvider == null)
            Debug.LogError($"[Enemy] Missing ITargetProvider on {gameObject.name}!", this);
    }

    void OnEnable()
    {
        if (enemyData != null && enemyHealthComponent != null)
        {
            enemyHealthComponent.SetMaxHealth(enemyData.maxHealth);
        }

        if (Agent != null && enemyData != null)
        {
            Agent.speed = enemyData.moveSpeed;
            Agent.enabled = true;
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

        if (Agent != null)
        {
            Agent.enabled = false;
        }

        UnsubscribeFromEvents();
    }

    private void Update()
    {


        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                EndStun();
            }
            return;
        }

        if (target == null || enemyData == null) return;
        if (isStunned) return;

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
        Instantiate(destroyedModel, transform.position, transform.rotation);
        pooledObject.ReturnToPool(0.1f);
    }



    #region Stun Logic

    public void TakeStun(float duration)
    {
        stunTimer = Mathf.Max(stunTimer, duration);

        if (!isStunned)
            BeginStun();
    }

    private void BeginStun()
    {
        isStunned = true;
        if (animator != null) animator.SetBool("IsStunned", true);
        if (Agent != null && Agent.isOnNavMesh) Agent.isStopped = true;
    }

    private void EndStun()
    {
        isStunned = false;
        if (animator != null) animator.SetBool("IsStunned", false);
        if (Agent != null && Agent.isOnNavMesh) Agent.isStopped = false;
    }

    #endregion
}