using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        budgetCalculator = GetComponent<BudgetCalculator>();
    }

    public void StartSpawningWave(int wave)
    {
        int totalBudget = budgetCalculator.GetBudgetForWave(wave);
        StartCoroutine(SpawnSquadsRoutine(totalBudget));
    }

    public bool AreAllEnemiesDead()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count == 0;
    }

    private IEnumerator SpawnSquadsRoutine(int totalBudget)
    {
        // Split budget into N roughly equal chunks
        int baseChunk = totalBudget / squadCount;
        int remainder = totalBudget % squadCount;

        for (int i = 0; i < squadCount; i++)
        {
            int squadBudget = baseChunk + (i == squadCount - 1 ? remainder : 0);
            List<GameObject> squad = GetEnemiesToSpawn(squadBudget);

            foreach (GameObject prefab in squad)
            {
                Vector3 spawnPos = GetGroundedSpawnPosition();
                GameObject enemy = PoolManager.Instance.Spawn(prefab, spawnPos +
                new Vector3(0, prefab.gameObject.transform.localScale.y, 0),
                Quaternion.identity);
                if (enemy != null)
                    activeEnemies.Add(enemy);
            }

            if (i < squadCount - 1)
                yield return new WaitForSeconds(delayBetweenSquads);
        }
    }

    private Vector3 GetGroundedSpawnPosition()
    {
        // Pick a random XZ point inside the box, cast down to find ground Y
        Vector3 center = transform.position;
        float x = Random.Range(-spawnBoxSize.x / 2f, spawnBoxSize.x / 2f);
        float z = Random.Range(-spawnBoxSize.z / 2f, spawnBoxSize.z / 2f);
        Vector3 origin = center + new Vector3(x, raycastOriginHeight, z);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastOriginHeight * 2f, groundMask))
            return hit.point;

        // Fallback: no ground found, use flat position
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

            EnemyEntry selected = affordable[Random.Range(0, affordable.Count)];
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