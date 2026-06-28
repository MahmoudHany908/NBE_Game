using UnityEngine;

public class MeleeRobotAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1f;

    private Enemy enemy;
    private HealthComponent playerHealth;
    private float attackTimer;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void Start()
    {
        playerHealth = PlayerReference.Instance.PlayerTransform.GetComponent<HealthComponent>();
    }

    void Update()
    {
        if (PlayerReference.Instance == null) return;

        Transform player = PlayerReference.Instance.PlayerTransform;
        float distance = Vector3.Distance(transform.position, player.position);

        attackTimer -= Time.deltaTime;

        if (distance <= attackRange)
        {
            Attack();
        }
        else
        {
            Chase(player);
        }
    }

    private void Chase(Transform player)
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, player.position, enemy.MoveSpeed * Time.deltaTime);

        // face the player
        if (direction != Vector3.zero)
            transform.forward = direction;
    }

    private void Attack()
    {
        if (attackTimer > 0f) return;
        attackTimer = attackCooldown;
        playerHealth?.TakeDamage(attackDamage);
        Debug.Log("Melee Robot attacked player");
    }
}