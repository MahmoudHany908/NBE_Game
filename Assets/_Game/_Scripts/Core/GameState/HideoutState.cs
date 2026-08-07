using UnityEngine;

public class HideoutState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        //Debug.Log("Entered Hideout. Enabling UI and passive production.");
    }

    public void UpdateState(GameStateManager manager)
    {

    }

    public void ExitState(GameStateManager manager)
    {
        //Debug.Log("Leaving Hideout...");
    }
}
