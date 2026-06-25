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
        pooledObject.ReturnToPool(lifeTime);
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime * speed;
    }


}