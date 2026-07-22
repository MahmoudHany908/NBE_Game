using UnityEngine;

public class BootState : IGameState
{
    private bool _isBootComplete = false;
    private readonly ISaveSystem _saveSystem;

    public BootState(ISaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    public void EnterState(GameStateManager manager)
    {
        Debug.Log("Initializing App... Loading Save Files...");
        _isBootComplete = true;
    }

    public void UpdateState(GameStateManager manager)
    {
        if (_isBootComplete)
        {
            _isBootComplete = false;
            manager.LoadSceneAndChangeState("TitleScene", new TitleScreenState(_saveSystem));
        }
    }

    public void ExitState(GameStateManager manager)
    {
        Debug.Log("Boot complete.");
    }
}