using UnityEngine;
using UnityEngine.InputSystem.XR;

public class IronRevolver : MonoBehaviour, IAttack
{
    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private Transform attackPos;
    [SerializeField] private float fireRate = 1f;

    private ITargetProvider targetProvider;
    private float timer;

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
        GameObject target = targetProvider.GetTarget();

        if (target == null)
            return;

        Vector3 pos = target.transform.position;
        Vector3 dir = (pos - attackPos.position).normalized;

        Projectile projectile = PoolManager.Instance.Spawn<Projectile>(
            bulletPrefab.gameObject,
            attackPos.position,
            Quaternion.LookRotation(dir)
        );

        projectile.Initialize(
            attackPos.position,
            dir,
            Quaternion.LookRotation(dir)
        );
    }
}