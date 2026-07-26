using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SeeleAbility : MonoBehaviour, IAbility
{
    [Header("Ability Settings")]
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private int effectCount = 3;
    [SerializeField] private GameObject shockVFX;
    [SerializeField] private SoundData soundData;

    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private LayerMask enemyLayer;

    private GameObject player;
    [Inject] private AudioManager audioManager;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void ActivateAbility()
    {
        List<Transform> enemiesInRange = GetEnemiesInRange();
        List<Transform> selectedTargets = PickRandomTargets(enemiesInRange, effectCount);
        foreach (Transform target in selectedTargets)
        {
            target.GetComponent<Enemy>()?.TakeStun(stunDuration);
            audioManager.PlaySoundAtPosition(soundData, target.position);
            Vector3 offset = new Vector3(0, 1, 0);
            Instantiate(shockVFX, target.position, Quaternion.identity);
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
        List<Transform> shuffled = new List<Transform>(pool);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int takeCount = Mathf.Min(count, shuffled.Count);
        return shuffled.GetRange(0, takeCount);
    }
}
