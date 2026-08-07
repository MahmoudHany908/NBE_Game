using UnityEngine;

public class LoadingState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        //Debug.Log("[LoadingState] Entered Loading State.");
    }

    public void UpdateState(GameStateManager manager)
    {
        // Logic while loading (if any)
    }

    public void ExitState(GameStateManager manager)
    {
        //Debug.Log("[LoadingState] Exited Loading State.");
    }
}
