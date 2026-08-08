using UnityEngine;
using VContainer;

public class Projectile : MonoBehaviour, IPoolable
{
    [Header("Movement")]
    [SerializeField] private float lifeTime;
    [SerializeField] private float speed;

    [Header("Aim Assist")]
    [SerializeField] private bool useAimAssist = true;
    [SerializeField] private float assistRadius = 8f;
    [SerializeField] private float assistAngle = 30f;       // cone half-angle, degrees, from current travel direction
    [SerializeField] private float turnSpeed = 180f;        // degrees/sec steering toward target
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Hit VFX/SFX")]
    [SerializeField] private GameObject enemyHitEffectPrefab;
    [SerializeField] private SoundData enemyHitSound;
    [SerializeField] private GameObject surfaceHitEffectPrefab;
    [SerializeField] private SoundData surfaceHitSound;

    private PooledObject pooledObject;

    private Vector3 velocity = Vector3.zero;
    private bool hasHit = false;
    private int damage;
    private Transform assistTarget;

    [Inject] private PoolManager poolManager;
    [Inject] private AudioManager audioManager;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Initialize(Vector3 position, Vector3 vel, Quaternion rotation, int damageValue)
    {
        velocity = vel;
        transform.position = position;
        transform.rotation = rotation;
        damage = damageValue;

        assistTarget = useAimAssist ? FindAssistTarget(position, vel.normalized) : null;
    }

    public void OnDespawned()
    {
        assistTarget = null;
    }

    public void OnSpawned()
    {
        hasHit = false;
        pooledObject.ReturnToPool(lifeTime);
    }

    private void Update()
    {
        if (assistTarget != null)
        {
            SteerTowardTarget();
        }

        transform.position += velocity * Time.deltaTime * speed;
    }

  
    private Transform FindAssistTarget(Vector3 origin, Vector3 direction)
    {
        Collider[] candidates = Physics.OverlapSphere(origin, assistRadius, enemyLayerMask);

        Transform best = null;
        float bestAngle = assistAngle;

        foreach (Collider col in candidates)
        {
            if (col.GetComponent<IDamageable>() == null) continue;

            Vector3 toTarget = col.transform.position - origin;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) continue;

            float angle = Vector3.Angle(direction, toTarget.normalized);
            if (angle < bestAngle)
            {
                bestAngle = angle;
                best = col.transform;
            }
        }

        return best;
    }

    private void SteerTowardTarget()
    {
        if (assistTarget == null) return;

        Vector3 toTarget = assistTarget.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        Vector3 desiredDir = toTarget.normalized;
        Vector3 currentDir = velocity.normalized;

        Vector3 newDir = Vector3.RotateTowards(
            currentDir,
            desiredDir,
            turnSpeed * Mathf.Deg2Rad * Time.deltaTime,
            0f
        );

        velocity = newDir * velocity.magnitude;
        transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (hasHit) return;
        hasHit = true;

        if (other.GetComponentInParent<PlayerHealthComponent>() != null)
        {
            hasHit = false;
            return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            SpawnHitEffects(hitPoint, enemyHitEffectPrefab, enemyHitSound);
        }
        else
        {
            SpawnHitEffects(hitPoint, surfaceHitEffectPrefab, surfaceHitSound);
        }

        pooledObject.ReturnToPool();
    }

    private void SpawnHitEffects(Vector3 position, GameObject effectPrefab, SoundData sound)
    {
        if (effectPrefab != null)
        {
            if (poolManager != null)
                poolManager.Spawn(effectPrefab, position, Quaternion.identity);
            else
                Object.Instantiate(effectPrefab, position, Quaternion.identity);
        }

        if (sound != null && audioManager != null)
        {
            audioManager.PlaySoundAtPosition(sound, position);
        }
    }
}