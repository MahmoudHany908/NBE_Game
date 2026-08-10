using System;
using UnityEngine;
using Alchemy.Inspector;

[Serializable]
public class ChaseMovementStrategy : IMovementStrategy {
    public void Move(Enemy enemy, Transform target)
    {
        if (enemy.Agent == null || !enemy.Agent.isOnNavMesh) return;

        float distance = Vector3.Distance(enemy.transform.position, target.transform.position);

        if (distance <= enemy.Data.attackRange) 
        {
            enemy.Agent.isStopped = true;
            return;
        }

        enemy.Agent.isStopped = false;
        enemy.Agent.SetDestination(target.position);
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
        if (enemy.Agent == null || !enemy.Agent.isOnNavMesh) return;

        if (enableFlying)
        {
            if (enemy.TargetHeight == 0f)
            {
                enemy.TargetHeight = UnityEngine.Random.Range(minHeight, maxHeight);
            }
            
            enemy.Agent.baseOffset = Mathf.MoveTowards(enemy.Agent.baseOffset, enemy.TargetHeight, enemy.Data.moveSpeed * Time.deltaTime);
        }

        Vector3 flatSelf = new Vector3(enemy.transform.position.x, 0f, enemy.transform.position.z);
        Vector3 flatTarget = new Vector3(target.position.x, 0f, target.position.z);

        float distance = Vector3.Distance(flatSelf, flatTarget);
        float difference = distance - preferredDistance;

        if (Mathf.Abs(difference) <= distanceThreshold) 
        {
            enemy.Agent.isStopped = true;
            return;
        }

        Vector3 direction = (flatSelf - flatTarget).normalized;
        enemy.Agent.isStopped = false;

        if (difference > 0f)
        {
            // Too far, chase target
            enemy.Agent.SetDestination(target.position);
        }
        else
        {
            // Too close, move away
            enemy.Agent.Move(direction * enemy.Data.moveSpeed * Time.deltaTime);
        }
    }
}