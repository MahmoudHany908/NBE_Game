using UnityEngine;

public struct EnemyTakeDamageEvent : IGameEvent
{
    public int Damage;
    public Enemy Enemy;
    public Vector3 DamagePosition;
}

