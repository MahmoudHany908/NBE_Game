using UnityEngine;
using UnityEngine.InputSystem;

public class HealthComponent : MonoBehaviour
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
        EventBus.Publish(new EnemyTakeDamageEvent
        {
            Damage = amount,
            Enemy = GetComponent<Enemy>(),
            DamagePosition = transform.position
        });

        Debug.Log(currentHealth);
        if (currentHealth <= 0f) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        EventBus.Publish(new EnemyDiedEvent { gameObject = gameObject, DeathPosition = transform.position });

    }
}