using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifeTime;
    [SerializeField] private float speed;

    private PooledObject pooledObject;

    private Vector3 velocity = new Vector3(0, 0, 0);
    private bool hasHit = false;
    private int damage;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }
    public void Initialize(Vector3 position, Vector3 vel, Quaternion rotation, int damageValue)
    {
        velocity = vel;
        transform.position = position;
        transform.rotation = rotation;
        damage = damageValue;
    }

    public void OnDespawned()
    {

    }

    public void OnSpawned()
    {
        hasHit = false;
        pooledObject.ReturnToPool(lifeTime);
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Ignore other triggers (like the player's ClosestTargeter radius or enemy aggro ranges)
        if (other.isTrigger) return;

        if (hasHit) return;
        hasHit = true;

        // 2. Ignore collisions with the Player so it doesn't destroy itself on spawn
        if (other.GetComponentInParent<PlayerHealthComponent>() != null) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log("Projectile hit something: " + other.name);
        }

        // Destroy the projectile if it hits a solid object like a wall
        pooledObject.ReturnToPool();
    }


}