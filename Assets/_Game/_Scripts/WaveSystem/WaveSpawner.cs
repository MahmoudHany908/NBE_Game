using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Config")]
    [SerializeField] private List<EnemyEntry> enemies = new List<EnemyEntry>();

    [Header("Squad Config")]
    [SerializeField] private int squadCount = 3;
    [SerializeField] private float delayBetweenSquads = 2f;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 spawnBoxSize = new Vector3(20f, 1f, 20f);
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastOriginHeight = 10f;

    private BudgetCalculator budgetCalculator;
    private List<GameObject> activeEnemies = new List<GameObject>();

    [Inject] private PoolManager poolManager;

    public event Action OnAllSquadsSpawned;

    private void Awake()
    {
        budgetCalculator = GetComponent<BudgetCalculator>();
        if (budgetCalculator == null)
            Debug.LogError($"[{nameof(WaveSpawner)}] Missing required BudgetCalculator component on {gameObject.name}.", this);

        int removed = enemies.RemoveAll(e => e.enemyPrefab == null);
        if (removed > 0)
            Debug.LogWarning($"[{nameof(WaveSpawner)}] Removed {removed} EnemyEntry with a null prefab on {gameObject.name}. Check the 'enemies' list.", this);
    }

    public void StartSpawningWave(int wave)
    {
        if (budgetCalculator == null)
        {
            Debug.LogError($"[{nameof(WaveSpawner)}] Cannot start wave {wave}: BudgetCalculator is missing.", this);
            return;
        }

        int totalBudget = budgetCalculator.GetBudgetForWave(wave);
        if (totalBudget <= 0)
        {
            Debug.LogWarning($"[{nameof(WaveSpawner)}] Wave {wave} has zero/negative budget ({totalBudget}). No enemies will spawn.", this);
        }

        StartCoroutine(SpawnSquadsRoutine(totalBudget));
    }

    /// <summary>Removes dead/null enemies from tracking and reports whether any remain alive.</summary>
    public bool AreAllEnemiesDead()
    {
        activeEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);
        return activeEnemies.Count == 0;
    }

    private IEnumerator SpawnSquadsRoutine(int totalBudget)
    {
        int baseChunk = totalBudget / squadCount;
        int remainder = totalBudget % squadCount;

        for (int i = 0; i < squadCount; i++)
        {
            int squadBudget = baseChunk + (i == squadCount - 1 ? remainder : 0);
            List<GameObject> squad = GetEnemiesToSpawn(squadBudget);

            foreach (GameObject prefab in squad)
            {
                if (poolManager == null)
                {
                    Debug.LogError($"[{nameof(WaveSpawner)}] PoolManager dependency is null! Cannot spawn enemy '{prefab.name}'. Make sure PoolManager is registered in VContainer and injected.", this);
                    continue;
                }

                Vector3 spawnPos = GetGroundedSpawnPosition();
                Vector3 offset = new Vector3(0f, prefab.transform.localScale.y -1, 0f);
                GameObject enemy = poolManager.Spawn(prefab, spawnPos + offset, Quaternion.identity);
                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                }
            }

            if (i < squadCount - 1)
                yield return new WaitForSeconds(delayBetweenSquads);
        }

        OnAllSquadsSpawned?.Invoke();
    }

    private Vector3 GetGroundedSpawnPosition()
    {
        Vector3 center = transform.position;
        float x = UnityEngine.Random.Range(-spawnBoxSize.x / 2f, spawnBoxSize.x / 2f);
        float z = UnityEngine.Random.Range(-spawnBoxSize.z / 2f, spawnBoxSize.z / 2f);
        Vector3 origin = center + new Vector3(x, raycastOriginHeight, z);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastOriginHeight * 2f, groundMask))
            return hit.point;

        return center + new Vector3(x, 0f, z);
    }

    private List<GameObject> GetEnemiesToSpawn(int points)
    {
        List<GameObject> enemiesToSpawn = new List<GameObject>();
        int remaining = points;
        int maxAttempts = 100;
        int attempts = 0;

        while (remaining > 0 && attempts < maxAttempts)
        {
            attempts++;

            List<EnemyEntry> affordable = enemies.FindAll(e => e.cost <= remaining);
            if (affordable.Count == 0) break;

            EnemyEntry selected = affordable[UnityEngine.Random.Range(0, affordable.Count)];
            enemiesToSpawn.Add(selected.enemyPrefab);
            remaining -= selected.cost;
        }

        return enemiesToSpawn;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawCube(transform.position, spawnBoxSize);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireCube(transform.position, spawnBoxSize);
    }
}

[System.Serializable]
public class EnemyEntry
{
    public GameObject enemyPrefab;
    public int cost;
}