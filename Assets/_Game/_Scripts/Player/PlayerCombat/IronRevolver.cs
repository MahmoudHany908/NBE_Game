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

    [Header("Manual Aim")]
    [SerializeField] private PlayerInputs playerInputs;

    [Tooltip("If true, weapon fires only while attack direction input is active. If false, it keeps firing at the last aim direction.")]
    [SerializeField] private bool fireOnlyWhileAimInputActive = true;


    private float timer;
    private Vector3 fallbackDirection = Vector3.forward;

    [Inject] private PoolManager poolManager;
    [Inject] private AudioManager audioManager;

    private void Awake()
    {
        if (playerInputs == null)
            playerInputs = GetComponentInParent<PlayerInputs>();
    }

    private void Update()
    {
        if (playerInputs == null)
            return;

        bool canAim = playerInputs.HasAimDirection;

        if (!canAim && !fireOnlyWhileAimInputActive)
            canAim = playerInputs.HasLastAimDirection;

        if (!canAim)
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
        if (bulletPrefab == null || attackPos == null || playerInputs == null || poolManager == null)
            return;

        Vector3 dir = playerInputs.HasAimDirection
            ? playerInputs.CurrentAimDirection
            : playerInputs.LastAimDirection;

        if (dir.sqrMagnitude < 0.0001f)
            dir = fallbackDirection;

        dir.y = 0f;
        dir = dir.normalized;

        fallbackDirection = dir;

        Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);

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

        if (shootEffectPrefab != null)
        {
            poolManager.Spawn(shootEffectPrefab, attackPos.position, rotation);
        }

        if (audioManager != null && shootSound != null)
        {
            audioManager.PlaySoundAtPosition(shootSound, attackPos.position);
        }
    }
}