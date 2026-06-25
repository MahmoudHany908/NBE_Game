
using UnityEngine;





public class PlayerDeathState : IGameState
{

    public void EnterState(GameStateManager manager)
    {
        Debug.Log("Player has died. ");
    }

    public void ExitState(GameStateManager manager)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState(GameStateManager manager)
    {
        throw new System.NotImplementedException();
    }
}