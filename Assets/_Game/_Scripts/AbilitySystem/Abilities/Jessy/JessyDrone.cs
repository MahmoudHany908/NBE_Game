using UnityEngine;

public class JessyDrone : MonoBehaviour
{
    [Header("Drone")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform attackPos;
    [SerializeField] private GameObject shootEffectPrefab;

    [Header("Visuals & Animation")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float swaySpeed = 2f;
    [SerializeField] private float swayAmount = 0.5f;
    [SerializeField] private float attackRecoilDistance = 0.5f;
    [SerializeField] private float attackRecoilDuration = 0.15f;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float followDistance = 3f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Audio")]
    [SerializeField] private SoundData shootSound;

    private ITargetProvider targetProvider;
    private Transform target;
    private float lastAttackTime;

    private PoolManager poolManager;
    private AudioManager audioManager;

    // Animation state
    private Vector3 visualDefaultLocalPos;
    private float currentRecoilTime;
    private bool isRecoiling;

    public void Initialize(PoolManager pm, AudioManager am)
    {
        poolManager = pm;
        audioManager = am;
    }

    private void Awake()
    {
        targetProvider = GetComponentInChildren<ITargetProvider>();
        if (visualTransform != null)
        {
            visualDefaultLocalPos = visualTransform.localPosition;
        }
    }

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
    }

    private void Update()
    {
        HandleMovement();
        HandleAttack();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (visualTransform == null) return;

        // Idle Sway (Vertical)
        float swayY = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        Vector3 targetLocalPos = visualDefaultLocalPos + new Vector3(0, swayY, 0);

        // Attack Recoil (Backward in local space)
        if (isRecoiling)
        {
            currentRecoilTime += Time.deltaTime;
            float recoilProgress = currentRecoilTime / attackRecoilDuration;
            
            if (recoilProgress >= 1f)
            {
                isRecoiling = false;
                currentRecoilTime = 0f;
            }
            else
            {
                // PingPong effect for recoil: go back, then return
                float recoilFactor = Mathf.PingPong(recoilProgress * 2f, 1f);
                targetLocalPos -= Vector3.forward * (recoilFactor * attackRecoilDistance);
            }
        }

        visualTransform.localPosition = Vector3.Lerp(visualTransform.localPosition, targetLocalPos, Time.deltaTime * 10f);
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
            target = targetProvider?.GetTarget()?.transform;
            if (target == null)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 5f);
                return;
            }
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

        if (poolManager != null)
        {
            JessyDroneProjectile projectile = poolManager.Spawn<JessyDroneProjectile>(
                projectilePrefab,
                attackPos.position,
                rotation
            );

            projectile?.Initialize(attackPos.position, rotation, attackDamage);

            if (shootEffectPrefab != null)
            {
                poolManager.Spawn(shootEffectPrefab, attackPos.position, rotation);
            }
        }

        if (audioManager != null && shootSound != null)
        {
            audioManager.PlaySoundAtPosition(shootSound, attackPos.position);
        }

        // Trigger recoil
        isRecoiling = true;
        currentRecoilTime = 0f;
    }

    public void SetSpeed(float newSpeed) => speed = Mathf.Max(0f, newSpeed);
    public void SetAttackRange(float newRange) => attackRange = Mathf.Max(0f, newRange);
    public void SetAttackDamage(int newDamage) => attackDamage = Mathf.Max(0, newDamage);
    public void SetFollowDistance(float newDist) => followDistance = Mathf.Max(0f, newDist);
}