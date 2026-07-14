using UnityEngine;

/// <summary>Raised the moment a wave has been fully spawned and every enemy in it is dead.</summary>
public readonly struct WaveClearedEvent : IGameEvent
{
    public readonly int WaveNumber;
    public WaveClearedEvent(int waveNumber) => WaveNumber = waveNumber;
}

/// <summary>
/// Raised instead of auto-advancing when a milestone wave (every 5th) is cleared.
/// UI should show the extract-or-continue prompt in response to this.
/// </summary>
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
    public PlayerTakeDamageEvent(int damage) => this.damage = damage;

}