using UnityEngine;
using VContainer;

public class DoctorAbility : MonoBehaviour, IAbility
{
    [SerializeField] private GameObject healingAreaPrefab;
    [SerializeField] private float spawnRadius = 5f;

    [Header("Cooldown")]
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private UnityEngine.UI.Image cooldownImage;

    private GameObject player;
    private float currentCooldown = 0f;

    [Inject] private AudioManager AudioManager;

    public bool IsOnCooldown => currentCooldown > 0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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

        //spawn a healing area in a random position around the player the position need to be valid and not inside a wall or other object
        Vector3 randomPosition = player.transform.position + Random.insideUnitSphere * spawnRadius;
        randomPosition.y = player.transform.position.y; // Keep the same height as the player



        GameObject go = Instantiate(healingAreaPrefab, randomPosition, Quaternion.identity);
        go.GetComponent<HealthArea>().Init(AudioManager);


    }
}
