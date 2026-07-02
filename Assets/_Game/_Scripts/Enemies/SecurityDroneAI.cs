using UnityEngine;

public class SecurityDroneAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float distanceThreshold = 1f;
    [SerializeField] private float moveSpeed = 6f;

    [Header("Height")]
    [SerializeField] private float minHeight = 2f;
    [SerializeField] private float maxHeight = 5f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    private Transform target;
    private float attackTimer;
    private float targetHeight;
    private ITargetProvider targetProvider;

    void Awake()
    {
        targetProvider = GetComponentInChildren<ITargetProvider>();
        if (targetProvider == null)
            Debug.LogError("Target Provider is not set!");
    }

    void OnEnable()
    {
        targetHeight = Random.Range(minHeight, maxHeight);

        if (targetProvider != null)
            targetProvider.TargetChanged += SetTarget;
    }

    void OnDisable()
    {
        if (targetProvider != null)
            targetProvider.TargetChanged -= SetTarget;
    }

    void Update()
    {
        if (target == null) return;

        HandleHeight();
        HandleDistance();

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Attack();
        }
    }

    private void HandleHeight()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetHeight, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    private void HandleDistance()
    {
        Vector3 flatSelf = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatTarget = new Vector3(target.position.x, 0f, target.position.z);

        float distance = Vector3.Distance(flatSelf, flatTarget);
        float difference = distance - preferredDistance;

        if (Mathf.Abs(difference) <= distanceThreshold) return;

        Vector3 direction = (flatSelf - flatTarget).normalized; // points away from player

        if (difference > 0f)
            direction = -direction; // too far away, flip to move toward player

        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void Attack()
    {
        attackTimer = attackCooldown;
        var projectile = PoolManager.Instance.Spawn<DroneProjectile>(projectilePrefab, transform.position, Quaternion.identity);
        projectile?.Init(target.position, attackDamage);
    }

    private void SetTarget(GameObject obj)
    {
        target = obj.transform;
    }
}