using UnityEngine;

public class HubState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        Debug.Log("Entered Hub!");
    }

    public void UpdateState(GameStateManager manager)
    {
    }

    public void ExitState(GameStateManager manager)
    {
    }
}