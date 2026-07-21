using UnityEngine;

public class BootState : IGameState
{
    private bool _isBootComplete = false;

    public void EnterState(GameStateManager manager)
    {
        Debug.Log("Initializing App... Loading Save Files...");
        // We'll load our actual save data later on here
        _isBootComplete = true;
    }

    public void UpdateState(GameStateManager manager)
    {
        if (_isBootComplete)
        {
            _isBootComplete = false;
            manager.LoadSceneAndChangeState("TitleScene", new TitleScreenState());
        }
    }

    public void ExitState(GameStateManager manager)
    {
        Debug.Log("Boot complete.");
    }
}