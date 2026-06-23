using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    
    private float currentHealth;
    private bool isDead;
    
    public static event System.Action<GameObject> OnDied;
    public static event System.Action<GameObject, float> OnDamageTaken;

    void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = value;
    }

    public void TakeDamage(float amount)
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