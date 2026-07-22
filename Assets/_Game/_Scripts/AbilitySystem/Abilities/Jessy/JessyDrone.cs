using UnityEngine;

public class JessyDrone : MonoBehaviour
{
    [Header("Drone")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform attackPos;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float followDistance = 3f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1f;

    private ITargetProvider targetProvider;
    private Transform target;
    private float lastAttackTime;

    private PoolManager poolManager;

    public void Initialize(PoolManager pm) => poolManager = pm;

    private void OnEnable()
    {
        if (targetProvider != null)
            targetProvider.TargetChanged += SetTarget;
    }

    private void OnDisable()
    {
        if (targetProvider != null)
            targetProvider.TargetChanged -= SetTarget;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        targetProvider = GetComponentInChildren<ITargetProvider>();

        if (targetProvider != null)
            targetProvider.TargetChanged += SetTarget;
    }

    private void Update()
    {
        HandleMovement();
        HandleAttack();
    }

    private void SetTarget(GameObject newTarget)
    {
        if (newTarget != null && !newTarget.activeInHierarchy)
            newTarget = null;

        target = newTarget != null ? newTarget.transform : null;
    }

    private void HandleMovement()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            target = null;
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceToTarget = direction.magnitude;

        // Always face the target, even when stopped
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction.normalized);

        if (distanceToTarget <= followDistance)
            return;

        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void HandleAttack()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            target = null;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            PerformAttack();
        }
    }

    private Vector3 GetTargetAimPoint()
    {
        var col = target.GetComponent<Collider>();
        return col != null ? col.bounds.center : target.position;
    }

    private void PerformAttack()
    {
        if (attackPos == null)
        {
            Debug.LogWarning($"[JessyDrone] attackPos is not assigned on {gameObject.name}!", this);
            return;
        }

        Vector3 aimPoint = GetTargetAimPoint();
        Vector3 dir = (aimPoint - attackPos.position).normalized;

        if (dir == Vector3.zero)
            dir = attackPos.forward;

        Quaternion rotation = Quaternion.LookRotation(dir);

        JessyDroneProjectile projectile = poolManager.Spawn<JessyDroneProjectile>(
            projectilePrefab,
            attackPos.position,
            rotation
        );

        projectile?.Initialize(attackPos.position, rotation, attackDamage);
    }

    public void SetSpeed(float newSpeed) => speed = Mathf.Max(0f, newSpeed);
    public void SetAttackRange(float newRange) => attackRange = Mathf.Max(0f, newRange);
    public void SetAttackDamage(int newDamage) => attackDamage = Mathf.Max(0, newDamage);
    public void SetFollowDistance(float newDist) => followDistance = Mathf.Max(0f, newDist);
}