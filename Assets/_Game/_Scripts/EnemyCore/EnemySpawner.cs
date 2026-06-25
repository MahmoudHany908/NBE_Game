using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")] 
    [SerializeField] private GameObject meleeRobotPrefab;
    [SerializeField] private GameObject securityDronePrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Enemy SpawnEnemy(EnemyType type, Vector3 position)
    {
        GameObject prefab = GetPrefab(type);

        if (prefab == null)
        {
            Debug.LogWarning("No prefab found for enemy type: " + type);
            return null;
        }
        return PoolManager.Instance.Spawn<Enemy>(prefab, position, Quaternion.identity);
    }

    private GameObject GetPrefab(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.MeleeRobot: return meleeRobotPrefab;
            case EnemyType.SecurityDrone: return securityDronePrefab;
            default: return null;
        }
    }
    
    public enum EnemyType
    {
        MeleeRobot,
        SecurityDrone
    }
}