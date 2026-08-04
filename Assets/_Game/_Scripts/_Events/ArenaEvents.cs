using UnityEngine;
using VaultSurvivors.Core.Production;

public readonly struct WaveClearedEvent : IGameEvent
{
    public readonly int WaveNumber;
    public WaveClearedEvent(int waveNumber) => WaveNumber = waveNumber;
}


public readonly struct ExtractionAvailableEvent : IGameEvent
{
    public readonly int WaveNumber;
    public ExtractionAvailableEvent(int waveNumber) => WaveNumber = waveNumber;
}

public readonly struct ExtractionDecisionEvent : IGameEvent
{
    public readonly bool DidExtract;
    public ExtractionDecisionEvent(bool didExtract) => DidExtract = didExtract;
}


public readonly struct PlayerDiedEvent : IGameEvent
{
    public readonly Vector3 deathPostion;
    public PlayerDiedEvent(Vector3 position) => deathPostion = position;

}

public readonly struct PlayerTakeDamageEvent : IGameEvent
{
    public readonly int damage;
    public readonly float healthPercentage;
    public PlayerTakeDamageEvent(int damage, float healthPercentage)
    {
        this.damage = damage;
        this.healthPercentage = healthPercentage;
    }

}
public readonly struct PlayerHealedEvent : IGameEvent
{
    public readonly int healAmount;
    public readonly float healthPercentage;
    public PlayerHealedEvent(int healAmount, float healthPercentage)
    {
        this.healAmount = healAmount;
        this.healthPercentage = healthPercentage;
    }
}

/// <summary>
/// Fired once per resource drop when an enemy is killed.
/// Used by the loot popup UI to show individual drop notifications.
/// </summary>
public readonly struct LootGainedEvent : IGameEvent
{
    public readonly ResourceType ResourceType;
    public readonly int Amount;

    public LootGainedEvent(ResourceType resourceType, int amount)
    {
        ResourceType = resourceType;
        Amount = amount;
    }
}

/// <summary>
/// Fired when XP is gained from killing an enemy.
/// Used by the loot popup UI to show XP notifications.
/// </summary>
public readonly struct XpGainedEvent : IGameEvent
{
    public readonly int Amount;
    public XpGainedEvent(int amount) => Amount = amount;
}