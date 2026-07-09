using UnityEngine;

public class MeleeRobotAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float moveSpeed = 3f;


    private Enemy enemy;
    private Vector3 direction;
    private Transform target;
    private float attackTimer;
    private ITargetProvider targetProvider;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        targetProvider = GetComponentInChildren<ITargetProvider>();
        if (targetProvider == null)
            Debug.LogError("Target Provider is not set! ");
    }

    void OnEnable()
    {
        // Added null check to prevent crashes if targetProvider failed to assign
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

        direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        attackTimer -= Time.deltaTime;

        FacePlayer();

        if (distance <= attackRange)
        {
            Attack();
        }
        else
        {
            Chase();
        }
    }

    private void FacePlayer()
    {
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);


        if (flatDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(flatDirection);
        }
    }

    private void Chase()
    {
        if (target == null) return;

        Vector3 targetDirection = direction;
        targetDirection.y = 0f;
        transform.position += targetDirection * moveSpeed * Time.deltaTime;
    }

    private void Attack()
    {
        if (attackTimer > 0f) return;
        attackTimer = attackCooldown;

        if (target.TryGetComponent<PlayerHealthComponent>(out PlayerHealthComponent player)) player.TakeDamage(attackDamage);
    }



    private void SetTarget(GameObject _target)
    {
        target = _target.transform;
    }

}
