using System;
using System.Collections.Generic;
using UnityEngine;
using VaultSurvivors.Core.Production;

/// <summary>
/// Tracks accumulated XP and resources during an arena run.
/// Listens for enemy deaths and rolls loot, publishes per-drop events for the popup UI.
/// On successful extraction, writes rewards to the save file.
/// On player death, discards all accumulated rewards.
/// 
/// Registered as a scoped singleton in <see cref="ArenaLifetimeScope"/>.
/// This is a plain C# class â€” not a MonoBehaviour.
/// </summary>
public class ArenaRewardTracker : IDisposable
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IEventSubscriber _eventSubscriber;
    private readonly ISaveSystem _saveSystem;

    private int _accumulatedXP;
    private readonly Dictionary<ResourceType, int> _accumulatedResources = new Dictionary<ResourceType, int>();

    private bool _isSubscribed;

    public ArenaRewardTracker(
        IEventPublisher eventPublisher,
        IEventSubscriber eventSubscriber,
        ISaveSystem saveSystem)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
        _saveSystem = saveSystem;

        ResetAccumulation();
        Subscribe();
    }

    #region Event Subscriptions

    private void Subscribe()
    {
        if (_isSubscribed || _eventSubscriber == null) return;

        _eventSubscriber.Subscribe<EnemyDiedEvent>(HandleEnemyDied);
        _eventSubscriber.Subscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
        _eventSubscriber.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || _eventSubscriber == null) return;

        _eventSubscriber.Unsubscribe<EnemyDiedEvent>(HandleEnemyDied);
        _eventSubscriber.Unsubscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
        _eventSubscriber.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
        _isSubscribed = false;
    }

    public void Dispose()
    {
        Unsubscribe();
    }

    #endregion

    #region Event Handlers

    private void HandleEnemyDied(EnemyDiedEvent evt)
    {
        if (evt.Enemy == null)
        {
            //Debug.LogWarning("[ArenaRewardTracker] EnemyDiedEvent received with null Enemy reference.");
            return;
        }

        EnemyLootTableSO lootTable = evt.Enemy.Data?.lootTable;
        if (lootTable == null) return; // Enemy has no loot table â€” skip silently

        LootResult result = lootTable.EvaluateDrops();

        // Accumulate XP
        if (result.xp > 0)
        {
            _accumulatedXP += result.xp;
            _eventPublisher?.Publish(new XpGainedEvent(result.xp));
        }

        // Accumulate resources
        if (result.grantedResources != null)
        {
            for (int i = 0; i < result.grantedResources.Count; i++)
            {
                ResourceGrant grant = result.grantedResources[i];

                if (_accumulatedResources.ContainsKey(grant.resourceType))
                    _accumulatedResources[grant.resourceType] += grant.amount;
                else
                    _accumulatedResources[grant.resourceType] = grant.amount;

                _eventPublisher?.Publish(new LootGainedEvent(grant.resourceType, grant.amount));
            }
        }
    }

    private void HandleExtractionDecision(ExtractionDecisionEvent evt)
    {
        if (!evt.DidExtract) return;

        ApplyRewardsToSave();
        ResetAccumulation();
    }

    private void HandlePlayerDied(PlayerDiedEvent evt)
    {
        //Debug.Log("[ArenaRewardTracker] Player died â€” all accumulated rewards discarded.");
        ResetAccumulation();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Returns current accumulated XP for display on the extraction panel.
    /// </summary>
    public int GetAccumulatedXP() => _accumulatedXP;

    /// <summary>
    /// Returns a copy of accumulated resources for display on the extraction panel.
    /// </summary>
    public Dictionary<ResourceType, int> GetAccumulatedResources()
    {
        return new Dictionary<ResourceType, int>(_accumulatedResources);
    }

    /// <summary>
    /// Returns true if the player has earned any rewards this run.
    /// </summary>
    public bool HasAnyRewards()
    {
        if (_accumulatedXP > 0) return true;

        foreach (var kvp in _accumulatedResources)
        {
            if (kvp.Value > 0) return true;
        }

        return false;
    }

    #endregion

    #region Internals

    private void ResetAccumulation()
    {
        _accumulatedXP = 0;
        _accumulatedResources.Clear();

        // Pre-populate all resource types with zero for consistent iteration
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            _accumulatedResources[type] = 0;
        }
    }

    private void ApplyRewardsToSave()
    {
        if (_saveSystem == null)
        {
            //Debug.LogError("[ArenaRewardTracker] Cannot apply rewards â€” ISaveSystem is null.");
            return;
        }

        GameData data = _saveSystem.Load();

        // Apply XP and check for level ups
        data.player.xp += _accumulatedXP;
        CheckLevelUp(data.player);

        // Apply resources
        foreach (var kvp in _accumulatedResources)
        {
            if (kvp.Value <= 0) continue;

            switch (kvp.Key)
            {
                case ResourceType.Gold:
                    data.resources.gold += kvp.Value;
                    break;
                case ResourceType.Food:
                    data.resources.food += kvp.Value;
                    break;
                case ResourceType.Energy:
                    data.resources.energy += kvp.Value;
                    break;
                case ResourceType.Electronics:
                    data.resources.electronics += kvp.Value;
                    break;
                default:
                    //Debug.LogWarning($"[ArenaRewardTracker] Unknown resource type: {kvp.Key}");
                    break;
            }
        }

        _saveSystem.Save(data);

        //Debug.Log($"[ArenaRewardTracker] Rewards applied â€” XP: {_accumulatedXP}, " + $"Gold: {_accumulatedResources[ResourceType.Gold]}, " + $"Food: {_accumulatedResources[ResourceType.Food]}, " + $"Energy: {_accumulatedResources[ResourceType.Energy]}, " + $"Electronics: {_accumulatedResources[ResourceType.Electronics]}");
    }

    /// <summary>
    /// Simple level-up check: required XP per level = level * 100.
    /// Loops in case the player accumulated enough XP for multiple level-ups.
    /// </summary>
    private void CheckLevelUp(PlayerData playerData)
    {
        int requiredXP = GetRequiredXPForLevel(playerData.level);

        while (playerData.xp >= requiredXP)
        {
            playerData.xp -= requiredXP;
            playerData.level++;

            //Debug.Log($"[ArenaRewardTracker] Player leveled up to level {playerData.level}!");

            requiredXP = GetRequiredXPForLevel(playerData.level);
        }
    }

    /// <summary>
    /// Returns the XP required to advance from the given level to the next.
    /// Formula: level * 100 (e.g., level 1 â†’ 100 XP, level 5 â†’ 500 XP).
    /// </summary>
    private int GetRequiredXPForLevel(int level)
    {
        return level * 100;
    }

    #endregion
}
