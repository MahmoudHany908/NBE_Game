using UnityEngine;


public class PlayerHealthComponent : MonoBehaviour
{

    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;


    void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        currentHealth = value;
    }


    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        EventBus.Publish(new PlayerTakeDamageEvent
        {
            Damage = amount,

        });

        Debug.Log("Player took damage: " + currentHealth);
        if (currentHealth <= 0f) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        EventBus.Publish(new PlayerDeathEvent { deathPostion = transform.position });

    }
}