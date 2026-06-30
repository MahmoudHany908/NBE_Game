using System.Collections.Generic;
using UnityEngine;

public class ClosestTargeter : MonoBehaviour, ITargetProvider
{
    [Header("Detection")]
    [SerializeField] private LayerMask targetLayers;

    private readonly List<Transform> targetsInRange = new();

    public event System.Action<GameObject> TargetChanged;

    private float timer;
    private float checkInterval;

    private void OnEnable()
    {
        checkInterval = Random.Range(0.5f, 1.5f);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;
            checkInterval = Random.Range(0.5f, 1.5f);

            targetsInRange.RemoveAll(t => t == null);
        }
    }

    public GameObject GetTarget()
    {
        if (targetsInRange.Count == 0)
            return null;

        Transform closest = null;
        float closestDistance = float.MaxValue;

        Vector3 position = transform.position;

        foreach (var target in targetsInRange)
        {
            float sqrDistance = (target.position - position).sqrMagnitude;

            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closest = target;
            }
        }

        return closest?.gameObject;
    }

    public bool HasTarget()
    {
        return GetTarget() != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTarget(other.gameObject))
        {
            targetsInRange.Add(other.transform);
            TargetChanged?.Invoke(GetTarget());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        targetsInRange.Remove(other.transform);
    }

    private bool IsValidTarget(GameObject target)
    {
        return (targetLayers.value & (1 << target.layer)) != 0;
    }
}