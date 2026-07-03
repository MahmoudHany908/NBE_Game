using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifeTime;
    [SerializeField] private float speed;



    private PooledObject pooledObject;

    private Vector3 velocity = new Vector3(0, 0, 0);


    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }
    public void Initialize(Vector3 position, Vector3 vel, Quaternion rotation)
    {
        velocity = vel;
        transform.position = position;
        transform.rotation = rotation;
    }

    public void OnDespawned()
    {

    }

    public void OnSpawned()
    {

    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Ignore other triggers (like the player's ClosestTargeter radius or enemy aggro ranges)
        if (other.isTrigger) return;

        // 2. Ignore collisions with the Player so it doesn't destroy itself on spawn
        if (other.GetComponentInParent<PlayerHealthComponent>() != null) return;

        // 3. Look for the HealthComponent on this object or its parents (in case the collider is on a child object)
        HealthComponent health = other.GetComponentInParent<HealthComponent>();

        if (health != null)
        {
            health.TakeDamage(20);
            Debug.Log("Projectile hit something: " + other.name);
            pooledObject.ReturnToPool();
            return;
        }

        // Destroy the projectile if it hits a solid object like a wall
        pooledObject.ReturnToPool();
    }


}