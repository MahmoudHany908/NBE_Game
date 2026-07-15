using UnityEngine;

public interface IMovementStrategy
{
    void Move(Enemy enemy, Transform target);
}

public interface IAttackStrategy
{
    void HandleAttack(Enemy enemy, Transform target, PoolManager poolManager);
}