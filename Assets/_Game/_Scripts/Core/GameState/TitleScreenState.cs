using UnityEngine;

public class TitleScreenState : IGameState
{
    private readonly ISaveSystem _saveSystem;

    public TitleScreenState(ISaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    public void EnterState(GameStateManager manager)
    {
        Debug.Log("[TitleScreenState] Entered Title Screen.");
    }

    public void UpdateState(GameStateManager manager)
    {
        // No logic needed in Update for the state itself, UI handles it.
    }

    // Call this method from whatever UI script handles the "Play" button!
    public void StartGame(GameStateManager manager)
    {
        GameData data = _saveSystem != null ? _saveSystem.Load() : new GameData();

        if (!data.player.hasCompletedTutorial)
        {
            manager.LoadSceneAndChangeState("HubScene", new HideoutState());
        }
        else
        {
            manager.LoadSceneAndChangeState("HubScene", new HideoutState());
        }
    }

    public void ExitState(GameStateManager manager)
    {
        Debug.Log("[TitleScreenState] Exiting Title Screen.");
    }
}
