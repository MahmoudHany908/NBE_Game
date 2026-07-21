using UnityEngine;

public class TitleScreenState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        Debug.Log("[TitleScreenState] Entered Title Screen.");
    }

    public void UpdateState(GameStateManager manager)
    {
        // No logic needed in Update for the state itself, UI handles it.
    }

    public void ExitState(GameStateManager manager)
    {
        Debug.Log("[TitleScreenState] Exiting Title Screen.");
    }
}
