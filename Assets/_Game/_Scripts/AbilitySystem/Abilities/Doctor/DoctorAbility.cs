using UnityEngine;
using VContainer;

public class DoctorAbility : MonoBehaviour, IAbility
{
    [SerializeField] private GameObject healingAreaPrefab;
    [SerializeField] private float spawnRadius = 5f;
    private GameObject player;
    [Inject] private AudioManager AudioManager;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void ActivateAbility()
    {
        //spawn a healing area in a random position around the player the position need to be valid and not inside a wall or other object
        Vector3 randomPosition = player.transform.position + Random.insideUnitSphere * spawnRadius;
        randomPosition.y = player.transform.position.y; // Keep the same height as the player



        GameObject go = Instantiate(healingAreaPrefab, randomPosition, Quaternion.identity);
        go.GetComponent<HealthArea>().Init(AudioManager);


    }
}
