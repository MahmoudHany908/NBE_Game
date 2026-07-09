using UnityEngine; 

public interface IGameEvent
{
}


public struct PlayerDeathEvent : IGameEvent
{
    public Vector3 deathPostion;
}