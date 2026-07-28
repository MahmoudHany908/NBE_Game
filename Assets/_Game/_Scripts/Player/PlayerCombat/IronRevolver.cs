using UnityEngine;
using VContainer;

public class IronRevolver : MonoBehaviour, IAttack
{
    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private Transform attackPos;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 20;
    [SerializeField] private GameObject shootEffectPrefab;
    [SerializeField] private SoundData shootSound;

    private ITargetProvider targetProvider;
    private float timer;
    [Inject] private PoolManager poolManager;
    [Inject] private AudioManager audioManager;

    private void Awake()
    {
        targetProvider = GetComponentInChildren<ITargetProvider>();
    }

    private void Update()
    {
        if (!targetProvider.HasTarget())
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Attack();
            timer = 1f / fireRate;
        }
    }

    public void Attack()
    {
        if (bulletPrefab == null || attackPos == null)
            return;

        GameObject target = targetProvider.GetTarget();

        if (target == null)
            return;

        Vector3 pos = target.transform.position;
        Vector3 dir = (pos - attackPos.position).normalized;

        if (dir == Vector3.zero)
        {
            dir = attackPos.forward;
        }

        Quaternion rotation = Quaternion.LookRotation(dir);

        Projectile projectile = poolManager.Spawn<Projectile>(
            bulletPrefab.gameObject,
            attackPos.position,
            rotation
        );

        if (projectile != null)
        {
            projectile.Initialize(
                attackPos.position,
                dir,
                rotation,
                damage
            );
        }

        if (poolManager != null && shootEffectPrefab != null)
        {
            poolManager.Spawn(shootEffectPrefab, attackPos.position, rotation);
        }

        if (audioManager != null && shootSound != null)
        {
            audioManager.PlaySoundAtPosition(shootSound, attackPos.position);
        }
    }
}