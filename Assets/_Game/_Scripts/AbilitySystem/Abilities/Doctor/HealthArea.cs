
using UnityEngine;

public class HealthArea : MonoBehaviour
{
    [SerializeField] private int healAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealthComponent playerHealth = other.GetComponent<PlayerHealthComponent>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
            }
        }
    }

}
