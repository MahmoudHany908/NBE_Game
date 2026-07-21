using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class RookAbility : MonoBehaviour, IAbility
{
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private int rocketCount = 3;
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float spawnHeight = 20f;
    [SerializeField] private LayerMask enemyLayer;

    private Vector3 castOrigin => transform.position;
    private List<Transform> targets = new List<Transform>();
    private GameObject player;
    [Inject] private PoolManager poolManager;



    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    public void ActivateAbility()
    {
        List<Transform> enemiesInRange = GetEnemiesInRange();
        List<Transform> selectedTargets = PickRandomTargets(enemiesInRange, rocketCount);

        Debug.Log($"Found {enemiesInRange.Count} enemies in range. Selected {selectedTargets.Count} targets for rockets.");
        foreach (Transform target in selectedTargets)
        {
            Debug.Log($"Spawning rocket for target: {target.name}");
            SpawnRocket(target);
        }
    }

    private List<Transform> GetEnemiesInRange()
    {
        List<Transform> found = new List<Transform>();
        Collider[] hits = Physics.OverlapSphere(player.transform.position, detectionRadius, enemyLayer);

        foreach (Collider hit in hits)
        {
            found.Add(hit.transform);
        }

        return found;
    }

    private List<Transform> PickRandomTargets(List<Transform> pool, int count)
    {
        // Shuffle a copy (Fisher-Yates), then take however many we can afford
        List<Transform> shuffled = new List<Transform>(pool);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int takeCount = Mathf.Min(count, shuffled.Count);
        return shuffled.GetRange(0, takeCount);
    }

    private void SpawnRocket(Transform target)
    {
        Vector3 spawnPos = player.transform.position + Vector3.up * spawnHeight;

        GameObject rocket = poolManager.Spawn(rocketPrefab, spawnPos, Quaternion.identity);

        if (rocket.TryGetComponent<Rocket>(out var rocketComponent))
        {
            rocketComponent.Init(target);
        }
    }
}