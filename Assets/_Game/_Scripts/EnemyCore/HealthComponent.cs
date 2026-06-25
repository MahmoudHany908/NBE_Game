using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100f;
    
    private int currentHealth;
    private bool isDead;
    
    public static event System.Action<GameObject> OnDied;
    public static event System.Action<GameObject, int> OnDamageTaken;

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
        if (!isDead) return;
        currentHealth -= amount;
        OnDamageTaken?.Invoke(gameObject, amount);
        if (currentHealth <= 0f) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDied?.Invoke(gameObject);
    }
}