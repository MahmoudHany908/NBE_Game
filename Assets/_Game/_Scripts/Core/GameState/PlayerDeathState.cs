using UnityEngine;

public class PlayerDeathState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        //Debug.Log("Player has died. Triggering death sequence...");
        // Publish PlayerDiedEvent here
    }

    public void UpdateState(GameStateManager manager)
    {
        //No logic needed while dead
    }

    public void ExitState(GameStateManager manager)
    {
        //Debug.Log("Resetting from death state...");
    }
}
