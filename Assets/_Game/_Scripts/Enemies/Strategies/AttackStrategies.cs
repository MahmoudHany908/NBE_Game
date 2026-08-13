using System;
using UnityEngine;

[Serializable]
public class MeleeAttackStrategy : IAttackStrategy
{
    public void HandleAttack(Enemy enemy, Transform target, PoolManager poolManager, AudioManager audioManager)
    {
        float distance = Vector3.Distance(enemy.transform.position, target.position);
        if (distance > enemy.Data.attackRange) return;

        if (target.TryGetComponent<PlayerHealthComponent>(out var playerHealth))
        {
            playerHealth.TakeDamage(enemy.Data.attackDamage);
        }
    }
}

[Serializable]
public class ProjectileAttackStrategy : IAttackStrategy
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private SoundData attackSound;

    public void HandleAttack(Enemy enemy, Transform target, PoolManager poolManager, AudioManager audioManager)
    {
        if (projectilePrefab == null) return;

        Vector3 aimTarget = target.position + Vector3.up * 1.5f;
        Vector3 direction = (aimTarget - enemy.transform.position).normalized;

        Quaternion rotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f)
            : Quaternion.Euler(90f, 0f, 0f);

        var projectile = poolManager.Spawn<DroneProjectile>(
            projectilePrefab, enemy.transform.position, rotation);

        projectile?.Init(aimTarget, enemy.Data.attackDamage);
        audioManager?.PlaySoundAtPosition(attackSound, enemy.transform.position);
    }
}