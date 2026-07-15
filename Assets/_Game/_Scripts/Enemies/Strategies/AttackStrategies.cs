using System;
using UnityEngine;

[Serializable]
public class MeleeAttackStrategy : IAttackStrategy
{
    public void HandleAttack(Enemy enemy, Transform target, PoolManager poolManager)
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

    public void HandleAttack(Enemy enemy, Transform target, PoolManager poolManager)
    {
        if (projectilePrefab == null) return;

        Vector3 aimTarget = target.position + Vector3.up * 1.5f;
        Vector3 direction = (aimTarget - enemy.transform.position).normalized;
        
        Quaternion rotation = direction.sqrMagnitude > 0.001f 
            ? Quaternion.LookRotation(direction) 
            : Quaternion.identity;

        var projectile = poolManager.Spawn<DroneProjectile>(
            projectilePrefab, enemy.transform.position, rotation);
        
        projectile?.Init(aimTarget, enemy.Data.attackDamage);
    }
}