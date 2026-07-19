using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

public class JessyAbility : MonoBehaviour, IAbility
{
    [Header("Drone")]
    [SerializeField] private int count = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private JessyDrone dronePrefab;

    private Transform player;

    [Inject] private PoolManager poolManager;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void ActivateAbility()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = Random.Range(3f, 5f);

            Vector3 spawnPos = player.position + randomOffset;

            JessyDrone drone = Object.Instantiate(dronePrefab, spawnPos, Quaternion.identity);
            drone.Initialize(poolManager);
        }
    }
}