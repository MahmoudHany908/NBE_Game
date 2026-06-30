using UnityEngine;
public struct PlayerTakeDamageEvent : IGameEvent
{
    public int Damage;

}

public struct PlayerDeathEvent : IGameEvent
{
    public Vector3 deathPostion;

}

