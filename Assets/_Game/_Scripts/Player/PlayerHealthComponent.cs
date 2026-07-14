using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerHealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;
    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) TakeDamage(20);
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

        _eventPublisher?.Publish(new PlayerTakeDamageEvent(damage: amount));

        Debug.Log("Player took damage: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        _eventPublisher?.Publish(new PlayerDiedEvent(position: transform.position));
    }
}