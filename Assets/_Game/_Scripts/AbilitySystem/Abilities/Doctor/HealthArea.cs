
using UnityEngine;

public class HealthArea : MonoBehaviour
{
    [SerializeField] private int healAmount = 10;
    [SerializeField] private GameObject actvateVFX;
    [SerializeField] private SoundData healSound;
    private AudioManager audioManager;
    public void Init(AudioManager ad)
    {
        audioManager = ad;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealthComponent playerHealth = other.GetComponent<PlayerHealthComponent>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                Instantiate(actvateVFX, transform.position, Quaternion.identity);
                audioManager.PlaySoundAtPosition(healSound, transform.position);
                Destroy(gameObject);
            }
        }
    }

}
