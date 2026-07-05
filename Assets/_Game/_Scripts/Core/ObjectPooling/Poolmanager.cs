using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{


    [System.Serializable]
    public class PrewarmEntry
    {
        public GameObject prefab;
        [Min(0)] public int count = 10;
        [Min(1)] public int maxSize = 100;
    }

    [SerializeField] private List<PrewarmEntry> prewarmPools = new();


    [SerializeField] private Transform poolParent;

    private readonly Dictionary<int, ObjectPool<GameObject>> _pools = new();
    private readonly Dictionary<int, GameObject> _prefabLookup = new();
    private readonly List<ScheduledDespawn> _scheduledDespawns = new();

    private struct ScheduledDespawn
    {
        public GameObject instance;
        public PooledObject pooled;
        public int version;
        public float expireTime;
    }

    private void Awake()
    {

        DontDestroyOnLoad(gameObject);
        if (poolParent == null)
        {
            var holder = new GameObject("PooledObjects");
            holder.transform.SetParent(transform);
            poolParent = holder.transform;
        }

        foreach (var entry in prewarmPools)
        {
            if (entry.prefab == null) continue;
            GetOrCreatePool(entry.prefab, entry.maxSize);
            Prewarm(entry.prefab, entry.count);
        }
    }

    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab, int maxSize = 100)
    {
        int key = prefab.GetInstanceID();
        if (_pools.TryGetValue(key, out var existing))
            return existing;

        _prefabLookup[key] = prefab;

        var pool = new ObjectPool<GameObject>(
            createFunc: () => CreateInstance(prefab, key),
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: instance => Destroy(instance),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: maxSize
        );

        _pools[key] = pool;
        return pool;
    }

    private GameObject CreateInstance(GameObject prefab, int prefabKey)
    {
        var instance = Instantiate(prefab, poolParent);
        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null)
            pooled = instance.AddComponent<PooledObject>();
        pooled.PrefabId = prefabKey;
        return instance;
    }

    private static void OnGet(GameObject instance)
    {
        instance.SetActive(true);

        var pooled = instance.GetComponent<PooledObject>();
        if (pooled != null)
            pooled.Version++;

        foreach (var poolable in instance.GetComponents<IPoolable>())
            poolable.OnSpawned();
    }

    private void OnRelease(GameObject instance)
    {
        foreach (var poolable in instance.GetComponents<IPoolable>())
            poolable.OnDespawned();

        instance.SetActive(false);
        instance.transform.SetParent(poolParent);
    }

    public void Prewarm(GameObject prefab, int count, int maxSize = 100)
    {
        var pool = GetOrCreatePool(prefab, maxSize);
        var temp = new List<GameObject>(count);
        for (int i = 0; i < count; i++)
            temp.Add(pool.Get());
        foreach (var instance in temp)
            pool.Release(instance);
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var pool = GetOrCreatePool(prefab);
        var instance = pool.Get();

        var t = instance.transform;
        t.SetParent(parent, false);
        t.SetPositionAndRotation(position, rotation);

        return instance;
    }

    public GameObject Spawn(GameObject prefab, Transform parent = null)
    {
        return Spawn(prefab, prefab.transform.position, prefab.transform.rotation, parent);
    }

    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        var instance = Spawn(prefab, position, rotation, parent);
        return instance.GetComponent<T>();
    }

    public void Despawn(GameObject instance)
    {
        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null || !_pools.TryGetValue(pooled.PrefabId, out var pool))
        {
            Debug.LogWarning($"[PoolManager] '{instance.name}' was not spawned by this pool. Destroying instead.", instance);
            Destroy(instance);
            return;
        }

        pool.Release(instance);
    }


    public void DespawnAfter(GameObject instance, float delay)
    {
        var pooled = instance.GetComponent<PooledObject>();
        _scheduledDespawns.Add(new ScheduledDespawn
        {
            instance = instance,
            pooled = pooled,
            version = pooled != null ? pooled.Version : 0,
            expireTime = Time.time + delay
        });
    }

    private void Update()
    {
        for (int i = _scheduledDespawns.Count - 1; i >= 0; i--)
        {
            var entry = _scheduledDespawns[i];
            if (Time.time < entry.expireTime)
                continue;

            // Swap-remove: O(1), order doesn't matter for timers
            int last = _scheduledDespawns.Count - 1;
            _scheduledDespawns[i] = _scheduledDespawns[last];
            _scheduledDespawns.RemoveAt(last);

            if (entry.instance == null)
                continue;

            // Skip if the object was already returned/re-spawned since this was scheduled
            if (entry.pooled != null && entry.pooled.Version != entry.version)
                continue;

            if (entry.instance.activeInHierarchy)
                Despawn(entry.instance);
        }
    }


    public void ClearPool(GameObject prefab)
    {
        int key = prefab.GetInstanceID();
        if (_pools.TryGetValue(key, out var pool))
        {
            pool.Clear();
            _pools.Remove(key);
            _prefabLookup.Remove(key);
        }
    }
}
