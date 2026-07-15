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

        var projectile = poolManager.Spawn<DroneProjectile>(
            projectilePrefab, enemy.transform.position, Quaternion.identity);
        
        projectile?.Init(target.position, enemy.Data.attackDamage);
    }
}