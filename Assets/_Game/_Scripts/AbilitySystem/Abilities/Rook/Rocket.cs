using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class Rocket : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 20f;

    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetimeFailsafe = 3f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float fallSpeed = 15f;
    [SerializeField] private float retargetRadius = 8f;
    [SerializeField] private float retargetCheckInterval = 0.15f;
    [SerializeField] private LayerMask enemySearchLayer;
    [Header("Effects")]
    [SerializeField] private GameObject Effect;
    [SerializeField] private SoundData ExlodeSound;
    private PooledObject pooledObject;
    private Transform target;
    private bool hasHit;
    private bool isFalling;
    private float retargetTimer;

    private int enemyLayer;
    private int groundLayer;

    private AudioManager audioManager;
    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        enemyLayer = LayerMask.NameToLayer("Enemy");
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    public void Init(Transform targetTransform, AudioManager audioManager)
    {
        target = targetTransform;
        this.audioManager = audioManager;
        if (!this.audioManager) Debug.Log("audio null");
        isFalling = false;

        if (target != null)
        {
            transform.forward = (target.position - transform.position).normalized;
        }
    }

    private void Update()
    {
        if (IsTargetGone())
        {
            isFalling = true;
        }

        if (isFalling)
        {
            TryFindNewTarget();
        }

        if (isFalling)
        {
            FallToGround();
        }
        else
        {
            HomeToTarget();
        }
    }

    private bool IsTargetGone()
    {
        return target == null || !target.gameObject.activeInHierarchy;
    }

    private void TryFindNewTarget()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer > 0f) return;
        retargetTimer = retargetCheckInterval;

        Collider[] hits = Physics.OverlapSphere(transform.position, retargetRadius, enemySearchLayer);
        if (hits.Length == 0) return;

        Transform closest = null;
        float closestDistSqr = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.gameObject.activeInHierarchy) continue;

            float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = hit.transform;
            }
        }

        if (closest != null)
        {
            target = closest;
            isFalling = false;
        }
    }

    private void HomeToTarget()
    {
        Vector3 toTarget = (target.position - transform.position).normalized;
        transform.forward = Vector3.Slerp(transform.forward, toTarget, turnSpeed * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void FallToGround()
    {
        transform.forward = Vector3.Slerp(transform.forward, Vector3.down, turnSpeed * Time.deltaTime);
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    public void OnSpawned()
    {
        hasHit = false;
        isFalling = false;
        retargetTimer = 0f;


        pooledObject.ReturnToPool(lifetimeFailsafe);
    }

    public void OnDespawned()
    {
        target = null;
        hasHit = false;
        isFalling = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        int otherLayer = other.gameObject.layer;

        if (otherLayer == enemyLayer)
        {
            hasHit = true;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
            audioManager.PlaySoundAtPosition(ExlodeSound, transform.position);
            Instantiate(Effect, transform.position, Effect.transform.rotation);
            pooledObject.ReturnToPool();
        }
        else if (otherLayer == groundLayer)
        {
            hasHit = true;
            audioManager.PlaySoundAtPosition(ExlodeSound, transform.position);
            Instantiate(Effect, transform.position, Effect.transform.rotation);
            pooledObject.ReturnToPool();
        }
    }
}