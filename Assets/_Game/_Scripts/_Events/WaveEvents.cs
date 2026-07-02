using UnityEngine;



public struct WaveStartedEvent : IGameEvent
{
    public int WaveNumber;       // 1-based
    public int TotalEnemies;     // enemies scheduled for this wave
    public float SpawnInterval;  // seconds between spawns
}

public struct WaveCompletedEvent : IGameEvent
{
    public int WaveNumber;
    public float ClearTimeSeconds; // how long the wave took
}

public struct EnemySpawnedEvent : IGameEvent
{
    public int WaveNumber;
    public int EnemyId;          // Instance ID of the spawned GameObject
    public Vector3 SpawnPosition;
    public int RemainingToSpawn; // enemies still queued this wave
}

public struct EnemyKilledEvent : IGameEvent
{
    public int WaveNumber;
    public int EnemyId;
    public Vector3 DeathPosition;
    public int RemainingAlive;   // enemies still alive this wave
}

/// <summary>
/// Fired when a milestone wave is reached (every MilestoneInterval waves).
/// WaveManager pauses spawning until a MilestoneChoiceEvent is published back.
/// </summary>
public struct MilestoneReachedEvent : IGameEvent
{
    public int WaveNumber;      // the wave that just completed (e.g. 5, 10, 15…)
    public int MilestoneIndex;  // 1 = first milestone, 2 = second, …
    public RunStats Stats;      // lifetime stats for the UI to display
}

/// <summary>
/// Publish this to resolve a pending milestone (from UI or any system).
/// </summary>
public struct MilestoneChoiceEvent : IGameEvent
{
    public MilestoneChoice Choice;
    public int AtWaveNumber;
}
