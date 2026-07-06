using UnityEngine;

public struct EnemyDiedEvent : IGameEvent
{
    public GameObject gameObject;
    public Enemy Enemy;
    public Vector3 DeathPosition;
}

