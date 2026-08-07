using UnityEngine;


// template for an effect that can be pooled

public class Effect : MonoBehaviour, IPoolable
{
    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }
    public void OnDespawned()
    {
        //Debug.Log("Effect Despawned");
    }

    public void OnSpawned()
    {
        //Debug.Log("Effect Spawned");
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }
        pooledObject.ReturnToPool(2f);
    }

}
