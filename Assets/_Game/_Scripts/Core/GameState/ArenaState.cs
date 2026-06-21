using UnityEngine;

public class ArenaState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        Debug.Log("Entered Arena. Spawning enemies...");
    }

    public void UpdateState(GameStateManager manager) { }

    public void ExitState(GameStateManager manager)
    {
        Debug.Log("Leaving Arena...");
    }
}