using System;
using System.Collections.Generic;
using UnityEngine;
using VaultSurvivors.Core.Production;

/// <summary>
/// Defines the loot an enemy can drop on death.
/// Assign to <see cref="EnemyDataSO.lootTable"/> for per-enemy-type balancing.
/// </summary>
[CreateAssetMenu(fileName = "NewLootTable", menuName = "Game/Enemy/LootTable")]
public class EnemyLootTableSO : ScriptableObject
{
    [Header("XP")]
    [Tooltip("Guaranteed XP awarded on kill.")]
    [SerializeField] private int xpReward = 10;

    [Header("Resource Drops")]
    [Tooltip("Each entry is rolled independently on kill.")]
    [SerializeField] private List<ResourceDrop> resourceDrops = new List<ResourceDrop>();

    /// <summary>
    /// Rolls all drop entries and returns the resulting loot.
    /// Call this once per kill — it uses UnityEngine.Random internally.
    /// </summary>
    public LootResult EvaluateDrops()
    {
        var result = new LootResult
        {
            xp = xpReward,
            grantedResources = new List<ResourceGrant>()
        };

        for (int i = 0; i < resourceDrops.Count; i++)
        {
            ResourceDrop drop = resourceDrops[i];

            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll > drop.dropChance) continue;

            int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);
            if (amount <= 0) continue;

            result.grantedResources.Add(new ResourceGrant
            {
                resourceType = drop.resourceType,
                amount = amount
            });
        }

        return result;
    }
}

/// <summary>
/// A single resource drop entry in a loot table.
/// Each entry is rolled independently.
/// </summary>
[Serializable]
public struct ResourceDrop
{
    public ResourceType resourceType;

    [Tooltip("Chance to drop this resource (0–100%).")]
    [Range(0f, 100f)]
    public float dropChance;

    [Tooltip("Minimum amount dropped (inclusive).")]
    [Min(1)]
    public int minAmount;

    [Tooltip("Maximum amount dropped (inclusive).")]
    [Min(1)]
    public int maxAmount;
}

/// <summary>
/// The result of evaluating a loot table — guaranteed XP plus any rolled resource grants.
/// </summary>
public struct LootResult
{
    public int xp;
    public List<ResourceGrant> grantedResources;
}

/// <summary>
/// A single confirmed resource grant (type + amount).
/// </summary>
public struct ResourceGrant
{
    public ResourceType resourceType;
    public int amount;
}
