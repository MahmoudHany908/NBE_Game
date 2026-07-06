using UnityEngine;

public class DroneProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask playerLayer;

    private Vector3 direction;
    private int damage;
    private PooledObject pooledObject;

    void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Init(Vector3 targetPosition, int damageAmount)
    {
        direction = (targetPosition - transform.position).normalized;
        damage = damageAmount;
    }

    public void OnSpawned()
    {
        pooledObject.ReturnToPool(lifetime);
    }

    public void OnDespawned()
    {
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        if (other.TryGetComponent<PlayerHealthComponent>(out var playerHealth))
        {
            playerHealth.TakeDamage(damage);
        }

        pooledObject.ReturnToPool();
    }
}