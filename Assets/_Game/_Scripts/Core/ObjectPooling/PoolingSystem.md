# Object Pooling System

A lightweight pooling layer built on `UnityEngine.Pool.ObjectPool<T>`. Pools are
created automatically per prefab — no manual registration needed.

**Files:** `PoolManager.cs`, `PooledObject.cs`, `IPoolable.cs`
**Namespace:** `GameSystems.Pooling`

---

## Setup

1. Add a `PoolManager` component to an empty GameObject in your scene (e.g. `Systems/PoolManager`).
2. (Optional) In the inspector, add prefabs to **Prewarm Pools** with a `count` and `maxSize`
   to pre-instantiate inactive copies on scene load.

That's it — any other prefab gets its pool created the first time you spawn it.

---

## Spawning

```csharp
GameObject vfx = PoolManager.Instance.Spawn(vfxPrefab, position, rotation);
```

```csharp
// Generic overload, returns a component directly
Projectile p = PoolManager.Instance.Spawn<Projectile>(prefab, firePoint.position, firePoint.rotation);
```

```csharp
// Optional parent transform
PoolManager.Instance.Spawn(prefab, position, rotation, parentTransform);
```

---

## Despawning

From outside the object:

```csharp
PoolManager.Instance.Despawn(instance);
PoolManager.Instance.DespawnAfter(instance, 2f); // returns after a delay
```

From the object itself (every spawned instance has a `PooledObject` component):

```csharp
GetComponent<PooledObject>().ReturnToPool();
GetComponent<PooledObject>().ReturnToPool(2f);
```

`DespawnAfter` does **not** use coroutines — it's a single list checked once per
frame in `PoolManager.Update()`, so it's cheap even with hundreds of active timers.
If an object is reused before its delayed despawn fires, the stale timer is
automatically discarded (tracked via `PooledObject.Version`).

---

## Resetting state with `IPoolable`

Implement `IPoolable` on any component on the prefab to run setup/cleanup logic
every time the object is reused:

```csharp
public class Projectile : MonoBehaviour, IPoolable
{
    private PooledObject pooledObject;
    private Rigidbody rb;

    private void Awake()
    {
        // Cache references in Awake, NOT Start.
        pooledObject = GetComponent<PooledObject>();
        rb = GetComponent<Rigidbody>();
    }

    public void OnSpawned()
    {
        rb.linearVelocity = transform.forward * 20f;
        pooledObject.ReturnToPool(2f);
    }

    public void OnDespawned()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
```

> ⚠️ **Always cache references in `Awake()`, not `Start()`.**
> `OnSpawned()` fires the moment the object is first created — during
> `Instantiate()`, *before* `Start()` has run. If you cache `PooledObject` (or
> anything else) in `Start()`, the first spawn will hit a `NullReferenceException`.

- `OnSpawned()` — called every time the object is taken from the pool and re-activated. Reset velocity, replay particles, clear trails, etc.
- `OnDespawned()` — called every time it's returned to the pool and deactivated. Stop sounds, clear state, zero out physics.

---

## Other useful methods

| Method | Description |
|---|---|
| `Prewarm(prefab, count, maxSize)` | Manually fill a pool at runtime (e.g. before a wave starts) |
| `ClearPool(prefab)` | Destroys all inactive instances in a pool — useful on scene/level transitions |

---

## Gotchas

- **`isPressed` vs `wasPressedThisFrame`** — if testing spawns via input, use
  `wasPressedThisFrame`; `isPressed` fires every frame the key is held and will
  flood the pool.
- Despawning a `GameObject` that wasn't spawned by the pool (no `PooledObject`
  component) logs a warning and destroys it instead — safe fallback, but worth
  checking if you see that warning unexpectedly.
- Pooled objects are reparented under `PooledObjects` (a child of `PoolManager`)
  while inactive. Don't rely on their hierarchy position while despawned.
