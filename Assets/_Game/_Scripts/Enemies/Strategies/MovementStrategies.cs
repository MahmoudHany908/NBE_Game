using System;
using UnityEngine;
using Alchemy.Inspector;

[Serializable]
public class ChaseMovementStrategy : IMovementStrategy {
    public void Move(Enemy enemy, Transform target)
    {
        float distance = Vector3.Distance(enemy.transform.position, target.transform.position);

        if (distance <= enemy.Data.attackRange) return;

        Vector3 direction = (target.position - enemy.transform.position).normalized;
        direction.y = 0f;
        enemy.transform.position += direction * enemy.Data.moveSpeed * Time.deltaTime;
    }
}

[Serializable]
public class KeepDistanceMovementStrategy : IMovementStrategy
{
    [SerializeField] private float preferredDistance = 4f;
    [SerializeField] private float distanceThreshold = 1f;
    [LabelText("Enable Flying Height")]
    [SerializeField] private bool enableFlying = true;
    
    [ShowIf(nameof(enableFlying))]
    [SerializeField] private float minHeight = 3f;
    
    [ShowIf(nameof(enableFlying))]
    [SerializeField] private float maxHeight = 5f;
    public void Move(Enemy enemy, Transform target)
    {
        if (enableFlying)
        {
            if (enemy.TargetHeight == 0f)
            {
                enemy.TargetHeight = UnityEngine.Random.Range(minHeight, maxHeight);
            }
            
            Vector3 pos = enemy.transform.position;
            pos.y = Mathf.MoveTowards(pos.y, enemy.TargetHeight, enemy.Data.moveSpeed * Time.deltaTime);
            enemy.transform.position = pos;
        }

        Vector3 flatSelf = new Vector3(enemy.transform.position.x, 0f, enemy.transform.position.z);
        Vector3 flatTarget = new Vector3(target.position.x, 0f, target.position.z);

        float distance = Vector3.Distance(flatSelf, flatTarget);
        float difference = distance - preferredDistance;

        if (Mathf.Abs(difference) <= distanceThreshold) return;
        Vector3 direction = (flatSelf - flatTarget).normalized;

        if (difference > 0f)
            direction = -direction;
        enemy.transform.position += direction * enemy.Data.moveSpeed * Time.deltaTime;
    }
}