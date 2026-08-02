using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

public class JessyAbility : MonoBehaviour, IAbility
{
    [Header("Drone")]
    [SerializeField] private int count = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private JessyDrone dronePrefab;

    [Header("Cooldown")]
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private UnityEngine.UI.Image cooldownImage;

    private Transform player;
    private float currentCooldown = 0f;

    [Inject] private PoolManager poolManager;
    [Inject] private AudioManager audioManager;

    public bool IsOnCooldown => currentCooldown > 0f;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = currentCooldown / cooldownTime;
            }
        }
    }

    public void ActivateAbility()
    {
        if (IsOnCooldown) return;

        currentCooldown = cooldownTime;
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1f;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = Random.Range(3f, 5f);

            Vector3 spawnPos = player.position + randomOffset;

            JessyDrone drone = Object.Instantiate(dronePrefab, spawnPos, Quaternion.identity);
            drone.Initialize(poolManager, audioManager);
        }
    }
}