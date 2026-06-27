using UnityEngine;

public struct EnemyDiedEvent : IGameEvent
{
    public Enemy Enemy;
    public Vector3 DeathPosition;
}

