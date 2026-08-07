using UnityEngine;
using VContainer;

public class EnemyHealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    private Enemy cachedEnemy;
    private IEventPublisher eventPublisher;

    [Inject]
    public void Construct(IEventPublisher publisher)
    {
        eventPublisher = publisher;
    }

    private void Awake()
    {
        cachedEnemy = GetComponent<Enemy>();
    }

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

        //Debug.Log("Enemy Took Damage: " + currentHealth);

        eventPublisher?.Publish(new EnemyTakeDamageEvent
        {
            Damage = amount,
            Enemy = cachedEnemy,
            DamagePosition = transform.position
        });


        if (currentHealth <= 0f) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        eventPublisher?.Publish(new EnemyDiedEvent
        {
            gameObject = gameObject,
            Enemy = cachedEnemy,
            DeathPosition = transform.position
        });
    }
}
