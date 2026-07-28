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
        Debug.Log("Initializing App...");
        _isBootComplete = true;
    }

    public void UpdateState(GameStateManager manager)
    {
        if (_isBootComplete)
        {
            _isBootComplete = false;
            
            // Loads HubScene directly without needing a Title or Hub state script
            manager.LoadSceneAndChangeState("HubScene", null);
        }
    }

    public void ExitState(GameStateManager manager)
    {
        Debug.Log("Boot complete.");
    }
}