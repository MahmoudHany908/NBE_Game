using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class OrbitalBeam : MonoBehaviour, IPoolable
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private float damageRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effects")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private SoundData beamSound;

    private PooledObject pooledObject;
    private AudioManager audioManager;
    private float lifeTimer;
    private float tickTimer;

    private static readonly Collider[] hitColliders = new Collider[16];

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Init(AudioManager audioManager)
    {
        this.audioManager = audioManager;
    }

    public void OnSpawned()
    {
        lifeTimer = duration;
        // Tick immediately on spawn, then every damageInterval after that.
        tickTimer = 0f;

        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, impactEffect.transform.rotation);
        }

        if (audioManager != null && beamSound != null)
        {
            audioManager.PlaySoundAtPosition(beamSound, transform.position);
        }

        pooledObject.ReturnToPool(duration);
    }

    public void OnDespawned()
    {
        audioManager = null;
    }

    private void Update()
    {
        if (lifeTimer <= 0f) return;

        lifeTimer -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            tickTimer = damageInterval;
            DamageEnemiesInRange();
        }
    }

    private void DamageEnemiesInRange()
    {
        // Sphere check around the beam's base. If you want a strict vertical
        // cylinder instead (so height doesn't affect who gets hit), swap this
        // for Physics.OverlapCapsule from transform.position up to position + height.
        int numHits = Physics.OverlapSphereNonAlloc(transform.position, damageRadius, hitColliders, enemyLayer);

        for (int i = 0; i < numHits; i++)
        {
            if (hitColliders[i].TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damagePerTick);
            }
        }
    }
}