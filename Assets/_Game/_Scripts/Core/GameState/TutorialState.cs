using UnityEngine;

public class TutorialState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        //Debug.Log("Entered TutorialState. Welcome to the game!");
    }

    public void UpdateState(GameStateManager manager)
    {
        
    }

    public void ExitState(GameStateManager manager)
    {
        //Debug.Log("Exiting TutorialState...");
    }
}
